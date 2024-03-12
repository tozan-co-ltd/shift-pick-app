using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.Properties;
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
    bool isItemTapped = false;
    public DateTime selectedDate;
    private string? _handyApiUrl;
    public int overallProgress;
    public int companyID;
    public int handyMenuID;
    public int numberOfBoxes = 1;
    public int shipmentScheduleID;
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
    public ObservableCollection<D_ShipmentSchedule> lstDShipmentSchedules;
    public ObservableCollection<D_ShipmentSchedule> lstDShipmentSchedulesForCheckDuplicate;
    public ObservableCollection<D_ShipmentSchedule> lstHistoryDShipmentSchedules;
    public ObservableCollection<D_ShipmentSchedule> listHistoryForDisplay = new ObservableCollection<D_ShipmentSchedule>();
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
    public List<D_ShipmentModel> listDShipment;
    public List<D_ShipmentModel> listDShipmentSqlLite = new List<D_ShipmentModel>();
    public List<M_UserModel> mUserList= new List<M_UserModel>();
    public DeliveryKanbanModel deliveryKanbanModel = new DeliveryKanbanModel();
    public List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanShipmentModel> listScanShipment = new List<ScanShipmentModel>();
    public List<M_ShipmentScheduleQRCodeModel> listMShipmentScheduleQRCode = new List<M_ShipmentScheduleQRCodeModel>();    

    public RegisterStoreOutPage(int menuID, int selectedCompanyID, string deliveryName, string currentDate, string selectedDeliveryTimeClass, string deliveryLocation, List<M_CompanyModel> mCompanySqlLite, ObservableCollection<D_ShipmentSchedule> dShipmentScheduleListSearch, List<D_ShipmentModel> dShipmentsListSearch, List<M_UserModel> mUserListSearch, List<M_ProductModel> listProductItems, string DeliverySlipNumber)
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
      

        // 出庫画面の設定
        SetRegisterStoreOutPage();

        // 進捗を数える
        totalNumberOfBoxesRemaining = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxesRemaining);        
        UpdateProgressBar( totalNumberOfBoxesRemaining);

        // 履歴一覧用
        lstHistoryDShipmentSchedules = new ObservableCollection<D_ShipmentSchedule>(dShipmentScheduleListSearch);
        // 重複をチェック用
        lstDShipmentSchedulesForCheckDuplicate = new ObservableCollection<D_ShipmentSchedule>(dShipmentScheduleListSearch);

        // 残りが0の行が表示されない, 箱数降順
        var lstDShipmentSchedulesSort = dShipmentScheduleListSearch.Where(p => p.NumberOfBoxesRemaining != 0).OrderByDescending(p => p.NumberOfBoxes);
        //残り箱数の初期値を箱数に代入
        foreach (var item in lstDShipmentSchedulesSort) {
            item.NumberOfBoxes = item.NumberOfBoxesRemaining;
        }
        lstDShipmentSchedules = new ObservableCollection<D_ShipmentSchedule>(lstDShipmentSchedulesSort);

        // No昇順
        UpdateListDShipmentSchedulesNo();

        // 未検品の出荷指示が無い
        if (lstDShipmentSchedulesSort.Count() == 0)
        {
            shippingListTable.IsVisible = false;
            emptyLabel.IsVisible = true;
        }

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
            //管理者エラー解除画面表示中はスキャンさせない。
            if (adminErrorCancel.IsVisible) return;
            
            //　QRコードのコンテンツを取得
            string clipboardContent = Clipboard.GetTextAsync().Result;

            // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
            listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();

            // 未検品の出荷指示がないためスキャンできません
            if (progressBar.Progress == 1 || totalNumberOfBoxesRemaining == 0) { ShowErrorTextInLabel($"E2025: {ErrorMessagesResources.E2025}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);  return; }

            if (string.IsNullOrEmpty(clipboardContent))
                ShowErrorTextInLabel($"E2015: {ErrorMessagesResources.E2015}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);

            //ローカルで出荷実績一覧を取得
            listDShipmentSqlLite = await _dbService.GetDShipmentAsync();

            string deliveryIdentifyString = string.Empty;
            if (isScanDelivery) 
            {

                //納入先かんばんをスキャンした時の文字列を格納
                deliveryScanContent = clipboardContent;
                supplierScanContent = string.Empty;
                // 一回目コードの形式と日付をチェックすし、データを取得する
                var deliveryQrIdentifyMatchStringList = listMShipmentScheduleQRCode.Where(x => x.IdentifyString == Util.QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
                if(deliveryQrIdentifyMatchStringList.Count  <= 0 ) { ShowErrorTextInLabel($"E2001: {ErrorMessagesResources.E2001}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                deliveryIdentifyString = deliveryQrIdentifyMatchStringList.FirstOrDefault().IdentifyString;

                // 全てエラーテキストを削除
                productNumberLabel.Text = string.Empty;
                statusScanLabel.Text = string.Empty;
  
                // QR中身を制御コードで区切れるよう変換するのをチェック用
                bool isInValidateDelivery =  SetValueFromQrCode(clipboardContent);
                if (!isInValidateDelivery) { ShowErrorTextInLabel($"E2015: {ErrorMessagesResources.E2015}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                //マスターのプロダクトリストに存在納入先品番をチェック
                bool existDeliveryProductNumber = listProducts.Any(product => product.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && product.LotQuantity == deliveryKanbanModel.LotQuantity);
                // 納入先品番が存在をチェック
                if (!existDeliveryProductNumber) { ShowErrorTextInLabel($"E2002: {string.Format(ErrorMessagesResources.E2002, "品番マスター")}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                // 納入先品番をチェック
                var dShipmentSchedule = ExistDeliveryProductNumberCheck(_loginUserSqlLite.DepoID, companyID, deliveryKanbanModel.DeliveryDate, deliveryKanbanModel.DeliveryTimeClass, deliveryKanbanModel.DeliveryLocation, deliveryKanbanModel.DeliverySlipNumber);
                if (dShipmentSchedule == null) {
                    // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                    SoundPlayer.PlaySound(resultErrorSound);
                    errorMessengerLabel.Text = $"E2024: {ErrorMessagesResources.E2024}";
                    registerStoreOut.IsVisible = false;
                    adminErrorCancel.IsVisible = true;
                    return; 
                }
                
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
                        errorMessengerLabel.Text = $"E2022: {ErrorMessagesResources.E2022}";
                        registerStoreOut.IsVisible = false;
                        adminErrorCancel.IsVisible = true;
                        return;
                    }
                    else
                    {
                        ShowErrorTextInLabel($"E2022: {ErrorMessagesResources.E2022}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                        isScanDelivery = true;
                        return;
                    }
                }

                // 納入先コードスキャンが成功した場合のラベル値を設定
                SetValueWhenSuccessScanForDelivery();                

                
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);

                isScanDelivery = false;
            }
            else　
            {
                //納入先かんばんフラグをスイッチ
                isScanDelivery = true;
                // 新しいオブジェクトを初期化
                var _dShipmentLocal = new D_ShipmentModel();
                var d_ScanResultModel = new D_ScanResultModel();
                var _scanShipmentModel = new ScanShipmentModel();

                //仕入先かんばんをスキャンしたときの文字列を格納
                supplierScanContent = clipboardContent;

                // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
                var supplierIdentifyMatchStringList = listMSupplierKanban.Where(x => x.IdentifyString == Util.QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
               if(supplierIdentifyMatchStringList.Count <= 0) { ShowErrorTextInLabel($"E2001: {ErrorMessagesResources.E2001}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }

                supplierIdentifyString = supplierIdentifyMatchStringList.FirstOrDefault().IdentifyString;
                mSupplierKanban = supplierIdentifyMatchStringList.FirstOrDefault();
                // intへの変換ができるかチェック用
                string quantityCheck = clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim();
                if (!CheckIntType(quantityCheck).isInt){ ShowErrorTextInLabel($"E2001: {ErrorMessagesResources.E2001}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }
                int quantity = CheckIntType(quantityCheck).value;

                // マスターのプロダクトリストに存在仕入先品番をチェック
                var findListSupplierProduct = ExistSupplierProductNumberCheck(supplierIdentifyString, clipboardContent, quantity);
                //マスターのプロダクトに仕入先品番が存在をチェック
                if (findListSupplierProduct.Count <= 0) { ShowErrorTextInLabel($"E2002: {string.Format(ErrorMessagesResources.E2002, "品番マスター")}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName); return; }
                //納入先品番と仕入先品番一致をチェック
                bool isMatchSupplierAndDeliveryProductNumber = listProducts.Any(product => product.SupplierProductNumber == findListSupplierProduct.FirstOrDefault().SupplierProductNumber && product.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && product.LotQuantity == quantity);

                if (!isMatchSupplierAndDeliveryProductNumber) {
                    // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                    SoundPlayer.PlaySound(resultErrorSound);
                    errorMessengerLabel.Text = $"E2023: {ErrorMessagesResources.E2023}";
                    registerStoreOut.IsVisible = false;
                    adminErrorCancel.IsVisible = true;
                    return;
                }

                    // 仕入先かんばん分解 (出荷実績に値を入れる)
                _dShipmentLocal.SupplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
                _dShipmentLocal.SupplierProductNumber = Util.SubStringAfterDash(_dShipmentLocal.SupplierProductNumber);

                // 許可された重複チェック
                if (!mSupplierKanban.AllowedDuplicatesFlag && mSupplierKanban.MainProductKeyLength != 0)
                {
                    // キー
                    if (mSupplierKanban.MainProductKeyLength != 0)
                    {
                        _dShipmentLocal.MainProductKey = mSupplierKanban.MainProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();
                        _dShipmentLocal.FirstSubProductKey = mSupplierKanban.FirstSubProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.FirstSubProductKeyStartIndex - 1, mSupplierKanban.FirstSubProductKeyLength).Trim();
                        _dShipmentLocal.SecondSubProductKey = mSupplierKanban.SecondSubProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.SecondSubProductKeyStartIndex - 1, mSupplierKanban.SecondSubProductKeyLength).Trim();
                    }                  
                   
                    var isDuplicates = listDShipment.Any(x => x.SupplierProductNumber == _dShipmentLocal.SupplierProductNumber && x.MainProductKey == _dShipmentLocal.MainProductKey
                                                                            && x.FirstSubProductKey == _dShipmentLocal.FirstSubProductKey && x.SecondSubProductKey == _dShipmentLocal.SecondSubProductKey);
                    // ローカルで仕入先品番重複チェック
                    bool isScanDuplicateInSqlLite = listDShipmentSqlLite.Any(x => x.SupplierProductNumber == _dShipmentLocal.SupplierProductNumber && x.MainProductKey == _dShipmentLocal.MainProductKey &&
                                                                                                        x.FirstSubProductKey == _dShipmentLocal.FirstSubProductKey && x.SecondSubProductKey == _dShipmentLocal.SecondSubProductKey);

                    if (isDuplicates || isScanDuplicateInSqlLite)  // 仕入先品番重複チェック
                    {
                        // [共通関数]エラーメッセージ表示実績作成その後、エラー解除要否チェックをする。
                        // 権限が作業者(解除要)であれば、[共通関数] 管理者エラー解除画面表示
                        if (_loginUserSqlLite.AuthorizedKubun == Const.C_AUTHORIZED_KUBUN_WORKER)
                        {
                            SoundPlayer.PlaySound(resultErrorSound);
                            errorMessengerLabel.Text = $"E2017: {ErrorMessagesResources.E2017}";
                            registerStoreOut.IsVisible = false;
                            adminErrorCancel.IsVisible = true;
                            return;
                        }
                        else
                        {
                            ShowErrorTextInLabel($"E2017: {ErrorMessagesResources.E2017}", resultErrorSound, clipboardContent, _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                            return;
                        }
                    }
                }
                // 仕入先かんばん分解 (出荷実績に値を入れる)
                if (mSupplierKanban.LotLength != 0)
                    _dShipmentLocal.LotNumber = clipboardContent.Substring(mSupplierKanban.LotStartIndex - 1, mSupplierKanban.LotLength).Trim();

                _dShipmentLocal.NumberOfBoxes = numberOfBoxes;
                _dShipmentLocal.Quantity = quantity;
                _dShipmentLocal.CreatedAt = DateTime.Now;
                _dShipmentLocal.CreatedBy = _loginUserSqlLite.UserName;
                _dShipmentLocal.KanbanSerialNumber = deliveryKanbanModel.KanbanSerialNumber;
                // 読取実績モデル値を入れる
                _scanShipmentModel.d_ShipmentModel = _dShipmentLocal;
                d_ScanResultModel = SetScanResultModel(d_ScanResultModel, mSupplierKanban.SupplierKanbanID,  _loginUserSqlLite.DepoID, _loginUserSqlLite.UserName);
                _scanShipmentModel.d_ScanResultModel = d_ScanResultModel;
                listScanShipment.Add(_scanShipmentModel);

                // スキャンが終わったら、出荷指示リストと履歴一覧を更新
                UpdateListAndHistoryListDShipmentSchedules();

                // 内容を確認してリストを追加 スキャン発送
                CheckScanContentAndAddListScanShipment();

                // スキャン出来たら、ローカルで出荷実績モデルを保存
                _dShipmentLocal.ShipmentScheduleID = shipmentScheduleID;
                await _dbService.SaveDShipmentAsync(_dShipmentLocal);

                isScanDelivery = true;
                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
            }
        }
        catch (ArgumentOutOfRangeException arg)
        {
            await DisplayAlert("エラー", $"E2001: {ErrorMessagesResources.E2001}\n" + arg.Message, "OK");
            return;
        }
        catch (Exception ex)
        {
            // 全てコンテンツを消す
            deliveryScanContent = string.Empty;
            supplierScanContent = string.Empty;
            isScanDelivery = true;
            await DisplayAlert("エラー", ex.Message, "閉じる");
        }
    }

    /// <summary>
    ///  intへの変換ができるかチェック用
    /// </summary>
    /// <param name="valueCheck"></param>
    public (bool isInt, int value) CheckIntType(string valueCheck)
    {
        try
        {
            Match match = Regex.Match(valueCheck, @"^\d+");
            int quantity = int.Parse(match.Value);            
            return (true, quantity);
        }
        catch (Exception ex)
        {
            return (false, 0);
        }
    }

    /// <summary>
    ///  日付への変換ができるかチェック用
    /// </summary>
    /// <param name="deliveryDateString"></param>
    public (bool isDate, DateTime dateTime) CheckDateTimeType(string deliveryDateString)
    {
        try
        {
            DateTime dateTime = DateTime.ParseExact(deliveryDateString, "yyyyMMdd", CultureInfo.InvariantCulture);
            return (true, dateTime);
        }
        catch (Exception ex)
        {
            return (false, DateTime.Now);
        }
    }

    /// <summary>
    /// 出荷指示リストと履歴一覧を更新
    /// </summary>
    public void UpdateListAndHistoryListDShipmentSchedules()
    {
        // 出荷指示リスト更新
        var existDShipmentSchedule = lstDShipmentSchedules.FirstOrDefault(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == _loginUserSqlLite.DepoID && d.CompanyID == companyID && d.DeliveryDate == deliveryKanbanModel.DeliveryDate
                                                     && d.DeliveryTimeClass == deliveryKanbanModel.DeliveryTimeClass && d.DeliveryLocation == deliveryKanbanModel.DeliveryLocation && d.DeliverySlipNumber == deliveryKanbanModel.DeliverySlipNumber);
        if (existDShipmentSchedule != null && progressBar.Progress != 1)
        {
            existDShipmentSchedule.NumberOfBoxesRemaining--;
            if (existDShipmentSchedule.NumberOfBoxesRemaining == 0) // 残りが0の行が表示されない
            {
                lstDShipmentSchedules.Remove(existDShipmentSchedule);
                UpdateListDShipmentSchedulesNo();
            }
        }

        // 残りが0の行が表示されない, 箱数降順
        collectionView.ItemsSource = lstDShipmentSchedules;

        // 履歴一覧更新
        var exitsHistoryDShipmentSchedule = lstHistoryDShipmentSchedules.FirstOrDefault(d => d.DeliveryProductNumber == deliveryKanbanModel.DeliveryProductNumber && d.DepoID == _loginUserSqlLite.DepoID && d.CompanyID == companyID && d.DeliveryDate == deliveryKanbanModel.DeliveryDate
                                                               && d.DeliveryTimeClass == deliveryKanbanModel.DeliveryTimeClass && d.DeliveryLocation == deliveryKanbanModel.DeliveryLocation && d.DeliverySlipNumber == deliveryKanbanModel.DeliverySlipNumber);

        // スキャン後、履歴一覧を表示
        if(exitsHistoryDShipmentSchedule != null)
        {
            exitsHistoryDShipmentSchedule.NumScan++;
            var numberOfBox = exitsHistoryDShipmentSchedule.NumScan + exitsHistoryDShipmentSchedule.NumberOfBoxesRemaining;
            exitsHistoryDShipmentSchedule.NumScanAndNumBoxRemaining = $"{exitsHistoryDShipmentSchedule.NumScan}/{numberOfBox}";
            // 既存のデータを削除
            var existingDeliveryProductNumber = listHistoryForDisplay.Where(p => p.DeliveryProductNumber == exitsHistoryDShipmentSchedule.DeliveryProductNumber);
            if (existingDeliveryProductNumber.Count() == 0)
                listHistoryForDisplay.Insert(0, exitsHistoryDShipmentSchedule);

            // No降順
            UpdateHistoryDShipmentSchedulesNo();
        }

        collectionViewHistory.ItemsSource = listHistoryForDisplay;
    }


    /// <summary>
    /// 出荷指示リストでNo降順
    /// </summary>
    public void UpdateListDShipmentSchedulesNo()
    {
        for (int i = 0; i < lstDShipmentSchedules.Count; i++)
           lstDShipmentSchedules[i].No = i + 1;
    }


    /// <summary>
    /// 履歴一覧でNo降順
    /// </summary>
    public void UpdateHistoryDShipmentSchedulesNo()
    {
        for (int i = 0; i < listHistoryForDisplay.Count; i++)
           listHistoryForDisplay[i].No = listHistoryForDisplay.Count - i;
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
    public bool SetValueFromQrCode(string clipboardContent)
    {
        string[] changeCodeResult = GetQrcode(clipboardContent);
        string stringDeliveryTimeClass = Regex.Replace(changeCodeResult[9], @"^+" + StartIndexString.DeliveryTimeClassIndex, "");
        string stringKanbanSerialNumber = Regex.Replace(changeCodeResult[15], @"^+" + StartIndexString.KanbanSerialNumberIndex, "");
        var deliverySlipNumberAndPage = Regex.Replace(changeCodeResult[10], @"^+" + StartIndexString.DeliverySlipNumber, "");
        var deliveryDateString = Regex.Replace(changeCodeResult[8], @"^+" + StartIndexString.DeliveryDateIndex, "");
        string stringLotQuantity = Regex.Replace(changeCodeResult[13], @"^+" + StartIndexString.QuantityIndex, "");
        // intやdatetimeへの変換できるようにチェック用
        if (!CheckIntType(stringDeliveryTimeClass).isInt || !CheckIntType(stringKanbanSerialNumber).isInt || !CheckIntType(stringLotQuantity).isInt || !CheckDateTimeType(deliveryDateString).isDate)
            return false;

        deliveryKanbanModel.DeliveryLocation = Regex.Replace(changeCodeResult[5], @"^+" + StartIndexString.Location1Index, "");
        deliveryKanbanModel.DeliveryTimeClass = CheckIntType(stringDeliveryTimeClass).value;
        deliveryKanbanModel.ProductAbbreviation = Regex.Replace(changeCodeResult[11], @"^+" + StartIndexString.ProductAbbreviationIndex, "");
        deliveryKanbanModel.KanbanSerialNumber = CheckIntType(stringKanbanSerialNumber).value;
        deliveryKanbanModel.DeliverySlipNumber = deliverySlipNumberAndPage.Substring(0, deliverySlipNumberAndPage.Length - 4);
        deliveryKanbanModel.DeliveryDate = CheckDateTimeType(deliveryDateString).dateTime;
        deliveryKanbanModel.DeliveryProductNumber = Regex.Replace(changeCodeResult[12], @"^+" + StartIndexString.ProductCodeIndex, "");
        deliveryKanbanModel.LotQuantity = CheckIntType(stringLotQuantity).value;

        return true;
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
            supplierProductNumberCheck = Util.SubStringAfterDash(supplierProductNumberTrim);
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
    public D_HandyErrorMessageModel SetHandyErrorMessage(int depoID, string userName, string scanTextError)
    {
        D_HandyErrorMessageModel d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
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
        listDHandyErrorMessage.Add(SetHandyErrorMessage(depoID, userName, error));

    }


    /// <summary>
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (!isItemTapped)
            {
                isItemTapped = true;
                if (listScanShipment.Count() > 0)
                {
                   bool confirm = await DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                    isItemTapped = false;
                    if (!confirm) return;                        
                }
                isItemTapped = false;
            }

            if (!NetworkCheck.CheckNetworkConnection()) // ネットワーク接続チェック
            {
                await DisplayAlert("エラー", $"E3003: {ErrorMessagesResources.E3003}", "OK");
                return;
            }
            //ロード画面表示
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);
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
            await DisplayAlert("エラー", $"E9999: {ErrorMessagesResources.E9999}", "OK");
        }
    }

    /// <summary>
    /// プログレスバーを更新する
    /// </summary>
    private async void UpdateProgressBar(int totalNumberOfBoxesRemaining)
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
            if (listScanShipment.Count() <=0) { 
                // ERRORサウンドを鳴らす
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultErrorSound);
                await DisplayAlert("エラー", $"E2016: {ErrorMessagesResources.E2016}", "OK");
                return;
            }
            D_StoreOutModel.StoreOutPostBody storeOutPostBody = new D_StoreOutModel.StoreOutPostBody();
            //出庫API送信用model設定
            storeOutPostBody.CompanyCode = _loginUserSqlLite.CompanyCode;
            storeOutPostBody.CompanyPassword = _loginUserSqlLite.CompanyPassword;
            storeOutPostBody.ScanShipmentModel = listScanShipment;
            storeOutPostBody.HandyErrorMessage = listDHandyErrorMessage;
            var jsonDataSend = JsonConvert.SerializeObject(storeOutPostBody);
            
            if (!NetworkCheck.CheckNetworkConnection())
            {
                await DisplayAlert("エラー", $"E3003: {ErrorMessagesResources.E3003}", "OK");
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
                   
            //APIの戻り値によって、表示内容を変える
            string registeredMessage = CreateRegisteredMessage(response.content);

            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            // OKサウンド
            SoundPlayer.PlaySound(resultOkSound);
            await DisplayAlert("完了", registeredMessage, "OK");

            // 送信後は再度ShipmentSchduleのAPIに再度接続して情報を最新化
            CallBackApiToUpdateDataAfterRegistered();
        }
        catch (Exception)
        {
            await DisplayAlert("エラー", $"E9999: {ErrorMessagesResources.E9999}", "OK");
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responceContent"></param>
    /// <returns></returns>
    private string CreateRegisteredMessage(string responceContent) {
        string defaultMessage = "データ登録が完了しました";
        try
        {
            // contentをJSONオブジェクトにデシリアライズ
            var storeOutResponseContent = JsonConvert.DeserializeObject<StoreOutRegisterModel.StoreOutResponseContent>(responceContent);
            if (storeOutResponseContent != null)
            {
                string minusStockProduct = string.Empty;
                string strDuplicateProduct = string.Empty;
                if (storeOutResponseContent.ProductsWithMinusNumberOfBoxes != null ) {
                    //在庫マイナス品番を羅列
                    // productWithMinusNumberOfBoxesからSupplierProductNumberのみを抽出して結合
                    minusStockProduct = "【在庫がマイナスになった品番】\n ";
                    minusStockProduct += string.Join("\n", storeOutResponseContent.ProductsWithMinusNumberOfBoxes.Select(p => p.SupplierProductNumber));// 改行で結合
                }

                if (storeOutResponseContent.ScanShipmentDuplicateData != null )
                {
                    strDuplicateProduct = "【重複があった品番】 \n";
                    //重複したかんばんシリアルと納入先品番を羅列                    
                    var duplicateProduct = storeOutResponseContent.ScanShipmentDuplicateData
                    .Select(p => p.d_ShipmentModel).Select(p => new { p.KanbanSerialNumber, p.SupplierProductNumber }).ToList();                   
                    // 結果の表示
                    foreach (var product in duplicateProduct)
                    {
                        strDuplicateProduct += $"かんばんシリアル: {product.KanbanSerialNumber}, 仕入先品番: {product.SupplierProductNumber}" + "\n";
                    }
                }
                return defaultMessage + '\n' + storeOutResponseContent.Message +  '\n' + minusStockProduct + '\n' + strDuplicateProduct;
            }
            return defaultMessage;
        }
        catch (JsonException)
        {
            return defaultMessage;
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
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(200);

        // 最新データを取得するため、もう一回APIにリクエストを送る
        var responseShipmentSchedule = await App.API.PostMethod(jsonDataShipmentScheduleSend, _handyApiUrl, "ShipmentSchedule/Find");

        if (responseShipmentSchedule.success)
        {
            // APIからコンテンツを取得
            var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduelResponseBody>(responseShipmentSchedule.content);
            var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList; // 出荷指示
            var dShipmentList = contentFromApi.D_ShipmentList; // 出荷実績

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
        if (string.IsNullOrWhiteSpace(loginID) || string.IsNullOrWhiteSpace(password) || (4 > password.Length || password.Length > 12))
        {
            DisplayAlert("エラー", $"E1023: {string.Format(ErrorMessagesResources.E1023, "ログインID・パスワード",12,4)}", "OK");
            return;
        }

        //// ログインIDは合致するデータがない
        var listUser = mUserList.Where(u => u.LoginID == loginID).ToList();

        if (listUser.Count <= 0)
        {
            DisplayAlert("エラー", $"E2003: {ErrorMessagesResources.E2003}", "OK");
            return;
        }

        //// 入力されたパスワードをハッシュ化
        byte[] salt = Hashing.ConvertStringToBytes(listUser.FirstOrDefault().Salt);
        string hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(password, salt);

        // ハッシュ化されたパスワードと一致するかチェック
        if (!listUser.FirstOrDefault().Password.Equals(hashedPassword))
        {
            DisplayAlert("エラー", $"E2026: {ErrorMessagesResources.E2026}", "OK");
            return;
        }

        // ハンディエラーメッセージ実績モデル
        D_HandyErrorMessageModel d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
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
