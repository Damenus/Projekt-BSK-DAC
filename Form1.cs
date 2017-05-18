using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        DBConnection connection;
        Task task;

        CancellationTokenSource ts;
        CancellationToken ct;

        public Form1()
        {
            InitializeComponent();

            // wyświetlanie loginform i pobranie nowego połaćzenia
            LoginForm form = new LoginForm();
            form.ShowDialog();
            connection = form.connection;

  

            if (connection != null)
            {
                toolStripStatusLabel1.Text = "Jesteś zalogowany jako " + connection.Login;
                connection.ListTabels = connection.GetTablesName();
                DelegateMethod(connection.GetTablesName());
                //Task task = Task.Run(() => chceckIfPrivilageChange("ksiazka"));
            }
        }

        private void chceckIfTablesChange()
        {
            while (true)
            {
                var list = connection.GetTablesName();
                if (dataGridView1.InvokeRequired)
                {
                    if (connection.comparisonTabels(list))
                    {
                        connection.ListTabels = list;
                        dataGridView1.BeginInvoke((new MyDelegate(DelegateMethod)), list);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void chceckIfPrivilageChange(String nameTable, CancellationToken ct)
        {

            while (true)
            {
                var list = connection.GetTablePrivilegesAllUsers(nameTable);
                if (dataGridView2.InvokeRequired)
                {
                    if (connection.isGranteeListTheSame(list))
                    {
                        connection.ListGrantee = list;
                        dataGridView2.BeginInvoke((new Delegate(refreshPreviligeTabele)), list);
                    }                    
                }
                Thread.Sleep(2000);
                if (ct.IsCancellationRequested)
                {
                    // another thread decided to cancel
                    Console.WriteLine("task canceled");
                    break;
                }
            }
        }

        public delegate void MyDelegate(List<String> myControl); 

        public void DelegateMethod(List<String> list)
        {
            var rem = 0;
            //rem = dataGridView1.SelectedCells[0].RowIndex;
            //dataGridView1.Rows[rem].Selected = false;       

            dataGridView1.Rows.Clear();
            foreach (var tabel in list)
            {
                dataGridView1.Rows.Add(tabel);
            }


             //dataGridView1.Rows[rem].Selected = true;
             dataGridView1.CurrentCell = dataGridView1[0, rem];
             //dataGridView1_CellClick(dataGridView1, new DataGridViewCellEventArgs(0, 0));
 
        }

        public delegate void Delegate(List<Grantee> myControl);

        private void refreshPreviligeTabele(List<Grantee> list)
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();

            connection.ListGrantee = list;
            foreach (var tabel in list)
            {
                dataGridView2.Rows.Add(tabel.UserName, tabel.Select, tabel.SelectIsGrantable, tabel.Insert, tabel.InsertIsGrantable, tabel.Delete, tabel.DeleteIsGrantable, tabel.Update, tabel.UpdateIsGrantable);
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
            
            var nameTable = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
            var list2 = connection.GetTablePrivilegesAllUsers(nameTable);
            connection.ListGrantee = list2;
            foreach (var tabel in list2)
            {
                dataGridView2.Rows.Add(tabel.UserName, tabel.Select, tabel.SelectIsGrantable, tabel.Insert, tabel.InsertIsGrantable, tabel.Delete, tabel.DeleteIsGrantable, tabel.Update, tabel.UpdateIsGrantable);
            }

            ts = new CancellationTokenSource();
            ct = ts.Token;
            if(task != null)
                ts.Cancel();
            task = Task.Factory.StartNew(() => chceckIfPrivilageChange(nameTable, ct));
        }



    }
}
