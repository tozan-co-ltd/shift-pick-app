using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.Maui;
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
    private string? _handyApiUrl;
    public int overallProgress;
    public int companyID;
    public int handyMenuID;
    public int numberOfBoxes = 1;
    public string mainProductKey;
    public int quantity;
    public int shipmentScheduleID;
    int totalNumberOfBoxes;
    int totalNumberOfBoxesRemaining;
    public bool isScanDelivery = true;
    public string deliveryScanContent = string.Empty;
    public string supplierScanContent = string.Empty;
    public string supplierIdentifyString = string.Empty;
    private string? resultOkSound = string.Empty;
    private string? resultErrorSound = string.Empty;
    private readonly LocalDbService _dbService;
    private LoginModel.LoginUserSqlLite _loginUserSqlLite;
    private HandySettingModel.HandySetting _handySetting;
    public M_SupplierKanbanModel mSupplierKanban;
    public List<D_ShipmentSchedule> lstDShipmentSchedules;
    public List<D_ShipmentSchedule> lstDShipmentSchedulesForCheckDuplicate;
    public List<D_ShipmentSchedule> lstHistoryDShipmentSchedules;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
    public List<D_ShipmentModel> listDShipment;
    public List<D_ShipmentModel> listDShipmentSqlLite = new List<D_ShipmentModel>();
    public List<M_UserModel> mUserList= new List<M_UserModel>();
    public D_ShipmentModel dShipmentLocal = new D_ShipmentModel();
    public DeliveryKanbanModel deliveryKanbanModel = new DeliveryKanbanModel();
    public List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public ScanShipmentModel scanShipmentModel = new ScanShipmentModel();
    public List<ScanShipmentModel> listScanShipment = new List<ScanShipmentModel>();
    public List<M_ShipmentScheduleQRCodeModel> listMShipmentScheduleQRCode = new List<M_ShipmentScheduleQRCodeModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public D_HandyErrorMessageModel d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
    public D_StoreOutModel.StoreOutPostBody storeOutPostBody = new D_StoreOutModel.StoreOutPostBody();

    public RegisterStoreOutPage(int menuID, int selectedCompanyID, string deliveryName, string currentDate, string selectedDeliveryTimeClass, string deliveryLocation, List<M_CompanyModel> mCompanySqlLite,  List<D_ShipmentSchedule> dShipmentScheduleListSearch, List<D_ShipmentModel> dShipmentsListSearch, List<M_UserModel> mUserListSearch, List<M_ProductModel> listProductItems, string DeliverySlipNumber)
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
        mUserList = mUserListSearch;
        handyMenuID = menuID;
        companyID = selectedCompanyID;
        _dbService = new LocalDbService();
        // 未検品の出荷指示が無い
        if (dShipmentScheduleListSearch.Count == 0)
        {
            shippingListTable.IsVisible = false;
            emptyLabel.IsVisible = true;
        }

        // 出庫画面の設定
        SetRegisterStoreOutPage();

        // 進捗を数える
        totalNumberOfBoxesRemaining = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxesRemaining);
        totalNumberOfBoxes = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxes);
        UpdateProgressBar(totalNumberOfBoxes, totalNumberOfBoxesRemaining);

        // 履歴一覧用
        lstHistoryDShipmentSchedules = dShipmentScheduleListSearch;

        // 重複をチェック用
        lstDShipmentSchedulesForCheckDuplicate = dShipmentScheduleListSearch;

        // 残りが0の行が表示されない, 箱数降順
        lstDShipmentSchedules = dShipmentScheduleListSearch.Where(p => p.NumberOfBoxesRemaining != 0).OrderByDescending(p => p.NumberOfBoxes).ToList();

        // No昇順
        for (int i = 0; i < lstDShipmentSchedules.Count; i++)
            lstDShipmentSchedules[i].No = i + 1;

    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        collectionView.ItemsSource = lstDShipmentSchedules;
        // Subscribe to clipboard changes
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
    }

    /// <summary>
    /// 画面をが閉じるイベントをサブスクライブします
    /// </summary>
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

        // ユーザー情報を取得
        var loginUser = await _dbService.GetLoginAsync();
        _loginUserSqlLite = loginUser.Last();

        // 設定情報を取得
        var settings = await _dbService.GetSetting();
        _handyApiUrl = settings.Last().HandyApiUrl;
      
        // ローカルで出荷指示QRコードデータを取得
        listMShipmentScheduleQRCode = await _dbService.GetMShipmentScheduleQRCodeAsync();

        // サウンドパスを取得
        resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
        resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
    }

    /// <summary>
    /// ハンディリーダーでスキャンする時、処理する
    /// </summary>
    protected async void OnClipboardContentChanged(object sender, EventArgs e)
    {
        try
        {
            //　QRコードのコンテンツを取得
            string clipboardContent = Clipboard.GetTextAsync().Result;
            // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
            listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();

            // 未検品の出荷指示がないためスキャンできません
            if (progressBar.Progress == 1) { ShowErrorTextInLabel("E2001: 未検品の出荷指示がないためスキャンできません。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);  return; }

            if (string.IsNullOrEmpty(clipboardContent))
                ShowErrorTextInLabel("E2001: QRコードが読めません。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);

            //ローカルで出荷実績一覧を取得
            listDShipmentSqlLite = await _dbService.GetDShipmentAsync();

            string deliveryIdentifyString = string.Empty;
            if (isScanDelivery) 
            {
                // 一回目コードの形式と日付をチェックすし、データを取得する
                var deliveryQrIdentifyMatchStringList = listMShipmentScheduleQRCode.Where(x => x.IdentifyString == QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
                if(deliveryQrIdentifyMatchStringList.Count  <= 0 ) { ShowErrorTextInLabel("E2001: 納入先コードを読み取ってください。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                deliveryIdentifyString = deliveryQrIdentifyMatchStringList.FirstOrDefault().IdentifyString;

                // 全てエラーテキストを削除
                productNumberLabel.Text = string.Empty;
                statusScanLabel.Text = string.Empty;
  
                // QR中身を制御コードで区切れるよう変換する
                SetValueFromQrCode(clipboardContent);

                //マスターのプロダクトリストに存在納入先品番をチェック
                bool existDeliveryProductNumber = listProducts.Any(product => product.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && product.LotQuantity == deliveryKanbanModel.LotQuantity);
                // 納入先品番が存在をチェック
                if (!existDeliveryProductNumber) { ShowErrorTextInLabel("E2015: 納入先品番が存在しません。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                // 納入先品番をチェック
                var dShipmentSchedule = ExistDeliveryProductNumberCheck(_loginUserSqlLite.DepoID, companyID, deliveryKanbanModel.DeliveryDate, deliveryKanbanModel.DeliveryTimeClass, deliveryKanbanModel.DeliveryLocation, deliveryKanbanModel.DeliverySlipNumber);
                if (dShipmentSchedule == null) {
                    // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                    SoundPlayer.PlaySound(resultErrorSound);
                    errorMessengerLabel.Text = "E2015: 出荷指示リストに一致するデータがありません。";
                    registerStoreOut.IsVisible = false;
                    adminErrorCancel.IsVisible = true;
                    return; 
                }
                quantity = dShipmentSchedule.Quantity;
                shipmentScheduleID = dShipmentSchedule.ShipmentScheduleID;

                // ローカルで読取実績モデルの出荷実績モデルに出荷指示IDとシリアル番号(かんばん識別番号)をチェック
                bool isScanDuplicateInSqlLite = listDShipmentSqlLite.Any(item => item.ShipmentScheduleID == shipmentScheduleID && item.KanbanSerialNumber == deliveryKanbanModel.KanbanSerialNumber );
                // APIから出荷実績一覧に出荷指示IDとシリアル番号(かんばん識別番号)をチェック
                bool isScanDuplicate = listDShipment.Any(item => item.ShipmentScheduleID == shipmentScheduleID && item.KanbanSerialNumber == deliveryKanbanModel.KanbanSerialNumber );
                if (isScanDuplicateInSqlLite || isScanDuplicate)
                {
                    // [共通関数]エラーメッセージ表示実績作成その後、エラー解除要否チェックをする。
                    // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                    if (_loginUserSqlLite.AuthorizedKubun == Const.C_AUTHORIZED_KUBUN_WORKER)
                    {
                        SoundPlayer.PlaySound(resultErrorSound);
                        errorMessengerLabel.Text = "E2015: 既にスキャン済みの納入先かんばんです。";
                        registerStoreOut.IsVisible = false;
                        adminErrorCancel.IsVisible = true;
                        return;
                    }
                    else
                    {
                        ShowErrorTextInLabel("E2015: 既にスキャン済みの仕入先かんばんです。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                        isScanDelivery = true;
                        return;
                    }
                }

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
               if(supplierIdentifyMatchStringList.Count <= 0) { ShowErrorTextInLabel("E2015: QRコードが正しくありません。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); isScanDelivery = true; return; }

                supplierIdentifyString = supplierIdentifyMatchStringList.FirstOrDefault().IdentifyString;
                mSupplierKanban = supplierIdentifyMatchStringList.FirstOrDefault();
                Match match = Regex.Match(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim(), @"^\d+");
                int quantity = int.Parse(match.Value);

                // マスターのプロダクトリストに存在仕入先品番をチェック
                var findListSupplierProduct= ExistSupplierProductNumberCheck(supplierIdentifyString, clipboardContent, quantity);
                //マスターのプロダクトに仕入先品番が存在をチェック
                if (findListSupplierProduct.Count <= 0) { ShowErrorTextInLabel("E2015:  仕入先品番が存在しません。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); isScanDelivery = true; return; }
                //納入先品番と仕入先品番一致をチェック
                bool isMatchSupplierAndDeliveryProductNumber = listProducts.Any(product => product.SupplierProductNumber == findListSupplierProduct.FirstOrDefault().SupplierProductNumber && product.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && product.LotQuantity == quantity);

                if (!isMatchSupplierAndDeliveryProductNumber) {
                    // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                    SoundPlayer.PlaySound(resultErrorSound);
                    errorMessengerLabel.Text = "E2015: 納入先品番と仕入先品番は一致しません。";
                    registerStoreOut.IsVisible = false;
                    adminErrorCancel.IsVisible = true;
                    isScanDelivery = true;
                    return;
                }

                dShipmentLocal.SupplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
                if(dShipmentLocal.SupplierProductNumber.IndexOf("-") != -1) // -の後ろの文字を削除する
                    dShipmentLocal.SupplierProductNumber = dShipmentLocal.SupplierProductNumber.Substring(0, dShipmentLocal.SupplierProductNumber.IndexOf('-'));

                // 許可された重複チェック
                if (!mSupplierKanban.AllowedDuplicatesFlag && mSupplierKanban.MainProductKeyLength != 0)
                {
                    // 仕入先かんばん分解 (出荷実績に値を入れる)
                    dShipmentLocal.FirstSubProductKey = string.Empty;
                    dShipmentLocal.SecondSubProductKey = string.Empty;
                    dShipmentLocal.MainProductKey = clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();
                   
                    var isDuplicates = listDShipment.Any(x => x.SupplierProductNumber == dShipmentLocal.SupplierProductNumber && x.MainProductKey == mainProductKey
                                                                            && x.FirstSubProductKey == dShipmentLocal.FirstSubProductKey && x.SecondSubProductKey == dShipmentLocal.SecondSubProductKey);
                    // ローカルで仕入先品番重複チェック
                    bool isScanDuplicateInSqlLite = listDShipmentSqlLite.Any(x => x.SupplierProductNumber == dShipmentLocal.SupplierProductNumber && x.MainProductKey == dShipmentLocal.MainProductKey &&
                                                                                                        x.FirstSubProductKey == dShipmentLocal.FirstSubProductKey && x.SecondSubProductKey == dShipmentLocal.SecondSubProductKey);

                    if (isDuplicates || isScanDuplicateInSqlLite)  // 仕入先品番重複チェック
                    {
                        // [共通関数]エラーメッセージ表示実績作成その後、エラー解除要否チェックをする。
                        // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                        if (_loginUserSqlLite.AuthorizedKubun == Const.C_AUTHORIZED_KUBUN_WORKER)
                        {
                            SoundPlayer.PlaySound(resultErrorSound);
                            errorMessengerLabel.Text = "E2015: 既にスキャン済みの仕入先かんばんです。";
                            registerStoreOut.IsVisible = false;
                            adminErrorCancel.IsVisible = true;
                            isScanDelivery = true;
                            return;
                        }
                        else
                        {
                            ShowErrorTextInLabel("E2015: 既にスキャン済みの仕入先かんばんです。", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                            isScanDelivery = true;
                            return;
                        }
                    }
                }

                // 仕入先かんばん分解 (出荷実績に値を入れる)
                if (mSupplierKanban.LotLength != 0)
                    dShipmentLocal.LotNumber = clipboardContent.Substring(mSupplierKanban.LotStartIndex - 1, mSupplierKanban.LotLength).Trim();

                dShipmentLocal.NumberOfBoxes = numberOfBoxes;
                dShipmentLocal.Quantity = quantity;
                dShipmentLocal.CreatedAt = DateTime.Now;
                dShipmentLocal.CreatedBy = _loginUserSqlLite.UserName;
                dShipmentLocal.KanbanSerialNumber = deliveryKanbanModel.KanbanSerialNumber ;

                supplierScanContent = Clipboard.GetTextAsync().Result;

                // 読取実績モデル値を入れる
                scanShipmentModel.d_ShipmentModel = dShipmentLocal;
                var d_ScanResultModel = new D_ScanResultModel();
                d_ScanResultModel = SetScanResultModel(d_ScanResultModel, mSupplierKanban.SupplierKanbanID,  _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                scanShipmentModel.d_ScanResultModel = d_ScanResultModel;
                listScanShipment.Add(scanShipmentModel);

                // スキャンが終わったら、出荷指示リストと履歴一覧を更新
                UpdateListAndHistoryListDShipmentSchedules();

                // 内容を確認してリストを追加 スキャン発送
                CheckScanContentAndAddListScanShipment();

                // スキャン出来たら、ローカルで出荷実績モデルを保存
                dShipmentLocal.ShipmentScheduleID = shipmentScheduleID;
                await _dbService.SaveDShipmentAsync(dShipmentLocal);

                isScanDelivery = true;
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
            }
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
    /// 出荷指示リストと履歴一覧を更新
    /// </summary>
    public void UpdateListAndHistoryListDShipmentSchedules()
    {
        // 出荷指示リスト更新
        var existDShipmentSchedule = lstDShipmentSchedules.Find(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == _loginUserSqlLite.DepoID && d.CompanyID == companyID && d.DeliveryDate == deliveryKanbanModel.DeliveryDate
                                                     && d.DeliveryTimeClass == deliveryKanbanModel.DeliveryTimeClass && d.DeliveryLocation == deliveryKanbanModel.DeliveryLocation && d.DeliverySlipNumber == deliveryKanbanModel.DeliverySlipNumber);
        
        if (existDShipmentSchedule != null && progressBar.Progress != 1)
        {
            existDShipmentSchedule.NumberOfBoxesRemaining--;
            collectionView.ItemsSource = null;
            collectionView.ItemsSource = lstDShipmentSchedules;
        }

        // 履歴一覧更新
        var exitsHistoryDShipmentSchedule = lstHistoryDShipmentSchedules.Find(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == _loginUserSqlLite.DepoID && d.CompanyID == companyID && d.DeliveryDate == deliveryKanbanModel.DeliveryDate
                                                               && d.DeliveryTimeClass == deliveryKanbanModel.DeliveryTimeClass && d.DeliveryLocation == deliveryKanbanModel.DeliveryLocation && d.DeliverySlipNumber == deliveryKanbanModel.DeliverySlipNumber);

        // スキャン後、履歴一覧を表示
        var listHistoryForDisplay = new List<D_ShipmentSchedule>();
        if(exitsHistoryDShipmentSchedule != null)
        {
            listHistoryForDisplay.Add(exitsHistoryDShipmentSchedule);
            exitsHistoryDShipmentSchedule.NumScan++;
            var numberOfBox = exitsHistoryDShipmentSchedule.NumScan + exitsHistoryDShipmentSchedule.NumberOfBoxesRemaining;
            exitsHistoryDShipmentSchedule.NumScanAndNumBoxRemaining = $"{exitsHistoryDShipmentSchedule.NumScan}/{numberOfBox}";
            collectionViewHistory.ItemsSource = null;
            // No降順
            for (int i = 0; i < listHistoryForDisplay.Count; i++)
                listHistoryForDisplay[i].No = (listHistoryForDisplay.Count - i);
        }

        collectionViewHistory.ItemsSource = listHistoryForDisplay;
    }


    /// <summary>
    /// Qrコード値の部分文字列
    /// </summary>
    /// <param name="index"></param>
    /// <param name="stringLength"></param>
    /// <param name="qr"></param>
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
        deliveryKanbanModel.DeliveryLocation = Regex.Replace(changeCodeResult[5], @"^+" + StartIndexString.Location1Index, "");
        deliveryKanbanModel.DeliveryTimeClass = Int32.Parse(Regex.Replace(changeCodeResult[9], @"^+" + StartIndexString.DeliveryTimeClassIndex, ""));
        deliveryKanbanModel.ProductAbbreviation = Regex.Replace(changeCodeResult[11], @"^+" + StartIndexString.ProductAbbreviationIndex, "");
        deliveryKanbanModel.KanbanSerialNumber  = Int32.Parse(Regex.Replace(changeCodeResult[15], @"^+" + StartIndexString.KanbanSerialNumberIndex, ""));
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
    public D_ShipmentSchedule ExistDeliveryProductNumberCheck(int depoID, int companyID, DateTime deliveryDate, int deliveryTimeClass, string deliveryLocation, string deliverySlipNumber)
    {
        var d_ShipmentSchedule = new D_ShipmentSchedule();
         d_ShipmentSchedule = lstDShipmentSchedulesForCheckDuplicate.Where(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == depoID && d.CompanyID == companyID && 
         d.DeliveryDate == deliveryDate && d.DeliveryTimeClass == deliveryTimeClass && d.DeliveryLocation == deliveryLocation && d.DeliverySlipNumber == deliverySlipNumber).FirstOrDefault();
        if (d_ShipmentSchedule != null)
            return d_ShipmentSchedule;

        return null;
    }

    /// <summary>
    /// 重複品番をチェック
    /// </summary>
    /// <param name="substrings"></param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="resultErrorSound"></param>
    /// <returns></returns>
    public List<M_ProductModel> ExistSupplierProductNumberCheck(string supplierIdentifyString, string clipboardContent, int quantity) 
    {
        List<M_ProductModel> findListProduct = new List<M_ProductModel>();
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

        findListProduct = listProducts.Where(product => product.SupplierProductNumber == supplierProductNumberCheck && product.LotQuantity == quantity).ToList();

        return findListProduct;
    }

    /// <summary>
    /// 内容を確認してリストを追加 スキャン発送
    /// </summary>
    public void CheckScanContentAndAddListScanShipment()
    {
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

            // 全体進捗++
            overallProgress++;

            double completeProgress = (double)overallProgress / (double)totalNumberOfBoxesRemaining;
            progressBarValueLabel.Text = $"{overallProgress}/ " + totalNumberOfBoxesRemaining.ToString();
            progressBar.Progress = completeProgress;
            if (progressBar.Progress == 1)
            {
                completeLabel.Text = "完了";
                completeLabel.TextColor = Color.FromHex("#F44E4B");
                progressBar.ProgressColor = Color.FromHex("#F44E4B");
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
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int supplierKanbanID, int depoID,  string userName)
    {
        d_ScanResultModel.SupplierKanbanID = supplierKanbanID;
        d_ScanResultModel.DepoID = depoID;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.FirstScanedString = deliveryScanContent;
        d_ScanResultModel.SecondScanedString = supplierScanContent;
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
    public D_ShipmentModel SetDShipmentModel(D_ShipmentModel d_ShipmentModel, int KanbanSerialNumber , string supplierProductNumber, int numberOfBoxes, string userName, int quantity, int shipmentScheduleID)
    {
        d_ShipmentModel.ShipmentScheduleID = shipmentScheduleID;
        d_ShipmentModel.ShipmentDatetime = DateTime.Now;
        d_ShipmentModel.KanbanSerialNumber = KanbanSerialNumber ;
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
    public D_HandyErrorMessageModel SetHandyErrorMessage(D_HandyErrorMessageModel d_HandyErrorMessageModel, int depoID, string userName, string scanTextError)
    {
        d_HandyErrorMessageModel.DepoID = depoID;
        d_HandyErrorMessageModel.HandyMenuID = handyMenuID;
        d_HandyErrorMessageModel.FirstScanedString = deliveryScanContent;
        d_HandyErrorMessageModel.SecondScanedString = supplierScanContent;
        d_HandyErrorMessageModel.ErrorMessage = scanTextError;
        d_HandyErrorMessageModel.CreatedAt = DateTime.Now;
        d_HandyErrorMessageModel.CreatedBy = userName;

        return d_HandyErrorMessageModel;
    }


    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    public void ShowErrorTextErrorCancel(string error, string resultErrorSound, int depoID, string userName)
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
        d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, depoID, userName, error);
        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
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
        d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, depoID, userName, error);
        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
    }


    /// <summary>
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (!NetworkCheck.CheckNetworkConnection()) // ネットワーク接続チェック
            {
                await Application.Current.MainPage.DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
                return;
            }

            var shipmentScheduleRequestBody = new
            {
                CompanyID = companyID,
                CompanyCode = _loginUserSqlLite.CompanyCode,
                CompanyPassword = _loginUserSqlLite.CompanyPassword,
                DepoID = _loginUserSqlLite.DepoID,
                DeliveryDate = selectedDateLabel.Text.Replace("/", "-"),
                DeliveryTimeClass = selectedDeliveryTimeClassLabel.Text,
                DeliveryLocation = selectedDeliveryLocationLabel.Text,
            };
            var jsonDataShipmentScheduleSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

            // 最新データを取得するため、もう一回APIにリクエストを送る
            var responseShipmentSchedule = await App.API.PostMethod(jsonDataShipmentScheduleSend, _handyApiUrl, "ShipmentSchedule/Find");

            if (responseShipmentSchedule.success)
            {
                // APIからコンテンツを取得
                var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduelResponseBody>(responseShipmentSchedule.content);
                lstDShipmentSchedules = contentFromApi.D_ShipmentScheduleList; // 出荷指示
            }
            else
                await DisplayAlert("エラー", responseShipmentSchedule.content, "OK");

            Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, companyID, selectedDeliveryNameLabel.Text, selectedDate, selectedDeliveryTimeClassLabel.Text, selectedDeliveryLocationLabel.Text, listMCompany, lstDShipmentSchedules, listProducts, deliverySlipNumberLabel.Text);

        }
        catch (Exception)
        {
            await DisplayAlert("エラー", "E9999: 予期せぬエラーが発生しました。", "OK");
        }
    }

    /// <summary>
    /// プログレスバーを更新する
    /// </summary>
    private async void UpdateProgressBar(int totalNumberOfBoxes, int totalNumberOfBoxesRemaining)
    {
        try
        {
            double completeProgress = (double)(overallProgress) / (double)totalNumberOfBoxesRemaining;
            progressBarValueLabel.Text = $"{overallProgress}/ " + totalNumberOfBoxesRemaining.ToString();
            progressBar.Progress = completeProgress;
            progressBar.ProgressColor = Color.FromHex("#008080");

            if (progressBar.Progress == 1)
            {
                completeLabel.Text = "完了";
                completeLabel.TextColor = Color.FromHex("#F44E4B");
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
            SetValueAndVisibleLabelAfterRegisterAndErrorCancel();

            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            // OKサウンド
            SoundPlayer.PlaySound(resultOkSound);

            await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "OK");

            // 送信後は再度ShipmentSchduleのAPIに再度接続して情報を最新化
            CallBackApiToUpdateDataAfterRegistered();
        }
        catch (Exception)
        {
            await DisplayAlert("エラー", "E9999: 予期せぬエラーが発生しました。", "OK");
        }
    }

    /// <summary>
    /// 送信後は再度ShipmentSchduleのAPIに再度接続して情報を最新化
    /// </summary>
    public async void CallBackApiToUpdateDataAfterRegistered()
    {
        var shipmentScheduleRequestBody = new
        {
            CompanyID = companyID,
            CompanyCode = _loginUserSqlLite.CompanyCode,
            CompanyPassword = _loginUserSqlLite.CompanyPassword,
            DepoID = _loginUserSqlLite.DepoID,
            DeliveryDate = selectedDateLabel.Text.Replace("/", "-"),
            DeliveryTimeClass = selectedDeliveryTimeClassLabel.Text,
            DeliveryLocation = selectedDeliveryLocationLabel.Text,
        };
        var jsonDataShipmentScheduleSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

        // 最新データを取得するため、もう一回APIにリクエストを送る
        var responseShipmentSchedule = await App.API.PostMethod(jsonDataShipmentScheduleSend, _handyApiUrl, "ShipmentSchedule/Find");

        if (responseShipmentSchedule.success)
        {
            // APIからコンテンツを取得
            var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduelResponseBody>(responseShipmentSchedule.content);
            var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList; // 出荷指示
            var dShipmentList = contentFromApi.D_ShipmentList; // 出荷実績
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);
            Application.Current.MainPage = new RegisterStoreOutPage(handyMenuID, companyID, (string)selectedDeliveryNameLabel.Text, selectedDateLabel.Text, selectedDeliveryTimeClassLabel.Text, selectedDeliveryLocationLabel.Text, listMCompany, shipmentScheduleList, dShipmentList, mUserList, listProducts, deliverySlipNumberLabel.Text);
        }
        else
            await DisplayAlert("エラー", responseShipmentSchedule.content, "OK");
    }

    /// <summary>
    /// テキスト値を設定し、成功登録後にラベルに表示されます
    /// </summary>
    public void SetValueAndVisibleLabelAfterRegisterAndErrorCancel()
    {
        productNumberLabel.Text = string.Empty;
        statusScanLabel.Text = string.Empty;
        productNumberLabel.IsVisible = true;
        statusScanLabel.IsVisible = true;
        scanTextResult.Text = string.Empty;
        scanTextResult.IsVisible = false;
        deliveryScanContent = string.Empty;
        supplierScanContent = string.Empty;
        txtLoginId.Text = string.Empty;
        txtPassword.Text = string.Empty;
    }

    /// <summary>
    /// 解除要求
    /// </summary>
    private void OnErrorCancelClicked(object sender, EventArgs e)
    {
        var loginID = txtLoginId.Text;
        var password = txtPassword.Text;

        //ログインIDまたはパスワードが空欄
        //パスワードが4桁未満、または10桁を超える場合
        if (string.IsNullOrWhiteSpace(loginID) || (4 > password.Length || password.Length > 10))
        {
            DisplayAlert("エラー", "E2011: ログインIDまたはパスワードが正しくありません。", "OK");
            return;
        }

        //// ログインIDは合致するデータがない
        var listUser = mUserList.Where(u => u.LoginID == loginID).ToList();

        if (listUser.Count <= 0)
        {
            DisplayAlert("エラー", "E2004: ログインIDが正しくありません。正しい値を入力してください。", "OK");
            return;
        }

        //// 入力されたパスワードをハッシュ化
        byte[] salt = Hashing.ConvertStringToBytes(listUser.FirstOrDefault().Salt);
        string hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(password, salt);

        // ハッシュ化されたパスワードと一致するかチェック
        if (!listUser.FirstOrDefault().Password.Equals(hashedPassword))
        {
            DisplayAlert("エラー", "E2004: パスワードが間違っています。", "OK");
            return;
        }

        // ハンディエラーメッセージ実績モデル
        d_HandyErrorMessageModel.UnlockedAt = DateTime.Now;
        d_HandyErrorMessageModel.UnlockedBy = _loginUserSqlLite.UserName;
        d_HandyErrorMessageModel.DepoID = _loginUserSqlLite.DepoID;
        d_HandyErrorMessageModel.HandyMenuID = handyMenuID;
        d_HandyErrorMessageModel.FirstScanedString = deliveryScanContent;
        d_HandyErrorMessageModel.SecondScanedString = supplierScanContent;
        d_HandyErrorMessageModel.ErrorMessage = errorMessengerLabel.Text;
        d_HandyErrorMessageModel.CreatedAt = DateTime.Now;
        d_HandyErrorMessageModel.CreatedBy = _loginUserSqlLite.UserName;

        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
  
        SetValueAndVisibleLabelAfterRegisterAndErrorCancel();
        // 出庫登録画面を表示
        registerStoreOut.IsVisible = true;
        adminErrorCancel.IsVisible = false;
    }
}
