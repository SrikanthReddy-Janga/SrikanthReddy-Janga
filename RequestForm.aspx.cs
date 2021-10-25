using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using BenefitSummary.ViewModel;
using BenefitSummary.Model;
using BenefitSummary.Common;
using ExtensionMethods;
using Amerigroup.HPSS.FeeSchedulePolling;
using AGP.FSA.Library;
using System.IO;
using FeeSchedulePolling.Workflows;
using System.Text;
using System.Data;
using System.Reflection;
using DevExpress.Web;
using MigrationRunRequest.Domain.Models;
using System.Net;
using MigrationRunRequest.Domain;
using Newtonsoft.Json;

namespace BenefitSummary.UI
{

    public partial class RequestForm : System.Web.UI.Page
    {
        public int templateID;
        public int savefilename;

        protected void Page_Load(object sender, EventArgs e)
        {
            Modules.SystemHealthCheck checking = new Modules.SystemHealthCheck();
            checking.SystemCheck(sender, e);
            // initializations
            if (!Page.IsPostBack)
            {
                litFnMaxSize.Text = BenefitSummary.Common.Globals.MaximumRawFileSize.ToString();

                lblMessage.Text = "* indicates required fields.  Please complete all fields and specify an Excel (.xlsx) file based on the Benefit Summary Excel template file.";

                btnCancel.Click += new EventHandler(btnCancel_Click);
                btnSubmit.Click += new EventHandler(btnSubmit_Click);

                RequestViewModel vm = new RequestViewModel(new SqlRepository());
                ddlTemplateTypes.DataSource = vm.GetAllActiveTemplateTypes();
                ddlTemplateTypes.DataTextField = "TEMPLATETYPE";
                ddlTemplateTypes.DataValueField = "TEMPLATEID";
                ddlTemplateTypes.DataBind();
                ddlTemplateTypes.SelectedIndex = 0;  // default to Medicaid
                ddlTemplateTypes.Focus();
                hdnTemplateID.Value = (ddlTemplateTypes.SelectedIndex + 1).ToString();
                ddlTemplateTypes_SelectedIndexChanged(this, EventArgs.Empty);

                string templateName = vm.GetTemplateName(1);
                string emailaddress = string.Empty;
                grdViewRequests.AutoFilterByColumn(grdViewRequests.Columns["EmailAddress"], BenefitSummary.Common.NetworkUtils.GetEmailAccountFromAD(Page.User.Identity.Name.StripDomain(), Page.User.Identity.Name.CaptureDomain()));

                try
                {
                    emailaddress = BenefitSummary.Common.NetworkUtils.GetEmailAccountFromAD(Page.User.Identity.Name.StripDomain(), Page.User.Identity.Name.CaptureDomain());
                    if (Page.User.Identity.Name.StripDomain().Equals("kmoren1sa")) emailaddress = "kirk.moren@amerigroup.com";
                    tknEmailAddress.Text = emailaddress;
                    if (String.IsNullOrEmpty(emailaddress)) Logger.Current.LogDebug("DEBUG: Unable to retreive email address for user :" + Page.User.Identity.Name.StripDomain());
                }
                catch (Exception ex)
                {
                    Logger.Current.LogDebug(string.Format("DEBUG: Exception retrieving email address for user : {0} :: exception {1}.", Page.User.Identity.Name.StripDomain(), ex.Message));
                }
            }
        }

        protected void Page_PreRender(Object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                txtTrackingNumber.Text = String.Empty;
            }
        }

        protected void Timer1_Tick(Object sender, EventArgs e)
        {
            grdViewRequests.DataBind();
        }

        /// <summary>
        /// Handle submit button actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSubmit_Click(Object sender, EventArgs e)
        {
            Modules.SystemHealthCheck checking = new Modules.SystemHealthCheck();
            checking.SystemCheck(sender, e);
            if (Page.IsValid)
            {
                btnSubmit.Enabled = true;

                // perform initial validations
                if (string.IsNullOrEmpty(ddlTemplateTypes.Text.Trim()) || string.IsNullOrEmpty(txtTrackingNumber.Text.Trim()) || string.IsNullOrEmpty(tknEmailAddress.Text.Trim()))
                {
                    lblMessage.Text = "Please complete all fields and try again.";
                    return;
                }
                if (!(tknEmailAddress.IsValid))
                {
                    lblMessage.Text = "Please enter valid email address";
                    return;
                }
                // if all validations pass validate file itself
                const string msgFileError = "Please supply a Benefit Summary Excel (xlsx) file based on the Benefit Summary template (see template download link above).  Please see help for additional validation details.";
                if (uplFeeScheduleFile.HasFile)
                {
                    int maxUploadFileSize = Int32.Parse(ConfigurationManager.AppSettings["MaximumUploadFileSize"]);
                    if (uplFeeScheduleFile.PostedFile.ContentLength > maxUploadFileSize)
                    {
                        lblMessage.Text = string.Format("Supplied file is too large (> {0}).  Please reduce file or submit as separate files.", ConfigurationManager.AppSettings["MaximumUploadFileSize"].FormatBytes());
                        return;
                    }

                    if (System.IO.Path.GetFileName(uplFeeScheduleFile.PostedFile.FileName).Length > BenefitSummary.Common.Globals.MaximumRawFileSize)
                    {
                        lblMessage.Text = string.Format("Supplied filename is too long. The limit is {0} characters and is currently {1}).  Please rename the file.",
                            BenefitSummary.Common.Globals.MaximumRawFileSize, System.IO.Path.GetFileName(uplFeeScheduleFile.PostedFile.FileName).Length);
                        return;
                    }

                    if ((uplFeeScheduleFile.PostedFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" && uplFeeScheduleFile.PostedFile.ContentType != "application/octet-stream")
                        || !uplFeeScheduleFile.FileName.ToLower().EndsWith(".xlsx"))
                    {
                        lblMessage.Text = msgFileError + " (002).";
                        return;
                    }
                    bool isAccrUnique = new RequestViewModel(new SqlRepository()).ValidateAccrUniqueness(txtTrackingNumber.Text.Trim());
                    if (!isAccrUnique)
                    {
                        lblMessage.Text = string.Format("Tracking Number can only be reused after an ETL or Data Error.  Please supply a unique tracking number or add a suffix value (i.e. PCM-1234 becomes PCM-1234_1) and resubmit.");
                        return;
                    }

                    if (!BenefitSummary.Common.Globals.IsValidTrackNumber(txtTrackingNumber.Text))  // Tracking number is used to create a filename and has to meet windows filename naming requirements
                    {
                        lblMessage.Text = string.Format("Supplied tracking number may not contain special characters < > : \" \\ / | ? *. Please correct the tracking number and resubmit.");
                        return;
                    }
                    else
                        lblMessage.Text = "";

                }
                else
                {
                    lblMessage.Text = msgFileError + ".";
                    return;
                }
                Logger.Current.LogInformation(string.Format("Initial request for {0} with original filename {1} has passed tier 1 validations.", Page.User.Identity.Name, uplFeeScheduleFile.FileName));

                Request req = this.BuildRequestDTO();
                string filename = Path.GetFileName(uplFeeScheduleFile.FileName);
                try
                {
                    if (!Directory.Exists(req.ReportsPath))
                    {
                        Logger.Current.LogInformation(string.Format("Impersonation {0}:Windows Identity {1}:IsAuthenticated {2}:IsAnonymous {3}:Page Identity {4}:Thread Identity {5}",
                            System.Security.Principal.WindowsIdentity.GetCurrent().ImpersonationLevel.ToString(),
                            System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                            System.Security.Principal.WindowsIdentity.GetCurrent().IsAuthenticated,
                            System.Security.Principal.WindowsIdentity.GetCurrent().IsAnonymous,
                            Page.User.Identity.Name,
                            System.Threading.Thread.CurrentPrincipal.Identity.Name));
                        Directory.CreateDirectory(req.ReportsPath);
                    }

                    uplFeeScheduleFile.SaveAs(Path.Combine(req.ReportsPath + req.ComputedFileName));
                    Session["FileName"] = Path.Combine(req.ReportsPath + req.ComputedFileName);
                }
                catch (PathTooLongException pex)
                {
                    lblMessage.Text = "Unable to upload Benefit Summary file successfully because the filename is too long.  Please shorten and try again.";
                    Logger.Current.LogError(lblMessage.Text, pex);
                    return;
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Unable to upload Benefit Summary successfully.  Please try again.";
                    Logger.Current.LogError(lblMessage.Text, ex);
                    return;
                }

                // perform tier 1 (immediate) validations
                ValidationEngine validator = new ValidationEngine(req);
                string errorMsg = string.Empty;
                bool passed = validator.PerformTierOneValidations(out errorMsg);
                if (!passed)
                {

                    if (Directory.Exists(req.ReportsPath))
                    {
                        Directory.Delete(req.ReportsPath, true);
                    }
                    lblMessage.Text = errorMsg;
                    Logger.Current.LogWarn(lblMessage.Text);
                    return;
                }

                // insert request
                try
                {
                    req.RequestID = new RequestViewModel(new SqlRepository()).InsertRequest(req);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Unable to save request.  Please contact administrator if problem persists.";
                    Logger.Current.LogError(lblMessage.Text, ex);
                    return;
                }

                // return response immediate to UI while tasktierTwos task continues emailing results
                lblMessage.Text = "Your request has passed all initial validations and is queued for additional processing. Results will be emailed shortly.";
                this.resetFieldsPostValidations(true);

                // continue asynchronously to tier two validations and workflow
                System.Threading.Tasks.Task taskTierTwos = System.Threading.Tasks.Task.Factory.StartNew(() => this.DoAsyncTierTwoAndWorkflowStart(req)); // will continue after UI status and form clearing
            }
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        public Request BuildRequestDTO()
        {
            Request req = new Request();
            req.With(r =>
            {
                r.CreatedDate = DateTime.Now;
                r.RequestStatusID = (int)BenefitSummary.Model.Request.RequestStatus.Submitted;
                r.OriginalFileName = uplFeeScheduleFile.FileName;
                r.TemplateID = Convert.ToInt32(ddlTemplateTypes.Text);
                r.TemplateType = ddlTemplateTypes.SelectedItem.Text;
                r.TrackingNumber = txtTrackingNumber.Text.ToUpper().Trim();
                r.FacetsID = "FAC2D";
                r.UserName = Page.User.Identity.Name.StripDomain();
                r.UserDomain = Page.User.Identity.Name.CaptureDomain();
                r.UserEmailAddress = tknEmailAddress.Text.Trim();
                r.ComputedFileName = r.ComputeFileName(r.UserName, r.OriginalFileName);
                r.ReportsPath = r.FormatReportsPath(r.TrackingNumber);
            });
            return req;
        }

        protected string DetermineContact(string emailContact)
        {
            return string.Format("<a href='mailto:{0}'>{0}</a>", emailContact);
        }

        protected string FormatReportLink(string reportsPath)
        {
            string browserType = HttpContext.Current.Request.Browser.Browser;
            string template = reportsPath;

            if (browserType.ToUpper() == "IE" || browserType.ToUpper() == "INTERNETEXPLORER")
                return string.Format("<a href='file:///" + template + "'><img src=\'../content/images/icon_folder_open.png' title='Click to view associated reports...' style='border:none;' /></a>");
            else
            {
                template = template.Replace("\\", "*");
                return string.Format("<a href='#' onclick=\"copyToClipBoard('" + template + "');\"><img src=\'../content/images/icon_folder_open.png' title='Click to copy location of associated reports for use in Windows Explorer...' style='border:none;' display='inline;' /></a>");
                //return string.Format("<a href='#' onclick=\"copyToClipBoard('" + template + "');\"><img src=\'../images/icon_folder_open.png' title='Click to copy location of associated reports for use in Windows Explorer...' style='border:none;' display='inline;' /></a>");
            }
        }

        private void DoAsyncTierTwoAndWorkflowStart(Request req)
        {
            RequestViewModel vm = new RequestViewModel(new SqlRepository());
            DataTable dtTemplateColumns = vm.GetTemplateColumns(req.TemplateID);

            Logger.Current.LogInformation("Starting request: " + Utilities.DumpProperties(req) + ".");

            // create data for validation
            ExcelHelper excel = new ExcelHelper();
            List<string> bsbsPrefixes = new List<string>();
            List<string> bsbsTypes = new List<string>();
            List<DevExCell> data = excel.CreateDatasFromExcel(req.ReportsPath, req.ReportsPath, req.ComputedFileName, out bsbsPrefixes, out bsbsTypes); // create data collection for workflow
            if (data.Count != 0) req.TotalRows = (data.Count / BenefitSummary.Common.Globals.MaxColumns); // used for metrics (data is a cells collection)

            // do tier two validations (email if error with error report)
            ValidationEngine validateEngine = new ValidationEngine(req);
            List<RequestError> errors = validateEngine.PerformTierTwoValidations(data, bsbsPrefixes.ToArray(), bsbsTypes.ToArray(), req, dtTemplateColumns);

            req.WarningCount = (from e in errors where e.CategoryID == 5 select e).Count();
            req.ErrorCount = (from e in errors where e.CategoryID != 5 select e).Count();

            if (req.ErrorCount > 0)
            {
                // create error report (if errors exist)
                ErrorReport report = new ErrorReport(errors, req, bsbsPrefixes);
                report.CreateErrorReportDev();
                req.CompletedDate = DateTime.Now;

                Logger.Current.LogError(string.Format("Request ID {0} has error(s).  See error report @ {1} for details.", req.RequestID, req.ReportsPath), null);

                string targetEnvironment = "Dev";
                switch (ConfigurationManager.AppSettings["TargetBSEnvironment"])
                {
                    case "1":
                        targetEnvironment = "Dev";
                        break;
                    case "2":
                        targetEnvironment = "QA";
                        break;
                    case "3":
                        targetEnvironment = "Production";
                        break;
                    default:
                        Logger.Current.LogError("Invalid Benefit Summary Loader environment value specified. Defaulting to 'Dev'.", null);
                        break;
                }

                // send email and update status
                BenefitSummary.Common.EmailHelper emailer = new BenefitSummary.Common.EmailHelper();
                string subject;
                string body = emailer.FormatDataErrorMessage(req, out subject, "BSBS_REQ_DATAERROR", "BS : " + targetEnvironment + ": Request Failed with errors for " + req.TrackingNumber);
                emailer.SendSync(req.UserEmailAddress, ConfigurationManager.AppSettings["PCSSupportEmail"], string.Empty, ConfigurationManager.AppSettings["PCSSupportEmail"], subject, body);

                req.RequestStatusID = (int)Model.Request.RequestStatus.DataError;

                vm.UpdateRequestStatus(req);
                vm.RecordRequestErrors(req.RequestID, errors);
                vm.UpdateRequestStatusCompletedDate(req);

                return;
            }

            if (req.WarningCount > 0)
            {
                // create error report (if errors exist)
                ErrorReport report = new ErrorReport(errors, req, bsbsPrefixes);
                report.CreateErrorReportDev();
                req.CompletedDate = DateTime.Now;

                Logger.Current.LogError(string.Format("Request {0} has warnings(s).  See error report @ {1} for details.", req.TrackingNumber, req.ReportsPath), null);

                vm.RecordRequestErrors(req.RequestID, errors);
            }

            vm.UpdateRequestStatus(req);

            this.BuildRawData(data, req, dtTemplateColumns);

            if (ConfigurationManager.AppSettings["TestingMode"] == "1")
            {
                req.RequestStatusID = (int)Model.Request.RequestStatus.ETL_Error;
                req.CompletedDate = DateTime.Now;
                vm.UpdateRequestStatusCompletedDate(req);

                Logger.Current.LogWarn("TestingMode enabled.  Workflow, events, and related email path not executed");
                System.Diagnostics.Debug.Assert(1 == 1, "TestingMode in effect. Used to test all pre-workflow functionality and avoid external triggers such as workflow start, event logging and email.");
                return;
            }

            // start workflow (workflow must be registered with FSP / TODO: break this by refactoring workflow library with interchangeable / polymorphic interfaces)
            string errormsg = string.Empty; int wfRunID = 0;
            WorkflowContext wfContext;
            Amerigroup.HPSS.FeeSchedulePolling.BLL.Workflow workflow = null;
            WorkflowPayload payload = null;
            try
            {
                payload = new WorkflowPayload(req.RequestID, System.IO.Path.Combine(req.ReportsPath, req.ComputedFileName),
                    req.ComputedFileName, 0, "Not used", req.TrackingNumber, 0, 0);
                workflow = new Amerigroup.HPSS.FeeSchedulePolling.BLL.Workflow(Amerigroup.HPSS.FeeSchedulePolling.BLL.Workflow.GetWorkflowByName("wf_BenefitSummaryLoad").WorkflowID);
                wfContext = new WorkflowContext();
                wfRunID = wfContext.StartWorkflow(workflow, payload, Int32.Parse(ConfigurationManager.AppSettings["TargetInformaticaSystem"]), out errormsg);

                req.RequestStatusID = (int)Model.Request.RequestStatus.Running;
                vm.UpdateRequestStatus(req);
            }
            catch (Exception ex)
            {
                req.RequestStatusID = (int)Model.Request.RequestStatus.ETL_Error;
                req.CompletedDate = DateTime.Now;
                vm.UpdateRequestStatusCompletedDate(req);

                Logger.Current.LogError(string.Format(@"Error starting BenefitSummary workflow.  Payload '{0}'.  Workflow '{1}'.  Message '{2}'.",
                    payload.TryDumpObjectProperties(), workflow.TryDumpObjectProperties(), ex.Message), ex);
                // if (excel != null) excel.Dispose();
            }

            // create file count metrics and wf_coordination entries
            if (wfRunID > 0)
            {
                vm.InsertMetric(req, "1001");
                Logger.Current.LogInformation("Workflow started with run ID of " + wfRunID);
            }
            else
            {
                req.RequestStatusID = (int)Model.Request.RequestStatus.ETL_Error;
                req.CompletedDate = DateTime.Now;
                vm.UpdateRequestStatusCompletedDate(req);

                BenefitSummary.Common.EmailHelper emailer = new BenefitSummary.Common.EmailHelper();
                string subject = "";
                string body = emailer.FormatDataErrorMessage(req, out subject, "BSBS_REQ_WFSTARTERROR", "Workflow Start Error");
                emailer.SendSync(req.UserEmailAddress, ConfigurationManager.AppSettings["PCSSupportEmail"], string.Empty,
                    ConfigurationManager.AppSettings["PCSSupportEmail"], subject, body);
                Logger.Current.LogWarn(string.Format(@"Issue starting BenefitSummary workflow. Payload '{0}'. Workflow '{1}'. Message '{2}'.",
                    payload.TryDumpObjectProperties(), workflow.TryDumpObjectProperties(), errormsg), null);
            }

            // final email sent and 'Completed' status updated from Informatica workflow
        }

        /// <summary>
        /// Handle cancel button actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(Object sender, EventArgs e)
        {
            ddlTemplateTypes.SelectedIndex = 0;
            hdnTemplateID.Value = (ddlTemplateTypes.SelectedIndex + 1).ToString();
            ddlTemplateTypes_SelectedIndexChanged(this, EventArgs.Empty);
            txtTrackingNumber.Text = string.Empty;
            lblMessage.Text = string.Empty;
        }

        private void resetFieldsPostValidations(bool postSubmit = false)
        {
            ddlTemplateTypes.SelectedIndex = 0;
            hdnTemplateID.Value = (ddlTemplateTypes.SelectedIndex + 1).ToString();
            ddlTemplateTypes_SelectedIndexChanged(this, EventArgs.Empty);
            txtTrackingNumber.Text = string.Empty;
            if (!postSubmit) uplFeeScheduleFile.ID = null;
        }

        /// <summary>
        /// Gets the select template filename
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlTemplateTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string templateID = hdnTemplateID.Value;
            string templateFileName = new SqlRepository().GetTemplateName(Convert.ToInt32(hdnTemplateID.Value));

            if (templateFileName.Length <= 0)
            {
                lblMessage.Text = string.Format("No valid template was found.");
            }
            else
            {
                hlTemplate.Text = templateFileName.Replace(".xlsx", String.Empty).ToString();
                hlTemplate.NavigateUrl = "../Model/" + templateFileName;
            }
        }

        /// <summary>
        /// Inserts validated data into RawData table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public void BuildRawData(List<DevExCell> data, Request req, DataTable templateColummsTable)
        {

            int rowCount = 0;
            int i = 0;
            int cellCnt = 0;
            int maxCols = Common.Globals.MaxColumns;
            StringBuilder sb = new StringBuilder();
            RequestViewModel vm = new RequestViewModel(new SqlRepository());

            string columnName = string.Empty;

            Logger.Current.LogInformation("Starting BuildRawData");

            RawData rawData = new RawData();
            rawData.RequestID = req.RequestID;
            rawData.TemplateID = req.TemplateID;
            foreach (DevExCell cell in data)
            {
                cellCnt++;

                try
                {
                    // first 4 columns are the same across all templates
                    switch (i)
                    {
                        case 0: // prefix
                            rawData.PDBC_TYPE = cell.value.Trim('\"');
                            break;
                        case 1: // prefix desc
                            rawData.PDPX_DESC = cell.value.Trim('\"');
                            break;
                        case 2: // benefit Type
                            rawData.BSBS_TYPE = cell.value.Trim('\"');
                            break;
                        case 3: // requirement text
                            rawData.BSBS_DESC = cell.value.Trim('\"');
                            break;
                        default: // loop
                                 // begin each section with a newline
                            if (cell.value.Trim().Length > 0)
                            {
                                columnName = templateColummsTable.Rows[i]["ColumnName"].ToString().ToUpper();
                                if (i != 4) //Do not add a new line for Benefit Description (first line in Benefit Text)
                                    sb.Append(Environment.NewLine);
                                sb.Append(columnName).Append(":");
                                if (cell.value.Length < 70)
                                    sb.Append(' ', 2);
                                else
                                    sb.Append(Environment.NewLine);

                                sb.Append(cell.value).Append(Environment.NewLine);

                            }
                            break;
                    }

                    i++;

                    if (i == maxCols)
                    {
                        rawData.BSBS_TEXT = sb.ToString().Replace("_x000D_", "");
                        vm.InsertRawData(rawData);
                        i = 0;
                        rowCount++;
                        if (rowCount % 1000 == 0) Logger.Current.LogInformation("BuildRawData " + rowCount + " rows");
                        sb.Clear();
                    }
                }
                catch (AggregateException ae)
                {
                    Logger.Current.LogError(string.Format("BuildRawData ", (ae.InnerExceptions == null) ? ae.Message : ae.InnerExceptions.Last().Message), ae);
                }
            }
        }
        protected void UpdatePanel_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page),
                new object[] { sender as UpdatePanel });
        }

        protected void grdViewRequests_CustomCallback(object sender, DevExpress.Web.ASPxGridViewCustomCallbackEventArgs e)
        {
            grdViewRequests.DataBind();
        }

        protected void grdViewRequests_BeforeHeaderFilterFillItems(object sender, DevExpress.Web.ASPxGridViewBeforeHeaderFilterFillItemsEventArgs e)
        {
            grdViewRequests.JSProperties["cpCol"] = e.Column.FieldName;
        }
        protected void hyp_run_ctl_id_Init(object sender, EventArgs e)
        {
            ASPxHyperLink link = sender as ASPxHyperLink;
            GridViewDataItemTemplateContainer container = link.NamingContainer as GridViewDataItemTemplateContainer;
            link.Text = DataBinder.Eval(container.DataItem, "RUNTIME_CTL").ToString();
            link.NavigateUrl = ConfigurationManager.AppSettings["MTLink"] + link.Text;
            link.Target = "_blank";
        }
        protected void Popup_Mig_Crea_WindowCallback(object source, PopupWindowCallbackArgs e)
        {
            hdn_reqid.Value = e.Parameter;
            if (grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "Status").ToString().ToUpper() == "SUCCESS")
            {
                string reqid = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString();
                ds_grd_MigrationRequest.SelectParameters["REQUEST_ID"].DefaultValue = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString();
                grd_MigrationRequest.DataBind();
                ds_grdkeys.SelectParameters["RequestID"].DefaultValue = reqid;
                grd_Keys.DataBind();
                if (lblMsg.Visible) lblMsg.Visible = false;
                dvTable1.Visible = true;
            }
            else
            {
                lblErrMsg.Text = "Migrations can be requested only for submissions which have been Succeded.";
                dvTable1.Visible = false;
            }
        }

        protected void grd_MigrationRequest_PageIndexChanged(object sender, EventArgs e)
        {
            ds_grd_MigrationRequest.SelectParameters["REQUEST_ID"].DefaultValue = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString();
            grd_MigrationRequest.DataBind();
        }
        protected void CMRCount_Init(object sender, EventArgs e)
        {
            ASPxHyperLink link = sender as ASPxHyperLink;
            GridViewDataItemTemplateContainer container = link.NamingContainer as GridViewDataItemTemplateContainer;
            string visibleIndex = container.VisibleIndex.ToString();
            string trackingNum = DataBinder.Eval(container.DataItem, "TrackingNumber").ToString();
            link.Text = DataBinder.Eval(container.DataItem, "MIG_REQ_CNT").ToString();
            link.ClientSideEvents.Click = string.Format("function (s, e) {{ TrackingNumOnclick('{0}','{1}') }}", trackingNum, visibleIndex);
        }
        protected void btn_Submit_Click(object sender, EventArgs e)
        {
            try
            {
                RequestViewModel vm = new RequestViewModel(new SqlRepository());
                MigrationRequestParameters ma = new MigrationRequestParameters();
                ma.SourceInstance = ddlSourceIns.Value.ToString().ToUpper();
                ma.TargetInstance = ddlTargetInstance.Value.ToString().ToUpper();
                ma.SubjectArea = ddl_SubjecArea.Value.ToString().ToUpper();
                ma.UserName = Page.User.Identity.Name;
                ma.Status = "QUEUED";
                ma.IsGroupable = true;
                ma.TrackingNumber = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "TrackingNumber").ToString();
                ma.ImpersonationUserName = ConfigurationManager.AppSettings["ImpersonationUserName"].ToString();
                int reqid = Convert.ToInt32(grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString());
                DataTable dt = vm.GetReqKeyData(reqid);
                MigrationRunRequestModel request = new MigrationRunRequestModel
                {
                    MigrationRequestParameters = ma,
                    AppName = ConfigurationManager.AppSettings["AppName"].ToString(),
                    InputParams = dt
                };
                var inputPayLoad = JsonConvert.SerializeObject(request);
                var wi = (System.Security.Principal.WindowsIdentity)HttpContext.Current.User.Identity;
                var wic = wi.Impersonate();
                using (WebClient client = new WebClient())
                {
                    client.UseDefaultCredentials = true;
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                    var responsePayLoad = client.UploadString(ConfigurationManager.AppSettings["Service_Url"].ToString(), inputPayLoad);
                    MigrationRunReponse response = JsonConvert.DeserializeObject<MigrationRunReponse>(responsePayLoad);
                    long RunTimeCtlID1 = response.RunControlId;
                    dvTable2.Visible = true;
                    if (RunTimeCtlID1 > 0)
                    {
                        vm.Insert_Migration_Details(Convert.ToInt32(reqid), RunTimeCtlID1, ma.SourceInstance, ma.TargetInstance, ma.SubjectArea, ma.TrackingNumber, HttpContext.Current.User.Identity.Name);
                        ds_grd_MigrationRequest.SelectParameters["REQUEST_ID"].DefaultValue = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString();
                        lblMsg.Text = response.RunControlMessage;
                        Logger.Current.LogInformation(string.Format("Keyed request for subject {0} with ID of {1} from source {2} to target {3} by {4} with groupable of {5}.", ma.SubjectArea, ma.TrackingNumber, ma.SourceInstance, ma.TargetInstance, HttpContext.Current.User.Identity.Name, ma.IsGroupable));
                        grd_MigrationRequest.DataBind();
                    }
                    else
                    {
                        lblMsg.Text = response.RunControlMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.LogInformation(string.Format("{0}", ex));
            }
        }
        protected void grd_Keys_PageIndexChanged(object sender, EventArgs e)
        {
            string reqid = grdViewRequests.GetRowValues(Convert.ToInt32(hdn_reqid.Value), "RequestID").ToString();
            ds_grdkeys.SelectParameters["RequestID"].DefaultValue = reqid;
            grd_Keys.DataBind();
        }
        protected void errorCount_Init(object sender, EventArgs e)
        {
            ASPxHyperLink link = sender as ASPxHyperLink;
            string trackingNum = null, errorCount = null, TemplateType = null, requestID = null;
            GridViewDataItemTemplateContainer container = link.NamingContainer as GridViewDataItemTemplateContainer;
            trackingNum = DataBinder.Eval(container.DataItem, "TrackingNumber").ToString();
            TemplateType = DataBinder.Eval(container.DataItem, "TemplateType").ToString();
            errorCount = DataBinder.Eval(container.DataItem, "ErrorCount").ToString(); ;
            requestID = DataBinder.Eval(container.DataItem, "RequestID").ToString();
            link.Text = errorCount;
            link.ClientSideEvents.Click = string.Format("function (s, e) {{ OnClick(s, e,'{0}','{1}','{2}') }}", trackingNum, TemplateType, requestID);
        }
        protected void ErrorCountStatus_WindowCallback(object source, PopupWindowCallbackArgs e)
        {
            string requestID = e.Parameter;
            try
            {
                hdn_reqidpopup.Value = requestID;
                ds_ErrorCountStatus.SelectParameters["RequestID"].DefaultValue = requestID;
                errorDetails.DataBind();              
            }
            catch (Exception ex)
            {
                Logger.Current.LogError(ex.Message, ex);
            }
        }
        protected void errorDetails_PageIndexChanged(object sender, EventArgs e)
        {
            string reqid = hdn_reqidpopup.Value;
            try
            {
                ds_ErrorCountStatus.SelectParameters["RequestID"].DefaultValue = reqid;
                errorDetails.DataBind();

            }
            catch (Exception ex)
            {
                Logger.Current.LogError(ex.Message, ex);
            }
        }

        protected void errorgrid_CustomCallback(object sender, EventArgs e)
        {
            errorDetails.DataBind();
        }
        

    }
}