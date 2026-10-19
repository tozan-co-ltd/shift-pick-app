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
    /// 予約・実績リストの高さを画面の約55%に固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            listCard.HeightRequest = height * 0.55;
    }
}
