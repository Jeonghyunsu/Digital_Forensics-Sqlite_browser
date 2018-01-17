using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;

namespace 졸작SE
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("Sqlite_fileCarving.dll")]
        extern public static void SQLiteAnalysis([MarshalAs(UnmanagedType.LPStr)] string filename, string resultPath, bool _deleteflag);
        
        string selectedFileName = "";
        byte[] fileBytes;
        byte[] newFileByte;
        string urlStr;
        bool isUrlParsing = false;
        string dateStr;

        static List<string> tableName;
        static List<string> tableSchema;
        static List<SqliteInfo> sqliteinfo;

        string url_table_name;
        string url_str;
        string date_str;
        string real_date_str;

        bool url_possible_flag = false;

        List<string> webviewUrl;

        public class SqliteInfo
        {
            public string tablename;
            public List<string> attributes;

            public List<List<string>> rows;

            public SqliteInfo()
            {
                attributes = new List<string>();
                rows = new List<List<string>>();

            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;

            listView1.Columns.Add("Table", 200);
            listView1.Columns.Add("Schema", 400);

            circularProgressBar1.Value = 0;
            circularProgressBar1.Visible = false;


         
        }

        public void writeToTextFile(string dateStrRes)
        {
            string defaultDirPath = folderBrowserDlg.SelectedPath + "\\";
            string path = "URLs\\"; ;
            DirectoryInfo urlDirInfo = new DirectoryInfo(path);
            string txtPath = path + dateStrRes + ".txt";

            try
            {
                if (urlDirInfo.Exists == false) // url 디렉토리 확인 
                    urlDirInfo.Create() ;   // 없으면 만들어

                if (File.Exists(txtPath) == false)
                    File.Create(txtPath).Close(); 
                
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(txtPath, append: true))
                {
                    file.WriteLine(dateStr);
                    file.WriteLine(urlStr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool UrlParse()
        {
            urlStr = "";
            isUrlParsing = false;

            int j = 0;
            for (int i = 20; i < 300; i++)
            {
                newFileByte[j] = fileBytes[i];
                j++;
            }

            string fileString = Encoding.UTF8.GetString(newFileByte);
            string[] filenameExtensionArray = { ".jpg", ".gif", ".png", ".jpeg", ".ico", ".PNG", "JPEG", ".GIF", ".JPG", ".ICO", ".js", ".css", ".JS",
                ".CSS", ".octet-stream", ".webp", "WEBP", ".html", ".HTML", ".svg", ".SVG" };

            int httpIndex = -1;
            int fileExtensionIndex = -1;

            while (!isUrlParsing)
            {
                string httpStr = fileString.Substring(19, 100);

                httpIndex = fileString.IndexOf("http");
                fileExtensionIndex = -1;

                for (int i = 0; i < filenameExtensionArray.Length; i++)
                {
                    if ((fileExtensionIndex = fileString.IndexOf(filenameExtensionArray[i])) != -1)
                    {
                        urlStr = fileString.Substring(httpIndex, fileExtensionIndex - httpIndex + filenameExtensionArray[i].Length);
                        isUrlParsing = true;
                        break;
                    }
                    else
                    {
                        isUrlParsing = false;
                        break;
                    }
                }
                if (!isUrlParsing)
                    break;
            }

            if (!isUrlParsing)
            {
                Console.WriteLine("확장자 리스트에 없는 것");
                urlStr = "URL Parsing Fail (It doesn't exist in the extension list.)";
                return false;
            }

            Console.WriteLine("[ URL : " + urlStr + " ]");
            Console.WriteLine("\nThe End");
            return true;
        }

        public void directoryManage(string dateStrRes)
        {
            string defaultDirPath = folderBrowserDlg.SelectedPath + "\\";
            string dateDir = defaultDirPath + dateStrRes;
            DirectoryInfo dateDirInfo = new DirectoryInfo(dateDir);

            if (dateDirInfo.Exists == false) 
                dateDirInfo.Create();  

            FileInfo selectedFileInfo = new FileInfo(selectedFileName);
            if (selectedFileInfo.Exists)
            {
                string fileName = Path.GetFileName(selectedFileName);
                selectedFileInfo.MoveTo(dateDir + "\\" + fileName);
                Console.WriteLine("Destination: " + selectedFileInfo);
            }
        }
        public bool classify_date()
        {
            dateStr = "";

            string dateStrRes = "";

            string fileString = Encoding.UTF8.GetString(fileBytes);

            if (fileString.IndexOf("Date:", StringComparison.OrdinalIgnoreCase) >= 0) 
            {
                int dateIndex = fileString.IndexOf("Date:", StringComparison.OrdinalIgnoreCase);
                dateStr = fileString.Substring(dateIndex + 6, 25);
            }

            DateTime daTi = Convert.ToDateTime(dateStr);
            dateStrRes = daTi.ToString().Substring(0, 10); 

            writeToTextFile(dateStrRes);

            UrlParse();

            directoryManage(dateStrRes);
            
            return true;
        }

        public void read_file(string selectedpath)
        {
            DirectoryInfo di = new DirectoryInfo(selectedpath);
            FileInfo[] fi = di.GetFiles("*.");

            circularProgressBar1.Visible = true;
            circularProgressBar1.Value = 0;
            circularProgressBar1.Minimum = 0;
            circularProgressBar1.Maximum = fi.Length;
           

            if (fi.Length == 0) MessageBox.Show("파일이 없습니다.");
            else
            {
                
                for (int i = 0; i < fi.Length; i++)
                {
                    selectedFileName = selectedpath + "\\" + fi[i].Name.ToString();
 
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        Thread.Sleep(3);

                        circularProgressBar1.Step = 1;
                        circularProgressBar1.PerformStep();              
                    }));
                   
                    Console.Write(circularProgressBar1.Value + " ");

                    try
                    {
                        using (FileStream fsSource = new FileStream(selectedFileName, FileMode.Open, FileAccess.Read))
                        {
                            // Read the source file into a byte array.
                            fileBytes = new byte[fsSource.Length];
                            newFileByte = new byte[300];
                            int numBytesToRead = (int)fsSource.Length;
                            int numBytesRead = 0;

                            while (numBytesToRead > 0)
                            {
                                int n = fsSource.Read(fileBytes, numBytesRead, numBytesToRead);
                                if (n == 0)
                                    break;

                                numBytesRead += n;
                                numBytesToRead -= n;
                            }
                            numBytesToRead = fileBytes.Length;

                            fsSource.Dispose();
                            classify_date();

                            fsSource.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }


                }
                
            }

            circularProgressBar1.Value = 0;
            circularProgressBar1.Visible = false;
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {


        }
        private void webViewReorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Clear();

            folderBrowserDlg.RootFolder = Environment.SpecialFolder.MyComputer;
            
            if(folderBrowserDlg.ShowDialog() != DialogResult.Cancel)
                read_file(folderBrowserDlg.SelectedPath);
        }

        public static void readSqliteInfo(string fileName)
        {

            tableName = new List<string>();
            tableSchema = new List<string>();
            sqliteinfo = new List<SqliteInfo>();

            StreamReader reader = new StreamReader(new FileStream(fileName, FileMode.Open));
            string schema_CREATE_TABLE = "CREATE TABLE ";
            string temp_read_line = "";

            SqliteInfo sql_unit = new SqliteInfo();
            List<string> row = new List<string>();

            bool fuckingstart = false;
            while (reader.EndOfStream == false)
            {
                temp_read_line = reader.ReadLine();
                Match match = Regex.Match(temp_read_line, schema_CREATE_TABLE);

                int startoffset = 0;
                if (match.Success)
                {
                    if (fuckingstart == false)
                    {
                        fuckingstart = true;
                    }
                    else
                    {
                        sqliteinfo.Add(sql_unit);
                        sql_unit = new SqliteInfo();
                    }

                    tableSchema.Add(temp_read_line);

                    int tableEnd = temp_read_line.IndexOf('(');
                    string tablename = temp_read_line.Substring(13, tableEnd - 13);
                    sql_unit.tablename = tablename.Trim();
                    tableName.Add(tablename);

                    int attributeCnt = 0;
                    int distributorCnt = 0;
                    bool startflag = true;

                    for (int i = 0; i < temp_read_line.Length; i++)
                    {
                        if (temp_read_line[i] == '(')
                        {
                            distributorCnt++;
                            if (startflag)
                            {
                                startoffset = i;
                                startflag = false;
                            }
                        }
                        else if (temp_read_line[i] == ')') distributorCnt--;

                        if ((temp_read_line[i] == ',' || temp_read_line[i] == '(') && distributorCnt == 1)
                        {
                            attributeCnt++;

                            string temp = "";
                            int idx = i + 1;


                            while (idx < temp_read_line.Length)
                            {
                                if (temp_read_line[idx] != ' ' && temp_read_line[idx] != ')')
                                {
                                    temp += temp_read_line[idx];
                                }
                                else if (temp_read_line[idx] == ' ' && temp == "")
                                {

                                }
                                else break;

                                idx++;
                            }

                            Match m = Regex.Match(temp.Trim(), "unique");
                            if (!m.Success)
                            {

                                sql_unit.attributes.Add(temp.Trim());
                            }
                            else attributeCnt--;
                        }
                    }
                    attributeCnt++;
                    if (attributeCnt == 1)
                    {
                        string temp = "";
                        int idx = startoffset + 1;

                        while (true)
                        {
                            if (temp_read_line[idx] != ' ')
                            {
                                temp += temp_read_line[idx];
                            }
                            else if (temp_read_line[idx] == ' ' && temp == "")
                            {

                            }
                            else break;

                            idx++;
                        }

                        sql_unit.attributes.Add(temp.Trim());

                    }


                }
                else if (temp_read_line == "*****")
                {
                    if (row.Count < sql_unit.attributes.Count)
                    {
                        int rowCnt = row.Count;
                        for (int i = 0; i < sql_unit.attributes.Count - rowCnt; i++) row.Add("Default");
                    }

                    sql_unit.rows.Add(row);
                    row = new List<string>();
                }
                else if (temp_read_line == "$$$")
                {
                    string hexValues = reader.ReadLine();
                    row.Add(hexValues);
                }
                else if (temp_read_line == "^^^")
                {
                    string hexValues = reader.ReadLine();
                    int byteCnt = hexValues.Trim().Length / 2;

                    if (hexValues.Length > 0) hexValues = hexValues.TrimEnd();

                    byte[] ba = new byte[byteCnt];
                    string[] hexValuesSplit = hexValues.Split(' ');
                    int i = 0;
                    bool null_flag = false;
                    foreach (String hex in hexValuesSplit)
                    {
                        if (i < byteCnt)
                        {
                            if (hex != "NULL") ba[i++] = Convert.ToByte(hex, 16);
                            else
                            {
                                null_flag = true;
                                break;
                            }
                        }
                    }
                    string str = System.Text.Encoding.UTF8.GetString(ba);
                    if (!null_flag) row.Add(str.TrimEnd().TrimStart());
                    else row.Add("NULL");
                }
                else
                {
                    if (temp_read_line == "") break;
                    row.Add(temp_read_line);
                }

                if (temp_read_line == "L*A*S*T")
                {
                    sqliteinfo.Add(sql_unit);
                    break;
                }

            }
            reader.Close();
        }
        
        public static bool WebViewCache_availableTable(string fileName, ref string url_tablename, ref string urlstr, ref string datestr)
        {
            bool url_possible_flag = false;
            switch (fileName)
            {
                case "Bookmark.db":
                    url_possible_flag = true;
                    url_tablename = "history";
                    urlstr = "url";
                    datestr = "date";
                    break;

                case "search.db":
                    url_possible_flag = true;
                    url_tablename = "visited_history ";
                    urlstr = "url";
                    datestr = "last_visited_date";
                    break;

                case "History.db":
                    url_possible_flag = true;
                    url_tablename = "urls";
                    urlstr = "url";
                    datestr = "last_visit_time";
                    break;
            }
            
            return url_possible_flag;
        }

        private void sQLiteRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool restore_flag = false;
            textBox1.Clear();

            tabControl1.SelectedIndex = 0;
            label2.Visible = false;
            label1.Visible = true;
            
            bool color_flag = false;

            listView1.Items.Clear();

            string filename = "";

            openFileDlg.InitialDirectory = "C:\\graduationproject\\org.chromium.android_webview";
            openFileDlg.Filter = "db files (*.db)|*.db|All files (*.*)|*.*";
            openFileDlg.FilterIndex = 2;

            if (openFileDlg.ShowDialog(this) != DialogResult.Cancel)
            {
                if (MessageBox.Show("         삭제된 것을 복구하시겠습니까?\n\n(원하지 않는 레코드까지 복구될 수도 있습니다!)", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    restore_flag = true;
                }
                
                listView2.Clear();

                filename = openFileDlg.FileName.Substring(openFileDlg.FileName.LastIndexOf("\\") + 1);
                Console.WriteLine(restore_flag);
                SQLiteAnalysis((System.String)openFileDlg.FileName, "DB.txt", restore_flag);
             
                readSqliteInfo("DB.txt");

                for (int i = 0; i < tableName.Count; i++)
                {
                    color_flag = WebViewCache_availableTable(filename, ref url_table_name, ref url_str, ref date_str);

                    real_date_str = Date_Parsing(date_str);

                    ListViewItem lvi = new ListViewItem(tableName[i]);
                    lvi.SubItems.Add(tableSchema[i]);

                    listView1.Items.Add(lvi);

                    if(color_flag && tableName[i] == url_table_name)
                    {
                        listView1.Items[i].BackColor = Color.PaleGreen;
                    }
                }
            }

            label1.Visible = true;
            label1.Text = filename;
        }

        private void label1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        public string Date_Parsing(string date)
        {
            string real_date="";
            return real_date;
        }
        
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            listView2.Clear();
            textBox1.Clear();

            if (listView1.Items[listView1.FocusedItem.Index].BackColor == Color.PaleGreen)
                url_possible_flag = true;
            else
                url_possible_flag = false;

            string table_name = listView1.FocusedItem.SubItems[0].Text;

            label1.Visible = false;
            label2.Visible = true;
            label2.Text = table_name;

            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;
            listView2.Scrollable = true;
            
            for(int i = 0; i < sqliteinfo[listView1.FocusedItem.Index].attributes.Count;i++)
            {
                listView2.Columns.Add(sqliteinfo[listView1.FocusedItem.Index].attributes[i], 100);
            }
            
            //item 역시 마찬가지
            for (int i = 0; i < sqliteinfo[listView1.FocusedItem.Index].rows.Count; i++)
            {
                ListViewItem lvi = new ListViewItem(sqliteinfo[listView1.FocusedItem.Index].rows[i][0]);

                for (int j = 1; j < sqliteinfo[listView1.FocusedItem.Index].rows[i].Count; j++)
                {
                    lvi.SubItems.Add(sqliteinfo[listView1.FocusedItem.Index].rows[i][j]);
                }
                
                listView2.Items.Add(lvi);

     
            }

            tabControl1.SelectedIndex = 1;
        }
        
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            int column_count = listView1.Columns.Count;

            textBox1.Clear();

            for (int i = 0; i < column_count; i++)
            {
                textBox1.AppendText(listView1.FocusedItem.SubItems[i].Text);
                textBox1.AppendText("\n\n");
                textBox1.AppendText("----------------------------------------------------\n");
            }
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            int column_count = listView2.Columns.Count;

            textBox1.Clear();

            for (int i = 0; i < column_count; i++)
            {
                textBox1.AppendText(listView2.FocusedItem.SubItems[i].Text);
                textBox1.AppendText("\n\n");
                textBox1.AppendText("----------------------------------------------------\n");
            }
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if(tabControl1.SelectedIndex == 0)
            {
                label1.Visible = true;
                label2.Visible = false;                
            }

            else if(tabControl1.SelectedIndex == 1)
            {
                label2.Visible = true;
                label1.Visible = false;
            }
        }

        public static DateTime SqlDate(long parsingDate)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = start.AddMilliseconds(parsingDate).ToLocalTime();

            return dt;
        }
        public static DateTime WebViewDate(string dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
            return dt;
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            webviewUrl = new List<string>();

            long long_date = 0;
            string htmlurl="";

            if (url_possible_flag)
            {
                for (int i = 0; i < sqliteinfo[listView1.FocusedItem.Index].attributes.Count; i++)
                {

                    if (sqliteinfo[listView1.FocusedItem.Index].attributes[i] == url_str)
                    {
                        htmlurl = listView2.FocusedItem.SubItems[i].Text.ToString();
                        break;
                    }
                }

                for (int i = 0; i < sqliteinfo[listView1.FocusedItem.Index].attributes.Count; i++)
                {
                   
                    if (sqliteinfo[listView1.FocusedItem.Index].attributes[i] == date_str)
                    {
                        string long_date_str = listView2.FocusedItem.SubItems[i].Text.ToString();
                       
                        long_date = Convert.ToInt64(long_date_str);
                        break;
                    }
                }

          
                DateTime sql_date;
                if (string.Equals(date_str, "last_visit_time", StringComparison.CurrentCulture))
                {
                    long time = long_date;
                    long convertedTime = (time - 11644473600000000) / 1000000;//divide by 1000000 because we are going to add Seconds on to the base date
                    sql_date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    sql_date = sql_date.AddSeconds(convertedTime);
                }
                else
                {
                    sql_date = SqlDate(long_date).AddHours(-9);
                    
                }
               
                List<string> urls = new List<string>();
                List<string> dates = new List<string>();
                
                string sql_dayStr = sql_date.Day.ToString();
                string sql_MonthStr = sql_date.Month.ToString();
               
                if (sql_date.Day < 10)
                    sql_dayStr = "0" + sql_dayStr;
                if (sql_date.Month < 10)
                    sql_MonthStr = "0" + sql_MonthStr;
    
                string _fileName = "Urls\\" + sql_date.Year.ToString() + "-" + sql_MonthStr + "-" + sql_dayStr + ".txt";

                using (System.IO.StreamReader file =
                    new System.IO.StreamReader(_fileName, true))
                {
                    while (file.EndOfStream == false)
                    {
                        dates.Add(file.ReadLine());
                        urls.Add(file.ReadLine());
                    }
                }

                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime wv_date = WebViewDate(dates[i]);

                    TimeSpan ts = sql_date - wv_date;
                    int result = ts.Hours * 60*60 + ts .Minutes*60 + ts.Seconds;
                   
                    if (result >= -10 && result <= 10)
                    {
                        if (urls[i].Length > 4 && urls[i].Substring(0, 4) == "http")
                        {
                            webviewUrl.Add(urls[i]);
                        }
                    }
                }

                WebForm wf = new WebForm(htmlurl, webviewUrl);
                wf.Show();
                    
            }
            else
            {
                MessageBox.Show("웹페이지 튜플이 아닙니다.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
           
        }
    }
}
