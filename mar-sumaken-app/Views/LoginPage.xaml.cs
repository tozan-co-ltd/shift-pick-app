using mar_sumaken_app.Interface;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace mar_sumaken_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private Setting.HandySetting _handySetting;
    private string? _handyApiUrl;
    public string? companyCode;
    public string? companyPassword;
    public decimal AppVersion;
    public LoginPage()
    {
        InitializeComponent();
        _dbService = new LocalDbService();
        companyCode = string.Empty;
        companyPassword = string.Empty;
        AppVersion = 0;
        // ログインIDの設定
        SetLoginId();
     }

    /// <summary>
    /// ログインIDの設定
    /// </summary>
    private async void SetLoginId()
    {
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
        {
            txtLoginId.Text = string.Empty;
        }
    }

    /// <summary>
    /// ログインボタンを押す
    /// </summary>
    /// <returns>メニュー画面</returns>
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            string loginId = txtLoginId.Text;
            var a = IsNotHalfWidthAlphanumeric(loginId);
            if (string.IsNullOrEmpty(loginId) || IsNotHalfWidthAlphanumeric(loginId) || (4 > loginId.Length && loginId.Length > 12))
            {
                loginError.Text = "「設定」から会社コードを登録してください";
            }
            else
            {
                if (Decimal.TryParse(GetAppVersion(), out decimal appVersion)) // アプリのバージョンをチェックとAPIを呼び出し
                {
                    AppVersion = appVersion;
                    var loginRequestBody = new LoginModel.LoginRequestBody();
                    loginRequestBody.LoginID = loginId;
                    loginRequestBody.CompanyCode = _handySetting.CompanyCode;
                    loginRequestBody.CompanyPassword = _handySetting.CompanyPassword;
                    loginRequestBody.HandyAppVersion = Convert.ToDecimal(AppVersion);
                    try
                    {
                        var jsonDataSend = JsonConvert.SerializeObject(loginRequestBody);
                        var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "Login");
                        if (response.success)
                        {
                            Application.Current.MainPage = new LoadingPage();
                            await Task.Delay(100);
                            Application.Current.MainPage = new MenuPage();
                        }
                        else
                            await DisplayAlert("警告", response.content, "OK");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                    await DisplayAlert("警告", "アプリバージョンの取得に失敗しました。", "OK");

                return;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// アプリのバージョンを取得する
    /// </summary>
    /// <returns>アプリのバージョン</returns>
    private string GetAppVersion()
    {
        var version = "";
        try
        {
            if (Device.RuntimePlatform == Device.Android)
                version = AppInfo.VersionString;
            else if (Device.RuntimePlatform == Device.iOS)
                version = DependencyService.Get<IAssemblyService>().GetVersionName();
        }
        catch (Exception)
        {
            throw;
        }
        return version;
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
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(100);
        Application.Current.MainPage = new HandySettingPage();
    }
}
