using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class R_HandyMenuSupplierKanbanModel
    {
        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public int HandyMenuID { get; set; }

        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        public int SupplierKanbanID { get; set; }
    }
}
