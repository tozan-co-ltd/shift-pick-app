using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Calendar.Models;
using shift_pick_app.Models;
using shift_pick_app.Services;
using System.Collections.ObjectModel;
using System.Globalization;

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
            if (value == null || !value.Any(d => _registeredDates.Contains(d.Date)))
                return;

            // カレンダーの選択処理中に差し替えないよう、次のタイミングで外す
            MainThread.BeginInvokeOnMainThread(() =>
                SelectedDates = new(SelectedDates?.Where(d => !_registeredDates.Contains(d.Date)) ?? []));
        }

        // 登録済みの日（背景色は Events で表示）
        private List<DateTime> _registeredDates = [];

        [ObservableProperty]
        private EventCollection _registeredEvents = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private void ShowThisMonth() => ShownDate = DateTime.Today;

        [RelayCommand]
        private void ShowNextMonth() => ShownDate = NextMonth;

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
                SetRegisteredDates([.. _registeredDates, .. SelectedDates]);
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
        /// 登録済みの日を更新する（Events を差し替えて再描画させる）
        /// </summary>
        private void SetRegisteredDates(List<DateTime> dates)
        {
            _registeredDates = dates.Select(d => d.Date).Distinct().ToList();

            var events = new EventCollection();
            foreach (var date in _registeredDates)
            {
                events.Add(date, new NoDotRegisteredDays<string>(["登録済み"]));
            }
            RegisteredEvents = events;  // 登録済みの日をセット
        }
    }
}
