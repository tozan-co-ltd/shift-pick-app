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
    /// SW004 で選択した内容を表示する。onRegistered：登録完了時に呼ぶ（SW004 の選択解除）
    /// </summary>
    public void SetSelection(List<YoyakuSlot> slots, Action onRegistered)
    {
        _viewModel.Slots = slots;
        _viewModel.OnRegistered = onRegistered;
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
    /// 予約内容テーブルの高さを画面の約55%に固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            tableCard.HeightRequest = height * 0.55;
    }
}
