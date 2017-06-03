using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{   //trzymanie informacji o uprawnieniach, by móc łatwo wyświetlić
    public class Grantee
    {
        public Grantee(String name)
        {
            fromWho = new Dictionary<string, string>();
            this.UserName = name;

            this.Select = false;
            this.Update = false;
            this.Delete = false;
            this.Insert = false;
            this.TakeOver = false;

            this.SelectIsGrantable = false;
            this.UpdateIsGrantable = false;
            this.DeleteIsGrantable = false;
            this.InsertIsGrantable = false;
            this.TakeOverIsGrantable = false;

            setFromWhoAsNone();

        }

        public Grantee(String name, String table)
        {
            fromWho = new Dictionary<string, string>();



            this.UserName = name;
            this.TableName = table;

            this.Select = false;
            this.Update = false;
            this.Delete = false;
            this.Insert = false;
            this.TakeOver = false;

            this.SelectIsGrantable = false;
            this.UpdateIsGrantable = false;
            this.DeleteIsGrantable = false;
            this.InsertIsGrantable = false;
            this.TakeOverIsGrantable = false;

            setFromWhoAsNone();

        }

        public Grantee(String name, String privilege, String grantable, String from)
        {
            fromWho = new Dictionary<string, string>();
            this.UserName = name;

            this.Select = false;
            this.Update = false;
            this.Delete = false;
            this.Insert = false;
            this.TakeOver = false;

            this.SelectIsGrantable = false;
            this.UpdateIsGrantable = false;
            this.DeleteIsGrantable = false;
            this.InsertIsGrantable = false;
            this.TakeOverIsGrantable = false;

            setFromWhoAsNone();


            SetPrivileges(privilege, grantable, from);

        }

        public Grantee(String name, String privilege, String grantable, String from, String table)
        {
            fromWho = new Dictionary<string, string>();
            this.UserName = name;
            this.TableName = table;

            this.Select = false;
            this.Update = false;
            this.Delete = false;
            this.Insert = false;
            this.TakeOver = false;

            this.SelectIsGrantable = false;
            this.UpdateIsGrantable = false;
            this.DeleteIsGrantable = false;
            this.InsertIsGrantable = false;
            this.TakeOverIsGrantable = false;

            setFromWhoAsNone();

            SetPrivileges(privilege, grantable, from);

        }

        public String UserName { get; set; }
        public String TableName { get; set; }

        public bool Select { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Insert { get; set; }
        public bool TakeOver { get; set; }

        public bool SelectIsGrantable { get; set; }
        public bool UpdateIsGrantable { get; set; }
        public bool DeleteIsGrantable { get; set; }
        public bool InsertIsGrantable { get; set; }
        public bool TakeOverIsGrantable { get; set; }
        public Dictionary<string, string> fromWho;

        private void setFromWhoAsNone()
        {
             fromWho.Add("SELECT", "none");
             fromWho.Add("DELETE", "none");
             fromWho.Add("INSERT", "none");
             fromWho.Add("UPDATE", "none");
        }

        public void SetPrivileges(String privileges, String grantable, String from)
        {
            //zamian YES/NO na true/false
            bool value = (grantable == "YES") ? true : false;

            //same wiellkie litery
            privileges = privileges.ToUpper();

            if (privileges == "SELECT")
            {
                this.Select = true;
                this.SelectIsGrantable = value;
            }
            else if (privileges == "UPDATE")
            {
                this.Update = true;
                this.UpdateIsGrantable = value;
            }
            else if (privileges == "DELETE")
            {
                this.Delete = true;
                this.DeleteIsGrantable = value;
            }
            else if (privileges == "INSERT")
            {
                this.Insert = true;
                this.InsertIsGrantable = value;
            }
            else if (privileges == "TAKEOVER")
            {
                this.TakeOver = true;
                this.TakeOverIsGrantable = value;
            }
            if (fromWho.ContainsKey(privileges))
                fromWho[privileges] = from;
            else
                fromWho.Add(privileges, from);
        }

        public void SetPrivileges(String privileges, String grantable, String from, String table)
        {

            this.TableName = table;

            //zamian YES/NO na true/false
            bool value = (grantable == "YES") ? true : false;

            //same wiellkie litery
            privileges = privileges.ToUpper();

            if (privileges == "SELECT")
            {
                this.Select = true;
                this.SelectIsGrantable = value;
            }
            else if (privileges == "UPDATE")
            {
                this.Update = true;
                this.UpdateIsGrantable = value;
            }
            else if (privileges == "DELETE")
            {
                this.Delete = true;
                this.DeleteIsGrantable = value;
            }
            else if (privileges == "INSERT")
            {
                this.Insert = true;
                this.InsertIsGrantable = value;
            }
            else if (privileges == "TAKEOVER")
            {
                this.TakeOver = true;
                this.TakeOverIsGrantable = value;
            }
            if (fromWho.ContainsKey(privileges))
                fromWho[privileges] = from;
            else
                fromWho.Add(privileges, from);
        }

        public string wyswietlUprawnienia()
        {
            string literki = String.Empty;

            //SELECT-------------------------------------------------
            if (Select == true) //sprawdzamy czy mamy uprawnienie
            {
                if(SelectIsGrantable == true) //sprawdzamy czy mamy Grant, jesli mamy dajemy duza literke
                {
                    literki += 'S';
                }
                else // nie mamy Grant, dajemy mala literke
                {
                    literki += 's';
                }
            }
            else //nie mamy uprawnienia to dajemy '-'
            {
                literki += '-';
            }

            //UPDATE--------------------------------------------------
            if (Update == true) //sprawdzamy czy mamy uprawnienie
            {
                if (UpdateIsGrantable == true) //sprawdzamy czy mamy Grant, jesli mamy dajemy duza literke
                {
                    literki += 'U';
                }
                else // nie mamy Grant, dajemy mala literke
                {
                    literki += 'u';
                }
            }
            else //nie mamy uprawnienia to dajemy '-'
            {
                literki += '-';
            }

            //INSERT---------------------------------------------------
            if (Insert == true) //sprawdzamy czy mamy uprawnienie
            {
                if (InsertIsGrantable == true) //sprawdzamy czy mamy Grant, jesli mamy dajemy duza literke
                {
                    literki += 'I';
                }
                else // nie mamy Grant, dajemy mala literke
                {
                    literki += 'i';
                }
            }
            else //nie mamy uprawnienia to dajemy '-'
            {
                literki += '-';
            }

            //DELETE-------------------------------------------------
            if (Delete == true) //sprawdzamy czy mamy uprawnienie
            {
                if (DeleteIsGrantable == true) //sprawdzamy czy mamy Grant, jesli mamy dajemy duza literke
                {
                    literki += 'D';
                }
                else // nie mamy Grant, dajemy mala literke
                {
                    literki += 'd';
                }
            }
            else //nie mamy uprawnienia to dajemy '-'
            {
                literki += '-';
            }

            return literki;
        }

    }
}
