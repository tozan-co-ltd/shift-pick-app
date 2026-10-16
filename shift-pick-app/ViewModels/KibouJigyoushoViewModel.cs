using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Models;
using shift_pick_app.Services;

namespace shift_pick_app.ViewModels
{
    public partial class KibouJigyoushoViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        public KibouJigyoushoViewModel(NavigationService navigation, DialogService dialog)
        {
            _navigation = navigation;
            _dialog = dialog;
        }

        [ObservableProperty]
        private string _userId = string.Empty;

        // false：都道府県選択 / true：希望の事業所
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPrefectureView))]
        private bool _isOfficeView;

        public bool IsPrefectureView => !IsOfficeView;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        // デモ用の固定値
        // 都道府県コード（JIS X 0401）順
        public List<Prefecture> Prefectures { get; } =
        [
            new("北海道"), new("青森県"), new("岩手県"), new("宮城県"), new("秋田県"),
            new("山形県"), new("福島県"), new("茨城県"), new("栃木県"), new("群馬県"),
            new("埼玉県"), new("千葉県"), new("東京都"), new("神奈川県"), new("新潟県"),
            new("富山県"), new("石川県"), new("福井県"), new("山梨県"), new("長野県"),
            new("岐阜県"), new("静岡県"), new("愛知県"), new("三重県"), new("滋賀県"),
            new("京都府"), new("大阪府"), new("兵庫県"), new("奈良県"), new("和歌山県"),
            new("鳥取県"), new("島根県"), new("岡山県"), new("広島県"), new("山口県"),
            new("徳島県"), new("香川県"), new("愛媛県"), new("高知県"), new("福岡県"),
            new("佐賀県"), new("長崎県"), new("熊本県"), new("大分県"), new("宮崎県"),
            new("鹿児島県"), new("沖縄県")
        ];

        private readonly List<Office> _allOffices =
        [
            new(1, "○○事業所", "○○市", 5.2),
            new(2, "○○事業所", "○○市", 1.3),
            new(3, "○○○○○○○○事業所", "○○市", 8.0),
            new(4, "○○事業所", "○○市", 3.4),
            new(5, "○○事業所", "○○市", 12.5),
            new(6, "○○事業所", "○○市", 9.1),
            new(7, "○○事業所", "○○市", 6.7),
            new(8, "○○事業所", "○○市", 15.0),
        ];

        [ObservableProperty]
        private List<Office> _offices = [];

        [ObservableProperty]
        private string _preferredOffices = string.Empty;

        [RelayCommand]
        private void TogglePrefecture(Prefecture prefecture) => prefecture.IsSelected = !prefecture.IsSelected;

        /// <summary>
        /// 都道府県選択：事業所ID順
        /// </summary>
        [RelayCommand]
        private void SelectByPrefecture() => ShowOffices([.. _allOffices.OrderBy(o => o.Id)]);

        /// <summary>
        /// GPSの近い事業所選択：現在位置から近い順（デモは固定の距離）
        /// </summary>
        [RelayCommand]
        private void SelectByGps() => ShowOffices([.. _allOffices.OrderBy(o => o.DistanceKm)]);

        private void ShowOffices(List<Office> offices)
        {
            Offices = offices;
            IsOfficeView = true;
        }

        /// <summary>
        /// 「予約」ボタン押下（選択の切り替え）
        /// </summary>
        [RelayCommand]
        private void ToggleOffice(Office office)
        {
            office.IsSelected = !office.IsSelected;
            ErrorMessage = string.Empty;
            PreferredOffices = string.Join("、", _allOffices.Where(o => o.IsSelected).Select(o => o.Name.Replace("事業所", "")));
        }

        [RelayCommand]
        private async Task Register()
        {
            ErrorMessage = string.Empty;
            if (!_allOffices.Any(o => o.IsSelected))
            {
                ErrorMessage = "希望事業所を選択してください。";
                return;
            }

            bool confirmed = await _dialog.ConfirmAsync("希望事業所登録", "選択した事業所を希望事業所として登録しますか？");
            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                // デモ：登録の代わり
                await Task.Delay(1000);
            }
            finally
            {
                IsBusy = false;
            }

            bool returnMenu = await _dialog.ConfirmAsync("登録しました", "希望事業所を登録しました。", "メインメニューへ", "閉じる");
            if (returnMenu)
            {
                await _navigation.PopAsync();
            }
        }
    }
}
