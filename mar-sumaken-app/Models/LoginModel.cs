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
        /// <summary>
        /// ログインリクエスト
        /// </summary>
        public class LoginRequestBody
        {
            public string LoginID { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyPassword { get; set; }
            public decimal HandyAppVersion { get; set; }
        }

        /// <summary>
        /// ログインユーザー Sqlite
        /// </summary>
        public class LoginUserSqlLite
        {
            [PrimaryKey]
            [AutoIncrement]
            [Column("Id")]
            public int Id { get; set; }
            public int CompanyID { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyPassword { get; set; }
            public string CompanyName { get; set; }
            public string LoginID { get; set; }
            public string UserName { get; set; }
            public int DepoID { get; set; }
            public string DepoCode { get; set; }
            public string DepoName { get; set; }
        }

        /// <summary>
        /// ログイン応答API
        /// </summary>
        public class LoginResponseBody
        {
            public string CompanyName { get; set; }
            public M_UserModel M_User { get; set; }
            public M_DepoModel M_Depo { get; set; }
            public List<M_HandyMenuModel> M_HandyMenuList { get; set; }
        }
    }
}
