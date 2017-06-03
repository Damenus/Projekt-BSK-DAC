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

                //dataGridView3.ClearSelection();
                //dataGridView2.ClearSelection();
                //dataGridView1.ClearSelection();
                chosenTable = dataGridView1.Rows[1].Cells["TableID"].Value.ToString();
                chosenUser = dataGridView3.Rows[1].Cells["User"].Value.ToString();

                disableAllCheckboxes();

                task = Task.Run(() => chceckIfChange());
                dataGridView2.AllowUserToAddRows = false;
            }
        }

        private void prepareDataGridView2()
        {
            //dataGridView2.Columns.Clear(); czyszczenie i wstawianie od nowa


            var tables = connection.ListTabels;
            kolumny = new ArrayList();

            foreach (var nameTable in tables)
            {

                var newTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
                kolumny.Add(newTable);

                newTable.HeaderText = nameTable;
                newTable.Name = nameTable;
                newTable.ReadOnly = true;
                newTable.Width = 120;

                this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { newTable });
            }
            // 
            // TakeOverIsGrantable
            // 
            var TakeOverIsGrantable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            kolumny.Add(TakeOverIsGrantable);
            TakeOverIsGrantable.HeaderText = "Przejmij";
            TakeOverIsGrantable.Name = "Przejmij";
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
                try
                {
                    int columnIndex = dataGridView2.Columns[g.TableName].Index;
                    dataGridView2.Rows[rowID].Cells[columnIndex].Value = g.wyswietlUprawnienia();
                }
                catch
                {

                }

                if (g.TakeOver && g.TakeOverIsGrantable)
                    dataGridView2.Rows[rowID].Cells[connection.ListTabels.Count + 1].Value = "Przejmij";
                else
                {
                    dataGridView2.Rows[rowID].Cells[connection.ListTabels.Count + 1].Value = "-";
                }
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
                    foreach (var table in connection.ListTabels)
                    {
                        myConnection.Open();
                        cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'NO', '{3}');", granteeName, table, privilage, connection.Login);
                        cmd.ExecuteReader();
                        myConnection.Close();
                    }
                else
                    foreach (var table in connection.ListTabels)
                    {
                        myConnection.Open();
                        cmd.CommandText = string.Format("INSERT INTO uprawnienia.user_privileges VALUES(\"{0}\", '{1}', '{2}', 'YES', '{3}');", granteeName, table, privilage, connection.Login);
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

        private void giveAllPrivileges() //oddaje wszystkie uprawnienia i odbiera wszystkim, którzy byli pod spodem
        {
            var userName = "'" + chosenUser + "'@'%'";
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

            foreach (var tableName in connection.ListTabels)
            {
                takeBackPrivilege(connection.Login, "SELECT", tableName);
                takeBackPrivilege(connection.Login, "INSERT", tableName);
                takeBackPrivilege(connection.Login, "DELETE", tableName);
                takeBackPrivilege(connection.Login, "UPDATE", tableName);
                takeBackPrivilege(connection.Login, "TAKEOVER", tableName);
            }
        }
        private bool checkIfParent(string currentUser, string wantedUser) //sprawdź, czy zaznaczony użytkownik jest przodkiem; zwraca true, kiedy znaleziono przodka
        {
            bool isParent = false;
            Grantee userPrivileges;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                userPrivileges = connection.getUserPrivileges(row.Cells[0].Value.ToString(), currentUser);
                if (userPrivileges.fromWho.Any(u => (u.Value == wantedUser)))//szukanie bezpośredniego przodka
                {
                    return true;
                }
                else //szukanie dalszego przodka
                {
                    foreach (var p in userPrivileges.fromWho)
                    {
                        if (p.Value != "none")
                            isParent = checkIfParent(p.Value, wantedUser);
                        if (isParent)
                        {
                            return isParent;
                        }
                    }
                }
            }
            return isParent;
        }

        private void takeOver()
        {
            string userName = chosenUser;

            foreach (var tableName in connection.ListTabels)
            {
                Grantee user = connection.ListGrantee.Find(x => (x.UserName == userName) && (x.TableName == tableName)); ; //szukamy uzytkownika, któremu usuwamy uprawnienia

                if (user.Select)
                {
                    takeBackPrivilege(user.UserName, "SELECT", tableName);
                }
                if (user.Insert)
                {
                    takeBackPrivilege(user.UserName, "INSERT", tableName);
                }
                if (user.Delete)
                {
                    takeBackPrivilege(user.UserName, "DELETE", tableName);
                }
                if (user.Update)
                {
                    takeBackPrivilege(user.UserName, "UPDATE", tableName);
                }
                if (user.TakeOver)
                {
                    takeBackPrivilege(user.UserName, "TAKEOVER", tableName);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (chosenTable != null && chosenUser != null)
            {
                var mojLogin = connection.Login;
                var uprawnieniaPrzekazywanemu = connection.ListGrantee.Find(x => (x.UserName == chosenUser) && (x.TableName == chosenTable));
                if (uprawnieniaPrzekazywanemu.TakeOver)
                {
                    takeOver(); //usuwanie uprawnien przejmującego
                    giveAllPrivileges(); //oddanie uprawnień 
                    return;
                }

                var mojeUprawnienia = connection.ListGrantee.Find(x => (x.UserName == connection.Login) && (x.TableName == chosenTable));

                bool giveGrantable = false;
                if (checkBox1.Checked && mojeUprawnienia.Insert.ToString() == "True")
                {
                    if (checkBox5.Checked && mojeUprawnienia.InsertIsGrantable.ToString() == "True")
                        giveGrantable = true;
                    else
                        giveGrantable = false;
                    grant("INSERT", giveGrantable);
                }
                if (checkBox2.Checked && mojeUprawnienia.Delete.ToString() == "True")
                {
                    if (checkBox6.Checked && mojeUprawnienia.DeleteIsGrantable.ToString() == "True")
                        giveGrantable = true;
                    else
                        giveGrantable = false;
                    grant("DELETE", giveGrantable);
                }
                if (checkBox3.Checked && mojeUprawnienia.Update.ToString() == "True")
                {
                    if (checkBox7.Checked && mojeUprawnienia.UpdateIsGrantable.ToString() == "True")
                        giveGrantable = true;
                    else
                        giveGrantable = false;
                    grant("UPDATE", giveGrantable);
                }
                if (checkBox4.Checked && mojeUprawnienia.Select.ToString() == "True")
                {
                    if (checkBox8.Checked && mojeUprawnienia.SelectIsGrantable.ToString() == "True")
                        giveGrantable = true;
                    else
                        giveGrantable = false;
                    grant("SELECT", giveGrantable);
                }
                if (checkBox9.Checked && mojeUprawnienia.TakeOver.ToString() == "True")
                {
                    if (checkBox10.Checked && mojeUprawnienia.TakeOverIsGrantable.ToString() == "True")
                    {
                        giveGrantable = true;
                    }
                    else
                        giveGrantable = false;
                    grant("TAKEOVER", giveGrantable);
                }
                //Zmiana Damiana-----------------------------------
                if (!checkBox4.Checked && uprawnieniaPrzekazywanemu.fromWho["SELECT"] == mojLogin)
                {
                    takeBackPrivilege(chosenUser, "SELECT", chosenTable);
                }
                if (!checkBox3.Checked && uprawnieniaPrzekazywanemu.fromWho["UPDATE"] == mojLogin)
                {
                    takeBackPrivilege(chosenUser, "UPDATE", chosenTable);
                }
                if (!checkBox1.Checked && uprawnieniaPrzekazywanemu.fromWho["INSERT"] == mojLogin)
                {
                    takeBackPrivilege(chosenUser, "INSERT", chosenTable);
                }
                if (!checkBox2.Checked && uprawnieniaPrzekazywanemu.fromWho["DELETE"] == mojLogin)
                {
                    takeBackPrivilege(chosenUser, "DELETE", chosenTable);
                }
                //----------------------------------------------
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
        //private void disableChceckboxes()
        //{
        //    DataGridViewRow row = dataGridView2.CurrentRow;
        //    var mojeUprawnienia = connection.ListGrantee.Find(x => (x.UserName == connection.Login) && (x.TableName == chosenTable));

        //    if (mojeUprawnienia.Insert.ToString() == "True" &&
        //        row.Cells[3].Value.ToString() == "False" && mojeUprawnienia.InsertIsGrantable.ToString() == "True")
        //    {
        //        checkBox1.Enabled = true;
        //        checkBox5.Enabled = true;
        //    }
        //    else
        //    {
        //        checkBox1.Enabled = false;
        //        checkBox5.Enabled = false;
        //    }
        //    if (mojeUprawnienia.Delete.ToString() == "True" &&
        //        row.Cells[5].Value.ToString() == "False" && mojeUprawnienia.DeleteIsGrantable.ToString() == "True")
        //    {
        //        checkBox2.Enabled = true;
        //        checkBox6.Enabled = true;
        //    }
        //    else
        //    {
        //        checkBox2.Enabled = false;
        //        checkBox6.Enabled = false;
        //    }
        //    if (mojeUprawnienia.Update.ToString() == "True" &&
        //        row.Cells[7].Value.ToString() == "False" && mojeUprawnienia.UpdateIsGrantable.ToString() == "True")
        //    {
        //        checkBox3.Enabled = true;
        //        checkBox7.Enabled = true;
        //    }
        //    else
        //    {
        //        checkBox3.Enabled = false;
        //        checkBox7.Enabled = false;
        //    }
        //    if (mojeUprawnienia.Select.ToString() == "True" &&
        //        row.Cells[1].Value.ToString() == "False" && mojeUprawnienia.SelectIsGrantable.ToString() == "True")
        //    {
        //        checkBox4.Enabled = true;
        //        checkBox8.Enabled = true;
        //    }
        //    else
        //    {
        //        checkBox4.Enabled = false;
        //        checkBox8.Enabled = false;
        //    }
        //    if (mojeUprawnienia.TakeOver.ToString() == "True" &&
        //        row.Cells[9].Value.ToString() == "False" && mojeUprawnienia.TakeOverIsGrantable.ToString() == "True")
        //    {
        //        checkBox9.Enabled = true;
        //        checkBox10.Enabled = true;
        //    }
        //    else
        //    {
        //        checkBox9.Enabled = false;
        //        checkBox10.Enabled = false;
        //    }
        //    uncheck();
        //}
        //kliknięcie wybranej tabeli, powoduje pojwenie się uprawnień użytkowników
        private void disableCheckboxes()
        {
                        
            connection.myPrivileges = connection.getUserPrivileges(dataGridView1.CurrentRow.Cells[0].Value.ToString(), connection.Login);
            String tableName = dataGridView1.CurrentRow.Cells[0].Value.ToString(), userName = dataGridView3.CurrentRow.Cells[0].Value.ToString();
            int columnIndex = dataGridView2.Columns[tableName].Index;
            int takeOverColumn = dataGridView2.Columns["Przejmij"].Index;

            String searchValue = userName;
            int rowIndex = -1;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells[0].Value.ToString().Equals(searchValue))
                {
                    rowIndex = row.Index;
                    break;
                }
            }

            var uprawnieniaZaznaczonego = connection.ListGrantee.Find(x => (x.UserName == chosenUser) && (x.TableName == chosenTable));
            var mojLogin = connection.Login;

            String userPrivileges = dataGridView2.Rows[rowIndex].Cells[columnIndex].Value.ToString();
            if (userName == connection.Login || checkIfParent(connection.Login, userName))
                disableAllCheckboxes();
            else
            {
                if (connection.myPrivileges.SelectIsGrantable && userPrivileges[0] == '-')
                {
                    checkBox4.Enabled = true;
                    checkBox8.Enabled = true;
                }
                else
                {
                    checkBox4.Enabled = false;
                    checkBox8.Enabled = false;
                }
                //warunek od Damian na usuwanie--------------------
                if (uprawnieniaZaznaczonego.fromWho["SELECT"] == mojLogin && userPrivileges[0] == 's')
                {
                    checkBox4.Enabled = true;
                    checkBox4.Checked = true;

                    checkBox8.Enabled = true;
                }
                else if (uprawnieniaZaznaczonego.fromWho["SELECT"] == mojLogin && userPrivileges[0] == 'S')
                {
                    checkBox4.Enabled = true;
                    checkBox4.Checked = true;

                    checkBox8.Enabled = true;
                    checkBox8.Checked = true;
                }
                //--------------------------------------
                if (connection.myPrivileges.UpdateIsGrantable && userPrivileges[1] == '-')
                {
                    checkBox3.Enabled = true;
                    checkBox7.Enabled = true;
                }
                else
                {
                    checkBox3.Enabled = false;
                    checkBox7.Enabled = false;
                }
                //warunek od Damian na usuwanie--------------------
                if (uprawnieniaZaznaczonego.fromWho["UPDATE"] == mojLogin && userPrivileges[1] == 'u')
                {
                    checkBox3.Enabled = true;
                    checkBox3.Checked = true;

                    checkBox7.Enabled = true;
                }
                else if (uprawnieniaZaznaczonego.fromWho["UPDATE"] == mojLogin && userPrivileges[1] == 'U')
                {
                    checkBox3.Enabled = true;
                    checkBox3.Checked = true;

                    checkBox7.Enabled = true;
                    checkBox7.Checked = true;
                }
                //--------------------------------------
                if (connection.myPrivileges.InsertIsGrantable && userPrivileges[2] == '-')
                {
                    checkBox1.Enabled = true;
                    checkBox5.Enabled = true;
                }
                else
                {
                    checkBox1.Enabled = false;
                    checkBox5.Enabled = false;
                }
                //warunek od Damian na usuwanie--------------------
                if (uprawnieniaZaznaczonego.fromWho["INSERT"] == mojLogin && userPrivileges[2] == 'i')
                {
                    checkBox1.Enabled = true;
                    checkBox1.Checked = true;

                    checkBox5.Enabled = true;
                }
                else if (uprawnieniaZaznaczonego.fromWho["INSERT"] == mojLogin && userPrivileges[2] == 'I')
                {
                    checkBox1.Enabled = true;
                    checkBox1.Checked = true;

                    checkBox5.Enabled = true;
                    checkBox5.Checked = true;
                }
                //--------------------------------------
                if (connection.myPrivileges.DeleteIsGrantable && userPrivileges[3] == '-')
                {
                    checkBox2.Enabled = true;
                    checkBox6.Enabled = true;
                }
                else
                {
                    checkBox2.Enabled = false;
                    checkBox6.Enabled = false;
                }
                //warunek od Damian na usuwanie--------------------
                if (uprawnieniaZaznaczonego.fromWho["DELETE"] == mojLogin && userPrivileges[3] == 'd')
                {
                    checkBox2.Enabled = true;
                    checkBox2.Checked = true;

                    checkBox6.Enabled = true;
                }
                else if (uprawnieniaZaznaczonego.fromWho["DELETE"] == mojLogin && userPrivileges[3] == 'D')
                {
                    checkBox2.Enabled = true;
                    checkBox2.Checked = true;

                    checkBox6.Enabled = true;
                    checkBox6.Checked = true;
                }
                //--------------------------------------
                if (dataGridView2.Rows[rowIndex].Cells[takeOverColumn].Value.ToString() == "-" && connection.myPrivileges.TakeOverIsGrantable)
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
        }

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
                disableCheckboxes();
                chosenTable = dataGridView1.Rows[e.RowIndex].Cells["TableID"].Value.ToString();
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            //   disableAllCheckboxes();
            uncheck();

            if (e.RowIndex >= 0)
            {
                disableCheckboxes();
                chosenUser = dataGridView3.Rows[e.RowIndex].Cells["User"].Value.ToString();
            }
        }
    }
}
