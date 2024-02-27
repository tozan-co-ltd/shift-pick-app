using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_HandyErrorMessageModel
    {
        /// <summary>
        /// ハンディエラーメッセージ実績ID
        /// </summary>
        public int HandyErrorMessageID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public int HandyMenuID { get; set; }

        /// <summary>
        /// スキャン文字列1
        /// </summary>
        public string FirstScanedString { get; set; }

        /// <summary>
        /// スキャン文字列2
        /// </summary>
        public string SecondScanedString { get; set; } = string.Empty;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 解除日時
        /// </summary>
        public DateTime? UnlockedAt { get; set; } = null;

        /// <summary>
        /// 解除ユーザーID
        /// </summary>
        public string UnlockedBy { get; set; } = string.Empty;
    }
}
