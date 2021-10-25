using AGP.FSA.Library;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FeeScheduleManager.Common;
using System.Net;
using Newtonsoft.Json;

namespace FeeScheduleManager.UI
{
    public partial class PdfToExcelExtract : System.Web.UI.Page
    {
        string ReportsPath = ConfigurationManager.AppSettings["OdrivePathTemplate"];
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void UpdatePanel_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
               .Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page),
                new object[] { sender as UpdatePanel });
        }
        protected void btnSubmit_Click(Object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;
            btnSubmitFiles.Enabled = false;
            
            string filename = string.Empty;

            HttpWebRequest myWebRequest = null;
            string url= string.Format(ConfigurationManager.AppSettings["Pdf2ExcelApi"].ToString(), ReportsPath + txtLinkDescription.Text);
            Logger.Current.LogInformation("Pdf to excel: " + url + ".");

            myWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myWebRequest.UseDefaultCredentials = true;
            myWebRequest.Timeout = 180000;

            WebResponse myWebResponse = myWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(myWebResponse.GetResponseStream());
            filename = JsonConvert.DeserializeObject<string>(streamReader.ReadToEnd());

            try
            {
                if (!File.Exists(filename))
                {
                    lblMessage.Text = "Please supply a valid pdf file";
                    Logger.Current.LogInformation("Error in converting file from pdf to excel: " + filename + ".");
                }
                else
                {
                    
                    Common.EmailHelper emailer = new Common.EmailHelper();
                    emailer.AsyncEmailFail += Emailer_AsyncEmailFailureHandler;
                    string email = FeeScheduleManager.Common.NetworkUtils.GetEmailAccountFromAD(Page.User.Identity.Name.StripDomain(), Page.User.Identity.Name.CaptureDomain());

                    emailer.SendSync(email, "", "",
                       ConfigurationManager.AppSettings["MailFromAddress"].ToString(), "Converted Excel file", "", ReportsPath + Path.GetFileName(filename)); // system email

                    Logger.Current.LogInformation("Download the file: " + filename + ".");
                    //Response.Clear();
                    //Response.ContentType = "application/octet-stream";
                    //Response.AddHeader("Content-Disposition", "attachment;filename=\"" + Path.GetFileName(filename) + "\"");
                    //Response.WriteFile(Server.MapPath("~/Model/" + Path.GetFileName(filename)));
                    //Response.End();
                    //Logger.Current.LogInformation("Download the file: " + Server.MapPath("~/Model/" + Path.GetFileName(filename)) + ".");
                    
                    RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                    var byteArray = new byte[4];
                    provider.GetBytes(byteArray);
                    var randomInteger = BitConverter.ToUInt32(byteArray, 0);
                    var byteArray2 = new byte[8];
                    provider.GetBytes(byteArray2);
                    var randomDouble = BitConverter.ToDouble(byteArray2, 0);
                    Response.Redirect(Request.Url.AbsoluteUri + "?r=" + randomDouble);

                }
            }
            catch (Exception ex)
            {
                Logger.Current.LogError("LogError:", ex);
            }

        }

        private void resetFieldsPostValidations(bool postSubmit = false)
        {
            if (!postSubmit) uplFeeSchedulePdfFiles.ID = null;
        }


        protected void uplFeeSchedulePdfFiles_FileUploadComplete(object sender, DevExpress.Web.FileUploadCompleteEventArgs e)
        {
            e.CallbackData = string.Empty;
            // if all validations pass validate file itself
            if (uplFeeSchedulePdfFiles.HasFile)
            {
                if (!(uplFeeSchedulePdfFiles.PostedFile.ContentLength > 0))
                {
                    lblMessage.Text = string.Format("Select a non empty file to upload.");
                    return;
                }

                if ((uplFeeSchedulePdfFiles.PostedFile.ContentType != "application/pdf")
                    || (!uplFeeSchedulePdfFiles.FileName.ToLower().EndsWith(".pdf")))
                {
                    lblMessage.Text = "Please supply a pdf file";
                    return;
                }

            }
            else
            {
                lblMessage.Text = "Please supply a pdf file.  Please see help for additional validation details.";

                return;
            }
            Logger.Current.LogInformation(string.Format("Initial request for {0} with original filename {1} has passed tier 1 validations.", Page.User.Identity.Name, uplFeeSchedulePdfFiles.FileName));



            try
            {
                if (!Directory.Exists(ReportsPath))
                {
                    Logger.Current.LogInformation(string.Format("Impersonation {0}:Windows Identity {1}:IsAuthenticated {2}:IsAnonymous {3}:Page Identity {4}:Thread Identity {5}",
                        System.Security.Principal.WindowsIdentity.GetCurrent().ImpersonationLevel.ToString(),
                        System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                        System.Security.Principal.WindowsIdentity.GetCurrent().IsAuthenticated,
                        System.Security.Principal.WindowsIdentity.GetCurrent().IsAnonymous,
                        Page.User.Identity.Name,
                        System.Threading.Thread.CurrentPrincipal.Identity.Name));
                    Directory.CreateDirectory(ReportsPath);
                }
                string filepath = ReportsPath + Path.GetFileName(uplFeeSchedulePdfFiles.FileName);
                uplFeeSchedulePdfFiles.SaveAs(Path.Combine(filepath));

            }

            catch (Exception ex)
            {
                lblMessage.Text = "Unable to upload fee schedule pdf file successfully.  Please try again.";
                Logger.Current.LogError(lblMessage.Text, ex);
                return;
            }
            e.CallbackData = Path.GetFileName(uplFeeSchedulePdfFiles.FileName);
            this.resetFieldsPostValidations(true);

        }


        /// <summary>
        /// Handle any async email send failures (log occurrence)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private static void Emailer_AsyncEmailFailureHandler(object sender, FSAEventArgs<string> msg)
        {
            Logger.Current.LogWarn(string.Format("Async email failed with exception of {0}", msg.Value));
        }
    }
}