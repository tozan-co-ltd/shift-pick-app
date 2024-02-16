using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class ShipmentScheduleModel
    {
        public class ShipmentScheduleRequestBody{ 
            public string CompanyCode { get; set; }
            public string CompanyPassword { get; set; }
            public int DepoID { get; set; }
            public int CompanyID { get; set; }
            public DateTime DeliveryDate { get; set; }
            public int DeliveryTimeClass { get; set; }
            public string DeliveryLocation { get; set; }

        }
        public class D_ShipmentSchedule
        {
            /// <summary>
            /// No
            /// </summary>
            public int No { get; set; }

            /// <summary>
            /// 出荷指示ID
            /// </summary>
            public int ShipmentScheduleID  { get; set; }

            /// <summary>
            /// 倉庫ID
            /// </summary>
            public int DepoID { get; set; }

            /// <summary>
            /// 会社ID
            /// </summary>
            public int CompanyID { get; set; }

            /// <summary>
            /// 発注元
            /// </summary>
            public string OrdererCode { get; set; }

            /// <summary>
            /// 発注元工区
            /// </summary>
            public string OrdererFactoryKubun { get; set; }

            /// <summary>
            /// 発注元名称
            /// </summary>
            public string OrdererName { get; set; }

            /// <summary>
            /// 発注元工場名
            /// </summary>
            public string OrdererFactoryName { get; set; }

            /// <summary>
            /// 出荷元
            /// </summary>
            public string ShipperCode { get; set; }

            /// <summary>
            /// 出荷元工区
            /// </summary>
            public string ShipperFactoryKubun { get; set; }

            /// <summary>
            /// 出荷元名称
            /// </summary>
            public string ShipperName { get; set; }

            /// <summary>
            /// 納入先
            /// </summary>
            public string DeliveryCode { get; set; }

            /// <summary>
            /// 納入先工区
            /// </summary>
            public string DeliveryFactoryKubun { get; set; }

            /// <summary>
            /// 納入場所
            /// </summary>
            public string DeliveryLocation { get; set; }

            /// <summary>
            /// 納入先名
            /// </summary>
            public string DeliveryName { get; set; }

            /// <summary>
            /// 納入先工場名
            /// </summary>
            public string DeliveryFactoryName { get; set; }

            /// <summary>
            /// 定期/不定期区分名称
            /// </summary>
            public string RegularKubun { get; set; }

            /// <summary>
            /// 発行日
            /// </summary>
            public DateTime IssuedDate { get; set; }
            /// <summary>
            /// 納入指示日
            /// </summary>
            public DateTime DeliveryDate { get; set; }

            /// <summary>
            /// 納入時刻
            /// </summary>
            public string DeliveryTime { get; set; }

            /// <summary>
            /// 便
            /// </summary>
            public int DeliveryTimeClass { get; set; }

            /// <summary>
            /// 輸送識別
            /// </summary>
            public string TranspotationIdentify { get; set; }

            /// <summary>
            /// 納入書番号
            /// </summary>
            public string DeliverySlipNumber { get; set; }

            /// <summary>
            /// ページ数
            /// </summary>
            public int DeliverySlipPageNumber { get; set; }

            /// <summary>
            /// 行No
            /// </summary>
            public int DeliverySlipRowNumber { get; set; }

            /// <summary>
            /// 表示用品番
            /// </summary>
            public string DeliveryProductNumber { get; set; }

            /// <summary>
            /// 背番号
            /// </summary>
            public string DeliveryProductAbbreviation { get; set; }

            /// <summary>
            /// 品名
            /// </summary>
            public string DeliveryProductName { get; set; }
            /// <summary>
            /// 収容数
            /// </summary>
            public int LotQuantity { get; set; }

            /// <summary>
            /// 枝番
            /// </summary>
            public int BranchNumber { get; set; }

            /// <summary>
            /// 納入指示数
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// 仕入先品番
            /// </summary>
            public string SupplierProductNumber { get; set; }

            /// <summary>
            /// 箱数
            /// </summary>
            public int NumberOfBoxes { get; set; }

            /// <summary>
            /// 残箱数
            /// </summary>
            public int NumberOfBoxesRemaining { get; set; }
        }

        public class ShipmentScheduelResponseBody
        {
            /// <summary>
            /// 出荷指示
            /// </summary>
            public List<D_ShipmentSchedule> D_ShipmentScheduleList { get; set; }

            /// <summary>
            /// 出荷実績
            /// </summary>
            public List<D_ShipmentModel> D_ShipmentList { get; set; }

            /// <summary>
            /// ユーザー
            /// </summary>
            public List<M_UserModel> M_UserList { get; set; }
        }
    }
}
