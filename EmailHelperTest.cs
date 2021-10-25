using ExtensionMethods;
using MedicalCoding.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGP.FSA.Library;

namespace UnitTest_MCL.CommonTests
{
    /// <summary>
    ///This is a test class for EmailHelperTests and is intended
    ///to contain all EmailHelperTests Unit Tests
    ///</summary>
    [TestClass()]
    public class EmailHelperTest_DataError
    {
       
        private static string fromAndToAddress = "test@ad.wellpoint.com";
        MedicalCoding.Common.EmailHelper emailhelper = null;
        public EmailHelperTest_DataError()
        {
            Logger logger;
            emailhelper = new MedicalCoding.Common.EmailHelper();
        }
        /// <summary>
        ///A test for FormatDataErrorMessage. 
        ///</summary>
        [TestMethod()]
        public void FormatDataErrorMessagePass()
        {
            MedicalCoding.Model.Request request = new MedicalCoding.Model.Request();
            request.With(r =>
            {
                r.CreatedDate = DateTime.Now;
                r.RequestStatusID = (int)MedicalCoding.Model.Request.RequestStatus.Submitted;
                r.TemplateID = 1;
                r.TemplateType = "IDCD";
                r.OriginalFileName = "IDCD_Template.xlsx";
                r.TrackingNumber = "Sample_TEST_1";
                r.FacetsID = "FAC2D";
                r.UserName = "AH11135";
                r.UserDomain = "TestDomain";
                r.UserEmailAddress = "test@ad.wellpoint.com";
                r.ComputedFileName = r.OriginalFileName;
                r.ReportsPath = "";
            });

            string templateName = "MCL_REQ_DATAERROR";
            string title = string.Empty;
            string formattedTemplate = emailhelper.FormatDataErrorMessage(request, out string sub,templateName, title);
           
            Assert.IsFalse(string.IsNullOrEmpty(formattedTemplate));
        }


        /// <summary>
        ///A test for SendSync.  Stub is better when trying to replace emailer within a class that aggregrates / uses it
        ///</summary>
        [TestMethod()]
        public void SendSyncViaStubPass()
        {
            bool actual = emailhelper.SendSync(fromAndToAddress, "test@ad.wellpoint.com", "test@ad.wellpoint.com", fromAndToAddress, "Test subject", "test text");
            Assert.IsTrue(actual);
        }

        /// <summary>
        ///A test for FixAddresses 
        ///</summary>
        ///<remarks>Run Pex against this to boundary test all scenarios.  This test will ensure contract is stable and common scenario works.</remarks>
        [TestMethod()]
        public void FixAddressesPass()
        {
            string notify = "test1;test2";
            string compare = string.Empty;
            string expected = "test1@amerigroup.com,test2@amerigroup.com";
            string actual;
            actual = emailhelper.FixAddresses(notify, compare);
         
            Assert.AreEqual(expected, actual);
        }
    }
    [TestClass()]
    public class EmailHelperTest_EtlError
    {

        MedicalCoding.Common.EmailHelper emailhelper = null;
        public EmailHelperTest_EtlError()
        {
            Logger logger;
            emailhelper = new MedicalCoding.Common.EmailHelper();
        }
        /// <summary>
        ///A test for FormatDataErrorMessage. 
        ///</summary>
        [TestMethod()]
        public void FormatDataErrorMessagePass()
        {
            MedicalCoding.Model.Request request = new MedicalCoding.Model.Request();
            request.With(r =>
            {
                r.CreatedDate = DateTime.Now;
                r.RequestStatusID = (int)MedicalCoding.Model.Request.RequestStatus.Submitted;
                r.TemplateID = 2;
                r.TemplateType = "IPCD";
                r.OriginalFileName = "IPCD_Template.xlsx";
                r.TrackingNumber = "Sample_TEST_2";
                r.FacetsID = "FAC2D";
                r.UserName = "Test";
                r.UserDomain = "TestDomain";
                r.UserEmailAddress = "";
                r.ComputedFileName = r.OriginalFileName;
                r.ReportsPath = "";
            });

            string templateName = "MCL_REQ_ETLERROR";
            string title = string.Empty;
            string formattedTemplate = emailhelper.FormatDataErrorMessage(request, out string sub, templateName, title);

            Assert.IsFalse(string.IsNullOrEmpty(formattedTemplate));
        }

}
    [TestClass()]
    public class EmailHelperTest_WfError
    {

        MedicalCoding.Common.EmailHelper emailhelper = null;
        public EmailHelperTest_WfError()
        {
            emailhelper = new MedicalCoding.Common.EmailHelper();
        }
        /// <summary>
        ///A test for FormatDataErrorMessage. 
        ///</summary>
        [TestMethod()]
        public void FormatDataErrorMessagePass()
        {
            MedicalCoding.Model.Request request = new MedicalCoding.Model.Request();
            request.With(r =>
            {
                r.CreatedDate = DateTime.Now;
                r.RequestStatusID = (int)MedicalCoding.Model.Request.RequestStatus.Submitted;
                r.TemplateID = 2;
                r.TemplateType = "IPCD";
                r.OriginalFileName = "IPCD_Template.xlsx";
                r.TrackingNumber = "Sample_TEST_3";
                r.FacetsID = "FAC2D";
                r.UserName = "Test";
                r.UserDomain = "TestDomain";
                r.UserEmailAddress = "";
                r.ComputedFileName = r.OriginalFileName;
                r.ReportsPath = "";
            });

            string templateName = "MCL_REQ_WFSTARTERR";
            string title = string.Empty;
            string formattedTemplate = emailhelper.FormatDataErrorMessage(request, out string sub, templateName, title);

            Assert.IsFalse(string.IsNullOrEmpty(formattedTemplate));
        }

    }
}
