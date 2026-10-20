using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Models;
using shift_pick_app.Services;

namespace shift_pick_app.ViewModels
{
    public partial class YoyakuListViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;

        public YoyakuListViewModel(NavigationService navigation)
        {
            _navigation = navigation;
        }

        // デモ用の固定値
        public List<QuickStatusItem> QuickStatus { get; } = DemoReservations.CountByStatus();

        public List<Reservation> Reservations => DemoReservations.Items;

        public string PreferredOffices => DemoOffices.PreferredIds.Count > 0 ? DemoOffices.PreferredText : "未登録";

        /// <summary>
        /// 戻る（ナビゲーションバー・端末の戻るボタン共通）→ SW003
        /// </summary>
        [RelayCommand]
        private Task Back() => RunBusyAsync(LoadingMessage, _navigation.PopAsync);
    }
}
