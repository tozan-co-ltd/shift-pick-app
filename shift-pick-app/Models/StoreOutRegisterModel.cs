using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class StoreOutRegisterModel
    {
        public class StoreOutRegisterPostBody
        {
            /// <summary>
            /// 会社コード
            /// </summary>
            public string CompanyCode { get; set; }
            /// <summary>
            /// 会社パスワード
            /// </summary>
            public string CompanyPassword { get; set; }
            /// <summary>
            /// 出荷実績
            /// </summary>
            public List<ScanShipmentModel> ScanShipmentModel { get; set; }
            /// <summary>
            /// ハンディエラーメッセージ実績
            /// </summary>
            public List<D_HandyErrorMessageModel> HandyErrorMessage { get; set; }
        }
        public class StoreOutResponseContent
        {
            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("productWithMinusNumberOfBoxes")]
            public List<M_ProductModel> ProductsWithMinusNumberOfBoxes { get; set; }

            [JsonProperty("scanShipmentDuplicateData")]
            public List<ScanShipmentModel> ScanShipmentDuplicateData { get; set; }
        }
    }
}
