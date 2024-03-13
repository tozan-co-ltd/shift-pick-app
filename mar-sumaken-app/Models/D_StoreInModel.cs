using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class D_StoreInModel
    {
        /// <summary>
        /// 入庫実績ID
        /// </summary>
        public int StoreInID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// 入庫日
        /// </summary>
        public DateTime StoreInDate { get; set; }

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
        public Boolean IsDeleted { get; set; }

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

        public class StoreInPostBody
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
            /// 読取実績
            /// </summary>
            public List<ScanReceiveModel> ScanReceive { get; set; }

            /// <summary>
            /// ハンディエラーメッセージ実績
            /// </summary>
            public List<D_HandyErrorMessageModel> HandyErrorMessage { get; set; }
        }
        public class StoreInResponceContent
        {
            // メッセージ
            public string Message { get; set; }
            // 重複している読取実績
            public List<ScanReceiveModel> ScanReceive { get; set; }
        }
    }
}
