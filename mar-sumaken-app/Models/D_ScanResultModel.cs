using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_ScanResultModel
    {
        /// <summary>
        /// 読取実績ID
        /// </summary>
        public int ScanResultID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public int HandyMenuID { get; set; }

        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        public int SupplierKanbanID { get; set; }

        /// <summary>
        /// 入力箱数
        /// </summary>
        public int NumberOfInputBoxes { get; set; }

        /// <summary>
        /// スキャン文字列1
        /// </summary>
        public string FirstScanedString { get; set; }

        /// <summary>
        /// スキャン文字列2
        /// </summary>
        public string SecondScanedString { get; set; } = string.Empty;

        /// <summary>
        /// 読取日時
        /// </summary>
        public DateTime ScanedAt { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }
    }
}
