using CommunityToolkit.Mvvm.ComponentModel;

namespace shift_pick_app.Models
{
    /// <summary>
    /// カレンダーの日の状態（背景・タップ可否。上から優先）
    /// </summary>
    public enum CalendarDayState
    {
        Hidden,      // 表示中の月以外（空白）
        Disabled,    // 選択範囲外（過去の登録済みも含む）
        Registered,  // 登録済み（変更不可）
        Selected,    // 選択中
        Normal,
    }

    /// <summary>
    /// 日の文字色（トリガーが重ならないよう、状態と祝日・土日から1つに決める）
    /// </summary>
    public enum CalendarDayTextTone
    {
        Default,       // 通常
        Red,           // 祝日・土日（背景なしの日のみ）
        OnRegistered,  // 登録済みの背景上
        OnSelected,    // 選択中の背景上
    }

    /// <summary>
    /// MonthCalendar の1マス（42マスを使い回し、月の切り替え時は中身だけ更新する）
    /// </summary>
    public partial class CalendarDay : ObservableObject
    {
        public CalendarDay(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }

        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private string _day = string.Empty;

        [ObservableProperty]
        private CalendarDayState _state;

        [ObservableProperty]
        private CalendarDayTextTone _textTone;

        [ObservableProperty]
        private bool _isToday;

        public bool IsTappable => State is CalendarDayState.Normal or CalendarDayState.Selected;
    }
}
