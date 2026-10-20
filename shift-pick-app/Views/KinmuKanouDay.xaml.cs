using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class KinmuKanouDay : ContentPage
{
    private readonly KinmuKanouDayViewModel _viewModel;

    public KinmuKanouDay(KinmuKanouDayViewModel viewModel)
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
}
