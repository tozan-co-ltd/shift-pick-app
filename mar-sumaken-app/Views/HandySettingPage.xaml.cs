using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;

namespace mar_sumaken_app.Views;

public partial class HandySettingPage : ContentPage
{
    List<string> scanModeList = new List<string> { "ハンディリーダー", "カメラ" };
    List<string> okSoundNameList = new List<string>();
    List<string> errorSsoundNameList = new List<string>();
    private readonly LocalDbService _dbService;
    private Setting.HandySetting _handySetting;
    private string _requestApiUrl;
    private bool _isChangedOkSound = true;
    private bool _isChangedErrorSound = true;
    public string txtUrlApi = string.Empty;
    public string companyCode = string.Empty;
    public string companyPassword = string.Empty;
    public string loginID = string.Empty;
    public HandySettingPage()
	{
        InitializeComponent();
        _dbService = new LocalDbService();

        // フィールドの設定
        SetHandySetting();
    }

    /// <summary>
    /// フィールドの設定
    /// </summary>
    private async void SetHandySetting()
    {
        // 接続先
        _requestApiUrl = "https://www.tozan.co.jp/sumaken-api-MRq2xg5/api/";
        txtUrl.Text = _requestApiUrl;
        txtUrlApi = txtUrl.Text;

        // スキャンモードの設定
        scanMode.ItemsSource = scanModeList;

        // サウンドピッカーの設定
        Sound sound = new Sound();
        okSoundNameList = sound.SoundOkeyList.Select(sound => sound.DisplayName).ToList();
        scanOkSound.ItemsSource = okSoundNameList;
        errorSsoundNameList = sound.SoundErrorList.Select(sound => sound.DisplayName).ToList();
        scanErrorSound.ItemsSource = errorSsoundNameList;

        var settings = await _dbService.GetSetting();
        if (settings.Count() > 0 && settings.Last() != null)
        {
            _handySetting = settings.Last();
            txtUrl.Text = _handySetting.HandyApiUrl;
            txtCompanyCode.Text = _handySetting.CompanyCode;
            txtLoginId.Text = _handySetting.LoginID;
            txtCompanyPassword.Text = _handySetting.CompanyPassword;
            scanMode.SelectedItem = _handySetting.ScanMode;
            scanOkSound.SelectedItem = _handySetting.ScanOkSound;
            scanErrorSound.SelectedItem = _handySetting.ScanErrorSound;
        }
        else
        {
            scanMode.SelectedItem = scanModeList.FirstOrDefault();
            scanOkSound.SelectedItem = okSoundNameList.FirstOrDefault();
            scanErrorSound.SelectedItem = errorSsoundNameList.FirstOrDefault();
        }
     }

    /// <summary>
    /// 戻る
    /// </summary>
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        var result = await Application.Current.MainPage.DisplayAlert("警告", "入力内容は破棄されます\n戻ってよろしいですか？", "OK", "キャンセル");
        if (result)
            Application.Current.MainPage = new LoginPage();
    }

    /// <summary>
    /// 登録
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var settingApiRequestBody = new Setting.SettingPostBody();

            companyCode = txtCompanyCode.Text;
            companyPassword = txtCompanyPassword.Text;
            loginID = txtLoginId.Text;
            settingApiRequestBody.CompanyCode = companyCode == null ? "" : companyCode.Trim();
            settingApiRequestBody.CompanyPassword = companyPassword == null ? "" : companyPassword.Trim();
            settingApiRequestBody.LoginID = loginID == null ? "" : loginID.Trim();

            if (txtUrlApi == "" || settingApiRequestBody.CompanyCode == "" || settingApiRequestBody.CompanyPassword == "" || settingApiRequestBody.LoginID == "")
            {
                await DisplayAlert("警告", "未入力項目があります。", "OK");
                return;
            }

            var jsonDataSend = JsonConvert.SerializeObject(settingApiRequestBody);
            // APIを接続
            var response = await App.API.PostMethod(jsonDataSend, txtUrlApi, "Setting");
            if (response.success)
            {
                await _dbService.Create(new Setting.HandySetting
                {
                    HandyApiUrl = txtUrlApi,
                    CompanyCode = settingApiRequestBody.CompanyCode,
                    CompanyPassword = settingApiRequestBody.CompanyPassword,
                    LoginID = settingApiRequestBody.LoginID,
                    ScanMode = (string)scanMode.SelectedItem,
                    ScanOkSound = (string)scanOkSound.SelectedItem,
                    ScanErrorSound = (string)scanErrorSound.SelectedItem
                });

                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
                await DisplayAlert("完了", "登録が完了しました。", "OK");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(100);
                Application.Current.MainPage = new LoginPage();
            }
            else
            {
                await DisplayAlert("警告", response.content, "OK");
                return;
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <summary>
    /// スキャンエラーサウンドが変更
    /// </summary>
    private void OnScanErrorSoundChanged(object sender, EventArgs e)
    { 
        if (_isChangedErrorSound)
        {
            _isChangedErrorSound = false;
            return;
        }
        string errorSoundNumber = new string(scanErrorSound.SelectedItem.ToString().Where(char.IsDigit).ToArray());
        scanErrorSound.SelectedItem = scanErrorSound.SelectedItem.ToString();

        //  サウンドのパスを取得する
        string resultErrorSound = "ScanErrorSound/ScanErrorSound" + errorSoundNumber + ".mp3";
        SoundPlayer.PlaySound(resultErrorSound);
    }

    /// <summary>
    /// スキャンエラーサウンドが変更
    /// </summary>
    private void OnScanOkSoundChanged(object sender, EventArgs e)
    {
        if (_isChangedOkSound)
        {
            _isChangedOkSound = false;
            return;
        }
        string okSoundNumber = new string(scanOkSound.SelectedItem.ToString().Where(char.IsDigit).ToArray());
        scanOkSound.SelectedItem = scanOkSound.SelectedItem.ToString();

        //  サウンドのパスを取得する
        string resultOkSound = "ScanOkeySound/ScanOkeySound" + okSoundNumber + ".mp3";
        SoundPlayer.PlaySound(resultOkSound);
    }

    /// <summary>
    /// スキャンモデルが変更
    /// </summary>
    private void OnScanModelChanged(object sender, EventArgs e)
    {
        scanMode.SelectedItem = scanMode.SelectedItem.ToString();
    }
}