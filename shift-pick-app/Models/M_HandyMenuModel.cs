using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class M_HandyMenuModel
    {
        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public int HandyMenuID { get; set; }

        /// <summary>
        /// ソース番号
        /// </summary>
        public int SortNumber { get; set; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { get; set; } = string.Empty;
    }
}
