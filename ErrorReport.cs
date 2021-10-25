using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using AGP.FSA.Library;
using BenefitSummary.Common;
using BenefitSummary.Model;
using System.Data;
using DevExpress.Spreadsheet;

namespace BenefitSummary.Common
{
    public class ErrorReport
    {
        public ErrorReport(List<RequestError> errors, Request req, List<string> fileTables)
        {
            this.Errors = errors;
            this.Request = req;
            this.Tables = (fileTables.Count > 0) ? fileTables : new List<string>() { "None provided." };
        }

        public List<RequestError> Errors { get; private set; }
        public Request Request { get; private set; }
        public List<string> Tables { get; private set; } // distinct list of nsvm tables specified in file

        public bool Create(out string fileAbsolutePath)
        {
            bool isSuccess = true;
            fileAbsolutePath = Path.Combine(Request.ReportsPath, string.Format("{0}-{1}-{2}.xlsx", "ErrorReport", this.Request.UserName, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-ff-tt")));

            if (File.Exists(fileAbsolutePath))
            {
                try
                {
                    File.Delete(fileAbsolutePath);
                }
                catch (System.Exception ex)
                {
                    isSuccess = false;
                    Logger.Current.LogError("Error deleting report file, occurred in ErrorReport.Create(" + fileAbsolutePath + ").", ex);
                }
            }
            File.Create(fileAbsolutePath).Close();

            return isSuccess;
        }

        public DataTable CreatErrorTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Row#");
            dt.Columns.Add("Column");
            dt.Columns.Add("Error Categery");
            dt.Columns.Add("Value");
            dt.Columns.Add("Error Message");

            foreach (RequestError error in this.Errors)
            {
                RequestError err = new RequestError();
                err.RowNumber = error.RowNumber;
                err.ColumnName = error.ColumnName;
                err.CategoryMessage = error.CategoryMessage;
                err.ActualValue = error.ActualValue;
                err.ErrorMessage = error.ErrorMessage;
                dt.Rows.Add(err.RowNumber, err.ColumnName, err.CategoryMessage, err.ActualValue, err.ErrorMessage);

            }
            dt.Columns[0].ColumnName = "Row#";
            dt.Columns[1].ColumnName = "Column";
            dt.Columns[2].ColumnName = "Error Categery";
            dt.Columns[3].ColumnName = "Value";
            dt.Columns[4].ColumnName = "Error Message";
            return dt;
        }
        public void CreateErrorReportDev()
        {
            if (Create(out string filePath))
            {
                try
                {
                    Workbook book = new Workbook();
                    book.LoadDocument(filePath);
                    Worksheet sheet = book.Worksheets[0];

                    sheet.Name = "BenefitSummary Errors";
                    sheet.ActiveView.ShowGridlines = false;
                    //sheet.ActiveView.ShowHeadings = false;
                    string imgpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\Images\\wp_logo.png");
                    var pic = sheet.Pictures.AddPicture(imgpath, 1, 1, 600, 1000, true);

                    sheet.Cells["C2"].Value = "Error Report";
                    sheet.Cells["C2"].Font.Color = System.Drawing.Color.Red;
                    sheet.Cells["C2"].Font.Bold = true;
                    sheet.Cells["C2"].RowHeight = 28;
                    var range = sheet.Range["A1:E5"].RowHeight = 70;
                    //sheet.MergeCells(range);

                    sheet.Cells["B7"].Value = "Request #: " + Request.RequestID.ToString();
                    sheet.Cells["D7"].Value = "Date : ";
                    sheet.Cells["E7"].Value = Request.CreatedDate.ToString();
                    sheet.Cells["B11"].Value = "Tracking Number: " + Request.TrackingNumber.ToString();
                    sheet.Cells["B12"].Value = "Computed FileName: " + Request.ComputedFileName.ToString();
                    sheet.Cells["B13"].Value = "Original FileName: " + Request.OriginalFileName.ToString();
                    sheet.Cells["B14"].Value = "Submitter: " + Request.UserDomain + "/" + Request.UserName;

                    DataTable dt = CreatErrorTable();
                    // Import data from the data table into the worksheet.
                    // Data starts with the 16 Row.
                    sheet.Import(dt, true, 16, 0);
                    // applying borders on a specified range of cells
                    var range2 = sheet.Range["A17:E17"];
                    range2.Font.Bold = true;
                    range2.FillColor = System.Drawing.Color.Ivory;
                    range2.AutoFitColumns();
                    range2.Borders.SetOutsideBorders(System.Drawing.Color.Black, BorderLineStyle.Thick);

                    sheet.DefaultRowHeight = 20;
                    sheet.Cells.Font.Size = 18;
                    sheet.Cells.AutoFitColumns();

                    book.SaveDocument(filePath, DevExpress.Spreadsheet.DocumentFormat.Xlsx);

                }
                catch (Exception ex)
                {
                    Logger.Current.LogError("ErrorReport.Create exception", ex);
                    File.Delete(filePath);
                }

            }

        }
    }
}