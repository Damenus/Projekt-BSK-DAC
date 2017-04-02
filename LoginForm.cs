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
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //sprawdzenie czy jest login i hasło ?sprawdzenie czy nie ma SQLinjesction
            if (textBoxLogin.Text == "" || textBoxPassword.Text == "")
            {
                MessageBox.Show("Please provide UserName and Password");
                return;
            }
            //moja klasa do łączenia się z bazą danych
            DBConnection com;

            try
            {
                //?skrót hasła
                //SHA1 sha = new SHA1CryptoServiceProvider();
                //var result = sha.ComputeHash();

                //stworzenie obiektu i łączenie się z bazą
                com = new DBConnection(textBoxLogin.Text, textBoxPassword.Text);
                com.IsConnect();

                //ukrycie ekranu logowania i pokazanie głównego menu
                this.Hide();
                MessageBox.Show("Login Successful!");
                Form1 form = new Form1();
                form.Show();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
