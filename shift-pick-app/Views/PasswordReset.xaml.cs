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

    /// <summary>
    /// 戻る（端末の戻るボタン）：「戻る」ボタンと同じ（オーバーレイを出して前の画面へ、処理中は受け付けない）
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        _viewModel.BackCommand.Execute(null);
        return true;
    }
}
