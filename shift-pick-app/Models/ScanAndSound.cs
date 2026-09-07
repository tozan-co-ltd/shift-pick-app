using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Models
{
    public class ScanAndSound
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("ok_sound")]
        public string OkSound { get; set; }
        [Column("error_sound")]
        public string ErrorSound { get; set; }
        [Column("scan_sound")]
        public string ScandMode { get; set; }
    }
}
