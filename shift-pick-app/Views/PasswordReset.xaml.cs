using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class PasswordReset : ContentPage
{
    private readonly PasswordResetViewModel _viewModel;

    public PasswordReset(PasswordResetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// ケース1（初回ログイン）として開く
    /// </summary>
    public void StartFirstLogin(string userId) => _viewModel.StartFirstLogin(userId);
}
