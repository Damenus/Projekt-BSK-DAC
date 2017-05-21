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

        }

        public Grantee(String name, String privilege, String grantable)
        {
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
            

            SetPrivileges(privilege, grantable);

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
        public void SetPrivileges(String privileges, String grantable)
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
            else if(privileges =="TAKEOVER")
            {
                this.TakeOver = true;
                this.TakeOverIsGrantable = value;
            }
        }

    }
}
