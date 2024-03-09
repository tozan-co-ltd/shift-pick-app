using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Common
{
    public static class Util
    {
        // 共通関数
        // 参考：https://gist.github.com/Buravo46/49c34e77ff1a75177340

        public static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            // ビルドアクションで埋め込みリソースにしたファイルを取ってくる
            var stream = assembly.GetManifestResourceStream(filename);
            return stream;
        }

        public static string AddCompanyPath(string url, int companyID)
        {
            string createUrl = url;

            var lastString = url.Substring(url.Length - 1);
            if (lastString != "/")
            {
                createUrl += "/";
            }

            createUrl += companyID.ToString();

            return createUrl;
        }

        /// <summary>
        /// QRコードの部分文字列
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stringLength"></param>
        /// <param name="qr"></param>
        /// <returns>抜き出した文字列</returns>
        public static string QrcodeValueSubstring(int index, int stringLength, string qr)
        {
            var substr = "";

            // 文字列を取得するための最小桁数
            int minLength = index - 1 + stringLength;
            if (qr.Length >= minLength)
            {
                substr = qr.Substring(index - 1, stringLength).Replace(" ", "");
            }
            return substr;
        }

        /// <summary>
        /// ダッシュの後のサブ文字列
        /// </summary>
        /// <param name="supplierProductNumberTrim"></param>
        /// <returns></returns>
        public static string SubStringAfterDash(string supplierProductNumberTrim)
        {
            int indexDash = supplierProductNumberTrim.IndexOf('-');
            if (indexDash != -1 && indexDash < supplierProductNumberTrim.Length - 1)
            {
                if (supplierProductNumberTrim.Length - indexDash - 1 == 1)
                {
                    return supplierProductNumberTrim.Substring(0, indexDash);
                }
            }
            return supplierProductNumberTrim;
        }

        /// <summary>
        /// 翌日(土日を除く)の日付を取得
        /// </summary>
        /// <param name="today">本日</param>
        public static DateTime GetNextday(DateTime today)
        {
            var nextday = today.AddDays(1);

            if (nextday.DayOfWeek == DayOfWeek.Saturday)
            {
                nextday = nextday.AddDays(2);
            }
            else if (nextday.DayOfWeek == DayOfWeek.Sunday)
            {
                nextday = nextday.AddDays(1);
            }
            return nextday;
        }

    }
}

