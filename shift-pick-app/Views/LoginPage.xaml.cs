using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Properties;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace shift_pick_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private HandySettingModel.HandySetting _handySetting;
    private string? _handyApiUrl;
    public string? companyCode;
    public string? companyPassword;
    public decimal AppVersion;
    bool hasOnLoginClick = false; // ログインクリック

    /// <summary>
    /// ログイン画面表示
    /// </summary>
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

        // アプリのバージョン取得
        AppVersion = 0;
        //appVersion.Text = "ver" + AppInfo.VersionString;
        if (Decimal.TryParse(AppInfo.VersionString, out decimal appVer))  // アプリバージョンチェック
            AppVersion = appVer;
        else
            await DisplayAlert("エラー", $"E2007:  {ErrorMessagesResources.E2007}", "OK");

        // ローカルDBのハンディ設定情報取得
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

        // 会社コード・会社パスワードが設定されていない場合はエラー
        if (string.IsNullOrWhiteSpace(companyCode) && string.IsNullOrWhiteSpace(companyPassword))
            loginError.Text = "「設定」から会社コードを登録してください。";
    }

    /// <summary>
    /// ログインボタン押下
    /// </summary>
    /// <returns>メニュー画面</returns>
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            // ローカルDBのハンディ設定情報取得
            var settings = await _dbService.GetSetting();
            if (settings.Count() == 0) { await DisplayAlert("エラー", $"E2008:  {ErrorMessagesResources.E2008}", "OK"); hasOnLoginClick = false; return; }

            // 連続タップ制御
            if (hasOnLoginClick) return;

            // ネットワーク接続チェック
            if (!NetworkCheck.CheckNetworkConnection()) { await DisplayAlert("エラー", $"E3003:  {ErrorMessagesResources.E3003}", "OK"); hasOnLoginClick = false; return; }

            hasOnLoginClick = true;
            string loginId = txtLoginId.Text;

            // ログインIDチェック
            if (IsNotHalfWidthAlphanumeric(loginId) || (4 > loginId.Length || loginId.Length > 12))
                await DisplayAlert("エラー", $"E2003:  {ErrorMessagesResources.E2003}", "OK");
            else
            {
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(100);

                loginError.Text = string.Empty;
                var loginUserSqlLite = new LoginModel.LoginUserSqlLite();
                var handyPages = new List<M_HandyMenuModel>();
                var loginRequestBody = new
                {
                    LoginID = loginId,
                    CompanyCode = _handySetting.CompanyCode,
                    CompanyPassword = _handySetting.CompanyPassword,
                    HandyAppVersion = Convert.ToDecimal(AppVersion)
                };

                // API接続
                var jsonDataSend = JsonConvert.SerializeObject(loginRequestBody);
                var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "Login");
                if (response.success)
                {
                    //エラーメッセージを消す
                    loginError.Text = string.Empty;
                    // APIからコンテンツを取得
                    var contentFromApi = JsonConvert.DeserializeObject<LoginModel.LoginResponseBody>(response.content);
                    // メニュー作成
                    handyPages = contentFromApi.M_HandyMenuList;
                    // ログインユーザーSqliteの値を設定する
                    loginUserSqlLite = SetValueForLoginUserSqlLite(loginUserSqlLite, response.content);
                    // ローカルDBの古いメニューを削除
                    await _dbService.ALLDeleteMenuAsync();
                    // ローカルDBに保存
                    await _dbService.SaveLoginAsync(loginUserSqlLite);
                    foreach (var page in handyPages)
                        await _dbService.SaveMenuAsync(page);

                    // メニュー画面表示
                    Application.Current.MainPage = new MenuPage();
                }
                else
                {
                    await DisplayAlert("エラー", response.content, "OK");
                    hasOnLoginClick = false;
                    Application.Current.MainPage = new LoginPage();
                }
            }

            hasOnLoginClick = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", $"E9999: {ErrorMessagesResources.E9999}\n" + ex.Message, "OK");
            hasOnLoginClick = false;
            Application.Current.MainPage = new LoginPage();
        }
    }

    /// <summary>
    /// ログインユーザーSqliteの値を設定する
    /// </summary>
    /// <param name="loginUserSqlLite">LoginModel.LoginUserSqlLite</param>
    /// <param name="content">string</param>
    /// <returns>ログインユーザーSqliteの値</returns>
    public LoginModel.LoginUserSqlLite SetValueForLoginUserSqlLite(LoginModel.LoginUserSqlLite loginUserSqlLite, string content)
    {
        var contentFromApi = JsonConvert.DeserializeObject<LoginModel.LoginResponseBody>(content);
        loginUserSqlLite.CompanyCode = _handySetting.CompanyCode;
        loginUserSqlLite.CompanyName = contentFromApi.CompanyName;
        loginUserSqlLite.CompanyPassword = _handySetting.CompanyPassword;
        loginUserSqlLite.LoginID = contentFromApi.M_User.LoginID;
        loginUserSqlLite.UserID = contentFromApi.M_User.UserID;
        loginUserSqlLite.UserName = contentFromApi.M_User.UserName;
        loginUserSqlLite.Password = contentFromApi.M_User.Password;
        loginUserSqlLite.Salt = contentFromApi.M_User.Salt;
        loginUserSqlLite.DepoID = contentFromApi.M_Depo.DepoID;
        loginUserSqlLite.AuthorizedKubun = contentFromApi.M_User.AuthorizedKubun;
        loginUserSqlLite.DepoName = contentFromApi.M_Depo.DepoName;

        return loginUserSqlLite;
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
    /// ハンディ設定画面表示
    /// </summary>
    /// <returns>ハンディ設定画面</returns>
    private void OnSettingClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new HandySettingPage();
    }
}