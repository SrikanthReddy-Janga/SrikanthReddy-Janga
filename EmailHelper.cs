using System;
using AGP.FSA.Library;
using System.Net.Mail;
using System.Configuration;
using FeeScheduleManager.Model;
using System.Net.Mime;

namespace FeeScheduleManager.Common
{
    public class EmailHelper 
    {
        private delegate bool SendMessage(string to, string cc, string bcc, string from, string subject, string body, string attachment);

        public event EventHandler AsyncEmailSuccess;
        public event EventHandler<FSAEventArgs<string>> AsyncEmailFail;

        public EmailHelper()
        {
           this.Client = new SmtpClient();
           this.FromAddress = (String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailFromAddress"])) ? "depthpssupport@amerigroup.com" : ConfigurationManager.AppSettings["MailFromAddress"];
           this.SmtpServer = (String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailServer"])) ? "smtp.amerigroup.com" : ConfigurationManager.AppSettings["MailServer"];
           this.SmtpPort = (String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailServerPort"])) ? "smtp.amerigroup.com" : ConfigurationManager.AppSettings["MailServerPort"];         
        }

        /// <summary>
        /// Seam for testing substitution
        /// </summary>
        public SmtpClient Client { get; private set; }
        public string FromAddress { get; private set; }
        public string SmtpServer { get; private set; }
        public string SmtpPort { get; private set; }
        
        /// <summary>
        /// Send email asynchronously
        /// </summary>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void SendAsync(string to, string cc, string bcc, string from, string subject, string body, string attachment = "")
        {
            Logger.Current.LogInformation("Asynchronous email request received for " + to + " with subject of " + subject);
            EmailHelper.SendMessage sendMessageAsync = new EmailHelper.SendMessage(SendSync);
            sendMessageAsync.BeginInvoke(to, cc, bcc, from, subject, body,attachment, null, null);
            Logger.Current.LogInformation("Asynchronous email request initiated.");
        }

        /// <summary>
        /// Send email message
        /// </summary>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>        
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public bool SendSync(string to, string cc, string bcc, string from, string subject, string body, string attachment = "")
        {
            bool success = false;
            if (String.IsNullOrEmpty(this.SmtpServer))
            {
                Logger.Current.LogWarn("SMTP host not configured.  Unable to send email.");
                return success;
            }
            to = FixAddresses(to, string.Empty);

            string smtpHost = this.SmtpServer;
            int smtpPort = Int32.Parse(this.SmtpPort);
            bool sendSecure = false;

            if (!String.IsNullOrWhiteSpace(cc))
            {   
                cc = FixAddresses(cc, string.Empty);
            }

            if (!String.IsNullOrWhiteSpace(bcc))
            {
                bcc = FixAddresses(bcc, string.Empty);
            }

            try
            {
                MailMessage message = new MailMessage();
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.To.Add(to); // can separate email addresses with comma to supply multiple primary recipients
                if (!string.IsNullOrEmpty(attachment))
                {
                    Attachment attachments = new Attachment(attachment, MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attachments);
                }
                message.From = new MailAddress(from);
                if (!string.IsNullOrWhiteSpace(bcc)) message.Bcc.Add(bcc);
                if (!string.IsNullOrWhiteSpace(cc)) message.CC.Add(cc);
                message.IsBodyHtml = true;
                switch (Priority)
                {
                    case EmailPriority.High:
                        message.Priority = System.Net.Mail.MailPriority.High;
                        break;
                    case EmailPriority.Normal:
                        message.Priority = System.Net.Mail.MailPriority.Normal;
                        break;
                    case EmailPriority.Low:
                        message.Priority = System.Net.Mail.MailPriority.Low;
                        break;
                    default:
                        message.Priority = System.Net.Mail.MailPriority.Normal;
                        break;
                }

                message.Subject = subject;
                message.Body = NewlinesToBRs(body);

                Client.Host = smtpHost;
                Client.Port = smtpPort;
                Client.DeliveryMethod = SmtpDeliveryMethod.Network;
                Client.EnableSsl = sendSecure;
#if DEBUG
                // Client.Host = "localhost"; // uncomment if machine name not in AGP relay list
#endif

                Client.Send(message);

                success = true;
            }
            catch (Exception e)
            {
                success = false;

                string errorMsg = String.Format("Email helper caught an exception. E-mail details follow. To: {0}, From: {1}, Subject: {2}, SMTPServer: {3}. Exception of '{4}'.", to, from, subject, smtpHost, e.GetBaseException().Message);
                Logger.Current.LogError(errorMsg, e);

                if (AsyncEmailFail != null)
                    AsyncEmailFail(this, new FSAEventArgs<string>(errorMsg)); 
            }

            if (AsyncEmailSuccess != null)
                AsyncEmailSuccess(this, new EventArgs());

            return success;
        }

        /// <summary>
        /// Converts newline characters \r\n windows, \n unix, \r mac line breaks to <br /> so displayed as entered in form textarea
        /// </summary>
        private static string NewlinesToBRs(string text)
        {
            string ret = text;
            ret = System.Text.RegularExpressions.Regex.Replace(ret, "\r\n|\n|\r", "<br />");
            return ret;
        }

        public EmailPriority Priority { get; set; }
        public enum EmailPriority
        {
            High, Normal, Low
        }

        /// <summary>
        /// Returns a comma delimited list of properly formatted email addresses from a source list of partial addresses.
        /// </summary>
        /// <param name="notify">the new list of email addresses</param>
        /// <param name="compare">the existing send-to list, used to ensure we don't have dupes</param>
        /// <returns>List of properly formatted email addresses.</returns>
        public static string FixAddresses(string notify, string compare)
        {
            // condition compare address(es)
            if (compare == null)
            {
                compare = string.Empty;
            }
            compare = compare.Trim().ToLower().Replace(" ", "");
            compare = "," + compare + ",";

            // condition notify address(es) and examine each for correctness and duplication in compare 
            string ret = string.Empty;
            if (notify != null)
            {
                ret = notify.Trim().ToLower().Replace(" ", "");
                ret = ret.Replace(";", ",");

                if ((!string.IsNullOrEmpty(ret)) && (!ret.EndsWith(",")))
                {
                    ret += ",";
                }

                string[] parts = ret.Split(',');
                ret = string.Empty;
                string cmp = string.Empty;
                foreach (string part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        if (!part.Contains("@"))
                        {
                            cmp = part + "@amerigroup.com,";
                        }
                        else
                        {
                            cmp = part + ",";
                        }

                        if (!compare.Contains("," + cmp))
                        {
                            ret += cmp;
                        }

                        if (compare.ToLower() == "," + cmp.ToLower()) ret += cmp; // special case when notify equals compare
                    }
                }

            }

            // cleanup (remove trailing comma)
            if (ret.EndsWith(","))
            {
                ret = ret.Substring(0, ret.Length - 1);
            }

            return ret;
        }
    }
}