using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class ScanReceiveModel
    {
        /// <summary>
        /// 読取実績モデル
        /// </summary>
        public D_ScanResultModel d_ScanResultModel { get; set; }
        /// <summary>
        /// 入荷実績モデル
        /// </summary>
        public D_ReceiveModel d_ReceiveModel { get; set; }
    }
}
