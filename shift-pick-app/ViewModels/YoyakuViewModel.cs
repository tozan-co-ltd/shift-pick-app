using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class YoyakuViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;

        public YoyakuViewModel(NavigationService navigation)
        {
            _navigation = navigation;
        }

        [ObservableProperty]
        private string _userId = string.Empty;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        // デモ用の固定値（今日から5日分）
        public List<YoyakuSlot> Slots { get; } =
        [
            new(DateTime.Today, "○○事業所"),
            new(DateTime.Today, "○○事業所"),
            new(DateTime.Today.AddDays(1), "○○○○○○○○事業所"),
            new(DateTime.Today.AddDays(1), "○○事業所"),
            new(DateTime.Today.AddDays(1), "○○事業所"),
            new(DateTime.Today.AddDays(1), "○○事業所"),
            new(DateTime.Today.AddDays(2), "○○事業所"),
            new(DateTime.Today.AddDays(2), "○○事業所"),
            new(DateTime.Today.AddDays(3), "○○事業所"),
            new(DateTime.Today.AddDays(3), "○○事業所"),
            new(DateTime.Today.AddDays(4), "○○事業所"),
            new(DateTime.Today.AddDays(4), "○○事業所"),
        ];

        public string PreferredOffices => DemoReservations.PreferredOffices;

        /// <summary>
        /// 「予約」ボタン押下（選択の切り替え）
        /// </summary>
        [RelayCommand]
        private void ToggleSlot(YoyakuSlot slot)
        {
            slot.IsSelected = !slot.IsSelected;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// 希望事業所登録ボタン押下
        /// </summary>
        [RelayCommand]
        private void RegisterPreferredOffices()
        {
            // TODO: 希望事業所の絞り込み（方針検討中）
        }

        /// <summary>
        /// 選択ボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Select()
        {
            ErrorMessage = string.Empty;

            var selected = Slots.Where(s => s.IsSelected).ToList();
            if (selected.Count == 0)
            {
                ErrorMessage = "予約を選択してください。";
                return;
            }

            bool hasSameDay = selected.GroupBy(s => s.WorkDate).Any(g => g.Count() > 1);
            if (hasSameDay)
            {
                ErrorMessage = "同一日付では1つの事業所のみ選択できます。";
                return;
            }

            await _navigation.PushAsync<YoyakuUpdate>(page => page.SetSelection(UserId, selected));
        }
    }
}
