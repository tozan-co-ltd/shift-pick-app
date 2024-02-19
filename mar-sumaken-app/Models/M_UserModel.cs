using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class M_UserModel
    {
        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// ログインID
        /// </summary>
        public string LoginID { get; set; } = string.Empty;

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// メイン倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 管理者権限区分
        /// </summary>
        public int AuthorizedKubun { get; set; }
    }
}
