using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Calendar.Models;
using shift_pick_app.Models;
using shift_pick_app.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace shift_pick_app.ViewModels
{
    public partial class KinmuKanouDayViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        private static readonly DateTime ThisMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private static readonly DateTime NextMonth = ThisMonth.AddMonths(1);

        public KinmuKanouDayViewModel(NavigationService navigation, DialogService dialog)
        {
            _navigation = navigation;
            _dialog = dialog;

            // デモ：登録済みの勤務希望日
            SetRegisteredDates([DateTime.Today.AddDays(3), DateTime.Today.AddDays(4), DateTime.Today.AddDays(10)]);

            _ = LoadHolidaysAsync();
        }

        public CultureInfo Culture { get; } = new("ja-JP");

        // 今日から翌月末まで選択可能
        public DateTime MinimumDate { get; } = DateTime.Today;
        public DateTime MaximumDate { get; } = NextMonth.AddMonths(1).AddDays(-1);

        public string ThisMonthText => $"{ThisMonth.Month}月";   // 当月
        public string NextMonthText => $"{NextMonth.Month}月";   // 翌月

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextMonth))]
        private DateTime _shownDate = DateTime.Today;

        public bool IsNextMonth => ShownDate >= NextMonth;

        // カレンダー側で新しいコレクションに差し替えられるため ObservableProperty にする
        [ObservableProperty]
        private ObservableCollection<DateTime> _selectedDates = [];

        /// <summary>
        /// 登録済みの日を選択から外す（変更不可）。
        /// DisabledDates を使うとライブラリが背景色を消すため、Events の背景色＋ここで選択解除する
        /// </summary>
        partial void OnSelectedDatesChanged(ObservableCollection<DateTime> value)
        {
            if (value == null || !value.Any(d => RegisteredDates.Contains(d.Date)))
                return;

            // カレンダーの選択処理中に差し替えないよう、次のタイミングで外す
            MainThread.BeginInvokeOnMainThread(() =>
                SelectedDates = new(SelectedDates?.Where(d => !RegisteredDates.Contains(d.Date)) ?? []));
        }

        // 登録済みの日（背景色は Events で表示）
        [ObservableProperty]
        private List<DateTime> _registeredDates = [];

        // 祝日（文字色は Events で表示、Resources/Raw/holidays.json から読み込み）
        [ObservableProperty]
        private HashSet<DateTime> _holidays = [];

        // テスト用の一時的な切り替え：独自の MonthCalendar / Plugin.Maui.Calendar
        public bool UseCustomCalendar { get; } = DemoSettings.CustomCalendar;
        public bool UsePluginCalendar => !UseCustomCalendar;

        // 登録済みの日＋祝日
        [ObservableProperty]
        private EventCollection _calendarEvents = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private void ShowThisMonth() => ShownDate = DateTime.Today;

        [RelayCommand]
        private void ShowNextMonth() => ShownDate = NextMonth;

        /// <summary>
        /// 曜日ラベル押下：表示中の月のその曜日をまとめて選択（すべて選択済みなら解除）。
        /// 選択できない日（今日より前・登録済み）は対象外
        /// </summary>
        [RelayCommand]
        private void SelectWeekday(DayOfWeek dayOfWeek)
        {
            var firstDay = new DateTime(ShownDate.Year, ShownDate.Month, 1);
            var targets = Enumerable.Range(0, DateTime.DaysInMonth(firstDay.Year, firstDay.Month))
                .Select(i => firstDay.AddDays(i))
                .Where(d => d.DayOfWeek == dayOfWeek
                    && d >= MinimumDate && d <= MaximumDate
                    && !RegisteredDates.Contains(d))
                .ToList();
            if (targets.Count == 0)
                return;

            var selected = SelectedDates?.Select(d => d.Date).ToList() ?? [];
            bool allSelected = targets.All(selected.Contains);

            SelectedDates = allSelected
                ? new(selected.Except(targets))
                : new(selected.Union(targets));
        }

        [RelayCommand]
        private async Task Register()
        {
            ErrorMessage = string.Empty;
            if (SelectedDates == null || SelectedDates.Count == 0)
            {
                ErrorMessage = "勤務希望日を選択してください。";
                return;
            }

            bool confirmed = await _dialog.ConfirmAsync("勤務希望日登録", "勤務希望日が確定したらキャンセルできません。\n登録しますか？");
            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                // デモ：登録・メール送信の代わり
                await Task.Delay(1000);
                SetRegisteredDates([.. RegisteredDates, .. SelectedDates]);
                SelectedDates = [];
            }
            finally
            {
                IsBusy = false;
            }

            bool returnMenu = await _dialog.ConfirmAsync("登録しました", "登録内容をメールで送信しました。", "メインメニューへ", "閉じる");
            if (returnMenu)
            {
                await _navigation.PopAsync();
            }
        }

        /// <summary>
        /// 登録済みの日を更新する
        /// </summary>
        private void SetRegisteredDates(List<DateTime> dates)
        {
            RegisteredDates = dates.Select(d => d.Date).Distinct().ToList();
            UpdateCalendarEvents();
        }

        /// <summary>
        /// 祝日を読み込む（デモ：https://holidays-jp.github.io/api/v1/date.json を保存したもの）
        /// </summary>
        private async Task LoadHolidaysAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("holidays.json");
            var holidays = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream) ?? [];

            Holidays = holidays.Keys
                .Select(key => DateTime.ParseExact(key, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                .ToHashSet();
            UpdateCalendarEvents();
        }

        /// <summary>
        /// Events を差し替えて再描画させる。登録済みと祝日が重なる日は登録済みを優先
        /// </summary>
        private void UpdateCalendarEvents()
        {
            var events = new EventCollection();
            foreach (var date in RegisteredDates)
            {
                events.Add(date, new NoDotRegisteredDays<string>(["登録済み"]));
            }

            // 祝日：文字色のみ赤（選択時は通常の選択色）
            Color holidayText = ThemeColor("Error");
            Color selected = ThemeColor("Primary");
            Color selectedText = ThemeColor("OnPrimary");
            foreach (var date in Holidays.Where(d => d >= ThisMonth && d <= MaximumDate && !RegisteredDates.Contains(d)))
            {
                events.Add(date, new HolidayDay(holidayText, selected, selectedText));
            }

            CalendarEvents = events;
        }

        /// <summary>
        /// テーマの色を取得する（画面表示中の Light/Dark 切り替えには追従しない）
        /// </summary>
        private static Color ThemeColor(string key) =>
            Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
                ? color
                : Colors.Transparent;
    }
}
