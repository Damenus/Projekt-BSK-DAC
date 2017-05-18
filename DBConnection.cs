using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;

// string cs = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\MyDatabase.mdf;Integrated Security=True;";

namespace WindowsFormsApplication1
{
    public class DBConnection
    {
        public DBConnection()
        {
            ListTabels = new List<String> { };
        }

        public DBConnection(string login, string password)
        {
            DatabaseName = "bsk"; //tabela musi istnieć w bazie danych
            Server = "localhost"; //ip serwera; xampp ->"127.0.0.1"
            Port = "3306"; //może w dockerze jest zmienny port
            Login = login;
            Password = password;
        }
        
        public string Server { get; set;  }

        public string Port { get; set; }

        public string DatabaseName { get; set; }

        public string Password { get; set; }

        public string Login { get; set; }

        public List<String> ListTabels { get; set; }

        public List<Grantee> ListGrantee { get; set; }

        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        //łączenie z bazą danych
        public bool IsConnect()
        {
            bool result = true;
            if (Connection == null)
            {
                string connetionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", Server, Port, DatabaseName, Login, Password);
                connection = new MySqlConnection(connetionString);
                connection.Open();
                result = true;
            }

            return result;
        }

        public void Close()
        {
            
            connection.Close();
        }

        //pobranie listy tabel z bazydanych
        public List<String> GetTablesName()
        {
            List<String> list = new List<string>();

            if (this.IsConnect() == true)
            {
                MySqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = string.Format("SHOW TABLES FROM {0};", this.DatabaseName);

                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    list.Add(myReader.GetString(0));
                }
                myReader.Close();
            }

            //ListTabels = list;
            return list;
        }

        //pobranie listy użytoników z bazydanych
        public List<String> GetUsers()
        {
            List<String> list = new List<string>();

            if (this.IsConnect() == true)
            {
                MySqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = string.Format("select user from mysql.user where Host = 'localhost';", this.DatabaseName);

                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    list.Add(myReader.GetString(0));
                }
                myReader.Close();
            }

            return list;
        }

        //będę pobierał wszystkie dane o uprawnieniach i wrzucał w obiekty
        //SELECT * from information_schema.TABLE_PRIVILEGES
        public List<Grantee> GetTablePrivileges(String tableName)
        {
            List<Grantee> list = new List<Grantee>();

            if (this.IsConnect() == true)
            {
                MySqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = string.Format("SELECT GRANTEE, PRIVILEGE_TYPE, IS_GRANTABLE FROM information_schema.TABLE_PRIVILEGES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}';", this.DatabaseName, tableName);

                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    String userName = myReader.GetString(0);
                    String privilegeType = myReader.GetString(1);
                    String isGrantable = myReader.GetString(2);

                    //czy istnieje już taki
                    if (list.Exists(x => x.UserName == userName))
                    {
                        list.Find(x => x.UserName == userName).SetPrivileges(privilegeType, isGrantable);
                    }
                    else //utworz jak nie istnieje
                    {
                        list.Add(new Grantee(userName, privilegeType, isGrantable));
                    }
                }

                myReader.Close();

            }

            return list;
        }

        public List<Grantee> GetTablePrivilegesAllUsers(String tableName)
        {
            List<Grantee> list = new List<Grantee>();
            var listUser = GetUsers();
            foreach (var user in listUser)
            {
                list.Add(new Grantee(user));
            }

            if (this.IsConnect() == true)
            {
                MySqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = string.Format("SELECT GRANTEE, PRIVILEGE_TYPE, IS_GRANTABLE FROM information_schema.TABLE_PRIVILEGES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}';", this.DatabaseName, tableName);

                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    String userName = myReader.GetString(0).Split('@').First().Trim('\''); // myReader.GetString(0) zwraca 'user'@'localhost' Split('@') zwraca 'damian' Trim damian
                    String privilegeType = myReader.GetString(1);
                    String isGrantable = myReader.GetString(2);

                    //czy istnieje już taki
                    if (list.Exists(x => x.UserName == userName))
                    {
                        list.Find(x => x.UserName == userName).SetPrivileges(privilegeType, isGrantable);
                    }
                    else //utworz jak nie istnieje
                    {
                        list.Add(new Grantee(userName, privilegeType, isGrantable));
                    }
                }

                myReader.Close();

            }

            return list;
        }

        // <------------------------------------ NOTATKI ----------------------------------------------->
        //SELECT * FROM mysql.db WHERE Db = 'bsk'
        //GRANT UPDATE ON bsk.user TO damian@localhost

        //SELECT * FROM `db`
        //show tabels

        //uprawnienia
        //show grants for 'user'@'host'
        /*SELECT GRANTEE, PRIVILEGE_TYPE FROM information_schema.user_privileges;
            SELECT User,Host,Db FROM mysql.db;*/
       
        //taki tam przykłąd
        public void Select(string filename)
        {
            //cmd.CommandText = "SELECT count(*) from tbUser WHERE UserName = @username and password=@password";
            //command.Parameters.Add("@username", txtUserName.Text);
            //command.Parameters.Add("@password", txtPassword.Text);
            //var count = cmd.ExecuteScalar();
            string query = "SELECT * FROM banners WHERE file = '" + filename + "'";

            //open connection
            if (this.IsConnect() == true) // OpenConnection == isConection
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Get the DataReader from the comment using ExecuteReader
                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    //Use GetString etc depending on the column datatypes.
                    Console.WriteLine(myReader.GetInt32(0));
                }

                //close connection
                //this.CloseConnection();
            }
        }

        //rejestracja

        //GRANT SELECT, INSERT ON bsk.ksiazka TO 'Marta'@'localhost';

        //CREATE USER 'Franek'@'%' IDENTIFIED BY 'pass';
        //GRANT ALL ON bsk.* TO 'Franek'@'%';
        //GRANT ALL ON mysql.* TO 'Franek'@'%';

        public bool comparisonTabels(List<string> list)
        {
            bool result = false;

            if (ListTabels == null)
                result = true;
            else
            if (list.Count() != ListTabels.Count())
                result = true;
  
                return result;

        }

        public bool isGranteeListTheSame(List<Grantee> list)
        {
            bool result = false;

            if (ListGrantee != null)
                foreach(var elementNew in list) {    
                    foreach(var elementOld in ListGrantee ) {
                        if (elementNew.UserName == elementOld.UserName)
                            if (!isGranteeElementsTheSame(elementNew, elementOld))
                                result = true;
                    }
                }

            return result;

        }

        public bool isGranteeElementsTheSame(Grantee a, Grantee b)
        {
            bool result = true;

            if (a.Select != b.Select) result = false;
            else if (a.Update != b.Update) result = false;
            else if (a.Delete != b.Delete) result = false;
            else if (a.Insert != b.Insert) result = false;
            else if (a.SelectIsGrantable != b.SelectIsGrantable) result = false;
            else if (a.UpdateIsGrantable != b.UpdateIsGrantable) result = false;
            else if (a.DeleteIsGrantable != b.DeleteIsGrantable) result = false;
            else if (a.InsertIsGrantable != b.InsertIsGrantable) result = false;

            return result;
        }
          

    }
}
