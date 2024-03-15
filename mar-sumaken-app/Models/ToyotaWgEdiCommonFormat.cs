using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mar_sumaken_app.Models
{
    public class ToyotaWgEdiCommonFormat
    {
        public static class StartIndexString
        {
            public const string SupplierCodeIndex = "6V";
            public const string SupplierCodeClassIndex = "11V";
            //public const string ShippingLocationIndex = "20L";
            public const string NextProcess1Index = "2L";
            public const string Location1Index = "1L";
            public const string CustomerCodeIndex = "7V";
            public const string DeliveryDateIndex = "16D";
            public const string DeliveryTimeClassIndex = "9D";
            public const string DeliverySlipNumber = "10K";
            public const string ProductAbbreviationIndex = "20P";
            public const string ProductCodeIndex = "P";
            public const string QuantityIndex = "Q";
            public const string KanbanSerialNumberIndex = "17K";
            public const string FreeAreaIndex = "Z";
        }

        public static string[] GetQrcode(string qrString)
        {
            // QR中身を制御コードで区切れるよう変換する http://nanoappli.com/blog/archives/4841

            string changeCode = "";
            string[] ctrlStr = {
                "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL",
                "BS",  "HT",  "LF",  "VT",  "NP",  "CR",  "SO",  "SI",
                "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB",
                "CAN", "EM",  "SUB", "ESC", "FS",  "GS",  "RS",  "US" };
            changeCode = Regex.Replace(qrString, @"\p{Cc}", str =>
            {
                int offset = str.Value[0];
                if (ctrlStr.Length > offset)
                {
                    return "[" + ctrlStr[offset] + "]";
                }
                else
                {
                    return string.Format("<{0:X2}>", (byte)str.Value[0]);
                }
            });

            // レコード区切り[RS]～[RS]の内容取得
            var rs = "[RS]";
            var rsIndex = changeCode.IndexOf("[RS]");
            string strValue = changeCode.Remove(0, rsIndex + 1 + rs.Length);
            if(strValue.LastIndexOf("E" + rs) != -1)
                strValue = strValue.Remove(strValue.LastIndexOf("E" + rs));

            // グループ区切り[GS]ごとで区切る
            var words = strValue.Split(new string[] { "[GS]" }, StringSplitOptions.RemoveEmptyEntries);
            words = words.Select(x => x = x.Trim()).ToArray();

            return words;
        }
    }
}
