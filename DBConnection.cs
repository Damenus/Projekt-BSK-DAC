using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

// string cs = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\MyDatabase.mdf;Integrated Security=True;";

namespace WindowsFormsApplication1
{
    class DBConnection
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
    }
}
