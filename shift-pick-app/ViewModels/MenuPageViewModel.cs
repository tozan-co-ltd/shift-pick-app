using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class MenuPageViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        public MenuPageViewModel(NavigationService navigation, DialogService dialog)
        {
            _navigation = navigation;
            _dialog = dialog;
        }

        [ObservableProperty]
        private string _userId = string.Empty;

        // デモ用の固定値
        public List<QuickStatusItem> QuickStatus { get; } = DemoReservations.CountByStatus();

        public List<string> News { get; } =
        [
            "○○：○○○○○○○○○○（たった今）",
            "○○：○○○○○○○○○○（15分）",
            "○○：○○○○○○○○○○（28分）",
            "○○：○○○○○○○○○○（3時間）",
            "○○：○○○○○○○○○○（8時間）",
            "○○：○○○○○○○○○○（昨日）",
            "○○：○○○○○○○○○○（2日前）",
        ];

        // デモ用の一時的な切り替え：SW003 に即時反映
        public bool HasNews
        {
            get => DemoSettings.HasNews;
            set
            {
                DemoSettings.HasNews = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNewsEmpty));
            }
        }

        public bool IsNewsEmpty => !HasNews;

        // デモ用の一時的な切り替え：SW008 を開いたときに反映
        public bool PrefectureImageWithName
        {
            get => DemoSettings.PrefectureImageWithName;
            set => DemoSettings.PrefectureImageWithName = value;
        }

        // デモ用の一時的な切り替え：SW007 を開いたときに反映
        public bool CustomCalendar
        {
            get => DemoSettings.CustomCalendar;
            set => DemoSettings.CustomCalendar = value;
        }

        public List<MainMenuItem> MenuItems { get; } =
        [
            new("予約登録"),        // SW004
            new("予約状況"),        // SW006
            new("勤務可能日登録"),  // SW007
            new("希望事業所登録"),  // SW008
        ];

        /// <summary>
        /// メニュー項目押下
        /// </summary>
        [RelayCommand]
        private async Task OpenMenu(MainMenuItem item)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                // 画面生成は UI スレッドを止めるため、先にオーバーレイを描画させる
                await Task.Delay(50);

                await (item.Title switch
                {
                    "予約登録" => _navigation.PushAsync<Yoyaku>(),
                    "予約状況" => _navigation.PushAsync<YoyakuList>(),
                    "勤務可能日登録" => _navigation.PushAsync<KinmuKanouDay>(),
                    "希望事業所登録" => _navigation.PushAsync<KibouJigyousho>(),
                    _ => Task.CompletedTask,
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// ログアウトボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            bool isConfirmed = await _dialog.ConfirmAsync("ログアウト", "ログアウトしてもよろしいですか？");
            if (isConfirmed)
            {
                _navigation.SetRoot<LoginPage>();
            }
        }
    }
}
