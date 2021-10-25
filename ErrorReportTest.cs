//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MedicalCoding.Model;
//using ExtensionMethods;
//using MedicalCoding.Common;

//namespace UnitTest_MCL.CommonTests
//{ /// <summary>
//  ///This is a test class for ErrorReportTest and is intended
//  ///to contain all ErrorReportTest Unit Tests
//  ///</summary>
//    [TestClass]
//    public class ErrorReportTest
//    {
//        /// <summary>
//        ///A test for Create ErrorReport Method
//        ///</summary>
//        ///<remarks></remarks>

//        [TestMethod]
//        public void CreatePass()
//        {
//            MedicalCoding.Model.Request request = new MedicalCoding.Model.Request();
//            request.With(r =>
//            {
//                r.CreatedDate = DateTime.Now;
//                r.RequestStatusID = (int)MedicalCoding.Model.Request.RequestStatus.Submitted;
//                r.OriginalFileName = "MultipleModifierInputTemplate.xlsx";
//                r.TrackingNumber = "Sample_TEST_1";
//                r.FacetsID = "FAC2D";
//                r.UserName = "Test";
//                r.UserDomain = "TestDomain";
//                r.UserEmailAddress = "";
//                r.ComputedFileName = r.OriginalFileName;
//                r.ReportsPath = "";
//            });
//            List<RequestError> errors = new List<RequestError>();
//            List<string> fileTables = new List<string>();
//            ErrorReport errorReport = new ErrorReport(errors, request, fileTables);
//            bool status = errorReport.Create();
//            Assert.IsTrue(status);
//        }
//    }
//}
