using System.Collections.ObjectModel;
using mar_sumaken_app.Models;
using mar_sumaken_app.ViewModels;
using mar_sumaken_app.Common;
using Plugin.Maui.Audio;
using mar_sumaken_app.Services;
using Microsoft.Maui.Animations;
using static SQLite.SQLite3;
using Newtonsoft.Json;
using System.ComponentModel.Design;

namespace mar_sumaken_app.Views;

public partial class RegisterStoreInPage : ContentPage
{
    int count = 0;
    int boxCout = 1;
    bool isItemTapped = false;
    private ObservableCollection<D_ReceiveModel> _numberOfBoxList;
    private ObservableCollection<D_ReceiveModel> _historyList;
    private ObservableCollection<D_ReceiveModel> _itemsHistory;
    private readonly LocalDbService _dbService;
    private string? _handyApiUrl;
    private HandySettingModel.HandySetting _handySetting;
    private bool _IsScanPageActive = false;
    public int handyMenuID = 0;
    public D_StoreInModel.StoreInPostBody storeInPostBody;
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage;
    public List<ScanReceiveModel> listScanReceive;
    public M_SupplierKanbanModel mSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;

    public RegisterStoreInPage(int menuID)
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        CounterBtn.Text = $"{count}";
        handyMenuID = menuID;
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        _numberOfBoxList = new ObservableCollection<D_ReceiveModel>();
        _historyList = new ObservableCollection<D_ReceiveModel>();
        _itemsHistory = new ObservableCollection<D_ReceiveModel>();
        storeInPostBody = new D_StoreInModel.StoreInPostBody();
        listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
        listScanReceive = new List<ScanReceiveModel>();
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        BindingContext = new RegisterStoreInPageViewModel();
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _IsScanPageActive = true;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの購読を解除する
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _IsScanPageActive = false;
    }

    /// <summary>
    /// 戻る
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
        if(!isItemTapped)
        {
            isItemTapped = true;
            if (_numberOfBoxList.Count() > 0)
            {
                bool clipboardContent = await Application.Current.MainPage.DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                if (clipboardContent)
                    Application.Current.MainPage = new MenuPage();
            }
            else
                Application.Current.MainPage = new MenuPage();

            isItemTapped = false;
        }
    }

    /// <summary>
    /// ハンディリーダーでスキャンする時、処理する
    /// </summary>
    protected async void OnClipboardContentChanged(object sender, EventArgs e)
    {
        try
        {
            var settings = await _dbService.GetSetting();
            _handyApiUrl = settings.Last().HandyApiUrl;
            var loginUser = await _dbService.GetLoginAsync();
            var d_HandyErrorMessageModel = new D_HandyErrorMessageModel();
            if (_IsScanPageActive)
            {
                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                string clipboardContent = Clipboard.GetTextAsync().Result;

                if (string.IsNullOrEmpty(clipboardContent))
                {
                    //d_HandyErrorMessageModel.HandyMenuID = settings.Last().Han;
                    scanTextResult.Text = "E2001: QRコードが読めません。";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                //  仕入先かんばん情報を取得
                listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();
                listRHandyMenuSupplierKanban = await _dbService.GetRHandyMenuSupplierKanbanByHandyMenuID(handyMenuID);
                listMSupplierKanbanForCheckQRCode = await _dbService.GetMSupplierKanbanByHandySupplierKanbanID(listRHandyMenuSupplierKanban.FirstOrDefault().SupplierKanbanID);
                mSupplierKanban = listMSupplierKanbanForCheckQRCode.FirstOrDefault();

                string[] substrings = clipboardContent.Split(' ');
                // (株)　刈谷支のQR社用
                if (substrings[0].Trim().IndexOf(mSupplierKanban.IdentifyString) != -1)
                {
                    string supplierProductNumber = string.Empty;
                    int numberOfBoxes = 0;
                    string mainProductKey = string.Empty;
                    supplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();　//品番桁数
                    numberOfBoxes = int.Parse(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim()); // 収容数
                    mainProductKey = clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();　//キー

                    // 既存の製品に同じ製品番号があるかチェックする
                    D_ReceiveModel existingProduct = _numberOfBoxList.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber);
                    // 既存の製品に同じ製品番号とキーがあるかチェックする 
                    D_ReceiveModel duplicateProduct = _historyList.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber && p.MainProductKey == mainProductKey);

                    var scanResultModel = new ScanReceiveModel();
                    var d_ScanResultModel = new D_ScanResultModel();
                    var d_ReceiveModel = new D_ReceiveModel();

                    if (duplicateProduct == null)
                    {
                        count++;
                        CounterBtn.Text = $"{count}";

                        if (existingProduct != null)
                        {
                            existingProduct.Quantity++;
                            _numberOfBoxList.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = existingProduct.Quantity });
                            // 既存のデータを削除する
                            _numberOfBoxList.Remove(existingProduct);
                            // 読取実績のデータを設定
                            scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numberOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                            scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, numberOfBoxes, loginUser.Last().UserName,  existingProduct.Quantity, mSupplierKanban.CompanyID);
                        }
                        else
                        {
                            _numberOfBoxList.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = boxCout });
                            // 読取実績のデータを設定
                            scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numberOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                            scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, numberOfBoxes, loginUser.Last().UserName, boxCout, mSupplierKanban.CompanyID);
                        }

                        // 重複ハンドル前のすべてのアイテムを取得する
                        _historyList.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = boxCout });
                    }
                    else
                    {
                        // エラーサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1003: 既にスキャン済みの仕入先かんばんです。";
                        scanTextResult.TextColor = Colors.Red;
                        d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                        return;
                    }

                    //  仕入先かんばんデータを設定
                    storeInPostBody.CompanyCode = settings.Last().CompanyCode;
                    storeInPostBody.CompanyPassword = settings.Last().CompanyPassword;
                    listScanReceive.Add(scanResultModel);

                    // OKサウンド
                    SoundPlayer.PlaySound(resultOkSound);
                    scanTextResult.Text = "スキャンOK";
                    scanTextResult.TextColor = Colors.Black;

                    _itemsHistory.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, MainProductKey = mainProductKey, NumberOfBoxes = numberOfBoxes });
                    // 履歴コレクションビュー
                    historyCollectionView.ItemsSource = _itemsHistory.Reverse();
                    // ボックスリストの数
                    collectionView.ItemsSource = _numberOfBoxList.Reverse();
                }
                else
                {
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = "E1000: QRコードが正しくありません。";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    /// <summary>
    ///  ハンディメッセージを設定
    /// </summary>
    /// <param name="d_ScanResultModel">読取実績モデル</param>
    /// <param name="depoId">デーポID</param>
    /// <param name="numberOfBoxes">ログインのユーザー</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="userName">ユーザー名</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoId, int numberOfBoxes, string clipboardContent, string userName, int supplierKanbanID)
    {
        d_ScanResultModel.DepoID = depoId;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.SupplierKanbanID = supplierKanbanID;
        d_ScanResultModel.NumberOfInputBoxes = numberOfBoxes;
        d_ScanResultModel.FirstScanedString = clipboardContent;
        d_ScanResultModel.ScanedAt = DateTime.Now;
        d_ScanResultModel.CreatedAt = DateTime.Now;
        d_ScanResultModel.CreatedBy = userName;

        return d_ScanResultModel;
    }


    /// <summary>
    ///  ハンディメッセージを設定
    /// </summary>
    /// <param name="d_ReceiveModel">ハンディエラーメッセージ</param>
    /// <param name="supplierProductNumber">仕入先品番</param>
    /// <param name="numberOfBoxes">入力箱数</param>
    /// <param name="boxCout">箱数</param>
    /// <returns></returns>
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, int numberOfBoxes, string userName, int boxCout , int companyID)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.ReceiveDatetime = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.NumberOfBoxes = numberOfBoxes;
        d_ReceiveModel.Quantity = boxCout;
        d_ReceiveModel.ScanedAt = DateTime.Now;
        d_ReceiveModel.CreatedAt = DateTime.Now;
        d_ReceiveModel.CreatedBy = userName;

        return d_ReceiveModel;
    }


    /// <summary>
    ///  ハンディメッセージを設定
    /// </summary>
    /// <param name="d_HandyErrorMessageModel">ハンディエラーメッセージ</param>
    /// <param name="loginUser">ログインのユーザー</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="scanTextError">スキャンテキストエラー</param>
    /// <returns></returns>
    public D_HandyErrorMessageModel SetHandyErrorMessage(D_HandyErrorMessageModel d_HandyErrorMessageModel, List<LoginModel.LoginUserSqlLite> loginUser, string clipboardContent, string scanTextError)
    {
        d_HandyErrorMessageModel.DepoID = loginUser.Last().DepoID;
        d_HandyErrorMessageModel.HandyMenuID = handyMenuID;
        d_HandyErrorMessageModel.FirstScanedString = clipboardContent;
        d_HandyErrorMessageModel.ErrorMessage = scanTextError;
        d_HandyErrorMessageModel.CreatedAt = DateTime.Now;
        d_HandyErrorMessageModel.CreatedBy = loginUser.Last().UserName;

        return d_HandyErrorMessageModel;
    }


    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (_numberOfBoxList.Count() > 0)
            {
                storeInPostBody.ScanReceive = listScanReceive;
                storeInPostBody.HandyErrorMessage = listDHandyErrorMessage;
                var jsonDataSend = JsonConvert.SerializeObject(storeInPostBody);
                // APIにリクエストを送る
                var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "StoreIn/Register");
                // APIからコンテンツを取得
                if (!response.success)
                {
                    await DisplayAlert("エラー", response.content, "OK");
                    return;
                }

                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);

                // OKサウン
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "閉じる");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(300);
                Application.Current.MainPage = new RegisterStoreInPage(handyMenuID);
            }
            else
            {
                // サウンドのパスを取得する
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                // エラーサウンド
                SoundPlayer.PlaySound(resultErrorSound);
                await Application.Current.MainPage.DisplayAlert("お知らせ", "データがありません", "OK");
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
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        numberOfBoxList.IsVisible = true;
        historyList.IsVisible = false;
    }

    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        numberOfBoxListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        numberOfBoxList.IsVisible = false;
        historyList.IsVisible = true;
    }
}