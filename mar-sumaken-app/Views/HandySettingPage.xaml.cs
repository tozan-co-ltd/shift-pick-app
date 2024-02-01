using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;

public partial class HandySettingPage : ContentPage
{
    List<string> items = new List<string> { "ハンディリーダー", "カメラ" };
    List<string> okSoundNameList = new List<string>();
    List<string> errorSsoundNameList = new List<string>();
    private readonly LocalDbService _dbService;
    private ScanAndSound _scanAndSound;
    private bool _isChangedOkSound = true;
    private bool _isChangedErrorSound = true;
    public HandySettingPage()
	{
        InitializeComponent();
        _dbService = new LocalDbService();
        Sound sound = new Sound();

        scanModel.ItemsSource = items;
        scanModel.SelectedItem = "ハンディリーダー";

        okSoundNameList = sound.SoundOkeyList.Select(sound => sound.DisplayName).ToList();
        scanOkSound.ItemsSource = okSoundNameList;

        errorSsoundNameList = sound.SoundErrorList.Select(sound => sound.DisplayName).ToList();
        scanErrorSound.ItemsSource = errorSsoundNameList;

        // サウンドピッカーの設定
        SetSoundPicker(okSoundNameList, errorSsoundNameList);
    }

    /// <summary>
    /// サウンドピッカーの設定
    /// </summary>
    /// <param name="okSoundNameList">OKサウン一覧</param>
    /// <param name="errorSsoundNameList">エラーサウン一覧</param>
    private async void SetSoundPicker(List<string> okSoundNameList, List<string> errorSsoundNameList)
    {
        var scanAndSoundList = await _dbService.GetScanAndSound();
        if (scanAndSoundList.Count() > 0)
        {
            _scanAndSound = scanAndSoundList.Last();
            if (_scanAndSound != null)
            {
                scanOkSound.SelectedItem = _scanAndSound.OkSound;
                scanErrorSound.SelectedItem = _scanAndSound.ErrorSound;
            }
            else
            {
                scanOkSound.SelectedItem = okSoundNameList.FirstOrDefault();
                scanErrorSound.SelectedItem = errorSsoundNameList.FirstOrDefault();
            }
        }
        else
        {
            scanOkSound.SelectedItem = okSoundNameList.First();
            scanErrorSound.SelectedItem = errorSsoundNameList.First();
        }
    }

    /// <summary>
    /// 戻る
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new LoginPage();
    }

    /// <summary>
    /// 登録
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        await _dbService.Create(new ScanAndSound
        {
            OkSound = (string)scanOkSound.SelectedItem,
            ErrorSound = (string)scanErrorSound.SelectedItem,
            ScandMode = (string)scanModel.SelectedItem
        });

        // サウンドのパスを取得する
        string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _scanAndSound);
        // OKサウンド
        SoundPlayer.PlaySound(resultOkSound);
        await DisplayAlert("完了", "登録が完了しました。", "OK");
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(100);
        Application.Current.MainPage = new LoginPage();
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
        scanModel.SelectedItem = scanModel.SelectedItem.ToString();
    }
}