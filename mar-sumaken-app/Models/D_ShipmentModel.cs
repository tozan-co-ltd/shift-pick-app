using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_ShipmentModel
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("ShipmentID")]
        /// <summary>
        /// 出荷実績ID
        /// </summary>
        public int ShipmentID { get; set; }
        /// <summary>
        /// 出荷指示ID
        /// </summary>
        
        public int ShipmentScheduleID { get; set; }
        /// <summary>
        /// 読取実績ID
        /// </summary>
        
        public int ScanResultID { get; set; }
        /// <summary>
        /// 出荷日時
        /// </summary>
        
        public DateTime ShipmentDatetime { get; set; } = DateTime.Now;
        /// <summary>
        /// シリアル番号(かんばん識別番号)
        /// </summary>

        public int KanbanSerialNumber { get; set; }
        /// <summary>
        /// 仕入先品番
        /// </summary>
        
        public string SupplierProductNumber { get; set; }
        /// <summary>
        /// 納入先品番
        /// </summary>
        public string DeliveryProductNumber { get; set; }
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

        public int NumberOfBoxes { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        
        public int Quantity { get; set; }

        /// <summary>
        /// 読取日時
        /// </summary>
        public DateTime ScanedAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }
    }
}
