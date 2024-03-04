using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class LatestMastersModel
    {
        public class LatestMastersPostBody
        {
            /// <summary>
            /// 会社コード
            /// </summary>
            public string CompanyCode { get; set; } = string.Empty;

            /// <summary>
            /// 会社パスワード
            /// </summary>
            public string CompanyPassword { get; set; } = string.Empty;

            /// <summary>
            /// ユーザーID
            /// </summary>
            public int UserID { get; set; }
        }

        public class LatestMastersPostBackContent
        {
            /// <summary>
            /// 品番
            /// </summary>
            public List<M_ProductModel> M_ProductList { get; set; }

            /// <summary>
            /// 出荷指示QRコード
            /// </summary>
            public List<M_ShipmentScheduleQRCodeModel> M_ShipmentScheduleQRCodeList { get; set; }

            /// <summary>
            /// 仕入先かんばん
            /// </summary>
            public List<M_SupplierKanbanModel> M_SupplierKanbanList { get; set; }

            /// <summary>
            /// ハンディメニュー仕入先かんばん
            /// </summary>
            public List<R_HandyMenuSupplierKanbanModel> R_HandyMenuSupplierKanbanList { get; set; }
        }
    }
}
