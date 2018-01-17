using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 졸작SE
{
    public partial class datatable : Form
    {
        private string dt_result_path;
        private string dt_table_name;

        //public datatable(string result_path, string table_name)
        //{
        //    InitializeComponent();
        //    this.dt_result_path = result_path;
        //    this.dt_table_name = table_name;
        //}

        public datatable()
        {
            InitializeComponent();
        }
        
        public void Result_Parsing()
        {

        }

        private void datatable_Load(object sender, EventArgs e)
        {
            label1.Text = dt_table_name;

            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;


            //column 동적으로 만들어야 함
            listView1.Columns.Add("Table", 200);
            listView1.Columns.Add("Schema", 400);

            //item 역시 마찬가지
            for(int i = 0; i < 10; i++)
            {
                ListViewItem lvi = new ListViewItem("attribute_value" + (i + 1) + "_1");
                lvi.SubItems.Add("attribute_value" + (i + 1) + "_2");

                listView1.Items.Add(lvi);

                //url이 있으면 색깔 설정
                if(i == 5)
                {
                    listView1.Items[i].BackColor = Color.Red;
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listView1.FocusedItem.BackColor != Color.Red)
                MessageBox.Show("It is not a webpage tuple");
            else
            {
                //인자에 string 배열로 url 넘거야 듯
                WebForm wf = new WebForm();
                wf.Show();
            }
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
    }
}
