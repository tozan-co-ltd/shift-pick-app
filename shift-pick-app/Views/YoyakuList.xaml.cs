using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class YoyakuList : ContentPage
{
    private readonly YoyakuListViewModel _viewModel;

    public YoyakuList(YoyakuListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
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
    /// 予約・実績リストの高さを画面の約55%に固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            listCard.HeightRequest = height * 0.55;
    }
}
