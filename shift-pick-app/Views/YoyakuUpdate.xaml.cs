using shift_pick_app.Models;
using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class YoyakuUpdate : ContentPage
{
    private readonly YoyakuUpdateViewModel _viewModel;

    public YoyakuUpdate(YoyakuUpdateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// SW004 で選択した内容を表示する
    /// </summary>
    public void SetSelection(List<YoyakuSlot> slots) => _viewModel.Slots = slots;

    /// <summary>
    /// 予約内容テーブルの高さを画面の約55%に固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            tableCard.HeightRequest = height * 0.55;
    }
}
