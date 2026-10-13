using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class HandySettingPage : ContentPage
{
    public HandySettingPage(HandySettingPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
