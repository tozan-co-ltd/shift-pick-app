using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public string currentDate = string.Empty;
    private string? _handyApiUrl;
    public int companyID;
    public int handyMenuID;
    public int numberOfBoxes;
    public string mainProductKey;
    public int quantity;
    public int shipmentScheduleID;
    public bool isScanDelivery = true;
    public string deliveryScanContent = string.Empty;
    public string supplierScanContent = string.Empty;
    public string supplierIdentifyString = string.Empty;
    private readonly LocalDbService _dbService;
    private LoginModel.LoginUserSqlLite _loginUserSqlLite;
    private HandySettingModel.HandySetting _handySetting;
    public M_SupplierKanbanModel mSupplierKanban;
    public List<D_ShipmentSchedule> lstDShipmentSchedules;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
    public List<D_ShipmentModel> listDShipment;
    public DeliveryKanbanModel deliveryKanbanModel = new DeliveryKanbanModel();
    public List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanShipmentModel> listScanShipment = new List<ScanShipmentModel>();
    public List<M_ShipmentScheduleQRCodeModel> listMShipmentScheduleQRCode = new List<M_ShipmentScheduleQRCodeModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public D_HandyErrorMessageModel d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
    public D_StoreOutModel.StoreOutPostBody storeOutPostBody = new D_StoreOutModel.StoreOutPostBody();

    public RegisterStoreOutPage(int menuID, int selectedCompanyID, string deliveryName, string currentDate, string selectedDeliveryTimeClass, string deliveryLocation, List<M_CompanyModel> mCompanySqlLite,  List<D_ShipmentSchedule> dShipmentScheduleListSearch, List<D_ShipmentModel> dShipmentsListSearch, List<M_ProductModel> listProductItems, string DeliverySlipNumber)
    {
        InitializeComponent();
        selectedDeliveryNameLabel.Text = $"{deliveryName}";
        selectedDateLabel.Text = $"{currentDate}";
        selectedDeliveryTimeClassLabel.Text = $"{selectedDeliveryTimeClass}";
        selectedDate = DateTime.Parse(currentDate);
        selectedDeliveryLocationLabel.Text = $"{deliveryLocation}";
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        deliverySlipNumberLabel.Text = DeliverySlipNumber;
        listDShipment = dShipmentsListSearch;
        listProducts = listProductItems;
        listMCompany = mCompanySqlLite;
        handyMenuID = menuID;
        companyID = selectedCompanyID;
        _dbService = new LocalDbService();
        // 出庫画面の設定
        SetRegisterStoreOutPage();
        // 進捗を数える
        int sumProgressBar = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxesRemaining);
        UpdateProgressBar(sumProgressBar);
        // 残りが0の行が表示されない, 箱数降順
        lstDShipmentSchedules = dShipmentScheduleListSearch.Where(p => p.NumberOfBoxesRemaining != 0).OrderByDescending(p => p.NumberOfBoxes).ToList();

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

        // No昇順
        for (int i = 0; i < lstDShipmentSchedules.Count; i++)
            lstDShipmentSchedules[i].No = i + 1;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        collectionView.ItemsSource = lstDShipmentSchedules;
        // Subscribe to clipboard changes
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from clipboard changes to prevent memory leaks
        Clipboard.ClipboardContentChanged -= OnClipboardContentChanged;
    }


    /// <summary>
    /// 出庫画面の設定
    /// </summary>
    private async void SetRegisterStoreOutPage()
    {
        // ローカルDBの古いメニューを削除
        await _dbService.ALLDeleteDShipmentAsync();
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;

        // ローカルで出荷指示QRコードデータを取得
        listMShipmentScheduleQRCode = await _dbService.GetMShipmentScheduleQRCodeAsync();
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
            // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
            listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();

            // check NumberOfBoxesRemaining

            if (string.IsNullOrEmpty(clipboardContent))
                ShowErrorTextInLabel("E2001 QRコードが読めません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);

            string deliveryIdentifyString = string.Empty;
            if (isScanDelivery) 
            {
                // 一回目コードの形式と日付をチェックすし、データを取得する
                var deliveryQrIdentifyMatchStringList = listMShipmentScheduleQRCode.Where(x => x.IdentifyString == QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
                if(deliveryQrIdentifyMatchStringList.Count  <= 0 ) { ShowErrorTextInLabel("E2001 納入先コードを読み取ってください。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                deliveryIdentifyString = deliveryQrIdentifyMatchStringList.FirstOrDefault().IdentifyString;

                // 全てエラーテキストを削除
                productNumberLabel.Text = string.Empty;
                statusScanLabel.Text = string.Empty;
  
                // QR中身を制御コードで区切れるよう変換する
                SetValueFromQrCode(clipboardContent);

                //マスターのプロダクトリストに存在納入先品番をチェック
                bool existsDeliveryProductNumber = listProducts.Any(product => product.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && product.LotQuantity == deliveryKanbanModel.LotQuantity);
                if (!existsDeliveryProductNumber) { ShowErrorTextInLabel("E2015:納入先品番が存在しません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                // 出荷指示IDを取得して、ScanShipmentModelに入れる
                var dShipmentSchedule = ExistsDeliveryProductNumberCheck(loginUser.Last().DepoID, loginUser.Last().CompanyID, deliveryKanbanModel.DeliveryDate, deliveryKanbanModel.DeliveryTimeClass, deliveryKanbanModel.DeliveryLocation, deliveryKanbanModel.DeliverySlipNumber);
                //if (dShipmentSchedule == null) { ShowErrorTextInLabel("E2015: 出荷指示リストに一致するデータがありません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }
                //quantity = dShipmentSchedule.Quantity;
                //numberOfBoxes = dShipmentSchedule.NumberOfBoxes;
                //shipmentScheduleID = dShipmentSchedule.ShipmentScheduleID;

                //-----サンプルデータ
                quantity = lstDShipmentSchedules.FirstOrDefault().Quantity;
                numberOfBoxes = lstDShipmentSchedules.FirstOrDefault().NumberOfBoxes;
                shipmentScheduleID = lstDShipmentSchedules.FirstOrDefault().ShipmentScheduleID;

                //ローカルで出荷実績一覧を取得
                var listDShipmentSqlLite = await _dbService.GetDShipmentAsync();
                // ローカルで読取実績モデルの出荷実績モデルに出荷指示IDとシリアル番号(かんばん識別番号)をチェック
                bool isScanDuplicateInSqlLite = listDShipmentSqlLite.Any(item => item.ShipmentScheduleID == shipmentScheduleID && item.KanbanSerialNumber == deliveryKanbanModel.ProductLabelBranchNumber);
                if (isScanDuplicateInSqlLite) { ShowErrorTextInLabel("E2001: 既にスキャン済みの納入先かんばんです。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                // APIから出荷実績一覧に出荷指示IDとシリアル番号(かんばん識別番号)をチェック
                bool isScanDuplicate = listDShipment.Any(item => item.ShipmentScheduleID == shipmentScheduleID && item.KanbanSerialNumber == deliveryKanbanModel.ProductLabelBranchNumber);
                if (isScanDuplicate) { ShowErrorTextInLabel("E2001: 既にスキャン済みの納入先かんばんです。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                // 納入先コードスキャンが成功した場合のラベル値を設定
                SetValueWhenSuccessScanForDelivery();

                deliveryScanContent = Clipboard.GetTextAsync().Result;
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);

                isScanDelivery = false;
            }
            else　
            {
                // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
                var supplierIdentifyMatchStringList = listMSupplierKanban.Where(x => x.IdentifyString == QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
                supplierIdentifyString = supplierIdentifyMatchStringList.FirstOrDefault().IdentifyString;
                mSupplierKanban = supplierIdentifyMatchStringList.FirstOrDefault();
                Match match = Regex.Match(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim(), @"^\d+");
                int quantity = int.Parse(match.Value);

                // マスターのプロダクトリストに存在仕入先品番をチェック
                bool isExists = ExistSupplierProductNumberCheck(supplierIdentifyString, clipboardContent, quantity);
                if (!isExists) { ShowErrorTextInLabel("E2015: QRコードが正しくありません。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName); return; }

                // 許可された重複チェック
                if (!mSupplierKanban.AllowedDuplicatesFlag && mSupplierKanban.MainProductKeyLength != 0)
                {
                    var supplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
                    var firstSubProductKey = string.Empty;
                    var secondSubProductKey = string.Empty;
                    mainProductKey = clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();
                    var isDuplicates = listDShipment.Any(x =>x.SupplierProductNumber == supplierProductNumber && x.MainProductKey == mainProductKey && x.FirstSubProductKey == firstSubProductKey && x.SecondSubProductKey == secondSubProductKey);
                    if (isDuplicates)  // 仕入先品番重複チェック
                    {
                        // [共通関数]エラーメッセージ表示実績作成その後、エラー解除要否チェックをする。
                        // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                        //とハンディエラーメッセージ実績挿入
                        //そうでなければ[共通関数]エラーメッセージ表示
                        //エラーコード： 既にスキャン済みの仕入先かんばんです
                        ShowErrorTextInLabel("E2015: 既にスキャン済みの仕入先かんばんです。", resultErrorSound, clipboardContent, loginUser.Last().DepoID, loginUser.Last().UserName);
                        return;
                    }
                }

                var product = lstDShipmentSchedules.Find(p => p.DeliveryProductNumber == "6173961");
                if (product != null)
                {
                    product.NumberOfBoxesRemaining--;
                    collectionView.ItemsSource = null;
                    collectionView.ItemsSource = lstDShipmentSchedules;
                }

                supplierScanContent = Clipboard.GetTextAsync().Result;
                isScanDelivery = true;
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
            }

            // 内容を確認してリストを追加 スキャン発送
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

    public void CheckIsDuplicateSupplier(string clipboardContent, string resultErrorSound, int depoID, string userName)
    {
        if (!mSupplierKanban.AllowedDuplicatesFlag)
        {
            var firstSubProductKey = string.Empty;
            var secondSubProductKey = string.Empty;
            mainProductKey = clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();
            var isDuplicates = listDShipment.Any(x => x.MainProductKey == mainProductKey && x.FirstSubProductKey == firstSubProductKey && x.SecondSubProductKey == secondSubProductKey);
            if (isDuplicates)
            {
                // [共通関数]エラーメッセージ表示実績作成その後、エラー解除要否チェックをする。
                // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                //とハンディエラーメッセージ実績挿入
                //そうでなければ[共通関数]エラーメッセージ表示
                //エラーコード： 既にスキャン済みの仕入先かんばんです
                ShowErrorTextInLabel("E2015: 既にスキャン済みの仕入先かんばんです。", resultErrorSound, clipboardContent, depoID, userName);
                return;
            }
            else
            {
                ShowErrorTextInLabel("E2015: 重複不可です。", resultErrorSound, clipboardContent, depoID, userName);
                return;
            }
        }
        
    }

    public static string QrcodeValueSubstring(int index, int stringLength, string qr)
    {
        var value = qr.Substring(index - 1, stringLength).Replace(" ", "");
        return value;
    }

    /// <summary>
    /// 納入先コードスキャンが成功した場合のラベル値を設定
    /// </summary>
    public void SetValueWhenSuccessScanForDelivery()
    {
        scanTextResult.Text = string.Empty;
        scanTextResult.IsVisible = false;
        productNumberLabel.Text = "①" + deliveryKanbanModel.DeliveryProductNumber;
        productNumberLabel.IsVisible = true;
        statusScanLabel.IsVisible = true;
    }

    /// <summary>
    /// QR中身を制御コードで区切れるよう変換する
    /// </summary>
    /// <param name="clipboardContent"></param>
    public void SetValueFromQrCode(string clipboardContent)
    {
        string[] changeCodeResult = GetQrcode(clipboardContent);
        deliveryKanbanModel.DeliveryProductNumber = Regex.Replace(changeCodeResult[12], @"^+" + StartIndexString.ProductCodeIndex, "");
        deliveryKanbanModel.DeliveryLocation = Regex.Replace(changeCodeResult[5], @"^+" + StartIndexString.Location1Index, "");
        deliveryKanbanModel.DeliveryTimeClass = Int32.Parse(Regex.Replace(changeCodeResult[9], @"^+" + StartIndexString.DeliveryTimeClassIndex, ""));
        deliveryKanbanModel.ProductAbbreviation = Regex.Replace(changeCodeResult[11], @"^+" + StartIndexString.ProductAbbreviationIndex, "");
        deliveryKanbanModel.ProductLabelBranchNumber = Int32.Parse(Regex.Replace(changeCodeResult[15], @"^+" + StartIndexString.KanbanSerialNumberIndex, ""));
        var deliverySlipNumberAndPage = Regex.Replace(changeCodeResult[10], @"^+" + StartIndexString.DeliverySlipNumber, "");
        deliveryKanbanModel.DeliverySlipNumber = deliverySlipNumberAndPage.Substring(0, deliverySlipNumberAndPage.Length - 4);
        var deliveryDateString = Regex.Replace(changeCodeResult[8], @"^+" + StartIndexString.DeliveryDateIndex, "");
        deliveryKanbanModel.DeliveryDate = DateTime.ParseExact(deliveryDateString, "yyyyMMdd", CultureInfo.InvariantCulture);
        deliveryKanbanModel.DeliveryProductNumber = Regex.Replace(changeCodeResult[12], @"^+" + StartIndexString.ProductCodeIndex, "");
        deliveryKanbanModel.LotQuantity =int.Parse(Regex.Replace(changeCodeResult[13], @"^+" + StartIndexString.QuantityIndex, ""));
    }


    /// <summary>
    /// 納入先品番をチェック
    /// </summary>
    /// <param name="depoID">倉庫ID</param>
    /// <param name="companyID">会社ID</param>
    /// <param name="deliveryDate">納入指示日</param>
    /// <param name="deliveryTimeClass">便</param>
    /// <param name="deliveryLocation">納入場所</param>
    /// <param name="deliverySlipNumber">納品書番号</param>
    /// <returns></returns>
    public D_ShipmentSchedule ExistsDeliveryProductNumberCheck(int depoID, int companyID, DateTime deliveryDate, int deliveryTimeClass, string deliveryLocation, string deliverySlipNumber)
    {
        var d_ShipmentSchedule = new D_ShipmentSchedule();
         d_ShipmentSchedule = lstDShipmentSchedules.Where(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == depoID && d.CompanyID == companyID && 
         d.DeliveryDate == deliveryDate && d.DeliveryTimeClass == deliveryTimeClass && d.DeliveryLocation == deliveryLocation && d.DeliverySlipNumber == deliverySlipNumber).FirstOrDefault();
        if (d_ShipmentSchedule != null)
            return d_ShipmentSchedule;

        return d_ShipmentSchedule;
    }

    /// <summary>
    /// 重複品番をチェック
    /// </summary>
    /// <param name="substrings"></param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="resultErrorSound"></param>
    /// <returns></returns>
    public bool ExistSupplierProductNumberCheck(string supplierIdentifyString, string clipboardContent, int quantity) 
    {
        // ユーザー情報を取得
        string supplierProductNumberCheck = string.Empty;
        if (mSupplierKanban.IdentifyString == supplierIdentifyString && supplierIdentifyString.Length == mSupplierKanban.IdentifyString.Length) // 識別文字は11
        {
            // 文字列をトリム
            var supplierProductNumberTrim = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).ToString().Trim();
            if (supplierProductNumberTrim.IndexOf("-") != -1) // -の後ろの文字を削除する
                supplierProductNumberCheck = supplierProductNumberTrim.Substring(0, supplierProductNumberTrim.IndexOf('-'));
            else
                supplierProductNumberCheck = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
        }

        bool existsSupplierProductNumber = listProducts.Any(product => product.SupplierProductNumber == supplierProductNumberCheck && product.LotQuantity == quantity);
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
            d_ShipmentModel = SetDShipmentModel(d_ShipmentModel, deliveryKanbanModel.ProductLabelBranchNumber, deliveryKanbanModel.DeliveryProductNumber, numberOfBoxes, loginUser.Last().UserName, quantity, shipmentScheduleID);
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
            productNumberLabel.Text = "①" + deliveryKanbanModel.DeliveryProductNumber;
            productNumberLabel.IsVisible = true;
            statusScanLabel.IsVisible = true;

            // スキャン出来たら、ローカルで出荷実績モデルを保存
            d_ShipmentModel.ShipmentScheduleID = shipmentScheduleID;
            d_ShipmentModel.KanbanSerialNumber = deliveryKanbanModel.ProductLabelBranchNumber;
            await _dbService.SaveDShipmentAsync(d_ShipmentModel);
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
    public D_ShipmentModel SetDShipmentModel(D_ShipmentModel d_ShipmentModel, int ProductLabelBranchNumber, string supplierProductNumber, int numberOfBoxes, string userName, int quantity, int shipmentScheduleID)
    {
        d_ShipmentModel.ShipmentScheduleID = shipmentScheduleID;
        d_ShipmentModel.ShipmentDatetime = DateTime.Now;
        d_ShipmentModel.KanbanSerialNumber = ProductLabelBranchNumber;
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
        isScanDelivery = true;
        Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, companyID, selectedDeliveryNameLabel.Text, selectedDate, selectedDeliveryTimeClassLabel.Text, selectedDeliveryLocationLabel.Text, listMCompany, lstDShipmentSchedules, listProducts, deliverySlipNumberLabel.Text);
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
