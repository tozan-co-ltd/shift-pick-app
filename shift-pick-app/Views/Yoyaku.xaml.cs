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
    /// SW008（希望事業所）から戻ったときに希望事業所を読み直す
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
    }

    /// <summary>
    /// 戻る（ナビゲーションバー・端末の戻るボタン共通）：オーバーレイを出して前の画面へ（処理中は受け付けない）
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        _viewModel.BackCommand.Execute(null);
        return true;
    }

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
