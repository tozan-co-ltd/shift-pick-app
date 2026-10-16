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

        public MenuPageViewModel(NavigationService navigation)
        {
            _navigation = navigation;
        }

        [ObservableProperty]
        private string _userId = string.Empty;

        // デモ用の固定値
        public List<QuickStatusItem> QuickStatus { get; } = DemoReservations.CountByStatus();

        public List<string> News { get; } =
        [
            "お知らせ：○○○○○○○○○○",
            "お知らせ：○○○○○○○○○○○○○○○○○○○○",
            "お知らせ：○○○○○○○○○○",
            "お知らせ：○○○○○○○○○○",
            "お知らせ：○○○○○○○○○○" +
            "",
        ];

        // デモ用の一時的な切り替え：SW008 を開いたときに反映
        public bool PrefectureImageWithName
        {
            get => DemoSettings.PrefectureImageWithName;
            set => DemoSettings.PrefectureImageWithName = value;
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
                    "予約登録" => _navigation.PushAsync<Yoyaku>(page => page.SetUser(UserId)),
                    "予約状況" => _navigation.PushAsync<YoyakuList>(page => page.SetUser(UserId)),
                    "勤務可能日登録" => _navigation.PushAsync<KinmuKanouDay>(),
                    "希望事業所登録" => _navigation.PushAsync<KibouJigyousho>(page => page.SetUser(UserId)),
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
        private void Logout()
        {
            _navigation.SetRoot<LoginPage>();
        }
    }
}
