using MedicalCoding.Common;
using MedicalCoding.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGP.FSA.Library;


namespace UnitTest_MCL.CommonTests
{/// <summary>
 ///This is a test class for ExcelHelperTest and is intended
 ///to contain all ExcelHelperTest Unit Tests
 ///</summary>
    [TestClass]
    public class ExcelHelperTest_IDCD
    {
        Logger logger;

        /// <summary>
        ///A test for ValidateMultipleModifierFormatTest 
        ///</summary>
        ///<remarks>Test to validate the format of the Excel template</remarks>
        string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        string source = "IDCD_Template.xlsx";
        [TestMethod]
        public void ValidateMedicalCodingFormat()
        {
            ExcelHelper emailHelper = new ExcelHelper();

            string path = Path.Combine(dir, source);
            int tempid = 1;//need to change the id
            emailHelper.ValidateMedicalCodingFormat(path, tempid, out string msg);
            Assert.IsTrue(string.IsNullOrEmpty(msg));
          
        }

        /// <summary>
        ///A test for CreateTest(ErrorReport) 
        ///</summary>
        ///<remarks>Test to check whether data is present in the Excel sheet</remarks>
        [TestMethod]
        public void CreateDatasFromExcelPass()
        {

            ExcelHelper emailHelper = new ExcelHelper();
            List<LibXLCell> data = emailHelper.CreateDatasFromExcel(dir, "", source, out List<string> xyz, out List<string> yz);
            Assert.IsTrue(data.Count == 0);
        }


    }

    [TestClass]
    public class ExcelHelperTest_IPCD
    {
        /// <summary>
        ///A test for ValidateMultipleModifierFormatTest 
        ///</summary>
        ///<remarks>Test to validate the format of the Excel template</remarks>
        string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        string source = "IPCD_Template.xlsx";
        [TestMethod]
        public void ValidateMedicalCodingFormat()
        {
            ExcelHelper emailHelper = new ExcelHelper();

            string path = Path.Combine(dir, source);
            int tempid = 2;//need to change the id
            emailHelper.ValidateMedicalCodingFormat(path, tempid, out string msg);
            Assert.IsTrue(string.IsNullOrEmpty(msg));
        }

        /// <summary>
        ///A test for CreateTest(ErrorReport) 
        ///</summary>
        ///<remarks>Test to check whether data is present in the Excel sheet</remarks>
        [TestMethod]
        public void CreateDatasFromExcelPass()
        {
            ExcelHelper emailHelper = new ExcelHelper();
            List<LibXLCell> data = emailHelper.CreateDatasFromExcel(dir, "", source, out List<string> xyz, out List<string> yz);
            Assert.IsTrue(data.Count == 0);
        }

    }
}
