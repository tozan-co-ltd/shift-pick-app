using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class M_ShipmentScheduleQRCodeModel
    {
        /// <summary>
        /// 出荷指示QRコードID
        /// </summary>

        public int ShippingScheduleQRCodeID { get; set; }
        /// <summary>
        /// 倉庫ID
        /// </summary>

        public int DepoID { get; set; }
        /// <summary>
        /// 会社ID
        /// </summary>

        public int CompanyID { get; set; }
        /// <summary>
        /// 納入先かんばん名
        /// </summary>

        public string DeliveryKanbanName { get; set; }
        /// <summary>
        /// トヨタEDIWGフラグ
        /// </summary>

        public int ToyotaEdiWgFlag { get; set; }
        /// <summary>
        /// 識別文字
        /// </summary>

        public string IdentifyString { get; set; }
        /// <summary>
        /// 識別文字開始位置
        /// </summary>

        public int IdentifyStringStartIndex { get; set; }
        /// <summary>
        /// 出荷指示テーブル名
        /// </summary>

        public string ShipmentScheduleTableName { get; set; }
    }
}
