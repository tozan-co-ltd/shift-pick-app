using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_ReceiveModel
    {
        /// <summary>
        /// 入荷実績ID
        /// </summary>
        public int ReceiveID { get; set; }

        /// <summary>
        /// 読取実績ID
        /// </summary>
        public int ScanResultID { get; set; }

        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// 入荷日時
        /// </summary>
        public DateTime ReceiveDatetime { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string SupplierProductNumber { get; set; }

        /// <summary>
        /// ロット番号
        /// </summary>
        public string LotNumber { get; set; } = string.Empty;

        /// <summary>
        /// メインキー
        /// </summary>
        public string MainProductKey { get; set; } = string.Empty;

        /// <summary>
        /// サブキー1
        /// </summary>
        public string FirstSubProductKey { get; set; } = string.Empty;

        /// <summary>
        /// サブキー2
        /// </summary>
        public string SecondSubProductKey { get; set; } = string.Empty;

        /// <summary>
        /// 箱数
        /// </summary>
        public int NumberOfBoxes { get; set; } = 1;

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

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

