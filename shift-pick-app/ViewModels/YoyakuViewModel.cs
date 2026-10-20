using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class YoyakuViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        public YoyakuViewModel(NavigationService navigation, DialogService dialog)
        {
            _navigation = navigation;
            _dialog = dialog;
            UpdateSlots();
        }

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        // true：希望事業所 / false：全事業所
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAllOfficesTab))]
        private bool _isPreferredTab = true;

        public bool IsAllOfficesTab => !IsPreferredTab;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        // 申込で同一日付エラーになった後は、選択を変えるたびに赤字の行を再判定する
        private bool _isConflictShown;

        // デモ用の固定値（今日から5日分）。選択状態は画面を閉じるまでここに残る（日付・タブを変えても保持）
        private readonly List<YoyakuSlot> _allSlots =
        [
            new(DateTime.Today, 1),
            new(DateTime.Today, 2),
            new(DateTime.Today.AddDays(1), 3),
            new(DateTime.Today.AddDays(1), 4),
            new(DateTime.Today.AddDays(1), 5),
            new(DateTime.Today.AddDays(1), 6),
            new(DateTime.Today.AddDays(2), 1),
            new(DateTime.Today.AddDays(2), 7),
            new(DateTime.Today.AddDays(3), 5),
            new(DateTime.Today.AddDays(3), 8),
            new(DateTime.Today.AddDays(4), 2),
            new(DateTime.Today.AddDays(4), 6),
        ];

        /// <summary>
        /// 表示中の行：開始日付以降（希望事業所タブは希望事業所のみ）
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNoSlotsVisible))]
        private List<YoyakuSlot> _slots = [];

        public string PreferredTitle => $"希望事業所（{DemoOffices.PreferredIds.Count}件）";

        public string PreferredOffices => DemoOffices.PreferredText;

        public bool HasPreferredOffices => DemoOffices.PreferredIds.Count > 0;

        // 希望事業所タブ：開始日付の下に希望事業所を表示（未登録なら一覧に案内を出す）
        public bool IsPreferredSummaryVisible => IsPreferredTab && HasPreferredOffices;

        public bool IsNoPreferredVisible => IsPreferredTab && !HasPreferredOffices;

        public bool IsNoSlotsVisible => Slots.Count == 0 && !IsNoPreferredVisible;

        public int SelectedCount => _allSlots.Count(s => s.IsSelected);

        public bool HasSelection => SelectedCount > 0;

        public string SelectButtonText => HasSelection ? $"申込（{SelectedCount}件）" : "申込";

        partial void OnStartDateChanged(DateTime value) => UpdateSlots();

        partial void OnIsPreferredTabChanged(bool value) => UpdateSlots();

        /// <summary>
        /// 画面表示時（SW008 から戻ったときなど）：希望事業所を読み直す
        /// </summary>
        public void Refresh() => UpdateSlots();

        private void UpdateSlots()
        {
            var preferredIds = DemoOffices.PreferredIds;
            foreach (var slot in _allSlots)
                slot.IsPreferredMarkVisible = !IsPreferredTab && preferredIds.Contains(slot.OfficeId);

            Slots = [.. _allSlots.Where(s => s.WorkDate >= StartDate
                                          && (!IsPreferredTab || preferredIds.Contains(s.OfficeId)))];

            OnPropertyChanged(nameof(PreferredTitle));
            OnPropertyChanged(nameof(PreferredOffices));
            OnPropertyChanged(nameof(HasPreferredOffices));
            OnPropertyChanged(nameof(IsPreferredSummaryVisible));
            OnPropertyChanged(nameof(IsNoPreferredVisible));
            OnPropertyChanged(nameof(IsNoSlotsVisible));
        }

        private void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectButtonText));
            ClearSelectionCommand.NotifyCanExecuteChanged();
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            _isConflictShown = false;
        }

        /// <summary>
        /// 同一日付で複数選択している行に印を付ける（表示していない行も含む）。該当があれば true
        /// </summary>
        private bool UpdateConflicts()
        {
            var conflictDates = _allSlots.Where(s => s.IsSelected)
                                         .GroupBy(s => s.WorkDate)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();
            foreach (var slot in _allSlots)
                slot.IsConflict = slot.IsSelected && conflictDates.Contains(slot.WorkDate);

            return conflictDates.Count > 0;
        }

        [RelayCommand]
        private void ShowPreferredTab() => IsPreferredTab = true;

        [RelayCommand]
        private void ShowAllOfficesTab() => IsPreferredTab = false;

        /// <summary>
        /// 「選択」ボタン押下（選択の切り替え）
        /// </summary>
        [RelayCommand]
        private void ToggleSlot(YoyakuSlot slot)
        {
            slot.IsSelected = !slot.IsSelected;

            // 同一日付エラーの表示中：解消した日付は赤字を戻し、すべて解消したらエラーを消す
            if (!_isConflictShown || !UpdateConflicts())
                ClearError();

            OnSelectionChanged();
        }

        /// <summary>
        /// 選択解除ボタン押下（確認後、表示していない行も含めてすべて）
        /// </summary>
        [RelayCommand(CanExecute = nameof(HasSelection))]
        private async Task ClearSelection()
        {
            bool confirmed = await _dialog.ConfirmAsync("選択解除", Util.PhraseJpText("|すべての|選択を|解除しますか？"));
            if (confirmed)
                ResetSelection();
        }

        /// <summary>
        /// 選択をすべて解除する（選択解除・応募登録の完了時）
        /// </summary>
        private void ResetSelection()
        {
            foreach (var slot in _allSlots.Where(s => s.IsSelected))
                slot.IsSelected = false;
            UpdateConflicts();
            ClearError();
            OnSelectionChanged();
        }

        /// <summary>
        /// 戻る（ナビゲーションバー・端末の戻るボタン共通）→ SW003
        /// </summary>
        [RelayCommand]
        private Task Back() => RunBusyAsync(LoadingMessage, _navigation.PopAsync);

        /// <summary>
        /// 希望事業所の「変更」・「希望事業所を登録」押下 → SW008（登録後は SW004 に戻る）
        /// </summary>
        [RelayCommand]
        private Task OpenPreferredOffices() =>
            RunBusyAsync(LoadingMessage, () => _navigation.PushAsync<KibouJigyousho>(page => page.OpenFromYoyaku()));

        /// <summary>
        /// 申込ボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Select()
        {
            ClearError();

            var selected = _allSlots.Where(s => s.IsSelected).ToList();
            if (selected.Count == 0)
            {
                ErrorMessage = "予約を選択してください。";
                return;
            }

            // 同一日付では1つの事業所のみ：該当する行を赤字にする
            if (UpdateConflicts())
            {
                ErrorMessage = "同一日付では1つの事業所のみ選択できます。";
                _isConflictShown = true;
                return;
            }

            // 応募登録が完了したら選択を解除する（二重申込防止）
            await RunBusyAsync(LoadingMessage, () => _navigation.PushAsync<YoyakuUpdate>(page => page.SetSelection(selected, ResetSelection)));
        }
    }
}
