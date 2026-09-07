using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class M_CompanyModel
    {
        // 会社ID
        public int CompanyID { get; set; }

        // 会社コード
        public string CompanyCode { get; set; } = string.Empty;

        // 会社名
        public string CompanyName { get; set; } = string.Empty;

        // データベース名
        public string DatabaseName { get; set; } = string.Empty;

        // ハンディアプリの最小バージョン
        public decimal HandyAppMinVersion { get; set; }

        public List<M_CompanyModel> mCompanyList { get; set; }

        public class M_CompanyPostBody
        {
            /// <summary>
            /// 会社コード
            /// </summary>
            public string CompanyCode { get; set; }

            /// <summary>
            /// 会社パスワード
            /// </summary>
            public string CompanyPassword { get; set; }
        }
    }
}
