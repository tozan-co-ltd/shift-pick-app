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
        public List<QuickStatusItem> QuickStatus { get; } =
        [
            new("予約", 3),
            new("確定", 2),
            new("NG", 1),
        ];

        public List<string> News { get; } =
        [
            "ニュース#1",
            "ニュース#2",
            "ニュース#3",
            "ニュース#4",
            "ニュース#5",
        ];

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
        private void OpenMenu(MainMenuItem item)
        {
            // TODO: SW004 / SW006 / SW007 / SW008 の画面作成後に遷移を追加
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
