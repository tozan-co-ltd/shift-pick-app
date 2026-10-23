using CommunityToolkit.Mvvm.ComponentModel;
using shift_pick_app.Common;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 予約テーブルの1行（日付・事業所・選択状態・エラー表示・希望事業所の印）
    /// </summary>
    public partial class YoyakuSlot : ObservableObject
    {
        public YoyakuSlot(DateTime workDate, int officeId)
        {
            WorkDate = workDate;
            OfficeId = officeId;
            OfficeName = DemoOffices.Get(officeId).Name;
        }

        public DateTime WorkDate { get; }
        public string WorkDateText => Util.ToJapaneseDay(WorkDate);
        public string WorkDateFullText => Util.ToJapaneseDate(WorkDate);
        public int OfficeId { get; }
        public string OfficeName { get; }

        [ObservableProperty]
        private bool _isSelected;

        // 同一日付で複数選択（申込でエラーになった行）：日付・事業所を赤字にする
        [ObservableProperty]
        private bool _isConflict;

        // 希望事業所の行：ブックマークを表示する（両タブ共通）
        [ObservableProperty]
        private bool _isPreferredMarkVisible;
    }
}
