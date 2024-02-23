using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace mar_sumaken_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private HandySettingModel.HandySetting _handySetting;
    private string? _handyApiUrl;
    public string? companyCode;
    public string? companyPassword;
    public decimal AppVersion;
    bool hasOnLoginClick = false; // ログインクリック

    public LoginPage()
    {
        InitializeComponent();
        _dbService = new LocalDbService();
        // ログインIDの設定
        SetLoginPage();
     }

    /// <summary>
    /// ログインIDの設定
    /// </summary>
    private async void SetLoginPage()
    {
        companyCode = string.Empty;
        companyPassword = string.Empty;
        AppVersion = 0;
        appVersion.Text = "ver" + AppInfo.VersionString;
        if (Decimal.TryParse(AppInfo.VersionString, out decimal appVer)) // アプリのバージョンをチェックとAPIを呼び出し
            AppVersion = appVer;

        var settings = await _dbService.GetSetting();
        if (settings.Count() > 0 && settings.Last() != null)
        {
            _handySetting = settings.Last();
            txtLoginId.Text = _handySetting.LoginID;
            companyCode = _handySetting.CompanyCode;
            companyPassword = _handySetting.CompanyPassword;
            _handyApiUrl = _handySetting.HandyApiUrl;
        }
        else
            txtLoginId.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(companyCode) && string.IsNullOrWhiteSpace(companyPassword))
            await Application.Current.MainPage.DisplayAlert("エラー", "「設定」から会社コードを登録してください。", "OK");
    }

    /// <summary>
    /// ログインボタンを押す
    /// </summary>
    /// <returns>メニュー画面</returns>
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (hasOnLoginClick)
                return;

            if (!NetworkCheck.CheckNetworkConnection()) // ネットワーク接続チェック
            {
                await Application.Current.MainPage.DisplayAlert("エラー", "E3003: ネットワークが利用できません。", "OK");
                hasOnLoginClick = false;
                return;
            }

            hasOnLoginClick = true;
            var settings = await _dbService.GetSetting();
            string loginId = txtLoginId.Text;
            var a = IsNotHalfWidthAlphanumeric(loginId);
           if (IsNotHalfWidthAlphanumeric(loginId) || (4 > loginId.Length || loginId.Length > 12)) // ログインIDンチェック
                await Application.Current.MainPage.DisplayAlert("エラー", "E2000: ログインIDが正しくありません。", "OK");
            else if (string.IsNullOrWhiteSpace(AppInfo.VersionString)) // アプリバージョンチェック
                await Application.Current.MainPage.DisplayAlert("エラー", "E2000: 使用ハンディメニューが登録されていません。", "OK");
            else
            {
                loginError.Text = string.Empty;
                var loginUserSqlLite = new LoginModel.LoginUserSqlLite();
                var handyPages = new M_HandyMenuModel[Const.C_NUMBER_OF_MENU];
                if (!Decimal.TryParse(AppInfo.VersionString, out decimal appVer)) // アプリのバージョンをチェックとAPIを呼び出し
                {
                    await DisplayAlert("エラー", "E2000: 使用ハンディメニューが登録されていません。", "OK");
                    hasOnLoginClick = false;
                    return;
                }

                var loginRequestBody = new
                    {
                        LoginID = loginId,
                        CompanyCode = _handySetting.CompanyCode,
                        CompanyPassword = _handySetting.CompanyPassword,
                        HandyAppVersion = Convert.ToDecimal(AppVersion)
                    };

                var jsonDataSend = JsonConvert.SerializeObject(loginRequestBody);
                // APIにリクエストを送る
                var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "Login");
                if (response.success)
                {
                    // APIからコンテンツを取得
                    var contentFromApi = JsonConvert.DeserializeObject<LoginModel.LoginResponseBody>(response.content);
                    loginUserSqlLite.CompanyCode = _handySetting.CompanyCode;
                    loginUserSqlLite.CompanyName = contentFromApi.CompanyName;
                    loginUserSqlLite.CompanyPassword = _handySetting.CompanyPassword;
                    loginUserSqlLite.LoginID = contentFromApi.User.LoginID;
                    loginUserSqlLite.UserName = contentFromApi.User.UserName;
                    loginUserSqlLite.DepoID = contentFromApi.Depo.DepoID;
                    loginUserSqlLite.DepoName = contentFromApi.Depo.DepoName;

                    handyPages = contentFromApi.HandyMenu;
                    //　ローカルDBに古いメニューを消す
                    await _dbService.ALLDeleteMenuAsync();
                    //　ローカルDBに保存
                    await _dbService.SaveLoginAsync(loginUserSqlLite);
                    foreach (var page in handyPages)
                        await _dbService.SavMenuAsync(page);

                    Application.Current.MainPage = new LoadingPage();
                    await Task.Delay(100);
                    Application.Current.MainPage = new MenuPage();
                }
                else
                    await DisplayAlert("エラー", "E2000:" +response.content, "OK");
            }

            hasOnLoginClick = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 半角英数字でないことをチェック
    /// </summary>
    /// <param name="text">string</param>
    /// <returns>Regex.IsMatch</returns>
    bool IsNotHalfWidthAlphanumeric(string text)
    {
        const string pattern = @"[^\u0020-\u007E]";
        return Regex.IsMatch(text, pattern);
    }

    /// <summary>
    /// 設定
    /// </summary>
    /// <returns>設定画面</returns>
    private async void OnSettingClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new HandySettingPage();
    }
}
