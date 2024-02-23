using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_HandyErrorMessageModel
    {
        // 倉庫ID
        public int DepoID { get; set; }

        // ハンディメニューID
        public int HandyMenuID { get; set; }

        // スキャン文字列1
        public string FirstScanedString { get; set; }

        // スキャン文字列2
        public string SecondScanedString { get; set; }

        // エラーメッセージ
        public string ErrorMessage { get; set; }

        // 作成日時
        public DateTime CreatedAt { get; set; }

        // 作成者
        public string CreatedBy { get; set; }

        // 解除日時
        public DateTime UnlockedAt { get; set; }

        // 解除ユーザーID
        public string UnlockedBy { get; set; }
    }
}
