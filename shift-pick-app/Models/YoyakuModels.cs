using CommunityToolkit.Mvvm.ComponentModel;
using shift_pick_app.Common;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 予約テーブルの1行（日付・事業所・選択状態）
    /// </summary>
    public partial class YoyakuSlot : ObservableObject
    {
        public YoyakuSlot(DateTime workDate, string officeName)
        {
            WorkDate = workDate;
            OfficeName = officeName;
        }

        public DateTime WorkDate { get; }
        public string WorkDateText => Util.ToJapaneseDay(WorkDate);
        public string OfficeName { get; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
