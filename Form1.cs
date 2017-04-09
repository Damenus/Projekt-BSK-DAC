using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        DBConnection connection;

       

        public Form1(DBConnection con)
        {
            InitializeComponent();
            connection = con;

            //załadnowanie listy tabel bazy danych
            var list = connection.GetTablesName();
            foreach (var tabel in list)
            {
                dataGridView1.Rows.Add(tabel);
            }

            var list2 = connection.GetTablePrivileges("user");
            foreach (var tabel in list2)
            {
                dataGridView2.Rows.Add(tabel.UserName,tabel.Select,tabel.SelectIsGrantable,tabel.Insert,tabel.InsertIsGrantable,tabel.Delete,tabel.DeleteIsGrantable,tabel.Update,tabel.UpdateIsGrantable);
            }

        //    String a = "select host, user, password from mysql.user;";

        //    List<String> myCars = new List<String>{
        //     "DD", "asd", "asdasd"
        //};
        //    SortableBindingList<String> myCarsBindingList;
        //    BindingSource carBindingSource;

        //    myCarsBindingList = new SortableBindingList<String>(myCars);
        //    carBindingSource = new BindingSource();
        //    carBindingSource.DataSource = myCarsBindingList;
        //    //Drag a DataGridView control from the Toolbox to the form.
        //    dataGridView3.DataSource = myCars;

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
