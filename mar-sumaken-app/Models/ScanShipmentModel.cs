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
        /// 読取実績モデル
        /// </summary>
        public D_ScanResultModel d_ScanResultModel { get; set; }
        /// <summary>
        /// 出荷実績モデル
        /// </summary>
        public D_ShipmentModel d_ShipmentModel { get; set; }
    }
}
