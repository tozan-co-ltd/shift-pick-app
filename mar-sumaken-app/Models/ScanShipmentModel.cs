using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class ScanShipmentModel
    {
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
        public string SecondScanedString { get; set; }
        /// <summary>
        /// 読取日時
        /// </summary>
        public DateTime ScanedAt { get; set; }
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
        public DateTime ShipmentDatetime { get; set; }
        /// <summary>
        /// シリアル番号(かんばん識別番号)	
        /// </summary>
        public int KanbanSerialNumber { get; set; }
        /// <summary>
        /// 会社ID:仕入先の会社ID
        /// </summary>
        public int CompanyID { get; set; }
        /// <summary>
        /// 納入指示日
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        /// <summary>
        /// 便
        /// </summary>
        public int DeliveryTimeClass { get; set; }
        /// <summary>
        /// 納品書番号
        /// </summary>
        public string DeliverySlipNumber { get; set; }
        /// <summary>
        /// 納入先品番
        /// </summary>
        public string DeliveryProductNumber { get; set; }
        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string SupplierProductNumber { get; set; }
        /// <summary>
        /// ロット番号
        /// </summary>
        public string LotNumber { get; set; }
        /// <summary>
        /// メインキー
        /// </summary>
        public string MainProductKey { get; set; }
        /// <summary>
        /// サブキー1
        /// </summary>
        public string FirstSubProductKey { get; set; }
        /// <summary>
        /// サブキー2
        /// </summary>
        public string SecondSubProductKey { get; set; }
        /// <summary>
        /// 箱数
        /// </summary>
        public int NumberOfBoxes { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remarks { get; set; }
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
