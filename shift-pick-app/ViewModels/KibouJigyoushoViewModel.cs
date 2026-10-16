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

            bool withName = DemoSettings.PrefectureImageWithName;
            Prefectures = [.. PrefectureData.Select(p =>
                new Prefecture(p.Name, withName ? $"named_{p.Key}.png" : $"{p.Key}.png", !withName))];
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
        private static readonly (string Name, string Key)[] PrefectureData =
        [
            ("北海道", "hokkaido"), ("青森県", "aomori"), ("岩手県", "iwate"),
            ("宮城県", "miyagi"), ("秋田県", "akita"), ("山形県", "yamagata"),
            ("福島県", "fukushima"), ("茨城県", "ibaraki"), ("栃木県", "tochigi"),
            ("群馬県", "gunma"), ("埼玉県", "saitama"), ("千葉県", "chiba"),
            ("東京都", "tokyo"), ("神奈川県", "kanagawa"), ("新潟県", "niigata"),
            ("富山県", "toyama"), ("石川県", "ishikawa"), ("福井県", "fukui"),
            ("山梨県", "yamanashi"), ("長野県", "nagano"), ("岐阜県", "gifu"),
            ("静岡県", "shizuoka"), ("愛知県", "aichi"), ("三重県", "mie"),
            ("滋賀県", "shiga"), ("京都府", "kyoto"), ("大阪府", "osaka"),
            ("兵庫県", "hyougo"), ("奈良県", "nara"), ("和歌山県", "wakayama"),
            ("鳥取県", "tottori"), ("島根県", "shimane"), ("岡山県", "okayama"),
            ("広島県", "hiroshima"), ("山口県", "yamaguchi"), ("徳島県", "tokushima"),
            ("香川県", "kagawa"), ("愛媛県", "ehime"), ("高知県", "kouchi"),
            ("福岡県", "fukuoka"), ("佐賀県", "saga"), ("長崎県", "nagasaki"),
            ("熊本県", "kumamoto"), ("大分県", "ooita"), ("宮崎県", "miyazaki"),
            ("鹿児島県", "kagoshima"), ("沖縄県", "okinawa")
        ];

        public List<Prefecture> Prefectures { get; }

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
        /// 戻る押下：希望の事業所 → 都道府県選択に戻す。都道府県選択なら false（通常どおり前の画面へ）
        /// </summary>
        public bool TryBackToPrefectureView()
        {
            if (!IsOfficeView)
                return false;

            ErrorMessage = string.Empty;
            IsOfficeView = false;
            return true;
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
