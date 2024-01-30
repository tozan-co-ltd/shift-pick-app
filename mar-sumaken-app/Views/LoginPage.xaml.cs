namespace mar_sumaken_app.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// ログイン
    /// </summary>
    /// <param name="model">AccountModel</param>
    /// <returns>メニュー画面</returns>
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            string loginId = txtUser.Text;
            if (loginId != "test01")
            {
                loginError.Text = "「設定」から会社コードを登録してください";
            }
            else
            {
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(100);
                Application.Current.MainPage = new MenuPage();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 設定
    /// </summary>
    /// <returns>設定画面</returns>
    private async void OnSettingClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(100);
        Application.Current.MainPage = new HandySettingPage();
    }
}
