using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace mar_sumaken_app.Views;

public partial class HandySettingPage : ContentPage
{
    List<string> scanModeList = new List<string> { "ハンディリーダー"};
    List<string> okSoundNameList = new List<string>();
    List<string> errorSsoundNameList = new List<string>();
    private readonly LocalDbService _dbService;
    private HandySettingModel.HandySetting _handySetting;
    private bool _isChangedOkSound = true;
    private bool _isChangedErrorSound = true;
    public string companyCode = string.Empty;
    public string companyPassword = string.Empty;
    public string loginID = string.Empty;
    bool hasRegiterClick = false; // 登録クリック

    /// <summary>
    /// ハンディ設定画面表示
    /// </summary>
    public HandySettingPage()
	{
        InitializeComponent();
        _dbService = new LocalDbService();

#if DEBUG
        // 開発環境(デバッグ用)
        txtUrl.Text  = "https://wbtzn/sumaken-api-MRq2xg5_test/api/";
#else
        // 本番環境
        txtUrl.Text = "https://www.tozan.co.jp/sumaken-api-MRq2xg5/api/";
#endif

        // フィールドの設定
        SetHandySetting();
    }

    /// <summary>
    /// フィールドの設定
    /// </summary>
    private async void SetHandySetting()
    {
        // スキャンモードの設定
        scanMode.ItemsSource = scanModeList;

        // サウンドピッカーの設定
        SoundModel sound = new SoundModel();
        okSoundNameList = sound.SoundOkeyList.Select(sound => sound.DisplayName).ToList();
        scanOkSound.ItemsSource = okSoundNameList;
        errorSsoundNameList = sound.SoundErrorList.Select(sound => sound.DisplayName).ToList();
        scanErrorSound.ItemsSource = errorSsoundNameList;

        // ローカルDBのハンディ設定情報取得
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
    /// 戻るボタン押下
    /// </summary>
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
         Application.Current.MainPage = new LoginPage();
    }

    /// <summary>
    /// 保存ボタン押下
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // 連続タップ制御
            if (hasRegiterClick)
                return;

            // ネットワーク接続チェック
            if (!NetworkCheck.CheckNetworkConnection())
            {
                await DisplayAlert("エラー", "E3003: ネットワークが利用できません。", "OK");
                hasRegiterClick = false;
                return;
            }

            hasRegiterClick = true;

            var settingApiRequestBody = new HandySettingModel.SettingPostBody();
            companyCode = txtCompanyCode.Text;
            companyPassword = txtCompanyPassword.Text;
            loginID = txtLoginId.Text;
            settingApiRequestBody.CompanyCode = companyCode == null ? "" : companyCode.Trim();
            settingApiRequestBody.CompanyPassword = companyPassword == null ? "" : companyPassword.Trim();
            settingApiRequestBody.LoginID = loginID == null ? "" : loginID.Trim();
            string txtUrlApi = txtUrl.Text;
            
            // 入力値チェック
            bool isValidUrl = Uri.TryCreate(txtUrlApi, UriKind.Absolute, out Uri result);
            if (string.IsNullOrWhiteSpace(txtUrlApi))
            {
                await DisplayAlert("エラー", "E2000: 接続先を入力してください。", "OK");
                hasRegiterClick = false;
                return;
            }
            if (!isValidUrl)
            {
                await DisplayAlert("エラー", "E2000: 接続先URLにアクセスできません。正しいURLを入力してください。", "OK");
                hasRegiterClick = false;
                return;
            }
       
            if (settingApiRequestBody.CompanyCode == "" || settingApiRequestBody.CompanyPassword == "")
            {
                await DisplayAlert("エラー", "E2000: 会社コード・会社パスワードはセットで入力してください。", "OK");
                hasRegiterClick = false;
                return;
            }

            // API接続
            var jsonDataSend = JsonConvert.SerializeObject(settingApiRequestBody);
            var response = await App.API.PostMethod(jsonDataSend, txtUrlApi, "Setting");
            if (response.success)
            {
                await _dbService.Create(new HandySettingModel.HandySetting
                {
                    HandyApiUrl = txtUrlApi,
                    CompanyCode = settingApiRequestBody.CompanyCode,
                    CompanyPassword = settingApiRequestBody.CompanyPassword,
                    LoginID = settingApiRequestBody.LoginID,
                    ScanMode = (string)scanMode.SelectedItem,
                    ScanOkSound = (string)scanOkSound.SelectedItem,
                    ScanErrorSound = (string)scanErrorSound.SelectedItem
                });

                // 保存したOKサウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                // OKサウンドを鳴らす
                SoundPlayer.PlaySound(resultOkSound);
                await DisplayAlert("完了", "登録が完了しました。", "OK");

                // ログイン画面表示
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(100);
                Application.Current.MainPage = new LoginPage();
            }
            else if (response.content.Contains("<!DOCTYPE html>"))
            {
                await DisplayAlert("エラー", "E2000: 接続先URLにアクセスできません", "OK");
                hasRegiterClick = false;
                return;
            }
            else
            {
                await DisplayAlert("エラー", response.content, "OK");
                hasRegiterClick = false;
                return;
            }

            hasRegiterClick = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
            throw;
        }
    }

    /// <summary>
    /// スキャンエラーサウンド変更
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
        string resultErrorSound = Const.C_SOUND_ERROR_FOLDER + errorSoundNumber + ".mp3";
        SoundPlayer.PlaySound(resultErrorSound);
    }

    /// <summary>
    /// スキャンエラーサウンド変更
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
        string resultOkSound = Const.C_SOUND_OK_FOLDER + okSoundNumber + ".mp3";
        SoundPlayer.PlaySound(resultOkSound);
    }

    /// <summary>
    /// スキャンモデル変更
    /// </summary>
    private void OnScanModelChanged(object sender, EventArgs e)
    {
        scanMode.SelectedItem = scanMode.SelectedItem.ToString();
    }
}