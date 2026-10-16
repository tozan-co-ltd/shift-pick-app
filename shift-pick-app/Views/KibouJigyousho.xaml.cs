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
    /// ログインしたユーザーを表示する
    /// </summary>
    public void SetUser(string userId) => _viewModel.UserId = userId;

    /// <summary>
    /// 都道府県・事業所リストの高さを固定する（回転時も再計算）
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (height > 0)
            prefectureCard.HeightRequest = officeCard.HeightRequest = height * 0.55;
    }
}
