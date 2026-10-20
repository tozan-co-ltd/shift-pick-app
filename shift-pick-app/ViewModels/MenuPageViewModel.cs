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

        // デモ用の一時的な切り替え：SW004・SW006・SW008 を開いたときに反映（オン = デモの希望事業所 / オフ = 未登録）
        public bool HasPreferredOffices
        {
            get => DemoOffices.PreferredIds.Count > 0;
            set => DemoOffices.PreferredIds = value ? [.. DemoOffices.DefaultPreferredIds] : [];
        }

        /// <summary>
        /// メニューを開いたとき：SW008 の登録で変わった希望事業所をスイッチに反映
        /// </summary>
        public void RefreshDemoSettings() => OnPropertyChanged(nameof(HasPreferredOffices));

        // デモ用の一時的な切り替え：SW008 を開いたときに反映
        public bool PrefectureImageWithName
        {
            get => DemoSettings.PrefectureImageWithName;
            set => DemoSettings.PrefectureImageWithName = value;
        }

        // デモ用の一時的な切り替え：SW008 を開いたときに反映（オン = 地方の見出し付き / オフ = 設計書どおりの見出しなし）
        public bool PrefectureByRegion
        {
            get => DemoSettings.PrefectureByRegion;
            set => DemoSettings.PrefectureByRegion = value;
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
