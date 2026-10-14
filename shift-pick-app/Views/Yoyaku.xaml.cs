using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class Yoyaku : ContentPage
{
    private readonly YoyakuViewModel _viewModel;

    public Yoyaku(YoyakuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// ログインしたユーザーを表示する
    /// </summary>
    public void SetUser(string userId) => _viewModel.UserId = userId;

    /// <summary>
    /// 予約テーブル（ヘッダー＋本体）の高さを画面の約55%に固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            tableCard.HeightRequest = height * 0.55;
    }
}
