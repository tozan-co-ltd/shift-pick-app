using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace shift_pick_app.Common
{
    public static class Util
    {
        // 共通関数
        // 参考：https://gist.github.com/Buravo46/49c34e77ff1a75177340

        /// <summary>
        /// サウンドファイル（Resources\Raw 配下の MauiAsset）を開く
        /// </summary>
        /// <param name="path">Resources\Raw からの相対パス。例：ScanOkeySound/ScanOkeySound1.mp3</param>
        public static Task<Stream> OpenSoundAsync(string path)
        {
            return FileSystem.OpenAppPackageFileAsync(path);
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
        /// <summary>
        ///  intへの変換ができるかチェック用
        /// </summary>
        /// <param name="valueCheck"></param>
        public static (bool isInt, int value) CheckIntType(string valueCheck)
        {
            try
            {
                Match match = Regex.Match(valueCheck, @"^\d+");
                int quantity = int.Parse(match.Value);
                return (true, quantity);
            }
            catch (Exception ex)
            {
                return (false, 0);
            }
        }

        /// <summary>
        ///  日付への変換ができるかチェック用
        /// </summary>
        /// <param name="deliveryDateString"></param>
        public static (bool isDate, DateTime dateTime) CheckDateTimeType(string deliveryDateString)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(deliveryDateString, "yyyyMMdd", CultureInfo.InvariantCulture);
                return (true, dateTime);
            }
            catch (Exception ex)
            {
                return (false, DateTime.Now);
            }
        }

        /// <summary>
        /// 日付を「（曜日）日」形式にする。例：（月）1
        /// 端末の言語設定に関係なく曜日を漢字にするため ja-JP を指定する
        /// </summary>
        public static string ToJapaneseDay(DateTime date)
        {
            return date.ToString("（ddd）d", new CultureInfo("ja-JP"));
        }

        /// <summary>
        /// 日付を「yyyy/MM/dd（曜日）」形式にする。例：2026/10/15（木）
        /// </summary>
        public static string ToJapaneseDate(DateTime date)
        {
            return date.ToString("yyyy/MM/dd（ddd）", new CultureInfo("ja-JP"));
        }
    }
}

