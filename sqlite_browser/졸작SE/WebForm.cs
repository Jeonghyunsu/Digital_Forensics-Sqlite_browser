using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace 졸작SE
{
    public partial class WebForm : Form
    {
        List<string> web_url;
        string html_url;
        List<string> currect;

        //새로운 생성자로 url string을 받아서 visited_data에 저장
        public WebForm()
        {
             SetBrowserFeatureControl();
            InitializeComponent();
        }
        private static void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx
            // WebBrowser Feature Control settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            // make the control is not running inside Visual Studio Designer
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
        }
        private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }
        private static UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }
            // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode. Default value for Internet Explorer 10.
            UInt32 mode = 10000;
            switch (browserVersion)
            {
                case 7:
                    // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    mode = 7000;
                    break;
                case 8:
                    // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    mode = 8000;
                    break;
                case 9:
                    // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    mode = 9000;
                    break;
                default:
                    // use IE10 mode by default
                    break;
            }

            Console.WriteLine(mode);
           // mode = 7000;
            return mode;
        }

        public WebForm(string htmlurl, List<string> weburl)
        {
           SetBrowserFeatureControl();
            InitializeComponent();

            html_url = htmlurl;
            web_url = weburl;

            currect = new List<string>();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            string selected_url;

            int i;

            i = listView1.FocusedItem.Index;

            selected_url = currect[i];
            try
            { pictureBox1.Load(selected_url); }
            catch (Exception) { }
        }
        

        private void WebForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = html_url;

            string url = html_url;
            webBrowser1.Navigate(url);

            for (int i = 0; i < web_url.Count; i++)
            {

                try
                {
                    pictureBox1.Load(web_url[i]);
                    currect.Add(web_url[i]);
                    imageList1.Images.Add(web_url[i], pictureBox1.Image);

                    Console.WriteLine(web_url[i]);
                }

                catch (Exception)
                { }
            }
          //  Console.WriteLine("@@@@@@@@@@@@@" + currect.Count);
            listView1.LargeImageList = imageList1;

            for (int i = 0; i < currect.Count; i++)
            {
                ListViewItem it = new ListViewItem();

                it.ImageIndex = i;

                listView1.Items.Add(it);
            }

            this.listView1.MouseClick += new MouseEventHandler(listView1_MouseClick);

            pictureBox1.Image = null;
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
            PictureForm pf = new PictureForm();
            pf.Picture = this.pictureBox1.Image;
            pf.Show();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void GetWebPage()
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(this.textBox1.Text);

            Request.Method = "GET";

            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();

            string Server = Response.Server;
            HttpStatusCode StatusCode = Response.StatusCode;

            if (StatusCode == HttpStatusCode.OK)
            {
                Stream ResponseStream = Response.GetResponseStream();
                StreamReader Reader = new StreamReader(ResponseStream);

                this.webBrowser1.DocumentText = Reader.ReadToEnd();
            }
            else
            {
         
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(html_url);
        }
    }
}