using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
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

        public bool HasNews => News.Count > 0;

        // データなし：「通知はありません。」
        public bool IsNewsEmpty => !HasNews;

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
        private Task OpenMenu(MainMenuItem item) =>
            RunBusyAsync(LoadingMessage, () => item.Title switch
            {
                "予約登録" => _navigation.PushAsync<Yoyaku>(),
                "予約状況" => _navigation.PushAsync<YoyakuList>(),
                "勤務可能日登録" => _navigation.PushAsync<KinmuKanouDay>(),
                "希望事業所登録" => _navigation.PushAsync<KibouJigyousho>(),
                _ => Task.CompletedTask,
            });

        /// <summary>
        /// ログアウトボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            bool isConfirmed = await _dialog.ConfirmAsync("ログアウト", Util.PhraseJpText("|ログアウトしても|よろしいですか？"), "ログアウト", "キャンセル");
            if (isConfirmed)
            {
                _navigation.SetRoot<LoginPage>();
            }
        }
    }
}
