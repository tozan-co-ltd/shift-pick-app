using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
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
    private string? _handyApiUrl;
    public int handyMenuID;
    public int numberOfBoxes;
    public int quantity;
    public int shipmentScheduleID;
    bool isScanDelivery = true;
    string deliveryScanContent = string.Empty;
    string supplierScanContent = string.Empty;
    string productCode = string.Empty;
    public List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanShipmentModel> listScanShipment = new List<ScanShipmentModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public D_HandyErrorMessageModel d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
    public M_SupplierKanbanModel mSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
    public D_StoreOutModel.StoreOutPostBody storeOutPostBody = new D_StoreOutModel.StoreOutPostBody();

    public RegisterStoreOutPage(int menuID, string receiver, string currentDate, string selectedTrip, string place, List<M_CompanyModel> mCompanySqlLite, List<D_ShipmentSchedule> dShipmentScheduleListSearch, List<M_ProductModel> listProductItems)
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
        listProducts = listProductItems;
        handyMenuID = menuID;

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
            // サウンドパスを取得
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);

            // ユーザー情報を取得
            var loginUser = await _dbService.GetLoginAsync();
            _loginUserSqlLite = loginUser.Last();

            // 設定情報を取得
            var settings = await _dbService.GetSetting();
            _handyApiUrl = settings.Last().HandyApiUrl;

            //　QRコードのコンテンツを取得
            string clipboardContent = Clipboard.GetTextAsync().Result;
            if (string.IsNullOrEmpty(clipboardContent))
                ShowErrorTextInLabel("E2001 QRコードが読めません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);

            // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
            listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();
            listRHandyMenuSupplierKanban = await _dbService.GetRHandyMenuSupplierKanbanByHandyMenuID(handyMenuID);
            listMSupplierKanbanForCheckQRCode = await _dbService.GetMSupplierKanbanByHandySupplierKanbanID(listRHandyMenuSupplierKanban.FirstOrDefault().SupplierKanbanID);
            mSupplierKanban = listMSupplierKanbanForCheckQRCode.FirstOrDefault();

            if (isScanDelivery) 
            {
                if (clipboardContent.Trim().IndexOf("[)>") == -1){ShowErrorTextInLabel("E2001 納入先コードを読み取ってください。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return;}

                // 全てエラーテキストを削除
                productNumberLabel.Text = string.Empty;
                statusScanLabel.Text = string.Empty;

                string deliveryDateFormat = string.Empty;
                string deliveryDate = Regex.Replace(GetQrcode(clipboardContent)[8], @"^+"+ StartIndexString.DeliveryDateIndex, "");
                if (!string.IsNullOrEmpty(deliveryDate))
                {
                    DateTime dateTime = DateTime.ParseExact(deliveryDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    deliveryDateFormat = dateTime.ToString("yyyy/MM/dd");
                }

                // 一回目コードの形式と日付をチェックすし、データを取得する
                if (clipboardContent.Contains("[)>") && deliveryDateFormat == selectedDateLabel.Text)
                {
                    productCode = Regex.Replace(GetQrcode(clipboardContent)[12], @"^+" + StartIndexString.ProductCodeIndex, "");
                    // 出荷指示リストに存在納入先品番をチェック
                    //bool isExists = ExistsDeliveryProductNumberCheck(clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);
                    //if (!isExists) { ShowErrorTextInLabel("E2015:納入先品番が存在しません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                    //ーーーー仕入先かんばん読むことできるように サンプルデータ
                    var supplierProduct = lstDShipmentSchedules.FirstOrDefault();
                    quantity = supplierProduct.Quantity;
                    numberOfBoxes = supplierProduct.NumberOfBoxes;
                    shipmentScheduleID = supplierProduct.ShipmentScheduleID;
                    //ーーーーーーーーーーーーーーー サンプルデータ

                    scanTextResult.Text = string.Empty;
                    scanTextResult.IsVisible = false;
                    productNumberLabel.Text = "①" + productCode;
                    productNumberLabel.IsVisible = true;
                    statusScanLabel.IsVisible = true;
                    deliveryScanContent = Clipboard.GetTextAsync().Result;
                    // OKサウンド
                    SoundPlayer.PlaySound(resultOkSound);
                }
                else
                {
                    ShowErrorTextInLabel("E2001 QRコードが読めません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);
                    return;
                }

                isScanDelivery = false;
            }
            else　
            {
                string[] substrings = clipboardContent.Split(' ');
                // 二回目コードの形式と日付をチェックすし、データを取得する
                if (substrings.First().Trim().IndexOf(mSupplierKanban.IdentifyString) != -1)
                {
                    // マスターのプロダクトリストに存在仕入先品番をチェック
                    //bool isExists = ExistsSupplierProductNumberCheck(substrings, clipboardContent);
                    //if (!isExists) { ShowErrorTextInLabel("E2015: 仕入先品番が存在しません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                    supplierScanContent = Clipboard.GetTextAsync().Result;
                    isScanDelivery = true;
                    // OKサウンド
                    SoundPlayer.PlaySound(resultOkSound);
                }
                else if (clipboardContent.Contains("[)>"))
                {
                    isScanDelivery = true;
                    deliveryScanContent = string.Empty;
                    ShowErrorTextInLabel("E2001 QRコードが正しくありません、\n①納入先から読み取ってください。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);
                    return;
                }
                else
                {
                    ShowErrorTextInLabel("E2001 QRコードが読めません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);
                    return;
                }
            }
            /// 内容を確認してリストを追加 スキャン発送
            CheckScanContentAndAddListScanShipment();
        }
        catch (Exception ex)
        {
            // 全てコンテンツを消す
            deliveryScanContent = string.Empty;
            supplierScanContent = string.Empty;
            await DisplayAlert("エラー", ex.Message, "閉じる");
        }
    }

    /// <summary>
    /// 納入先品番をチェック
    /// </summary>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="resultErrorSound"></param>
    /// <returns></returns>
    public  bool ExistsDeliveryProductNumberCheck(string clipboardContent, int depoID, string userName)
    {
        // ユーザー情報を取得
        bool existsSupplierProductNumber = lstDShipmentSchedules.Any(product => product.SupplierProductNumber == productCode);
        if (!existsSupplierProductNumber)
        {
            d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, depoID, userName, clipboardContent, scanTextResult.Text);
            listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 重複品番をチェック
    /// </summary>
    /// <param name="substrings"></param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="resultErrorSound"></param>
    /// <returns></returns>
    public bool ExistsSupplierProductNumberCheck(string[] substrings, string clipboardContent) 
    {
        // ユーザー情報を取得
        string supplierProductNumberCheck = string.Empty;
        if (mSupplierKanban.IdentifyString == "11" && substrings.First().Trim().Length == mSupplierKanban.IdentifyString.Length)
        {
            // 文字列をトリム
            var supplierProductNumberTrim = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).ToString().Trim();
            if (supplierProductNumberTrim.IndexOf("-") != -1) // -の後ろの文字を削除する
                supplierProductNumberCheck = supplierProductNumberTrim.Substring(0, supplierProductNumberTrim.IndexOf('-'));
        }
        if (mSupplierKanban.IdentifyString == "Q01")
            supplierProductNumberCheck = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();

        bool existsSupplierProductNumber = listProducts.Any(product => product.SupplierProductNumber == supplierProductNumberCheck);
        if (!existsSupplierProductNumber)
            return false;

        return true;
    }

    /// <summary>
    /// 内容を確認してリストを追加 スキャン発送
    /// </summary>
    public async void CheckScanContentAndAddListScanShipment()
    {
        // ユーザー情報を取得
        var loginUser = await _dbService.GetLoginAsync();
        var scanShipmentModel = new ScanShipmentModel();
        var d_ScanResultModel = new D_ScanResultModel();
        var d_ShipmentModel = new D_ShipmentModel();
        // データを処理
        if (!string.IsNullOrEmpty(deliveryScanContent) && !string.IsNullOrEmpty(supplierScanContent))
        {
            // QR中身を制御コードで区切れるよう変換する
            string[] changeCodeResult = GetQrcode(deliveryScanContent);
            string location = Regex.Replace(changeCodeResult[5], @"^+" + StartIndexString.Location1Index, "");
            string deliveryTimeClass = Regex.Replace(changeCodeResult[9], @"^+" + StartIndexString.DeliveryTimeClassIndex, "");
            string productAbbreviation = Regex.Replace(changeCodeResult[11], @"^+" + StartIndexString.ProductAbbreviationIndex, "");
            int kanbanSerialNumber = Int32.Parse(Regex.Replace(changeCodeResult[15], @"^+" + StartIndexString.KanbanSerialNumberIndex, ""));
            string deliverySlipNumberAndPage = Regex.Replace(changeCodeResult[10], @"^+" + StartIndexString.DeliverySlipNumber, "");
            string deliverySlipNumber = deliverySlipNumberAndPage.Substring(0, deliverySlipNumberAndPage.Length - 4);
            productCode = Regex.Replace(changeCodeResult[12], @"^+" + StartIndexString.ProductCodeIndex, "");

            d_ShipmentModel = SetDShipmentModel(d_ShipmentModel, kanbanSerialNumber, productCode, numberOfBoxes, loginUser.Last().UserName, quantity, shipmentScheduleID);
            d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, deliveryScanContent, supplierScanContent, loginUser.Last().UserName);

            scanShipmentModel.d_ShipmentModel = d_ShipmentModel;
            scanShipmentModel.d_ScanResultModel = d_ScanResultModel;
            listScanShipment.Add(scanShipmentModel);

            statusScanLabel.Text = "②スキャンOK";
            isScanDelivery = true;

            // 前のデータを消す
            deliveryScanContent = string.Empty;
            supplierScanContent = string.Empty;
            scanTextResult.Text = string.Empty;
            scanTextResult.IsVisible = false;
            productNumberLabel.Text = "①" + productCode;
            productNumberLabel.IsVisible = true;
            statusScanLabel.IsVisible = true;
        }
    }


    /// <summary>
    /// 読取実績モデル作成
    /// </summary>
    /// <param name="d_ScanResultModel">読取実績モデル</param>
    /// <param name="depoID">倉庫ID</param>
    /// <param name="firstScanedString"></param>
    /// <param name="secondScanedString"></param>
    /// <param name="userName">ユーザー名</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoID, string firstScanedString, string secondScanedString, string userName)
    {
        d_ScanResultModel.DepoID = depoID;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.FirstScanedString = firstScanedString;
        d_ScanResultModel.SecondScanedString = secondScanedString;
        d_ScanResultModel.ScanedAt = DateTime.Now;
        d_ScanResultModel.CreatedAt = DateTime.Now;
        d_ScanResultModel.CreatedBy = userName;

        return d_ScanResultModel;
    }

    /// <summary>
    /// 入荷実績モデル作成
    /// </summary>
    /// <param name="d_ShipmentModel">入荷実績モデル</param>
    /// <param name="supplierProductNumber">仕入先品番</param>
    /// <param name="numberOfBoxes">入力箱数</param>
    /// <param name="quantity">数量</param>
    /// <returns></returns>
    public D_ShipmentModel SetDShipmentModel(D_ShipmentModel d_ShipmentModel, int kanbanSerialNumber, string supplierProductNumber, int numberOfBoxes, string userName, int quantity, int shipmentScheduleID)
    {
        d_ShipmentModel.ShipmentScheduleID = shipmentScheduleID;
        d_ShipmentModel.ShipmentDatetime = DateTime.Now;
        d_ShipmentModel.KanbanSerialNumber = kanbanSerialNumber;
        d_ShipmentModel.SupplierProductNumber = supplierProductNumber;
        d_ShipmentModel.LotNumber = "";
        d_ShipmentModel.MainProductKey = "";
        d_ShipmentModel.FirstSubProductKey = "";
        d_ShipmentModel.SecondSubProductKey = "";
        d_ShipmentModel.NumberOfBoxes = numberOfBoxes;
        d_ShipmentModel.Quantity = quantity;
        d_ShipmentModel.ScanedAt = DateTime.Now;
        d_ShipmentModel.CreatedAt = DateTime.Now;
        d_ShipmentModel.CreatedBy = userName;

        return d_ShipmentModel;
    }

    /// <summary>
    /// ハンディメッセージを設定
    /// </summary>
    /// <param name="d_HandyErrorMessageModel">ハンディエラーメッセージモデル</param>
    /// <param name="loginUser">ログインのユーザー</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="scanTextError">スキャンテキストエラー</param>
    /// <returns></returns>
    public D_HandyErrorMessageModel SetHandyErrorMessage(D_HandyErrorMessageModel d_HandyErrorMessageModel, int depoID, string userName, string clipboardContent, string scanTextError)
    {
        d_HandyErrorMessageModel.DepoID = depoID;
        d_HandyErrorMessageModel.HandyMenuID = handyMenuID;
        d_HandyErrorMessageModel.FirstScanedString = clipboardContent;
        d_HandyErrorMessageModel.ErrorMessage = scanTextError;
        d_HandyErrorMessageModel.CreatedAt = DateTime.Now;
        d_HandyErrorMessageModel.CreatedBy = userName;

        return d_HandyErrorMessageModel;
    }


    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    public void ShowErrorTextInLabel(string error, string resultErrorSound, string clipboardContent, int depoID, string userName)
    {
        // 既にスキャンしている場合はエラー
        SoundPlayer.PlaySound(resultErrorSound);
        scanTextResult.Text = error;
        scanTextResult.IsVisible = true;
        scanTextResult.TextColor = Colors.Red;
        productNumberLabel.Text = string.Empty;
        productNumberLabel.IsVisible = false;
        statusScanLabel.Text = string.Empty;
        statusScanLabel.IsVisible = false;

        // 識別文字列が正しくない場合はエラー
        d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, depoID, userName, clipboardContent, error);
        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
    }


    /// <summary>
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, selectedReceiverLable.Text, selectedDate, selectedTripLabel.Text, selectedPlaceLabel.Text, listMCompany, lstDShipmentSchedules, listProducts);
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
            //ストアアウトポストボディ設定
            storeOutPostBody.CompanyCode = _loginUserSqlLite.CompanyCode;
            storeOutPostBody.CompanyPassword = _loginUserSqlLite.CompanyPassword;
            storeOutPostBody.ScanShipmentModel = listScanShipment;
            storeOutPostBody.HandyErrorMessage = listDHandyErrorMessage;
            var jsonDataSend = JsonConvert.SerializeObject(storeOutPostBody);

            // ネットワーク接続チェック
            if (!NetworkCheck.CheckNetworkConnection())
            {
                await DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
                return;
            }

            // APIにリクエストを送る
            var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "StoreOut/Register");
            // APIからコンテンツを取得
            if (!response.success)
            {
                await DisplayAlert("エラー", response.content, "OK");
                return;
            }

            //テキスト値を設定し、成功登録後にラベルに表示されます
            SetTextValueAndVisibleForLabelAfterRegister();

            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            // OKサウンド
            SoundPlayer.PlaySound(resultOkSound);

            await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "OK");
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// テキスト値を設定し、成功登録後にラベルに表示されます
    /// </summary>
    public void SetTextValueAndVisibleForLabelAfterRegister()
    {
        productNumberLabel.Text = string.Empty;
        statusScanLabel.Text = string.Empty;
        productNumberLabel.IsVisible = true;
        statusScanLabel.IsVisible = true;
        scanTextResult.Text = string.Empty;
        scanTextResult.IsVisible = false;
        deliveryScanContent = string.Empty;
        supplierScanContent = string.Empty;
    }
}
