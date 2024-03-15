using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class M_SupplierKanbanModel
    {
        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        public int SupplierKanbanID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// 仕入先かんばん名
        /// </summary>
        public string SupplierKanbanName { get; set; }

        /// <summary>
        /// 重複許容フラグ
        /// </summary>
        public bool AllowedDuplicatesFlag { get; set; }

        /// <summary>
        /// 識別文字
        /// </summary>
        public string IdentifyString { get; set; }

        /// <summary>
        /// 識別文字開始位置
        /// </summary>
        public int IdentifyStringStartIndex { get; set; }

        /// <summary>
        /// 品番桁数
        /// </summary>
        public int ProductNumberStartIndex { get; set; }

        /// <summary>
        /// 品番開始位置
        /// </summary>
        public int ProductNumberLength { get; set; }

        /// <summary>
        /// 数量桁数
        /// </summary>
        public int QuantityLength { get; set; }

        /// <summary>
        /// 数量開始位置
        /// </summary>
        public int QuantityStartIndex { get; set; }

        /// <summary>
        /// ロット番号桁数
        /// </summary>
        public int LotLength { get; set; }

        /// <summary>
        /// ロット番号開始位置
        /// </summary>
        public int LotStartIndex { get; set; }

        /// <summary>
        /// メインキー桁数
        /// </summary>
        public int MainProductKeyLength { get; set; }

        /// <summary>
        /// メインキー開始位置
        /// </summary>
        public int MainProductKeyStartIndex { get; set; }

        /// <summary>
        /// サブキー1桁数
        /// </summary>
        public int FirstSubProductKeyLength { get; set; }

        /// <summary>
        /// サブキー1開始位置
        /// </summary>
        public int FirstSubProductKeyStartIndex { get; set; }

        /// <summary>
        /// サブキー2桁数
        /// </summary>
        public int SecondSubProductKeyLength { get; set; }

        /// <summary>
        /// サブキー2開始位置
        /// </summary>
        public int SecondSubProductKeyStartIndex { get; set; }

        /// <summary>
        /// 枝番桁数
        /// </summary>
        public int ProductBranchNumberLength { get; set; }

        /// <summary>
        /// 枝番開始位置
        /// </summary>
        public int ProductBranchNumberStartIndex { get; set; }

        /// <summary>
        /// 注文番号桁数
        /// </summary>
        public int OrderNumberLength { get; set; }

        /// <summary>
        /// 注文番号開始位置
        /// </summary>
        public int OrderNumberStartIndex { get; set; }
    }
}
