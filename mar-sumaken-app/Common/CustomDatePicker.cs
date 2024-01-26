using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Common
{
    public class CustomDatePicker
    {
        /// <summary>
        /// datepickerのデフォルト日付を設定する
        /// </summary>
        public static DateTime SetDisplayDatePicker()
        {
            DateTime currentDate = DateTime.Now;
            DateTime setDisplayDate = currentDate.AddDays(currentDate.DayOfWeek == DayOfWeek.Friday ? 3 : 1);
            return setDisplayDate;
        }
    }
}
