using ExtensionMethods;
using MedicalCoding.Common;
using MedicalCoding.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_MCL.CommonTests
{
    /// <summary>
    ///This is a test class for ValidationEngineTest and is intended
    ///to contain all ValidationEngineTest Unit Tests
    ///</summary>
    [TestClass]
    public class ValidationEngineTest
    {
        public ValidationEngineTest()
        {
            Request = new MedicalCoding.Model.Request();
            Request.With(r =>
            {
                r.CreatedDate = DateTime.Now;
                r.RequestStatusID = (int)MedicalCoding.Model.Request.RequestStatus.Submitted;
                r.OriginalFileName = "IDCD_Template.xlsx";
                r.TemplateID = 1;// need to change this
                r.TrackingNumber = "Sample_TEST_1";
                r.FacetsID = "FAC2D";
                r.UserName = "Test";
                r.UserDomain = "TestDomain";
                r.UserEmailAddress = "";
                r.ComputedFileName = r.OriginalFileName;
                r.ReportsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName; ;
                               
            });
        }

        private Request Request
        {
            get;
            set;
        }
        /// <summary>
        ///A test for PerformTierOneValidationsTest 
        ///</summary>
        ///<remarks>Validating miscellaneous tier one (immediate) validations </remarks>
        [TestMethod]
        public void PerformTierOneValidationsPass()
        {
            ValidationEngine validationEngine = new ValidationEngine(Request);
            bool result = validationEngine.PerformTierOneValidations(out string errorMsg);
            Assert.IsTrue(result);
        }

       
    }
}
