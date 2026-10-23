using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
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
        [NotifyPropertyChangedFor(nameof(RegisterButtonText))]
        private ObservableCollection<DateTime> _selectedDates = [];

        // 登録ボタン：選択中の日数（例：登録（3日））
        public string RegisterButtonText => SelectedDates?.Count > 0 ? $"登録（{SelectedDates.Count}日）" : "登録";

        // 登録済みの日（変更不可）
        [ObservableProperty]
        private List<DateTime> _registeredDates = [];

        // 祝日（赤字、Resources/Raw/holidays.json から読み込み）
        [ObservableProperty]
        private HashSet<DateTime> _holidays = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private void ShowThisMonth() => ShownDate = DateTime.Today;

        [RelayCommand]
        private void ShowNextMonth() => ShownDate = NextMonth;

        /// <summary>
        /// 戻る（ナビゲーションバー・端末の戻るボタン共通、登録後の「メインメニューへ」）→ SW003
        /// </summary>
        [RelayCommand]
        private Task Back() => RunBusyAsync(LoadingMessage, _navigation.PopAsync);

        [RelayCommand]
        private async Task Register()
        {
            ErrorMessage = string.Empty;
            if (SelectedDates == null || SelectedDates.Count == 0)
            {
                ErrorMessage = "勤務希望日を選択してください。";
                return;
            }

            bool confirmed = await _dialog.ConfirmAsync(
                "勤務希望日登録", Util.PhraseJpText("|勤務希望日が|確定したら|キャンセル|できません。|登録しますか？"));
            if (!confirmed)
                return;

            await RunBusyAsync(ProcessingMessage, async () =>
            {
                // デモ：登録・メール送信の代わり
                await Task.Delay(1000);
                SetRegisteredDates([.. RegisteredDates, .. SelectedDates]);
                SelectedDates = [];
            });

            bool returnMenu = await _dialog.ShowResultAsync(
                "登録しました", Util.PhraseJpText("|登録内容を|メールで|送信しました。"), "メインメニューへ", "閉じる");
            if (returnMenu)
            {
                await Back();
            }
        }

        /// <summary>
        /// 登録済みの日を更新する
        /// </summary>
        private void SetRegisteredDates(List<DateTime> dates) =>
            RegisteredDates = dates.Select(d => d.Date).Distinct().ToList();

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
        }
    }
}
