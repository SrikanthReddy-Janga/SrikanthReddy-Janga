using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using AGP.FSA.Library;
using ExtensionMethods;
using System.Xml;
using DevExpress.Spreadsheet;


namespace BenefitSummary.Common
{
    /// <summary>
    /// Class to handle excel operations around Aspose.  
    /// </summary>
    /// <remarks>Commenting out IDisposable pattern implementation since not thread safe and don't have time to lock sheet, book, uindex and data and test.</remarks>
    public class ExcelHelper // : IDisposable
    {
        public ExcelHelper()
        {
        }

        public string ValidateBenefitSummaryFormat(string source, int templateID, out string msg)
        {
            Workbook book = new Workbook();
            Worksheet sheet = null;
            const string SHEETNAME = "benefitsummary";
            msg = string.Empty;
            DataTable templateColummsTable = new BenefitSummary.ViewModel.RequestViewModel(new BenefitSummary.Model.SqlRepository()).GetTemplateColumns(templateID);
            Globals.MaxColumns = templateColummsTable.Rows.Count;
            int maxCols = Globals.MaxColumns;
            try
            {
                book.LoadDocument(source);
                sheet = book.Worksheets[0];
                int sheetCount = book.Worksheets.Count;
                var range = sheet.GetDataRange();
                int maxdatarow = range.RowCount;
                int maxdatacol = range.ColumnCount;

                if (sheetCount > 2)
                {
                    msg = "Sheet count failed validation.";
                }

                if (sheet.Name.ToLower() != SHEETNAME)
                {
                    msg = (msg != string.Empty) ? msg += " Sheet name must be 'BenefitSummary'." : "Sheet name.";
                }

                if (maxdatarow <=1)
                {
                    msg = (msg != string.Empty) ? msg += " No data in template." : "No data.";
                }

                if (maxdatacol != maxCols)
                {
                    msg = (msg != string.Empty) ? msg += "Excel file has incorrect number of columns" + maxdatacol.ToString() + ". " : "Incorrect column count.";
                }

                // Count number of header columns
                int colCount = 0;
                string s = string.Empty;
                for (int col = 0; col < maxCols; col++)
                {
                    s = string.Empty;
                    if (sheet.Cells[0, col].Value.IsText)
                    {
                        s = sheet.Cells[0, col].Value.TextValue;
                        if (!string.IsNullOrEmpty(s))
                            colCount++;
                    }
                }
                String cellValue = String.Empty;
                string templateColumnName = String.Empty;
                for (int colNumber = 0; colNumber < maxCols; colNumber++)
                {
                    if (colNumber == maxCols)
                    {
                        break;
                    }
                    else
                    {
                        cellValue = sheet.Cells[0, colNumber].Value.ToString().Trim().Replace("\r", String.Empty).Replace("\n", String.Empty);
                        templateColumnName = templateColummsTable.Rows[colNumber]["ColumnName"].ToString().Replace("\r", String.Empty).Replace("\n", String.Empty);

                        if (!string.Equals(cellValue, templateColumnName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            msg = (msg != string.Empty) ? msg += " Header (" + templateColumnName + ")." : "Header (" + templateColumnName + ").";
                        }
                    }
                }
                sheet = null;
                book = null;
            }
            catch (System.Exception ex)
            {
                Logger.Current.LogError("ERROR: ValidateBenefitSummaryFormat(" + source + ") msg (" + msg + ") message: " + ex.Message, ex);
                if (msg != string.Empty) msg = "Validation Exception.";
                sheet = null;
                book = null;
                return msg;
            }

            return msg;
        }


        /// <summary>
        ///  Produce an index for immediate querying for duplicate matches.
        /// </summary>
        /// <param name="source">Location of excel file</param>
        public List<DevExCell> CreateDatasFromExcel(string source, string dest, string fn, out List<string> bsbsPrefixes, out List<string> bsbsTypes)
        {
            List<DevExCell> data = new List<DevExCell>();
            bsbsPrefixes = new List<string>();
            bsbsTypes = new List<string>();
            Dictionary<string, int> uniqueIndex = new Dictionary<string, int>();
            Workbook book = new Workbook();
            Worksheet sheet = null;
            try
            {
                 book.LoadDocument(source + fn);
                int sheetCount = 1;
                // each sheet gets its own csv
                for (int i = 0; i < sheetCount; i++)
                {
                    sheet = book.Worksheets[i];
                    var range = sheet.GetDataRange();
                    int maxdatarow = range.RowCount;
                    int maxdatacol = range.ColumnCount;
#pragma warning disable 414, 169
                    bool hasData = false; // check performed previously during tier 1 validations
#pragma warning restore 414, 169
                    try
                    {
                        if (maxdatarow > 1) hasData = true; // first row is header
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Current.LogWarn(string.Format("WARNING CreateDatasFromExcel: Unable to determine row count for file {0} sheet {1} with exception '{2}'.", source, sheet.Name, ex.GetBaseException().Message));
                    }
                    if (!hasData) Logger.Current.LogWarn(string.Format("CreateDatasFromExcel determined file {0} had no data", fn), "", 3, null);


                    string s = string.Empty;
                    int maxdatarow = maxdatarow;
                    int lastrow = maxdatarow;
                    // the total number of records is reduced by our starting position
                    int lastcol = Globals.MaxColumns - 1; // cl.col;

                    DevExRow xlRow = new DevExRow();
                    for (int row = 1; row < maxdatarow; row++) // skip 1 row: header row and instruction row
                    {
                        List<string> cells = new List<string>();

                        char columnCharInner = 'A';
                        char columnCharOuter = ' ';
                        for (int col = 0; col <= lastcol; col++)
                        {
                            s = string.Empty;
                            var CellVal = sheet.Cells[row, col];
                            var type = CellValueType.None;
                            if (CellVal != null)
                            {
                                type = sheet.Cells[row, col].Value.Type;
                            }
                            switch (type)
                            {
                                case CellValueType.Text:
                                    s = sheet.Cells[row, col].Value.TextValue;
                                    break;
                                case CellValueType.Numeric:
                                    var format = sheet.Cells[row, col].Value.ToString();
                                    if (format.Contains("$"))
                                    {
                                        s = format.Split('$').Last();
                                        if (s == "$") { s = format.Split('$').First(); };
                                    }
                                    else if (format.Contains("."))
                                    {
                                        s = sheet.Cells[row, col].Value.NumericValue.ToString("G7", CultureInfo.InvariantCulture);
                                    }
                                    else { s = format; }
                                    break;
                                case CellValueType.DateTime:
                                    s = sheet.Cells[row, col].Value.DateTimeValue.ToString("MM/dd/yyyy");
                                    break;
                                default:
                                    s = string.Empty;
                                    break;
                            }
                            switch (col)
                            {
                                case 0:
                                    xlRow.colA = (string.IsNullOrEmpty(s)) ? s : s.Trim();
                                    bsbsPrefixes.Add(xlRow.colA);
                                    s = xlRow.colA;
                                    break;
                                case 1:
                                    xlRow.colB = (string.IsNullOrEmpty(s)) ? s : s.Trim();
                                    s = xlRow.colB;
                                    break;
                                case 2:
                                    xlRow.colC = (string.IsNullOrEmpty(s)) ? s : s.Trim();
                                    bsbsTypes.Add(xlRow.colC);
                                    s = xlRow.colC;
                                    break;
                                case 3:
                                    xlRow.colD = (string.IsNullOrEmpty(s)) ? s : s.Trim();
                                    s = xlRow.colD;
                                    break;
                                case 4:
                                    xlRow.colE = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colE;
                                    break;
                                case 5:
                                    xlRow.colF = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colF;
                                    break;
                                case 6:
                                    xlRow.colG = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colG;
                                    break;
                                case 7:
                                    xlRow.colH = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colH;
                                    break;
                                case 8:
                                    xlRow.colI = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colI;
                                    break;
                                case 9:
                                    xlRow.colJ = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colJ;
                                    break;
                                case 10:
                                    xlRow.colK = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colK;
                                    break;
                                case 11:
                                    xlRow.colL = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colL;
                                    break;
                                case 12:
                                    xlRow.colM = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colM;
                                    break;
                                case 13:
                                    xlRow.colN = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colN;
                                    break;
                                case 14:
                                    xlRow.colO = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colO;
                                    break;
                                case 15:
                                    xlRow.colP = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colP;
                                    break;
                                case 16:
                                    xlRow.colQ = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colQ;
                                    break;
                                case 17:
                                    xlRow.colR = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colR;
                                    break;
                                case 18:
                                    xlRow.colS = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colS;
                                    break;
                                case 19:
                                    xlRow.colT = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colT;
                                    break;
                                case 20:
                                    xlRow.colU = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colU;
                                    break;
                                case 21:
                                    xlRow.colV = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colV;
                                    break;
                                case 22:
                                    xlRow.colW = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colW;
                                    break;
                                case 23:
                                    xlRow.colX = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colX;
                                    break;
                                case 24:
                                    xlRow.colY = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colY;
                                    break;
                                case 25:
                                    xlRow.colZ = (string.IsNullOrEmpty(s)) ? s : s;
                                    s = xlRow.colZ;
                                    break;
                            }

                            // Informatica hack for empty data in last column: (col==lastcol)                            
                            if ((!string.IsNullOrEmpty(s)) || (col == lastcol))
                            {
                                //replace embedded CR & LF
                                s = s.Replace("\n", Environment.NewLine);
                            }

                            DevExCell cell = new DevExCell();
                            if (col != lastcol)
                            {
                                data.Add(cell.With(c =>
                                {
                                    c.col = col + 1; c.row = row + 1; c.colName = columnCharInner.ToString(); c.value = s;
                                }));
                            }
                            else
                            {
                                data.Add(cell.With(c =>
                                {
                                    c.col = col + 1; c.row = row + 1; c.colName = columnCharInner.ToString(); c.value = s;
                                    c.HashCode = xlRow.CreateMD5Hash();   //xlRow.CreateKeyValue(); // xlRow.CreateMD5Hash(); 
                                    c.isDuplicate = uniqueIndex.ContainsKey(c.HashCode);
                                    c.duplicates = String.Join(",", uniqueIndex.Where(itm => itm.Key == c.HashCode).Select(kvp => kvp.Value));
                                }));
                                try
                                {
                                    uniqueIndex.Add(cell.HashCode, row + 1);
                                }
                                catch (ArgumentException ae)
                                {
                                    Logger.Current.LogWarn(string.Format("Duplicate detected. Rows {0} and {1}. Message '{2}'.",
                                        row + 1, cell.duplicates, ae.Message), "", 3, ae);
                                }
                            }

                            //next column (// A to B ..., handles full Excel column range A to IV)
                            if (columnCharInner == 'Z')
                            {
                                columnCharInner = 'A';
                                columnCharOuter = (columnCharOuter == ' ') ? 'A' : columnCharOuter++;
                            }
                            else
                                columnCharInner++;
                        } // end column iteration

                        if (row % 1000 == 0) Logger.Current.LogInformation("CreateDataFromExcel row # " + row + " of " + maxdatarow);
                    } // end row iteration

                    // sheet = null;

                } // sheet loop

                Logger.Current.LogInformation("CreateDataFromExcel file complete");

                sheet = null;
                book = null;
                uniqueIndex.Clear();
                uniqueIndex = null;
            }
            catch (System.Exception ex)
            {
                Logger.Current.LogError("ERROR: CreateExcelData(" + source + ", " + dest + ") error-" + ex.GetBaseException().Message, ex);

                sheet = null;
                uniqueIndex = null;
                book = null;
            }

            //de-dup lists
            bsbsPrefixes = bsbsPrefixes.Distinct().ToList();
            bsbsTypes = bsbsTypes.Distinct().ToList();
            return data;
        }
    }

    public class DevExCell
    {
        public int row; public int col; public string colName; public string value; public string HashCode; public bool isDuplicate; public string duplicates; // public List<int> duplicates;
    }

    public class DevExRow
    {
        public string colA { get; set; }
        public string colB { get; set; }
        public string colC { get; set; }
        public string colD { get; set; }
        public string colE { get; set; }
        public string colF { get; set; }
        public string colG { get; set; }
        public string colH { get; set; }
        public string colI { get; set; }
        public string colJ { get; set; }
        public string colK { get; set; }
        public string colL { get; set; }
        public string colM { get; set; }
        public string colN { get; set; }
        public string colO { get; set; }
        public string colP { get; set; }
        public string colQ { get; set; }
        public string colR { get; set; }
        public string colS { get; set; }
        public string colT { get; set; }
        public string colU { get; set; }
        public string colV { get; set; }
        public string colW { get; set; }
        public string colX { get; set; }
        public string colY { get; set; }
        public string colZ { get; set; }

        public string CreateMD5Hash()
        {
            // calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(this.colA + this.colB + this.colC + this.colD + this.colE + this.colF + this.colG + this.colH + this.colI + this.colJ + this.colK + this.colL + this.colM + this.colN + this.colO + this.colP + this.colQ + this.colR + this.colS + this.colT + this.colU + this.colV + this.colW + this.colX + this.colY + this.colZ);
            byte[] hash = md5.ComputeHash(inputBytes);

            // convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }



    }

}