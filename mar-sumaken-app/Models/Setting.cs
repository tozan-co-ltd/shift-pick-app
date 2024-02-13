using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
    public class Setting
    {
        public class SettingPostBody 
        {
            [Required]
            public string CompanyCode { get; set; } = string.Empty;
            [Required]
            public string CompanyPassword { get; set; } = string.Empty;
            [Required]
            public string LoginID { get; set; } = string.Empty;
        }

        public class SettingApiResponceBody
        {
            public int CompanyID { get; set; }
            public int HandyUserID { get; set; }
            public int PasswordMode { get; set; }
        }

        public class HandySetting
        {
            [PrimaryKey]
            [AutoIncrement]
            [Column("Id")]
            public int Id { get; set; }

            //[Column("CompanyID")]
            //public int CompanyID { get; set; }

            [Column("HandyApiUrl")]
            public string HandyApiUrl { get; set; }

            [Column("CompanyCode")]
            public string CompanyCode { get; set; }

            [Column("CompanyName")]
            public string CompanyName { get; set; }

            [Column("CompanyPassword")]
            public string CompanyPassword { get; set; }

            [Column("LoginID")]
            public string LoginID { get; set; }

            [Column("UserName")]
            public string UserName { get; set; }

            [Column("ScanMode")]
            public string ScanMode { get; set; }

            [Column("ScanOkeySound")]

            public string ScanOkSound { get; set; }

            [Column("ScanErrorSound")]
            public string ScanErrorSound { get; set; }
        }
    }
}
