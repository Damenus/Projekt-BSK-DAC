using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace WindowsFormsApplication1
{
    public partial class LoginForm : Form
    {

        public DBConnection connection { get; set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //sprawdzenie czy jest login i hasło ?sprawdzenie czy nie ma SQLinjesction
            //if (textBoxLogin.Text == "" || textBoxPassword.Text == "")
            //{
            //    MessageBox.Show("Please provide UserName and Password");
            //    return;
            //}
            //moja klasa do łączenia się z bazą danych
            try
            {
                //?skrót hasła
                //SHA1 sha = new SHA1CryptoServiceProvider();
                //var result = sha.ComputeHash();

                //stworzenie obiektu i łączenie się z bazą
                connection = new DBConnection(textBoxLogin.Text, textBoxPassword.Text);
                connection.IsConnect();

                //ukrycie ekranu logowania i pokazanie głównego menu
                this.Hide();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
