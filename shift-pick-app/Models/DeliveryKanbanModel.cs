using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class DeliveryKanbanModel
    {
        public string ShipperCode { get; set; } // 出荷元コード
        public string DeliveryCode { get; set; } // 納入先コード
        public string DeliveryLocation { get; set; } // 納入場所
        public string OrdererCode { get; set; } // 発注元コード
        public DateTime DeliveryDate { get; set; } // 納入指示日
        public int DeliveryTimeClass { get; set; } // 便
        public string DeliverySlipNumber { get; set; } // 納品書番号
        public string ProductAbbreviation { get; set; } // 背番号
        public string DeliveryProductNumber { get; set; } // 表示用品番
        public int LotQuantity { get; set; } // 収容数
        public int KanbanSerialNumber { get; set; } // かんばん識別番号
    }
}

