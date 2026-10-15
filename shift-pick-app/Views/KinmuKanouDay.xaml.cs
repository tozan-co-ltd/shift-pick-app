using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class KinmuKanouDay : ContentPage
{
    public KinmuKanouDay(KinmuKanouDayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
