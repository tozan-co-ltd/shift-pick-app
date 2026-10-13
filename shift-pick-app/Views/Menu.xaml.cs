using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class MenuPage : FlyoutPage
{
    private readonly MenuPageViewModel _viewModel;

    public MenuPage(MenuPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// ログインしたユーザーを表示する
    /// </summary>
    public void SetUser(string userId) => _viewModel.UserId = userId;
}
