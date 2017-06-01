using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Authentication;
using MySql.Data.Common;
using MySql.Data.Types;
using System.Windows.Forms;
using System.Data;
using System.IO;

// string cs = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\MyDatabase.mdf;Integrated Security=True;";

namespace WindowsFormsApplication1
{
    public class DBConnection
    {
        public DBConnection(string login, string password)
        {
            readConfiguration();

            DatabaseName = "bsk"; //tabela musi istnieć w bazie danych
            //Server = "localhost"; //ip serwera; xampp ->"127.0.0.1"
            //Port = "3306";
            //Server = "192.168.99.100"; //ip serwera; xampp ->"127.0.0.1"
            Login = login;
            Password = password;
        }
        
        public string Server { get; set;  }
        public string Port { get; set; }
        public string DatabaseName { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public bool SSL { get; set; }

        public MySqlConnection connection { get; set; }

        public Grantee myPrivileges;
        public List<String> ListTabels { get; set; }
        public List<String> ListUsers { get; set; }
        public List<Grantee> ListGrantee { get; set; }      

        void readConfiguration()
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("config.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    getConfiguration(line);
                    Console.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        void getConfiguration(String line)
        {
            line.Trim();
            var options = line.Split(';');

            foreach(var opt in options)
            {
                setConfiguration(opt);              
            }
        }

        void setConfiguration(String opt)
        {
              var value = opt.Split('=');

                switch (value[0])
                {
                    case "Server":
                        this.Server = value[1];
                        break;
                    case "Port":
                        this.Port = value[1];
                        break;
                    case "SSL":
                        this.SSL = (Int16.Parse(value[1]) == 1)? true : false;
                        break;
                    default:
                        Console.WriteLine("The attribute not allowed: " + value[0]);
                        break;
                }
        }

        //łączenie z bazą danych
        public void Connect()
        {           
            if (connection == null)
            {
                string connetionString;

                if(SSL == true) //Z SSL
                    connetionString = string.Format("Server={0}; Port={1}; database={2}; user={3}; password={4}; CertificateFile=client.pfx; CertificatePassword=pass; SSL Mode=Required; ", Server, Port, DatabaseName, Login, Password);
                else //Bez SSL
                    connetionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", Server, Port, DatabaseName, Login, Password);

                connection = new MySqlConnection(connetionString);
                connection.Open();

                this.ListTabels = GetTablesName();
                this.ListUsers = GetUsers();
                this.ListGrantee = GetTablePrivilegesAllUsersAllTabel();
            } 
        }

        public void Close()
        {            
            connection.Close();
        }

        //pobranie listy tabel z bazydanych
        public List<String> GetTablesName()
        {
            List<String> list = new List<string>();

            try
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
            catch (Exception e)
            {
                Console.WriteLine("Nie pobrano listy tabel!!!");
            }

            //ListTabels = list;
            return list;
        }

        //pobranie listy użytoników z bazydanych
        public List<String> GetUsers()
        {
            List<String> list = new List<string>();

            try
            {
                MySqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = string.Format("select user from mysql.user where Host = '%';", this.DatabaseName); //localhost

                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    list.Add(myReader.GetString(0));
                }
                myReader.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Nie pobrano listy uzytkownikow!!!");
            }

            return list;
        }

        //będę pobierał wszystkie dane o uprawnieniach i wrzucał w obiekty
        //SELECT * from information_schema.TABLE_PRIVILEGES
    //    public List<Grantee> GetTablePrivileges(String tableName)
    //    {
    //        List<Grantee> list = new List<Grantee>();

    ////        if (this.IsConnect() == true)
    // //       {
    //            MySqlCommand cmd = connection.CreateCommand();

    //            cmd.CommandText = string.Format("SELECT GRANTEE, PRIVILEGE_TYPE, IS_GRANTABLE FROM uprawnienia.user_privileges;");

    //            MySqlDataReader myReader = cmd.ExecuteReader();
    //            while (myReader.Read())
    //            {
    //                String userName = myReader.GetString(0);
    //                String privilegeType = myReader.GetString(1);
    //                String isGrantable = myReader.GetString(2);

    //                //czy istnieje już taki
    //                if (list.Exists(x => x.UserName == userName))
    //                {
    //                    list.Find(x => x.UserName == userName).SetPrivileges(privilegeType, isGrantable);
    //                }
    //                else //utworz jak nie istnieje
    //                {
    //                    list.Add(new Grantee(userName, privilegeType, isGrantable));
    //                }
    //            }

    //            myReader.Close();

    ////        }

    //        return list;
    //    }

        public List<Grantee> GetTablePrivilegesAllUsers(String tableName)
        {
            List<Grantee> list = new List<Grantee>();
            var listUser = GetUsers();
            foreach (var user in listUser)
            {
                list.Add(new Grantee(user));
            }

       //     if (this.IsConnect() == true)
        //    {

                string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", Server, Port, DatabaseName, Login, Password);
                MySqlConnection myConnection = new MySqlConnection(connectionString);
                myConnection.Open();
                MySqlCommand cmd = myConnection.CreateCommand();

                cmd.CommandText = string.Format("SELECT GRANTEE, PRIVILEGE_TYPE, IS_GRANTABLE, RECEIVED_FROM FROM uprawnienia.user_privileges WHERE TABLE_NAME = '{0}';", tableName);

                //  try
                //    {
                MySqlDataReader myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    String userName = myReader.GetString(0).Split('@').First().Trim('\''); // myReader.GetString(0) zwraca 'user'@'localhost' Split('@') zwraca 'damian' Trim damian
                    String privilegeType = myReader.GetString(1);
                    String isGrantable = myReader.GetString(2);
                    String table = tableName;//myReader.GetString(1);
                    String from = myReader.GetString(3);

                    //czy istnieje już taki
                    if (list.Exists(x => x.UserName == userName))
                    {
                        list.Find(x => x.UserName == userName).SetPrivileges(privilegeType, isGrantable, from);
                    }
                    else //utworz jak nie istnieje
                    {
                        list.Add(new Grantee(userName, privilegeType, isGrantable, from));
                    }
            }

                myReader.Close();
                //}
                //catch(Exception e)
                //{

                //}
                myConnection.Close();

            

            return list;
        }

        public List<Grantee> GetTablePrivilegesAllUsersAllTabel()
        {
            List<Grantee> list = new List<Grantee>();            

            foreach (var user in ListUsers)
            {
                foreach (var table in ListTabels)
                {
                    list.Add(new Grantee(user,table));
                }
            }

            string connectionString = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4};", Server, Port, DatabaseName, Login, Password);
            MySqlConnection myConnection = new MySqlConnection(connectionString);
            myConnection.Open();

            MySqlCommand cmd = myConnection.CreateCommand();
            cmd.CommandText = string.Format("SELECT GRANTEE, PRIVILEGE_TYPE, IS_GRANTABLE, RECEIVED_FROM, TABLE_NAME FROM uprawnienia.user_privileges;");

            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                String userName = myReader.GetString(0).Split('@').First().Trim('\''); // myReader.GetString(0) zwraca 'user'@'localhost' Split('@') zwraca 'damian' Trim damian
                String privilegeType = myReader.GetString(1);
                String isGrantable = myReader.GetString(2);
                String table = myReader.GetString(4).ToLower();
                String from = myReader.GetString(3);

                //czy istnieje już taki
                if (list.Exists(x => (x.UserName == userName) && (x.TableName == table)))
                {
                    list.Find(x => (x.UserName == userName) && (x.TableName == table)).SetPrivileges(privilegeType, isGrantable, from, table);
                }
                else //utworz jak nie istnieje
                {
                    list.Add(new Grantee(userName, privilegeType, isGrantable, from, table));
                }
            }
            myReader.Close();

            myConnection.Close();

            //ListGrantee = list;

            return list;
        }

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
                        if (elementNew.UserName == elementOld.UserName && elementNew.TableName == elementOld.TableName)
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

            else if (a.TakeOver != b.TakeOver) result = false;
            else if (a.TakeOverIsGrantable != b.TakeOverIsGrantable) result = false;

          //  else if (a.TableName != b.TableName) result = false;
          //  else if (a.UserName != b.UserName) result = false;

            return result;
        }
          

    }
}
