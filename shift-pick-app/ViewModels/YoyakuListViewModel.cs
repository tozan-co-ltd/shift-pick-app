using CommunityToolkit.Mvvm.ComponentModel;
using shift_pick_app.Models;

namespace shift_pick_app.ViewModels
{
    public partial class YoyakuListViewModel : BaseViewModel
    {
        // デモ用の固定値
        public List<QuickStatusItem> QuickStatus { get; } = DemoReservations.CountByStatus();

        public List<Reservation> Reservations => DemoReservations.Items;

        public string PreferredOffices => DemoReservations.PreferredOffices;
    }
}
