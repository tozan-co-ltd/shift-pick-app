using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageViewModel _viewModel;

    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// ユーザーID でキーボードの「次へ」：パスワード欄へ移動
    /// </summary>
    private void OnUserIdCompleted(object? sender, EventArgs e) => passwordEntry.Focus();

    /// <summary>
    /// 戻る（端末の戻るボタン）：処理中は受け付けない
    /// </summary>
    protected override bool OnBackButtonPressed() => _viewModel.IsBusy || base.OnBackButtonPressed();
}
