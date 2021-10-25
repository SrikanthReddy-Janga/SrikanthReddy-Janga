using AGP.FSA.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Text;
using DevExpress.Spreadsheet;

namespace FeeScheduleManager.UI
{
	public partial class ExcelToCsvExtract : System.Web.UI.Page
	{
        public static ArrayList Files = new ArrayList();
        public static string reportsFilePath = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
		{
            FileUpload1.Attributes["onchange"] = "UploadFile(this)";
            if (!IsPostBack)
            {

                LblMsg.Text = "Select File Location";
                LblMsg.Font.Bold = true;
                RadioButton1.Checked = true;
                TxtStartRow.Visible = false;

            }

        }

        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                    if(FileUpload1.PostedFiles.Count>0)
                    {
                        //for (int i = 0; i < FileUpload1.PostedFiles.Count; i++)
                        for(int i=0;i< FileUpload1.PostedFiles.Count;i++)
                        {
                            //String FileName = FileUpload1.PostedFiles[i].FileName;
                            string FileName = FileUpload1.PostedFiles[i].FileName;
                            string FileDirectory= Path.GetDirectoryName(FileName);
                            txtFileLocation.Text = FileDirectory;
                            reportsFilePath = FileDirectory + @"\csv";
                            string flname = System.IO.Path.GetFileName(FileName);
                            if (ListBox1.Items.Contains(new ListItem(flname)))
                            {
                                LblMsg.ForeColor = System.Drawing.Color.Orange;
                                LblMsg.Text = "File already in the ListBox";
                               return;
                            }
                        
                            else
                            { 
                            if(flname.ToLower().Contains("xlsx")||flname.ToLower().Contains("xls"))
                            {
                                Files.Add(FileUpload1); ListBox1.Items.Add(flname);
                                LblMsg.Text = "Add another file or click Upload to save them all";
                                LblMsg.ForeColor = System.Drawing.Color.Black;
                            }
                            else
                            {
                                LblMsg.Text = "Please Select Xlsx or Xls Files..!";
                                LblMsg.ForeColor = System.Drawing.Color.Red;
                                return;
                            }
                                
                            }
                            
                        }
                    }
                
                else
                {
                    LblMsg.ForeColor = System.Drawing.Color.Red;
                    LblMsg.Text = "Please select a file to add";
                    return;
                }

            }

            catch (Exception ex)
            {
                Logger.Current.LogError("ERROR: Add Button ()-in Excel to Csv Extractor ,Error message: " + ex.Message, ex);
               
            }

        }

        protected void BtnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (ListBox1.Items.Count > 0)
                {
                    if (ListBox1.SelectedIndex < 0)
                    {
                        LblMsg.Text = "Please select a file to remove";
                        LblMsg.ForeColor = System.Drawing.Color.Orange;
                        return;
                    }
                    else
                    {
                        Files.RemoveAt(ListBox1.SelectedIndex);
                        LblMsg.Text = "File :" + ListBox1.SelectedItem.Text + " Removed SucessFully..!!";
                        LblMsg.ForeColor = System.Drawing.Color.Orange;
                        ListBox1.Items.Remove(ListBox1.SelectedItem.Text);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Current.LogError("ERROR: Remove Button () -in Excel to Csv Extracto ,Error message: " + ex.Message, ex);
            }
        }

        protected void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            TxtStartRow.Visible = false;
        }

        protected void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            TxtStartRow.Visible = true;
        }

        protected void BtnConvert_Click(object sender, EventArgs e)
        {
            string msg = "";
            try
            {
                if (ListBox1.Items.Count > 0)
                {
                    string folder = reportsFilePath;
                    if (folder != string.Empty)
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        string fld = (txtFileLocation.Text.ToString().EndsWith(@"\")) ? txtFileLocation.Text.ToString() : txtFileLocation.Text.ToString() + @"\";

                        List<string> items = new List<string>();

                        for (int i = 0; i < ListBox1.Items.Count; i++)
                        {
                            items.Add(ListBox1.Items[i].Text);
                        }

                        ListBox1.SelectedIndex = -1;

                        int sizeLimit = 1000 * 25000;
                        foreach (string file in items)
                        {
                            Logger.Current.LogError("FileName : "+file+" File Convertion Started.", null);
                            ListBox1.SelectedIndex += 1;
                            string source = fld + file;
                            FileInfo finfo = new FileInfo(source);
                            if (finfo.Length > sizeLimit)
                            {
                                msg = msg + string.Format("File '{0}' is too large to process at {1} bytes.  File limit is set to {2} bytes. Please split the file and try again.", finfo.Name, finfo.Length, sizeLimit) + "File Size Exception";
                                Logger.Current.LogError(msg, null);
                                continue;
                            }

                            if (isValidExcelFile(source))
                            {
                                ConvertExcelToCsv(source, folder);
                                msg = "Conversion complete.";
                                Logger.Current.LogError(msg + "FileName: " + file, null);
                                ListBox1.Items.Remove(ListBox1.Items.FindByText(file));
                            }
                            else
                            {
                                msg = msg + String.Format("{0} cannot be converted because it is either already a csv file or an excel file built with html tables!", source);
                                Logger.Current.LogError(msg +"FileName: "+ file, null);
                            }
                        }

                        LblMsg.Text = msg;
                    }
                }
                ListBox1.Items.Clear();
                txtFileLocation.Text = "";
            }
            
            catch (Exception ex)
            {
                msg = ("ERROR: in Excel to Csv Extractor ,Error message: " + msg + " && " + ex.Message);
                Logger.Current.LogError(msg, ex);
                LblMsg.Text = msg;
                LblMsg.ForeColor = System.Drawing.Color.Red ;
                ListBox1.Items.Clear();
                txtFileLocation.Text = "";
                TxtStartRow.Text = "";

            }

        }

        private void ConvertExcelToCsv(string source, string destfolder)
        {
            try
            {
                bool useSheetNames = chkUseSheetNames.Checked; 
                string basefn = Path.GetFileNameWithoutExtension(Path.GetFileName(source));
                string dest = destfolder;
                dest += (dest.EndsWith(@"\")) ? basefn : @"\" + basefn;

                Workbook book = new Workbook();
                book.LoadDocument(source);

                int bc = book.Sheets.Count();
                for (int i = 0; i < bc; i++)
                {
                    bool hasData = false;

                    string sheetname = (useSheetNames) ? book.Worksheets[i].Name : "sheet" + i;
                    string pattern = @"[\\/:*?""<>|]"; // ensure no illegal file name characters in sheet that would prevent the file save and use
                    sheetname = Regex.Replace(sheetname, pattern, string.Empty);
                    sheetname = "_" + sheetname;
                    string sheetFinalName = basefn + sheetname + ".csv";
                    string ndest = dest + sheetname + ".csv";
                    string tempdest = dest + Path.GetRandomFileName() + ".csv";
                    if (File.Exists(tempdest))
                    {
                        try
                        {
                            File.Delete(tempdest);
                        }
                        catch (System.Exception ex)
                        {
                            string msg = "An error occurred deleting " + System.Environment.NewLine + System.Environment.NewLine + tempdest + System.Environment.NewLine + System.Environment.NewLine + "Continue?" + System.Environment.NewLine + System.Environment.NewLine + ex.Message;
                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + msg + " && " + ex.Message, ex);
                            LblMsg.Text = msg;
                            LblMsg.ForeColor = System.Drawing.Color.Red;
                        }
                    }

                    System.IO.StreamWriter file = new System.IO.StreamWriter(tempdest, false);

                    Worksheet sheet = book.Worksheets[i];
                    var range = sheet.GetDataRange();
                    int maxdatarow = range.RowCount;
                    int maxdatacol = range.ColumnCount;
                    //DevExCell cl = LastColumnWithData(sheet);
                    int StartingRow = 0;
                    string s = string.Empty;
                    int totalRecords = maxdatarow;
                    int lastrow = totalRecords;
                    // the total number of records is reduced by our starting position
                    int lastcol = maxdatacol;

                    //int startrow = (RadioButton1.Checked) ? StartingRow : int.Parse(TxtStartRow.Text) - 1;
                    //startrow = (startrow < 0) ? 0 : startrow;
                    int startrow = StartingRow;
                    bool strikeCell = false;

                    for (int row = startrow; row < totalRecords; row++)
                    {

                        StringBuilder sb = new StringBuilder(string.Empty);
                        List<string> cells = new List<string>();
                        bool lhidden = sheet.Rows[row].Visible;
                        string lstrike = string.Empty; // ALL, NONE or A,B,E,...
                        bool lstrikeAll = true;
                        char columnCharInner = 'A';
                        char columnCharOuter = ' ';

                        for (int col = 0; col < lastcol; col++)
                        {

                            s = string.Empty;

                            if (col > 0)
                            {
                                //insert a comma before the current cell (doing it here 
                                // keeps us from having to remove it at the end of the row)
                                sb.Append(",");
                            }
                            var type = sheet.Cells[row, col].Value.Type;
                            switch (type)
                            {
                                case CellValueType.Text:
                                    try
                                    {
                                        s = sheet.Cells[row, col].Value.ToString();
                                    }
                                    catch (Exception)
                                    {
                                        s = string.Empty;
                                        strikeCell = false;
                                        lstrikeAll = false;
                                    }

                                    if (s != string.Empty)
                                    {
                                        try
                                        {
                                            strikeCell = sheet.Cells[row, col].Font.Strikethrough;
                                            if (!strikeCell) lstrikeAll = false;
                                            if (strikeCell) lstrike += (lstrike == string.Empty) ? columnCharOuter.ToString().Trim() + columnCharInner.ToString() : columnCharOuter.ToString().Trim() + "," + columnCharInner.ToString();
                                        }
                                        catch (Exception ex) {
                                            LblMsg.ForeColor = System.Drawing.Color.Red;
                                            LblMsg.Text = string.Format("Error recording strikeouts (text) for row {0} col {1}", row, col);
                                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + LblMsg.Text + " && " + ex.Message, ex);
                                        }
                                    }

                                    break;

                                case CellValueType.Numeric:
                                    try 
                                    {
                                        if(type==CellValueType.Numeric&& cboxSpecialFormatting.Checked)
                                        {
                                            s = sheet.Cells[row, col].Value.ToString();
                                        }
                                        else { s = sheet.Cells[row, col].Value.ToString(); }
                                    
                                    }
                                    catch (Exception)
                                    {
                                        s = string.Empty;
                                        strikeCell = false;
                                        lstrikeAll = false;
                                    }

                                    if (s != string.Empty)
                                    {
                                        try
                                        {
                                            strikeCell = sheet.Cells[row, col].Font.Strikethrough;
                                            if (!strikeCell) lstrikeAll = false;
                                            if (strikeCell) lstrike += (lstrike == string.Empty) ? columnCharOuter.ToString().Trim() + columnCharInner.ToString() : columnCharOuter.ToString().Trim() + "," + columnCharInner.ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            LblMsg.Text = string.Format("Error recording strikeouts (text) for row {0} col {1}", row, col);
                                            LblMsg.ForeColor = System.Drawing.Color.Red;
                                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + LblMsg.Text + " && " + ex.Message, ex);
                                        }
                                    }
                                    break;
                                case CellValueType.DateTime:
                                    try
                                    {
                                        if(type== CellValueType.DateTime&& cboxSpecialFormatting.Checked)
                                        {
                                           s = sheet.Cells[row, col].Value.ToString(); 
                                        }
                                        else
                                        {
                                            try
                                            {
                                                Double ed = 0d;
                                                ed = (Double)sheet.Cells[row, col].Value.DateTimeValue.ToOADate();
                                                DateTime st = new DateTime(1900, 1, 1);
                                                //the subtraction of 2 is designed to handle the Lotus 1-2-3 bug that was built into Excel
                                                s = st.AddDays(ed - 2).ToString("d");
                                            }
                                            catch
                                            {
                                                s = sheet.Cells[row, col].Value.DateTimeValue.ToString("MM/dd/yyyy");
                                            }
                                           
                                        }
                                      
                                    }
                                    catch (Exception)
                                    {
                                        s = string.Empty;
                                        strikeCell = false;
                                        lstrikeAll = false;
                                    }

                                    if (s != string.Empty)
                                    {

                                        try
                                        {
                                            strikeCell = sheet.Cells[row, col].Font.Strikethrough;
                                            if (!strikeCell) lstrikeAll = false;
                                            if (strikeCell) lstrike += (lstrike == string.Empty) ? columnCharOuter.ToString().Trim() + columnCharInner.ToString() : columnCharOuter.ToString().Trim() + "," + columnCharInner.ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            LblMsg.ForeColor = System.Drawing.Color.Red;
                                            LblMsg.Text = string.Format("Error recording strikeouts (number) for row {0} col {1}", row, col);
                                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + LblMsg.Text + " && " + ex.Message, ex);
                                        }
                                    }
                                    break;
                                default:
                                    s = string.Empty;
                                    try
                                    {
                                        strikeCell = false;
                                        lstrikeAll = false;
                                        if (strikeCell) lstrike += (lstrike == string.Empty) ? columnCharOuter.ToString().Trim() + columnCharInner.ToString() : columnCharOuter.ToString().Trim() + "," + columnCharInner.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        LblMsg.ForeColor = System.Drawing.Color.Red;
                                        LblMsg.Text = string.Format("Error recording strikeouts (text) for row {0} col {1}", row, col);
                                        Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + LblMsg.Text + " && " + ex.Message, ex);
                                    }

                                    break;
                            }
                         
                            // Informatica hack for empty data in last column: (col==lastcol)                            
                            if ((!string.IsNullOrEmpty(s)) || (col == lastcol))
                            {
                                //beg quote
                                sb.Append("\"");
                                //content
                                s = s.Replace(System.Environment.NewLine, " ");
                                s = s.Replace("\r", " ");
                                s = s.Replace("\n", " ");
                                sb.Append(s.Replace("\"", "'"));
                                //end quote
                                sb.Append("\"");
                            }
                            //next column (// A to B ..., handles full Excel column range A to IV)
                            if (columnCharInner == 'Z')
                            {
                                columnCharInner = 'A';
                                columnCharOuter = (columnCharOuter == ' ') ? 'A' : columnCharOuter++;
                            }
                            else
                                columnCharInner++;
                        } // end columns

                        if (!lhidden)
                        {
                            sb.Insert(0, "\"hidden:YES\",");
                        }
                        else
                        {
                            sb.Insert(0, "\"hidden:NO\",");
                        }

                        if (lstrikeAll)
                        {
                            sb.Insert(0, "\"strikeout:ALL\",");
                        }
                        else
                        {
                            if (lstrike == string.Empty)
                            {
                                sb.Insert(0, "\"strikeout:NONE\",");
                            }
                            else
                            {
                                sb.Insert(0, string.Format("\"strikeout:{0}\",", lstrike));
                            }
                        }

                        //if ((bWrite) && (!string.IsNullOrEmpty(sb.ToString())))
                        if (!string.IsNullOrEmpty(sb.ToString()))
                        {
                            file.WriteLine(sb.ToString());
                        }

                        hasData = true;

                        //next row
                    }

                    sheet = null;

                    file.Close();

                    //some worksheets are empty, so delete the files created
                    if (hasData == false)
                    {
                        try
                        {
                            File.Delete(tempdest);
                        }
                        catch (System.Exception ex)
                        {
                            string msg = "An error occurred deleting the following empty worksheet:" + System.Environment.NewLine + System.Environment.NewLine + tempdest + System.Environment.NewLine + System.Environment.NewLine + "Continue?" + System.Environment.NewLine + System.Environment.NewLine + ex.Message;
                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + msg + " && " + ex.Message, ex);
                            LblMsg.Text = msg;
                            LblMsg.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    else
                    {
                        File.Copy(tempdest, ndest, true);
                        if(IsFileDownload(tempdest, sheetFinalName))
                        {
                            LblMsg.Text = "File Downloaded sucessfully.";
                            LblMsg.ForeColor = System.Drawing.Color.Blue;
                        }
                        else
                        {
                            LblMsg.Text = "File Download Error.!";
                            LblMsg.ForeColor = System.Drawing.Color.Red;
                            Logger.Current.LogError("Error in File Download. ",null);
                        }

                        try
                        {
                            File.Delete(tempdest);
                        }
                        catch (System.Exception ex)
                        {
                            string msg = "An error occurred deleting the a temporary file:" + System.Environment.NewLine + System.Environment.NewLine + tempdest + System.Environment.NewLine + System.Environment.NewLine + "Continue?" + System.Environment.NewLine + System.Environment.NewLine + ex.Message;
                            Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + msg + " && " + ex.Message, ex);
                            LblMsg.Text = msg;
                            LblMsg.ForeColor = System.Drawing.Color.Red;
                        }

                    }
                }

                book = null;
            }

            catch (System.Exception ex)
            {
                string msg = "An error occurred converting :" + System.Environment.NewLine + System.Environment.NewLine + source + System.Environment.NewLine + System.Environment.NewLine + ex.Message;
                 Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extracto ,Error message: " + msg + " && " + ex.Message, ex);
                LblMsg.Text = msg;
                LblMsg.ForeColor = System.Drawing.Color.Red;
            }

            Timer tmr = new Timer()
            {
                Interval = 3000,
                Enabled = true
            };
            tmr.Tick += new EventHandler(tmr_Tick);
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            LblMsg.Text = string.Empty;
        }
        internal bool IsFileDownload(string FileDest,string FileName)
        {
            bool filedownload = false;
            //try
            //{
                if (!string.IsNullOrEmpty(FileDest)&& !string.IsNullOrEmpty(FileName))
                {
                    FileInfo file = new FileInfo(FileDest);
                    if (file.Exists)
                    {
                       //String path=file.FullName.Replace(@"q:\quotewerks", "~").Replace(@"\", "/");
                        filedownload = true;
                        byte[] Filebites=System.IO.File.ReadAllBytes(FileDest);
                        Response.Clear();
                        Response.ClearHeaders();
                        Response.ClearContent();
                    
                       Response.ContentType = "application/csv";
                       // to open file prompt box open or save file    
                        Response.AppendHeader("content-disposition", "attachment;filename=" + FileName.ToString());
                        Response.Charset = "";
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        //Response.BinaryWrite((byte[])Filebites);
                       Response.TransmitFile(FileDest);
                        Response.Flush();
                        Response.Close();
                    

                }
                }
            //}
            //catch(Exception ex)
            //{
            //    Logger.Current.LogError("Error in File Download && " + ex.Message, ex);
                
            //}
            return filedownload;
        }


        /// <summary>
        /// This method checks for a couple of known factors about excel file contents to determine if the file was generated natively by Excel application or a web page
        ///   1. If the File has the string "DOCTYPE" or "Table" in contents, it is highly likely the file was generated by a web page
        ///   2. If the file is in a comma separated format then it may have been generated by Excel application but we don't want to convert a csv to csv
        /// Above three criterias are tested by the following method
        /// </summary>
        /// <param name="filePath">location of the excel file we are trying to convert</param>
        /// <returns></returns>
        private bool isValidExcelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new System.Exception("Invalid file path/name!");

            if (!File.Exists(filePath))
                throw new System.Exception(String.Format("File \"{0}\" not found!", filePath));

            //Make sure the file we are converting is either a .xls or .xlsx file
            if (!filePath.ToLower().EndsWith(".xls") && !filePath.ToLower().EndsWith(".xlsx") && !filePath.ToLower().EndsWith(".xlsm"))
                return false;

            try
            {
                // html check
                using (StreamReader sr = new StreamReader(filePath))
                {
                    String line = sr.ReadToEnd();
                    if (string.IsNullOrEmpty(line))
                        return false;
                    // file contains html content
                    if (line.IndexOf("<!DOCTYPE") > -1)
                        return false;
                    // file contains html table
                    if (line.ToUpper().IndexOf("<Table") > -1)
                        return false;
                }

                // csv check
                int maxRowsToInspect = 10; int rowsInspected = 0;
                int prevFldsCount = 0; int currentFldsCount = 0;
                bool isCSV = true;
                try
                {
                    using (TextFieldParser txtParser = new TextFieldParser(filePath))
                    {
                        txtParser.TextFieldType = FieldType.Delimited;
                        txtParser.Delimiters = new string[] { "," };
                        string[] currentRow;
                        while (!txtParser.EndOfData && rowsInspected < maxRowsToInspect)
                        {
                            currentRow = txtParser.ReadFields();
                            if (rowsInspected > 0)
                            {
                                currentFldsCount = currentRow.Count();
                                if (currentFldsCount <= 1 || currentFldsCount != prevFldsCount)
                                {
                                    isCSV = false;
                                    break;
                                }
                            }
                            else
                                prevFldsCount = currentRow.Count();

                            rowsInspected++;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // failed so not a csv parseable file
                    isCSV = false;
                    Logger.Current.LogError("ERROR:isValidExcelFile method in Excel 2 Csv Extractor ,Error message: " + "Csv Convertion Error"+ " && " + ex.Message, ex);
                    LblMsg.Text = "ERROR: CsvConvertion -in Excel to Csv Extractor, Error message: " + "Csv Convertion Error"+ " && " + ex.Message;
                    LblMsg.ForeColor = System.Drawing.Color.Red;
                }

                return !isCSV;
            }
            catch (System.OutOfMemoryException ex)
            {
                string msg="Unable to validate Excel file (" + filePath + "). Your system is running low on memory.  Please close any unnecessary applications. Processing will continue.";
                Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extractor ,Error message: " + msg + " && " + ex.Message, ex);
                LblMsg.Text = msg;
                LblMsg.ForeColor = System.Drawing.Color.Red;
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Current.LogError("ERROR: CsvConvertion -in Excel to Csv Extractor ,Error message: " + "Convertion Error" + " && " + ex.Message, ex);
                LblMsg.Text ="Exception in Csv Convertion : "+ex.Message;
                LblMsg.ForeColor = System.Drawing.Color.Red;
            }

            //If we hit this point, something was wrong about the file and it is not generated by the Excel application
            return false;

        }

    }
}