using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace mar_sumaken_app.Views;

using static mar_sumaken_app.Models.ShipmentScheduleModel;
using static mar_sumaken_app.Models.ToyotaWgEdiCommonFormat;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class RegisterStoreOutPage : ContentPage
{
    public DateTime selectedDate;
    public List<D_ShipmentSchedule> lstDShipmentSchedules;
    public string currentDate = string.Empty;
    private readonly LocalDbService _dbService;
    private LoginModel.LoginUserSqlLite _loginUserSqlLite;
    private HandySettingModel.HandySetting _handySetting;
    string firstScanContent = string.Empty;
    bool isFirstScan = true;
    string secondScanContent = string.Empty;
    string productCode = string.Empty;

    public RegisterStoreOutPage(string receiver, string currentDate, string selectedTrip, string place, List<D_ShipmentSchedule> dShipmentScheduleListSearch)
    {
        InitializeComponent();
        selectedReceiverLable.Text = $"{receiver}";
        selectedDateLabel.Text = $"{currentDate}";
        selectedTripLabel.Text = $"{selectedTrip}";
        selectedDate = DateTime.Parse(currentDate);
        selectedPlaceLabel.Text = $"{place}";
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        lstDShipmentSchedules = dShipmentScheduleListSearch;
        listMCompany = mCompanySqlLite;

        _dbService = new LocalDbService();
        // 進捗を数える
        int sumProgressBar = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxesRemaining);
        UpdateProgressBar(sumProgressBar);
        collectionView.ItemsSource = dShipmentScheduleListSearch;

        var newListDShipmentSchedule = dShipmentScheduleListSearch;
        foreach (var obj in newListDShipmentSchedule)
        {
            obj.NumScanAndNumBoxRemaining = $"1/{obj.NumberOfBoxesRemaining}";
        }
        collectionViewHistory.ItemsSource = newListDShipmentSchedule;

        if (newListDShipmentSchedule.Count == 0)
        {
            shippingListTable.IsVisible = false;
            emptyLabel.IsVisible = true;
        }

        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
    }

    /// <summary>
    /// ハンディリーダーでスキャンする時、処理する
    /// </summary>
    protected async void OnClipboardContentChanged(object sender, EventArgs e)
    {
        try
        {
            var storeOutRegisterPostBody = new StoreOutRegisterModel.StoreOutRegisterPostBody();
            var dHandyErrorMessageModel = new D_HandyErrorMessageModel();

            // ユーザー情報を取得
            var loginUsers = await _dbService.GetLoginAsync();
            _loginUserSqlLite = loginUsers.Last();

            var settings = await _dbService.GetSetting();
            // メニュー一覧を取得
            _handySetting = settings.Last();

            //　会社コードと会社パスワード
            storeOutRegisterPostBody.CompanyCode = _loginUserSqlLite.CompanyCode;
            storeOutRegisterPostBody.CompanyPassword = _loginUserSqlLite.CompanyPassword;

            //　QRコードのコンテンツを取得
            string result = Clipboard.GetTextAsync().Result;
            if (string.IsNullOrEmpty(result))
                ShowErrorTextInLabel("E2001 QRコードが読めません。");

            // QR中身を制御コードで区切れるよう変換する
            string[] changeCodeResult = GetQrcode(result);

            if (isFirstScan) 
            {
                if (changeCodeResult.Length < 17 && !result.Contains("[)>"))
                {
                    ShowErrorTextInLabel("E2001 納入先コードを読み取ってください。");
                    return;
                }

                productNumberLabel.Text = string.Empty;
                statusScanLabel.Text = string.Empty;

                string deliveryDateFormat = string.Empty;
                string deliveryDate = Regex.Replace(changeCodeResult[8], @"^+"+ StartIndexString.DeliveryDateIndex, "");
                if (!string.IsNullOrEmpty(deliveryDate))
                {
                    DateTime dateTime = DateTime.ParseExact(deliveryDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    deliveryDateFormat = dateTime.ToString("yyyy/MM/dd");
                }

                // 一回目コードの形式と日付をチェックすし、データを取得する
                if (result.Contains("[)>") && deliveryDateFormat == selectedDateLabel.Text)
                {
                    string location = Regex.Replace(changeCodeResult[5], @"^+" + StartIndexString.Location1Index, "");
                    string deliveryTimeClass = Regex.Replace(changeCodeResult[9], @"^+" + StartIndexString.DeliveryTimeClassIndex, "");
                    string productAbbreviation = Regex.Replace(changeCodeResult[11], @"^+" + StartIndexString.ProductAbbreviationIndex, "");
                    string productLabelBranchNumber = Regex.Replace(changeCodeResult[15], @"^+" + StartIndexString.ProductLabelBranchNumberIndex, "");
                    string deliverySlipNumberAndPage = Regex.Replace(changeCodeResult[10], @"^+" + StartIndexString.DeliverySlipNumber, "");
                    string deliverySlipNumber = deliverySlipNumberAndPage.Substring(0, deliverySlipNumberAndPage.Length - 4);
                    productCode = Regex.Replace(changeCodeResult[12], @"^+" + StartIndexString.ProductCodeIndex, "");

                    scanTextResult.Text = string.Empty;
                    scanTextResult.IsVisible = false;
                    productNumberLabel.Text = "①" + productCode;
                    productNumberLabel.IsVisible = true;
                    statusScanLabel.IsVisible = true;

                    firstScanContent = Clipboard.GetTextAsync().Result;
                }
                else
                {
                    ShowErrorTextInLabel("E2001 QRコードが読めません。");
                    return;
                }

                isFirstScan = false;
            }
            else　
            {
                string[] substrings = result.Split(' ');
                // 二回目コードの形式と日付をチェックすし、データを取得する
                if (substrings.Length > 3 && substrings[0].Trim() == "11" || substrings[0].Trim().IndexOf("Q01") != -1)
                {
                    secondScanContent = Clipboard.GetTextAsync().Result;
                    isFirstScan = true;
                }else if (result.Contains("[)>"))
                {
                    isFirstScan = true;
                    firstScanContent = string.Empty;
                    ShowErrorTextInLabel("E2001 QRコードが正しくありません、\n①納入先から読み取ってください。");
                    return;
                }
                else
                {
                    ShowErrorTextInLabel("E2001 QRコードが読めません。");
                    return;
                }
            }

            // データを処理
            if (!string.IsNullOrEmpty(firstScanContent) && !string.IsNullOrEmpty(secondScanContent))
            {
                // モデルに追加
                // ScanShipmentModel  
                // HandyErrorMessage   

                statusScanLabel.Text = "②スキャンOK";
                isFirstScan = true;

                // 前のデータを消す
                firstScanContent = string.Empty;
                secondScanContent = string.Empty;
                scanTextResult.Text = string.Empty;
                scanTextResult.IsVisible = false;
                productNumberLabel.Text = "①" + productCode;
                productNumberLabel.IsVisible = true;
                statusScanLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
        }
    }

    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    public void ShowErrorTextInLabel(string error)
    {
        scanTextResult.Text = error;
        scanTextResult.IsVisible = true;
        scanTextResult.TextColor = Colors.Red;
        productNumberLabel.Text = string.Empty;
        productNumberLabel.IsVisible = false;
        statusScanLabel.Text = string.Empty;
        statusScanLabel.IsVisible = false;
    }


    /// <summary>
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new DisplayShippingInstructionListPage(selectedReceiverLable.Text, selectedDate, selectedTripLabel.Text, selectedPlaceLabel.Text, listMCompany, lstDShipmentSchedules);
    }

    /// <summary>
    /// プログレスバーを更新する
    /// </summary>
    private async void UpdateProgressBar(int sumProgressBar)
    {
        try
        {
            progressBarValueLabel.Text = "10/"+sumProgressBar.ToString();
            progressBar.Progress = 0.5;
            progressBar.ProgressColor = Color.FromHex("#008080");

            if (progressBar.Progress == 1)
            {
                completeLabel.Text = "完了";
                completeLabel.TextColor = Color.FromHex("#F44E4B");
                progressBar.Progress = 1;
                progressBar.ProgressColor = Color.FromHex("#F44E4B");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }


    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnShippingListBtnClicked(object sender, EventArgs e)
    {
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        shippingList.IsVisible = true;
        historyList.IsVisible = false;
    }

    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        shippingListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        shippingList.IsVisible = false;
        historyList.IsVisible = true;
    }

    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            // OKサウンド
            SoundPlayer.PlaySound(resultOkSound);

            productNumberLabel.Text = string.Empty;
            statusScanLabel.Text = string.Empty;
            productNumberLabel.IsVisible = true;
            statusScanLabel.IsVisible = true;
            scanTextResult.Text = string.Empty;
            scanTextResult.IsVisible = false;

            await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "OK");
        }
        catch (Exception)
        {
            throw;
        }
    }
}
