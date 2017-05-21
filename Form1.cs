using MySql.Data.MySqlClient;
using System;
using System.Collections;
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
        ArrayList task;
        String chosenTable;


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

                task = new ArrayList();

                foreach (var i in connection.ListTabels)
                    task.Add( Task.Factory.StartNew(() => chceckIfPrivilageChange(i, ct)));
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
                if (chosenTable == nameTable)
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
        //nadawanie uprawnień
        private void grant(string privilage, bool grantable)
        {
            var granteeName = "'" + dataGridView2.CurrentCell.Value.ToString() + "'@'localhost'";
            var tableName = dataGridView1.CurrentCell.Value.ToString();
            string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", connection.Server, connection.Port, connection.DatabaseName, connection.Login, connection.Password);
            MySqlConnection myConnection = new MySqlConnection(connectionString);
            myConnection.Open();
            MySqlCommand cmd = myConnection.CreateCommand();
            if (!grantable)
                cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'NO');",granteeName, tableName, privilage);
            else
                cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'YES');", granteeName, tableName, privilage);
            cmd.ExecuteReader();
            myConnection.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            bool giveGrantable = false;
            if (checkBox1.Checked && connection.myPrivileges.Insert.ToString() == "True")
            {
                if (checkBox5.Checked && connection.myPrivileges.InsertIsGrantable.ToString() == "True")
                    giveGrantable = true;
                else
                    giveGrantable = false;
                grant("INSERT", giveGrantable);
            }
            if (checkBox2.Checked && connection.myPrivileges.Delete.ToString() == "True")
            {
                if (checkBox6.Checked && connection.myPrivileges.DeleteIsGrantable.ToString() == "True")
                    giveGrantable = true;
                else
                    giveGrantable = false;
                grant("DELETE", giveGrantable);
            }
            if (checkBox3.Checked && connection.myPrivileges.Update.ToString() == "True")
            {
                if (checkBox7.Checked && connection.myPrivileges.UpdateIsGrantable.ToString() == "True")
                    giveGrantable = true;
                else
                    giveGrantable = false;
                grant("UPDATE", giveGrantable);
            }
            if (checkBox4.Checked && connection.myPrivileges.Select.ToString() == "True")
            {
                if (checkBox8.Checked && connection.myPrivileges.SelectIsGrantable.ToString() == "True")
                    giveGrantable = true;
                else
                    giveGrantable = false;
                grant("SELECT", giveGrantable);
            }
            if(checkBox9.Checked && connection.myPrivileges.TakeOver.ToString() == "True")
            {
                if (checkBox9.Checked && connection.myPrivileges.TakeOverIsGrantable.ToString() == "True")
                {
                    giveGrantable = true;
                }
                else
                    giveGrantable = false;
                grant("TAKEOVER", giveGrantable);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView2.MultiSelect = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
        private void disableAllCheckboxes()
        {
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            checkBox3.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;
            checkBox7.Enabled = false;
            checkBox8.Enabled = false;
            checkBox9.Enabled = false;
            checkBox10.Enabled = false;

        }
        private void disableChceckboxes()
        {
            DataGridViewRow row = dataGridView2.CurrentRow;
            if (connection.myPrivileges.Insert.ToString() == "True" &&
                row.Cells[3].Value.ToString() == "False" && connection.myPrivileges.InsertIsGrantable.ToString() == "True")
            {
                    checkBox1.Enabled = true;
                    checkBox5.Enabled = true;
            }
            else
            {
                checkBox1.Enabled = false;
                checkBox5.Enabled = false;
            }
            if (connection.myPrivileges.Delete.ToString() == "True" &&
                row.Cells[5].Value.ToString() == "False" && connection.myPrivileges.DeleteIsGrantable.ToString() == "True")
            {
                checkBox2.Enabled = true;
                checkBox6.Enabled = true;
            }
            else
            {
                checkBox2.Enabled = false;
                checkBox6.Enabled = false;
            }
            if (connection.myPrivileges.Update.ToString() == "True" &&
                row.Cells[7].Value.ToString() == "False" && connection.myPrivileges.UpdateIsGrantable.ToString() == "True")
            {
                checkBox3.Enabled = true;
                checkBox7.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
                checkBox7.Enabled = false;
            }
            if (connection.myPrivileges.Select.ToString() == "True" &&
                row.Cells[1].Value.ToString() == "False" && connection.myPrivileges.SelectIsGrantable.ToString() == "True")
            {
                checkBox4.Enabled = true;
                checkBox8.Enabled = true;
            }
            else
            {
                checkBox4.Enabled = false;
                checkBox8.Enabled = false;
            }
            if(connection.myPrivileges.TakeOver.ToString() == "True" &&
                row.Cells[1].Value.ToString() == "False" && connection.myPrivileges.TakeOverIsGrantable.ToString() == "True")
            {
                checkBox9.Enabled = true;
                checkBox10.Enabled = true;
            }
            else
            {
                checkBox9.Enabled = false;
                checkBox10.Enabled = false;
            }
        }
        //kliknięcie wybranej tabeli, powoduje pojwenie się uprawnień użytkowników
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            disableAllCheckboxes();
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();
            if (e.RowIndex >= 0) 
            {
                chosenTable = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
                var nameTable = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
                var list2 = connection.GetTablePrivilegesAllUsers(nameTable);
                connection.ListGrantee = list2;
                foreach (var tabel in list2)
                {
                    if (tabel.UserName == connection.Login)
                        connection.myPrivileges = tabel;
                    dataGridView2.Rows.Add(tabel.UserName, tabel.Select, tabel.SelectIsGrantable, tabel.Insert, tabel.InsertIsGrantable, tabel.Delete, tabel.DeleteIsGrantable, tabel.Update, tabel.UpdateIsGrantable, tabel.TakeOver, tabel.TakeOverIsGrantable);
                }
                if(dataGridView2.RowCount > 0)
                dataGridView2.Rows[0].Selected = false;
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(dataGridView2.CurrentRow != null)
            disableChceckboxes();
        }
    }
}
