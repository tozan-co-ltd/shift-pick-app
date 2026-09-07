using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class D_StoreOutModel
    {
        /// <summary>
        /// 出荷実績ID
        /// </summary>
        public int StoreOutID { get; set; }
        /// <summary>
        /// 倉庫ID
        /// </summary>

        public int DepoID { get; set; }
        /// <summary>
        /// 会社ID:仕入先の会社ID
        /// </summary>

        public int CompanyID { get; set; }
        /// <summary>
        /// 出庫日
        /// </summary>

        public DateTime StoreOutDate { get; set; }
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
        public string Remarks { get; set; } = string.Empty;
        /// <summary>
        /// 削除フラグ
        /// </summary>

        public bool IsDeleted { get; set; }
        /// <summary>
        /// 作成日時
        /// </summary>

        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 作成者
        /// </summary>

        public string CreatedBy { get; set; }
        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; }

        public class StoreOutPostBody
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

        public class StoreOutPostBackContent
        {
            // メッセージ
            public string Message { get; set; }
            // 在庫がマイナスの品番
            public List<M_ProductModel> productWithMinusNumberOfBoxes { get; set; }
            // 重複している出荷実績
            public List<ScanShipmentModel> scanShipmentDuplicateData { get; set; }
        }
    }
}
