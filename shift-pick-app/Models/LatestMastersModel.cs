using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class LatestMastersModel
    {
        public class LatestMastersPostBody
        {
            /// <summary>
            /// 会社コード
            /// </summary>
            public string CompanyCode { get; set; } = string.Empty;

            /// <summary>
            /// 会社パスワード
            /// </summary>
            public string CompanyPassword { get; set; } = string.Empty;

            /// <summary>
            /// ユーザーID
            /// </summary>
            public int UserID { get; set; }
        }

        public class LatestMastersPostBackContent
        {
        }
    }
}
