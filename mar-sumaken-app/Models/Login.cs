using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class Login
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
            public string CompanyCode { get; set; }
            public string CompanyName { get; set; }
            public string LoginID { get; set; }
            public string UserName { get; set; }
        }

        /// <summary>
        /// ログイン応答API
        /// </summary>
        public class LoginResponseApi
        {
            public string CompanyName { get; set; }
            public User User { get; set; }
            public Depo Depo { get; set; }
            public HandyMenu[] HandyMenu { get; set; }
        }

        /// <summary>
        /// ユーザー
        /// </summary>
        public class User
        {
            public int UserID { get; set; }
            public string LoginID { get; set; }
            public string UserName { get; set; }
            public int DepoID { get; set; }
            public int AuthorizedKubun { get; set; }
        }
    }
}
