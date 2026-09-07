using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class M_DepoModel
    {
        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫コード
        /// </summary>
        public string DepoCode { get; set; } = string.Empty;

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string DepoName { get; set; } = string.Empty;
    }
}
