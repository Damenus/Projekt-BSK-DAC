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
    public partial class MainForm : Form
    {
        DBConnection connection;
        private ArrayList kolumny;
        Task task;

        String chosenTable;
        String chosenUser;

        public MainForm()
        {
            InitializeComponent();
        
            LoginForm form = new LoginForm();
            form.ShowDialog();
            connection = form.connection;

            if (connection != null)
            {
                
                toolStripStatusLabel1.Text = "Jesteś zalogowany jako " + connection.Login;
                prepareDataGridView2();

                tableNameRefresh(connection.GetTablesName());
                userNameRefresh(connection.GetUsers());
                refreshPreviligeTabele(connection.GetTablePrivilegesAllUsersAllTabel());

                task = Task.Run(() => chceckIfChange());
            }
        }


        private void prepareDataGridView2()
        {
            //dataGridView2.Columns.Clear(); czyszczenie i wstawianie od nowa


            var tables = connection.ListTabels;
            kolumny = new ArrayList ();

            foreach (var nameTable in tables)
            {
               
                var newTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
                kolumny.Add(newTable);

                newTable.HeaderText = nameTable;
                newTable.Name = nameTable;
                newTable.ReadOnly = true;
                newTable.Width = 120;

                this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {newTable});
            }
            // 
            // TakeOverIsGrantable
            // 
            var TakeOverIsGrantable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            kolumny.Add(TakeOverIsGrantable);
            TakeOverIsGrantable.HeaderText = "TakeOverIsGrantable";
            TakeOverIsGrantable.Name = "TakeOverIsGrantable";
            TakeOverIsGrantable.ReadOnly = true;
            TakeOverIsGrantable.Width = 110;
            dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { TakeOverIsGrantable });

        }
       
        public delegate void TableDelegate(List<String> myControl);
        public delegate void UserDelegate(List<String> myControl);
        public delegate void PreviligeDelegate(List<Grantee> myControl);

        private void refreshPreviligeTabele(List<Grantee> list)
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();

            foreach (var u in connection.ListUsers)
            {
                dataGridView2.Rows.Add(u);
            }

            var priviliges = connection.GetTablePrivilegesAllUsersAllTabel();

            foreach (var g in priviliges)
            {
                String searchValue = g.UserName;

                int rowID = -1;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells["Login"].Value != null) // Need to check for null if new row is exposed
                    {
                        if (row.Cells["Login"].Value.ToString().Equals(searchValue))
                        {
                            rowID = row.Index;
                            break;
                        }
                    }
                }

                int columnIndex = dataGridView2.Columns[g.TableName].Index;
                dataGridView2.Rows[rowID].Cells[columnIndex].Value = g.wyswietlUprawnienia();
            }
        }

        public void tableNameRefresh(List<String> list)
        {
            var rem = 0;

            if (dataGridView1.SelectedCells.Count != 0)
            {
                rem = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows[rem].Selected = false;
            }

            dataGridView1.Rows.Clear();

            foreach (var tabel in list)
            {
                dataGridView1.Rows.Add(tabel);
            }

            dataGridView1.Rows[rem].Selected = true;
            dataGridView1.CurrentCell = dataGridView1[0, rem];

            //dataGridView1_CellClick(dataGridView1, new DataGridViewCellEventArgs(0, 0));
        }

        public void userNameRefresh(List<String> list)
        {
            var rem = 0;

            if (dataGridView3.SelectedCells.Count != 0)
            {        
                rem = dataGridView3.CurrentCell.RowIndex;
                dataGridView3.Rows[rem].Selected = false;
            }

            dataGridView3.Rows.Clear();

            foreach (var tabel in list)
            {
                dataGridView3.Rows.Add(tabel);
            }

            dataGridView3.Rows[rem].Selected = true;
            dataGridView3.CurrentCell = dataGridView3[0, rem];

            //dataGridView3_CellClick(dataGridView1, new DataGridViewCellEventArgs(0, rem));
        }

        public bool compareList(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return true;

            int zliczamIleTakichSamych = 0;
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    if (list1[i] == list2[i])
                    {
                        zliczamIleTakichSamych++;
                    }
                }
            }

            if (zliczamIleTakichSamych != list1.Count)
                return false;
            else
                return true;
        }

        private void chceckIfChange()
        {
            while (true)
            {
                var newTables = connection.GetTablesName();
                var newUsers = connection.GetUsers();
                var newPreviliges = connection.GetTablePrivilegesAllUsersAllTabel();

                //zmiana przywiejów
                if (dataGridView2.InvokeRequired)
                {
                    if (connection.isGranteeListTheSame(newPreviliges))
                    {
                        connection.ListGrantee = newPreviliges;
                        dataGridView2.BeginInvoke((new PreviligeDelegate(refreshPreviligeTabele)), newPreviliges);
                    }
                }

                //zmiana tabeli
                if (dataGridView1.InvokeRequired)
                {
                    if (compareList(connection.ListTabels, newTables))
                    {
                        connection.ListTabels = newTables;
                        dataGridView1.BeginInvoke((new UserDelegate(tableNameRefresh)), newTables);
                        dataGridView2.BeginInvoke((new PreviligeDelegate(refreshPreviligeTabele)), newPreviliges);
                    }
                }
                //zmian użytkonikow
                if (dataGridView3.InvokeRequired)
                {
                    if (compareList(newUsers, connection.ListUsers))
                    {
                        connection.ListUsers = newUsers;
                        dataGridView3.BeginInvoke((new UserDelegate(userNameRefresh)), newUsers);
                        dataGridView2.BeginInvoke((new PreviligeDelegate(refreshPreviligeTabele)), newPreviliges);
                    }
                }

                Thread.Sleep(3000);
            }
        }

       
        //nadawanie uprawnień
        private void grant(string privilage, bool grantable)
        {
            var granteeName = "'" + chosenUser + "'@'%'";
            var tableName = chosenTable;
            string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", connection.Server, connection.Port, connection.DatabaseName, connection.Login, connection.Password);
            MySqlConnection myConnection = new MySqlConnection(connectionString);
            MySqlCommand cmd = myConnection.CreateCommand();
            if (privilage == "TAKEOVER")
            {
                if (!grantable)
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        myConnection.Open();
                        cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'NO', '{3}');", granteeName, row.Cells[0].Value.ToString(), privilage, connection.Login);
                        cmd.ExecuteReader();
                        myConnection.Close();
                    }
                else
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        myConnection.Open();
                        cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'YES', '{3}');", granteeName, row.Cells[0].Value.ToString(), privilage, connection.Login);
                        cmd.ExecuteReader();
                        myConnection.Close();
                    }
                return;
            }
            myConnection.Open();
            if (!grantable)
                cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'NO', '{3}');", granteeName, tableName, privilage, connection.Login);
            else
                cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'YES', '{3}');", granteeName, tableName, privilage, connection.Login);
            cmd.ExecuteReader();
            myConnection.Close();
        }

        private void giveAllPrivileges()//oddaje wszystkie uprawnienia i odbiera wszystkim, którzy byli pod spodem
        {
            // var list = connection.GetTablePrivilegesAllUsers(chosenTable);


            var userName = "'" + dataGridView2.CurrentCell.Value.ToString() + "'@'%'";
            var myUserName = "'" + connection.Login + "'@'%'";
            string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", connection.Server, connection.Port, connection.DatabaseName, connection.Login, connection.Password);
            MySqlConnection myConnection = new MySqlConnection(connectionString);
            MySqlCommand cmd = myConnection.CreateCommand();
            myConnection.Open();
            cmd.CommandText = string.Format("UPDATE uprawnienia.user_privileges SET `GRANTEE`=\"{0}\" WHERE GRANTEE=\"{1}\" AND IS_GRANTABLE='YES';", userName, myUserName);
            cmd.ExecuteReader();
            myConnection.Close();
            myConnection.Open();
            cmd.CommandText = string.Format("DELETE FROM uprawnienia.user_privileges WHERE GRANTEE=\"{0}\" AND PRIVILEGE_TYPE='TAKEOVER';", userName);
            cmd.ExecuteReader();
            myConnection.Close();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                takeBackPrivilege(connection.Login, "SELECT", row.Cells[0].Value.ToString());
                takeBackPrivilege(connection.Login, "INSERT", row.Cells[0].Value.ToString());
                takeBackPrivilege(connection.Login, "DELETE", row.Cells[0].Value.ToString());
                takeBackPrivilege(connection.Login, "UPDATE", row.Cells[0].Value.ToString());
                takeBackPrivilege(connection.Login, "TAKEOVER", row.Cells[0].Value.ToString());
            }
        }
        private void takeOver()
        {
            string userName = dataGridView2.CurrentRow.Cells[0].Value.ToString();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var usersPrivileges = connection.GetTablePrivilegesAllUsers(row.Cells[0].Value.ToString());
                Grantee user = usersPrivileges.FindLast(x => x.UserName == userName); //szukamy uzytkownika, któremu usuwamy uprawnienia
                if (user.Select)
                {
                    takeBackPrivilege(user.UserName, "SELECT", row.Cells[0].Value.ToString());
                }
                if (user.Insert)
                {
                    takeBackPrivilege(user.UserName, "INSERT", row.Cells[0].Value.ToString());
                }
                if (user.Delete)
                {
                    takeBackPrivilege(user.UserName, "DELETE", row.Cells[0].Value.ToString());
                }
                if (user.Update)
                {
                    takeBackPrivilege(user.UserName, "UPDATE", row.Cells[0].Value.ToString());
                }
                if (user.TakeOver)
                {
                    takeBackPrivilege(user.UserName, "TAKEOVER", row.Cells[0].Value.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentCell.ColumnIndex == 0)
            {
                if (dataGridView2.CurrentRow.Cells[9].Value.ToString() == "True")
                {
                    takeOver(); //usuwanie uprawnien przejmującego
                    giveAllPrivileges(); //oddanie uprawnień 
                    return;
                }
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
                if (checkBox9.Checked && connection.myPrivileges.TakeOver.ToString() == "True")
                {
                    if (checkBox10.Checked && connection.myPrivileges.TakeOverIsGrantable.ToString() == "True")
                    {
                        giveGrantable = true;
                    }
                    else
                        giveGrantable = false;
                    grant("TAKEOVER", giveGrantable);
                }
            }
            uncheck();
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
            if (connection != null)
                connection.Close();
        }

        private void disableAllCheckboxes()
        {
            checkBox1.Enabled = false;
            checkBox1.Checked = false;

            checkBox2.Enabled = false;
            checkBox2.Checked = false;

            checkBox3.Enabled = false;
            checkBox3.Checked = false;

            checkBox4.Enabled = false;
            checkBox4.Checked = false;

            checkBox5.Enabled = false;
            checkBox5.Checked = false;

            checkBox6.Enabled = false;
            checkBox6.Checked = false;

            checkBox7.Enabled = false;
            checkBox7.Checked = false;

            checkBox8.Enabled = false;
            checkBox8.Checked = false;

            checkBox9.Enabled = false;
            checkBox9.Checked = false;

            checkBox10.Enabled = false;
            checkBox10.Checked = false;


        }
        private void uncheck()
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;
            checkBox8.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
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
            if (connection.myPrivileges.TakeOver.ToString() == "True" &&
                row.Cells[9].Value.ToString() == "False" && connection.myPrivileges.TakeOverIsGrantable.ToString() == "True")
            {
                checkBox9.Enabled = true;
                checkBox10.Enabled = true;
            }
            else
            {
                checkBox9.Enabled = false;
                checkBox10.Enabled = false;
            }
            uncheck();
        }
        //kliknięcie wybranej tabeli, powoduje pojwenie się uprawnień użytkowników
  
        private void takeBackPrivilege(string userName, string privilege, string tableName)//kto traci, co traci, gdzie traci
        {
            string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", connection.Server, connection.Port, connection.DatabaseName, connection.Login, connection.Password);
            MySqlConnection myConnection = new MySqlConnection(connectionString);
            MySqlCommand cmd1 = myConnection.CreateCommand();
            var usersPrivileges = connection.GetTablePrivilegesAllUsers(tableName);
            if (privilege == "SELECT")
            {
                //  if (connection.myPrivileges.SelectIsGrantable) //jeżeli możemy nadawać uprawnienia
                foreach (var user in usersPrivileges)//sprawdzamy uprawnienia każdego użytkownika
                {
                    if (user.Select && user.fromWho["SELECT"] == userName)//jeżeli użytkownik ma uprawnienie i ma je ode mnie
                    {
                        takeBackPrivilege(user.UserName, privilege, tableName);
                    }
                }
            }
            else if (privilege == "INSERT")
            {
                //    if (connection.myPrivileges.InsertIsGrantable) //jeżeli możemy nadawać uprawnienia
                foreach (var user in usersPrivileges)//sprawdzamy uprawnienia każdego użytkownika
                {
                    if (user.Insert && user.fromWho["INSERT"] == userName)//jeżeli użytkownik ma uprawnienie i ma je ode mnie
                    {
                        takeBackPrivilege(user.UserName, privilege, tableName);
                    }
                }
            }
            else if (privilege == "DELETE")
            {
                //   if (connection.myPrivileges.DeleteIsGrantable) //jeżeli możemy nadawać uprawnienia
                foreach (var user in usersPrivileges)//sprawdzamy uprawnienia każdego użytkownika
                {
                    if (user.Delete && user.fromWho["DELETE"] == userName)//jeżeli użytkownik ma uprawnienie i ma je ode mnie
                    {
                        takeBackPrivilege(user.UserName, privilege, tableName);
                    }
                }
            }
            else if (privilege == "UPDATE")
            {
                //  if (connection.myPrivileges.UpdateIsGrantable) //jeżeli możemy nadawać uprawnienia
                foreach (var user in usersPrivileges)//sprawdzamy uprawnienia każdego użytkownika
                {
                    if (user.Update && user.fromWho["UPDATE"] == userName)//jeżeli użytkownik ma uprawnienie i ma je ode mnie
                    {
                        takeBackPrivilege(user.UserName, privilege, tableName);
                    }
                }
            }
            else if (privilege == "TAKEOVER")
            {
                // if (connection.myPrivileges.UpdateIsGrantable) //jeżeli możemy nadawać uprawnienia
                foreach (var user in usersPrivileges)//sprawdzamy uprawnienia każdego użytkownika
                {
                    if (user.TakeOver && user.fromWho["TAKEOVER"] == userName)//jeżeli użytkownik ma uprawnienie i ma je ode mnie
                    {
                        takeBackPrivilege(user.UserName, privilege, tableName);
                    }
                }
                var myUserNameOver = "'1" + userName + "'@'%'";
                myConnection.Open();
                cmd1.CommandText = string.Format("DELETE FROM uprawnienia.user_privileges WHERE GRANTEE = \"{0}\" AND PRIVILEGE_TYPE = '{2}';",
                myUserNameOver, tableName, privilege); //usuwamy swoje uprawnienie z każdej tabeli, ponieważ dotyczy to przejmowania, które jest globalne
                cmd1.ExecuteReader();
                myConnection.Close();
                return;
            }

            var myUserName = "'" + userName + "'@'%'";
            myConnection.Open();
            cmd1.CommandText = string.Format("DELETE FROM uprawnienia.user_privileges WHERE GRANTEE = \"{0}\" AND TABLE_NAME = '{1}' AND PRIVILEGE_TYPE = '{2}';",
            myUserName, tableName, privilege); //usuwamy swoje uprawnienie
            cmd1.ExecuteReader();
            myConnection.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string login = textBox1.Text;

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {

                if (row.Cells[0].Value.ToString().IndexOf(login) == -1)
                {
                    row.Visible = false;
                }
                else
                {
                    row.Visible = true;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          //  disableAllCheckboxes();
            uncheck();

            if (e.RowIndex >= 0)
            {
                chosenTable = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
         //   disableAllCheckboxes();
            uncheck();

            if (e.RowIndex >= 0)
            {
                chosenUser = dataGridView3.Rows[e.RowIndex].Cells["User"].Value.ToString();
            }
        }
    }
}
