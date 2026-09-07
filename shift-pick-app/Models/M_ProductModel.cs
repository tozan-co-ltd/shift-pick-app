using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class M_ProductModel
    {
        /// <summary>
        /// 品番ID
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// 仕入先ID 
        /// </summary>

        public int SupplierID { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string SupplierProductNumber { get; set; }

        /// <summary>
        /// 納入先ID
        /// </summary>
        public int DeliveryID { get; set; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        public string DeliveryProductNumber { get; set; }

        /// <summary>
        /// 品名
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        public int LotQuantity { get; set; }
    }
}
