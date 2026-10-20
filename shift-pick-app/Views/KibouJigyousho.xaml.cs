using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class KibouJigyousho : ContentPage
{
    private readonly KibouJigyoushoViewModel _viewModel;

    public KibouJigyousho(KibouJigyoushoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// SW004 の「変更」から開く（登録後の戻り先を SW004 にする）
    /// </summary>
    public void OpenFromYoyaku() => _viewModel.FromYoyaku = true;

    /// <summary>
    /// 戻る（ナビゲーションバー・端末の戻るボタン共通）：2画面目なら1画面目へ、1画面目なら（未登録の変更を確認して）前の画面へ。
    /// どちらもオーバーレイを出す（処理中は受け付けない）
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        _viewModel.BackCommand.Execute(null);
        return true;
    }

    /// <summary>
    /// 都道府県・事業所リストの高さを固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
        {
            prefectureCard.HeightRequest = height * 0.75;
            officeCard.HeightRequest = height * 0.55;
        }
    }
}
