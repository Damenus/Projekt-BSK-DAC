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
            DatabaseName = "bsk";
            Server = "127.0.0.1";
            Login = "damian";
            Password = "asd";
        }

        public DBConnection(string login, string password)
        {
            DatabaseName = "bsk";
            Server = "127.0.0.1";
            Login = login;
            Password = password;
        }

        public string Server { get; set;  }

        public string DatabaseName { get; set; }

        public string Password { get; set; }

        public string Login { get; set; }

        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        //nie wiem czy zasotawić
        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        //łączenie z bazą danych
        public bool IsConnect()
        {
            bool result = true;
            if (Connection == null)
            {               
                string connetionString = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, Login, Password);
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

        //SELECT * FROM mysql.db WHERE Db = 'bsk'
        //GRANT UPDATE ON bsk.user TO damian@localhost

        //SELECT * FROM `db`
        //show tabels

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
    }
}
