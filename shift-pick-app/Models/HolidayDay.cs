using Plugin.Maui.Calendar.Interfaces;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 祝日（文字色のみ変更、名前・ドットは表示しない）。
    /// Plugin.Maui.Calendar 2.0.x では日ごとの文字色を変える手段が Events しかないため、色だけのイベントにしている
    /// </summary>
    public sealed class HolidayDay : List<string>, IPersonalizableDayEvent, IMultiEventDay
    {
        public HolidayDay(Color textColor, Color selectedColor, Color selectedTextColor)
        {
            EventIndicatorColor = Microsoft.Maui.Graphics.Colors.Transparent;   // 背景なし
            EventIndicatorTextColor = textColor;
            EventIndicatorSelectedColor = selectedColor;
            EventIndicatorSelectedTextColor = selectedTextColor;
        }

        public Color EventIndicatorColor { get; set; }
        public Color EventIndicatorSelectedColor { get; set; }
        public Color EventIndicatorTextColor { get; set; }
        public Color EventIndicatorSelectedTextColor { get; set; }

        // ドットを非表示にするため、意図的に色を空にしている（NoDotRegisteredDays と同じ）
        public IReadOnlyList<Color> Colors => [];
    }
}
