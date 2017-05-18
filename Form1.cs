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

        public Form1()
        {
            InitializeComponent();

            // wyświetlanie loginform i pobranie nowego połaćzenia
            LoginForm form = new LoginForm();
            form.ShowDialog();
            connection = form.connection;

            //załadnowanie listy tabel bazy danych
            var list = connection.GetTablesName();
            foreach (var tabel in list)
            {
                dataGridView1.Rows.Add(tabel);
            }

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
        //kliknięcie wybranej tabeli, powoduje pojwenie się uprawnień użytkowników
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();

            var d = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
            var list2 = connection.GetTablePrivileges(d);
            foreach (var tabel in list2)
            {
                dataGridView2.Rows.Add(tabel.UserName, tabel.Select, tabel.SelectIsGrantable, tabel.Insert, tabel.InsertIsGrantable, tabel.Delete, tabel.DeleteIsGrantable, tabel.Update, tabel.UpdateIsGrantable);
            }
        }

    }
}
