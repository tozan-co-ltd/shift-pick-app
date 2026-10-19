using shift_pick_app.Models;
using System.Collections.ObjectModel;

namespace shift_pick_app.Views.Shared;

/// <summary>
/// 1か月分のカレンダー（複数選択）。仕様：.workspace/work/calendar-control-spec.md
/// 選択結果は SelectedDates（変更のたびに新しいコレクションを設定）で返す
/// </summary>
public partial class MonthCalendar : ContentView
{
    public static readonly BindableProperty ShownMonthProperty =
        BindableProperty.Create(nameof(ShownMonth), typeof(DateTime), typeof(MonthCalendar), DateTime.Today, propertyChanged: OnInputChanged);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime), typeof(MonthCalendar), DateTime.MinValue, propertyChanged: OnInputChanged);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime), typeof(MonthCalendar), DateTime.MaxValue, propertyChanged: OnInputChanged);

    public static readonly BindableProperty SelectedDatesProperty =
        BindableProperty.Create(nameof(SelectedDates), typeof(ObservableCollection<DateTime>), typeof(MonthCalendar), null,
            BindingMode.TwoWay, propertyChanged: OnInputChanged);

    public static readonly BindableProperty RegisteredDatesProperty =
        BindableProperty.Create(nameof(RegisteredDates), typeof(IEnumerable<DateTime>), typeof(MonthCalendar), null, propertyChanged: OnRegisteredDatesChanged);

    public static readonly BindableProperty HolidaysProperty =
        BindableProperty.Create(nameof(Holidays), typeof(IEnumerable<DateTime>), typeof(MonthCalendar), null, propertyChanged: OnInputChanged);

    /// <summary>表示する月（月内の任意の日付）</summary>
    public DateTime ShownMonth
    {
        get => (DateTime)GetValue(ShownMonthProperty);
        set => SetValue(ShownMonthProperty, value);
    }

    /// <summary>選択可能な最初の日</summary>
    public DateTime MinimumDate
    {
        get => (DateTime)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    /// <summary>選択可能な最後の日</summary>
    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    /// <summary>選択中の日</summary>
    public ObservableCollection<DateTime>? SelectedDates
    {
        get => (ObservableCollection<DateTime>?)GetValue(SelectedDatesProperty);
        set => SetValue(SelectedDatesProperty, value);
    }

    /// <summary>登録済みの日（選択不可）</summary>
    public IEnumerable<DateTime>? RegisteredDates
    {
        get => (IEnumerable<DateTime>?)GetValue(RegisteredDatesProperty);
        set => SetValue(RegisteredDatesProperty, value);
    }

    /// <summary>祝日（赤字、選択可）</summary>
    public IEnumerable<DateTime>? Holidays
    {
        get => (IEnumerable<DateTime>?)GetValue(HolidaysProperty);
        set => SetValue(HolidaysProperty, value);
    }

    // 7列×6行のマス（作成は1回のみ）
    private readonly List<CalendarDay> _days =
        [.. Enumerable.Range(0, 42).Select(i => new CalendarDay(i / 7, i % 7))];

    public MonthCalendar()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(daysGrid, _days);
        Refresh();
    }

    private static void OnInputChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((MonthCalendar)bindable).Refresh();

    /// <summary>
    /// 登録済みの日を選択から外す
    /// </summary>
    private static void OnRegisteredDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var calendar = (MonthCalendar)bindable;
        var registered = calendar.RegisteredDates?.Select(d => d.Date).ToHashSet() ?? [];
        var selected = calendar.GetSelection();
        if (selected.RemoveWhere(registered.Contains) > 0)
            calendar.SetSelection(selected);   // Refresh も呼ばれる
        else
            calendar.Refresh();
    }

    /// <summary>
    /// 日のタップ：選択／解除
    /// </summary>
    private void OnDayTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: CalendarDay day } || !day.IsTappable)
            return;

        var selected = GetSelection();
        if (!selected.Remove(day.Date))
            selected.Add(day.Date);
        SetSelection(selected);
    }

    /// <summary>
    /// 曜日のタップ：表示中の月のその曜日をまとめて選択（すべて選択済みなら解除）。選択できない日は対象外
    /// </summary>
    private void OnWeekdayTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not DayOfWeek dayOfWeek)
            return;

        var targets = _days.Where(d => d.IsTappable && d.Date.DayOfWeek == dayOfWeek).ToList();
        if (targets.Count == 0)
            return;

        var selected = GetSelection();
        if (targets.All(d => d.State == CalendarDayState.Selected))
            selected.ExceptWith(targets.Select(d => d.Date));
        else
            selected.UnionWith(targets.Select(d => d.Date));
        SetSelection(selected);
    }

    private HashSet<DateTime> GetSelection() => SelectedDates?.Select(d => d.Date).ToHashSet() ?? [];

    private void SetSelection(HashSet<DateTime> selected) => SelectedDates = new(selected.Order());

    /// <summary>
    /// 42マスの日付・状態を更新する
    /// </summary>
    private void Refresh()
    {
        var firstDay = new DateTime(ShownMonth.Year, ShownMonth.Month, 1);
        var start = firstDay.AddDays(-(int)firstDay.DayOfWeek);   // 日曜始まり
        var selected = GetSelection();
        var registered = RegisteredDates?.Select(d => d.Date).ToHashSet() ?? [];
        var holidays = Holidays?.Select(d => d.Date).ToHashSet() ?? [];

        for (int i = 0; i < _days.Count; i++)
        {
            var date = start.AddDays(i);
            bool isThisMonth = date.Month == firstDay.Month;

            var day = _days[i];
            day.Date = date;
            day.Day = isThisMonth ? date.Day.ToString() : string.Empty;
            day.IsToday = isThisMonth && date == DateTime.Today;
            day.State = !isThisMonth ? CalendarDayState.Hidden
                : date < MinimumDate.Date || date > MaximumDate.Date ? CalendarDayState.Disabled
                : registered.Contains(date) ? CalendarDayState.Registered
                : selected.Contains(date) ? CalendarDayState.Selected
                : CalendarDayState.Normal;

            // 祝日・土日の赤字は背景なしの日のみ（背景上では読みにくいため）。無効の日は XAML で薄くする
            bool isRed = holidays.Contains(date) || date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            day.TextTone = day.State switch
            {
                CalendarDayState.Registered => CalendarDayTextTone.OnRegistered,
                CalendarDayState.Selected => CalendarDayTextTone.OnSelected,
                _ => isRed ? CalendarDayTextTone.Red : CalendarDayTextTone.Default,
            };
        }
    }
}
