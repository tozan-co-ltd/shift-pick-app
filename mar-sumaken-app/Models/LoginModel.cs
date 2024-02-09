using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class LoginModel
    {
        public class LoginRequestBody
        {
            public string LoginID { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyPassword { get; set; }
            public decimal HandyAppVersion { get; set; }
        }

        public class LoginUserSqlLite
        {
            [PrimaryKey]
            [AutoIncrement]
            [Column("Id")]
            public int Id { get; set; }
            public string CompanyCode { get; set; }
            public string HandyUserCode { get; set; }
            public string CompanyName { get; set; }
            public string HandyUserName { get; set; }
            public int AdministratorFlag { get; set; }
            public int DepoID { get; set; }
            public string DepoCode { get; set; }
            public string DepoName { get; set; }
            public int DefaultHandyPageID { get; set; }
        }
    }
}
