using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
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

            List<Prefecture> prefectures = [.. PrefectureData.Select((p, i) =>
                new Prefecture(i + 1, p.Name, $"{p.Key}.png", DemoOffices.All.Any(o => o.PrefectureId == i + 1)))];
            int start = 0;
            foreach (var (name, count) in RegionData)
            {
                PrefectureGroups.Add(new PrefectureGroup(name, prefectures.Skip(start).Take(count)));
                start += count;
            }

            // 登録済みの希望事業所を選択済みで表示（都道府県を変えても選択は保持）
            _allOffices = [.. DemoOffices.All.Select(o => new Office(o) { IsSelected = DemoOffices.PreferredIds.Contains(o.Id) })];
            UpdatePreferredSummary();
        }

        // false：都道府県選択 / true：希望の事業所
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPrefectureView))]
        private bool _isOfficeView;

        public bool IsPrefectureView => !IsOfficeView;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// SW004 の「変更」から開いた場合 true（登録後は SW004 に戻る）
        /// </summary>
        public bool FromYoyaku { get; set; }

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

        // 8地方区分（JIS 順で連続）：地方名と都道府県の数
        private static readonly (string Name, int Count)[] RegionData =
        [
            ("北海道", 1), ("東北", 6), ("関東", 7), ("中部", 9),
            ("近畿", 7), ("中国", 5), ("四国", 4), ("九州・沖縄", 8)
        ];

        // 都道府県カード：地方ごとのグループ（JIS 順）
        public List<PrefectureGroup> PrefectureGroups { get; } = [];

        // 全事業所
        private readonly List<Office> _allOffices;

        // 2画面目に表示する都道府県（null = 全都道府県）
        private int? _scopePrefectureId;

        // 現在地（この画面を開いている間だけ使い回す）
        private Location? _location;

        private static readonly TimeSpan LocationMaxAge = TimeSpan.FromMinutes(5);

        private static readonly string LocatingMessage = Util.PhraseJpText("|現在地を|取得中…");

        [ObservableProperty]
        private List<Office> _offices = [];

        /// <summary>
        /// 2画面目の見出し
        /// </summary>
        [ObservableProperty]
        private string _officeListTitle = string.Empty;

        // 並び順：true = 近い順 / false = 事業所ID順
        [ObservableProperty]
        private bool _isSortByDistance;

        // 選択中の希望事業所（2画面目のリストの下）：見出し（○件）と事業所名
        [ObservableProperty]
        private string _preferredTitle = string.Empty;

        [ObservableProperty]
        private string _preferredOffices = string.Empty;

        public bool HasSelection => _allOffices.Any(o => o.IsSelected);

        // 選択が登録済みの希望事業所と違う（未登録の変更あり）
        private bool HasUnsavedChanges =>
            !_allOffices.Where(o => o.IsSelected).Select(o => o.Id).ToHashSet().SetEquals(DemoOffices.PreferredIds);

        partial void OnIsSortByDistanceChanged(bool value) => SortOffices();

        /// <summary>
        /// 都道府県カード押下：その都道府県の事業所を事業所ID順で2画面目へ。
        /// 事業所がない都道府県はお知らせのみ
        /// </summary>
        [RelayCommand]
        private async Task SelectPrefecture(Prefecture prefecture)
        {
            if (!prefecture.IsAvailable)
            {
                await _dialog.AlertAsync("事業所なし", Util.PhraseJpText($"|{prefecture.Name}には|まだ|事業所が|ありません。"));
                return;
            }

            await RunBusyAsync(Util.PhraseJpText($"|{prefecture.Name}の|事業所を|読み込み中…"), () =>
            {
                ShowOffices(prefecture.Id, $"{prefecture.Name}の事業所", sortByDistance: false);
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// 現在地から近い事業所を探す：現在地を取得し、全事業所を近い順で2画面目へ
        /// </summary>
        [RelayCommand]
        private async Task SelectByGps()
        {
            if (IsBusy)
                return;

            bool located = false;
            await RunBusyAsync(LocatingMessage, async () =>
            {
                located = await UpdateLocationAsync();
                if (located)
                    ShowOffices(null, "現在地から近い事業所", sortByDistance: true);
            });

            if (!located)
                await ShowLocationErrorAsync();
        }

        [RelayCommand]
        private void SortById() => IsSortByDistance = false;

        /// <summary>
        /// 近い順：現在地がなければ先に取得する（取得できなければ事業所ID順のまま）
        /// </summary>
        [RelayCommand]
        private async Task SortByDistance()
        {
            if (IsSortByDistance || IsBusy)
                return;

            bool located = false;
            await RunBusyAsync(LocatingMessage, async () => located = await UpdateLocationAsync());

            if (located)
                IsSortByDistance = true;
            else
                await ShowLocationErrorAsync();
        }

        private void ShowOffices(int? prefectureId, string title, bool sortByDistance)
        {
            _scopePrefectureId = prefectureId;
            IsSortByDistance = sortByDistance;
            SortOffices();
            OfficeListTitle = $"{title}（{Offices.Count}件）";
            IsOfficeView = true;
        }

        private void SortOffices()
        {
            var offices = _allOffices.Where(o => _scopePrefectureId == null || o.PrefectureId == _scopePrefectureId);
            Offices = IsSortByDistance ? [.. offices.OrderBy(o => o.DistanceKm)] : [.. offices.OrderBy(o => o.Id)];
        }

        /// <summary>
        /// 現在地を取得して各事業所の距離を更新する。5分以内の現在地は使い回す
        /// </summary>
        private async Task<bool> UpdateLocationAsync()
        {
            if (_location == null || DateTimeOffset.Now - _location.Timestamp > LocationMaxAge)
            {
                try
                {
                    var location = await Geolocation.Default.GetLastKnownLocationAsync();
                    if (location == null || DateTimeOffset.Now - location.Timestamp > LocationMaxAge)
                        location = await Geolocation.Default.GetLocationAsync(
                            new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

                    if (location == null)
                        return false;
                    _location = location;
                }
                catch (Exception)
                {
                    // 権限なし・位置情報オフ・非対応
                    return false;
                }
            }

            foreach (var office in _allOffices)
            {
                office.DistanceKm = Location.CalculateDistance(_location, office.Location, DistanceUnits.Kilometers);
            }

            return true;
        }

        private Task ShowLocationErrorAsync() =>
            _dialog.AlertAsync("現在地の取得", Util.PhraseJpText("|現在地を|取得でき|ませんでした。\n|位置情報の|設定を|確認して|ください。"));

        /// <summary>
        /// 戻る（ナビゲーションバー・端末の戻るボタン共通）：
        /// 2画面目なら1画面目に戻す。1画面目なら、未登録の変更があれば確認してから前の画面へ
        /// </summary>
        [RelayCommand]
        private async Task Back()
        {
            if (IsBusy)
                return;

            if (IsOfficeView)
            {
                await RunBusyAsync(LoadingMessage, () =>
                {
                    ErrorMessage = string.Empty;
                    IsOfficeView = false;
                    return Task.CompletedTask;
                });
                return;
            }

            if (HasUnsavedChanges
                && !await _dialog.ConfirmAsync("変更の破棄", Util.PhraseJpText("|変更を|保存せずに|戻りますか？")))
                return;

            await RunBusyAsync(LoadingMessage, _navigation.PopAsync);
        }

        /// <summary>
        /// 「選択」ボタン押下（選択の切り替え）
        /// </summary>
        [RelayCommand]
        private void ToggleOffice(Office office)
        {
            office.IsSelected = !office.IsSelected;
            ErrorMessage = string.Empty;
            UpdatePreferredSummary();
        }

        /// <summary>
        /// 選択解除ボタン押下（確認後、他の都道府県で選んだ事業所も含めてすべて。登録するまで保存しない）
        /// </summary>
        [RelayCommand(CanExecute = nameof(HasSelection))]
        private async Task ClearSelection()
        {
            bool confirmed = await _dialog.ConfirmAsync("選択解除", Util.PhraseJpText("|すべての|選択を|解除しますか？"));
            if (!confirmed)
                return;

            foreach (var office in _allOffices.Where(o => o.IsSelected))
                office.IsSelected = false;
            ErrorMessage = string.Empty;
            UpdatePreferredSummary();
        }

        private void UpdatePreferredSummary()
        {
            var selected = _allOffices.Where(o => o.IsSelected).ToList();
            PreferredTitle = selected.Count > 0 ? $"希望事業所（{selected.Count}件）" : "希望事業所";
            PreferredOffices = selected.Count > 0 ? DemoOffices.ToText(selected.Select(o => o.Name)) : "未選択";
            ClearSelectionCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private async Task Register()
        {
            ErrorMessage = string.Empty;
            // 未選択でも保存できる（希望事業所なし）。その場合は確認文を変える
            string message = HasSelection
                ? Util.PhraseJpText("|選択した|事業所を|希望事業所として|保存しますか？")
                : Util.PhraseJpText("|事業所が|選択されていません。\n|希望事業所なしで|保存しますか？");
            bool confirmed = await _dialog.ConfirmAsync("希望事業所の保存", message);
            if (!confirmed)
                return;

            await RunBusyAsync(ProcessingMessage, async () =>
            {
                // デモ：保存の代わり
                await Task.Delay(1000);
                DemoOffices.PreferredIds = [.. _allOffices.Where(o => o.IsSelected).Select(o => o.Id)];
            });

            bool goBack = await _dialog.ShowResultAsync("保存しました", Util.PhraseJpText("|希望事業所を|保存しました。"),
                                                        FromYoyaku ? "予約に戻る" : "メインメニューへ", "閉じる");
            if (goBack)
            {
                // 登録済みのため確認なしで前の画面へ
                await RunBusyAsync(LoadingMessage, _navigation.PopAsync);
            }
        }
    }
}
