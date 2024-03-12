using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using mar_sumaken_app.Properties;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;

public partial class RegisterStoreInPage : ContentPage
{
    int countOfScaned = 0;
    //入庫箱数の単位は必ず1
    int numOfBoxes = 1;
    bool isItemTapped = false;
    private string? _handyApiUrl;
    private bool _IsScanPageActive = false;
    public int handyMenuID = 0;

    /// <summary>
    /// ハンディ設定情報
    /// </summary>
    private HandySettingModel.HandySetting _handySetting;

    /// <summary>
    /// 箱数集計表示リスト
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _TotalNumberOfBoxesDisplay = new ObservableCollection<D_ReceiveModel>();

    /// <summary>
    /// 履歴一覧表示リスト
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _HistoryListDisplay = new ObservableCollection<D_ReceiveModel>();

    private readonly LocalDbService _dbService = new LocalDbService();
    public D_StoreInModel.StoreInPostBody storeInPostBody = new D_StoreInModel.StoreInPostBody();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public M_SupplierKanbanModel mSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;

    /// <summary>
    /// 入庫画面表示
    /// </summary>
    /// <param name="menuID"></param>
    /// <param name="listProductItems"></param>
    public RegisterStoreInPage(int menuID, List<M_ProductModel> listProductItems)
	{
		InitializeComponent();
        CounterBtn.Text = $"{countOfScaned}";
        handyMenuID = menuID;
        listProducts = listProductItems;
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        BindingContext = new RegisterStoreInPageViewModel();
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブ
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _IsScanPageActive = true;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの購読を解除
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _IsScanPageActive = false;
    }

    /// <summary>
    /// 戻るボタン押下
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
        if(!isItemTapped)
        {
            isItemTapped = true;
            if (_TotalNumberOfBoxesDisplay.Count() > 0)
            {
                bool clipboardContent = await DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                if (clipboardContent)
                    Application.Current.MainPage = new MenuPage();
            }
            else
                Application.Current.MainPage = new MenuPage();

            isItemTapped = false;
        }
    }

    /// <summary>
    /// ハンディリーダースキャン時
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
                // サウンドパスを取得
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                string clipboardContent = Clipboard.GetTextAsync().Result;

                if (string.IsNullOrEmpty(clipboardContent))
                {
                    scanTextResult.Text = $"E2015: {ErrorMessagesResources.E2015}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
                listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();
                listRHandyMenuSupplierKanban = await _dbService.GetRHandyMenuSupplierKanbanByHandyMenuID(handyMenuID);
                //listMSupplierKanbanForCheckQRCode = await _dbService.GetMSupplierKanbanByHandySupplierKanbanID(listRHandyMenuSupplierKanban.FirstOrDefault().SupplierKanbanID);
                listMSupplierKanbanForCheckQRCode = await _dbService.GetMSupplierKanbanByHandySupplierKanbanIDs(listRHandyMenuSupplierKanban.Select(item => item.SupplierKanbanID).ToList());
                // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
                var supplierIdentifyMatchStringList = listMSupplierKanbanForCheckQRCode.Where(x => x.IdentifyString == Util.QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, clipboardContent)).ToList();
                if (supplierIdentifyMatchStringList.Count <= 0)
                {
                    // 識別文字列が正しくない場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2001: {ErrorMessagesResources.E2001}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                mSupplierKanban = supplierIdentifyMatchStringList.FirstOrDefault();

               
                // 文字列をトリム
                string supplierProductNumber = string.Empty;
                int lotOfQuantity = 0;
                    
                // 品番
                supplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
                // 仕入先品番が存在をチェック
                bool existsSupplierProductNumber = listProducts.Any(product => product.SupplierProductNumber == supplierProductNumber);
                if (!existsSupplierProductNumber)
                {
                    // 品番マスターにない場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2002: {string.Format(ErrorMessagesResources.E2002, "品番マスター")}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                // 収容数
                int lotOfQuantity = 0;
                var intCheckResult = Util.CheckIntType(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim());
                lotOfQuantity = intCheckResult.value;
                if (!intCheckResult.isInt)
                {
                    // 数値が変換できない場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2001: {ErrorMessagesResources.E2001}";
                    scanTextResult.TextColor = Colors.Red;
                    listDHandyErrorMessage.Add(SetHandyErrorMessage(loginUser, clipboardContent, scanTextResult.Text));
                    return;
                }

                // キー
                string mainProductKey = string.Empty;
                string fisrtSubProductKey = string.Empty;
                string secondSubProductKey = string.Empty;
                if (mSupplierKanban.MainProductKeyLength != 0)
                {
                    mainProductKey = mSupplierKanban.MainProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();
                    fisrtSubProductKey = mSupplierKanban.FirstSubProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.FirstSubProductKeyStartIndex - 1, mSupplierKanban.FirstSubProductKeyLength).Trim();
                    secondSubProductKey = mSupplierKanban.SecondSubProductKeyLength == 0 ? string.Empty : clipboardContent.Substring(mSupplierKanban.SecondSubProductKeyStartIndex - 1, mSupplierKanban.SecondSubProductKeyLength).Trim();
                }

                // 箱数集計表示リスト内の同じ品番を取得
                D_ReceiveModel existingProduct = _TotalNumberOfBoxesDisplay.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber);

                // 履歴一覧表示リスト内の同じ品番・キーを取得
                //D_ReceiveModel duplicateProduct = _HistoryListDisplay.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber && p.MainProductKey == mainProductKey);
                
                // ローカルで仕入先品番重複チェック
                bool isKeyDuplicated = listScanReceive.Select(item=>item.d_ReceiveModel).ToList().Any(x => x.SupplierProductNumber == supplierProductNumber && x.MainProductKey == mainProductKey &&
                                                                                                    x.FirstSubProductKey == fisrtSubProductKey && x.SecondSubProductKey == secondSubProductKey);

                var scanResultModel = new ScanReceiveModel();
                var d_ScanResultModel = new D_ScanResultModel();
                var d_ReceiveModel = new D_ReceiveModel();

                if (isKeyDuplicated){
                    // 既にスキャンしている場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2017: {ErrorMessagesResources.E2017}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }


                countOfScaned++;
                CounterBtn.Text = $"{countOfScaned}";

                // 箱数集計表示リスト内に同じ品番がある場合は、同一行の箱数を加算
                if (existingProduct != null)
                {
                    existingProduct.NumberOfBoxes++;
                    _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = existingProduct.NumberOfBoxes, MainProductKey = mainProductKey, Quantity = lotOfQuantity });
                    // 既存のデータを削除
                    _TotalNumberOfBoxesDisplay.Remove(existingProduct);
                    // 読取実績・入荷実績のデータを設定
                    scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                    scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber,
                        numOfBoxes, loginUser.Last().UserName, lotOfQuantity, mSupplierKanban.CompanyID, mainProductKey, fisrtSubProductKey, secondSubProductKey);
                }
                else
                {
                    // 箱数集計表示リストに新しい行を追加
                    _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numOfBoxes, MainProductKey = mainProductKey, Quantity = lotOfQuantity });
                    // 読取実績・入荷実績のデータを設定
                    scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                    scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber,
                        numOfBoxes, loginUser.Last().UserName, lotOfQuantity, mSupplierKanban.CompanyID, mainProductKey, fisrtSubProductKey, secondSubProductKey);
                }
             
               
                // 仕入先かんばんデータを設定
                listScanReceive.Add(scanResultModel);

                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
                scanTextResult.Text = "スキャンOK";
                scanTextResult.TextColor = Colors.Black;

                // 箱数集計表示リスト追加
                boxesCollectionView.ItemsSource = _TotalNumberOfBoxesDisplay.Reverse();
                // 履歴一覧表示リスト追加
                _HistoryListDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, MainProductKey = mainProductKey, Quantity = lotOfQuantity });
                historyCollectionView.ItemsSource = _HistoryListDisplay.Reverse();
              
            }
        }
        catch (ArgumentOutOfRangeException arg)
        {
            await DisplayAlert("エラー", $"E2001: {ErrorMessagesResources.E2001}\n" + arg.Message, "OK");
            return;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
            return;
        }
    }


    /// <summary>
    /// 読取実績モデル作成
    /// </summary>
    /// <param name="d_ScanResultModel">読取実績モデル</param>
    /// <param name="depoID">倉庫ID</param>
    /// <param name="numberOfBoxes">入力箱数</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="userName">ユーザー名</param>
    /// <param name="supplierKanbanID">仕入先かんばんID</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoID, int numberOfBoxes, string clipboardContent, string userName, int supplierKanbanID)
    {
        d_ScanResultModel.DepoID = depoID;
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
    /// 入荷実績モデル作成
    /// </summary>
    /// <param name="d_ReceiveModel">入荷実績モデル</param>
    /// <param name="supplierProductNumber">仕入先品番</param>
    /// <param name="numberOfBoxes">入力箱数</param>
    /// <param name="quantity">数量</param>
    /// <returns></returns>
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, int numberOfBoxes, string userName, 
        int quantity , int companyID, string mainProductKey, string fisrtSubProductKey, string secondSubProductKey)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.ReceiveDatetime = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.NumberOfBoxes = numberOfBoxes;
        d_ReceiveModel.Quantity = quantity;
        d_ReceiveModel.MainProductKey = mainProductKey;
        d_ReceiveModel.FirstSubProductKey = fisrtSubProductKey;
        d_ReceiveModel.SecondSubProductKey = secondSubProductKey;
        d_ReceiveModel.ScanedAt = DateTime.Now;
        d_ReceiveModel.CreatedAt = DateTime.Now;
        d_ReceiveModel.CreatedBy = userName;

        return d_ReceiveModel;
    }


    /// <summary>
    /// ハンディメッセージを設定
    /// </summary>
    /// <param name="d_HandyErrorMessageModel">ハンディエラーメッセージモデル</param>
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
    /// 入庫データ登録
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (_TotalNumberOfBoxesDisplay.Count() > 0)
            {
                var settings = await _dbService.GetSetting();
                storeInPostBody.CompanyCode = settings.Last().CompanyCode;
                storeInPostBody.CompanyPassword = settings.Last().CompanyPassword;
                storeInPostBody.ScanReceive = listScanReceive;
                storeInPostBody.HandyErrorMessage = listDHandyErrorMessage;
                var jsonDataSend = JsonConvert.SerializeObject(storeInPostBody);

                // ネットワーク接続チェック
                if (!NetworkCheck.CheckNetworkConnection())
                {
                    await DisplayAlert("エラー", $"E3003: {ErrorMessagesResources.E3003}", "OK");
                    return;
                }

                // APIにリクエストを送る
                var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "StoreIn/Register");
                // APIからコンテンツを取得
                if (!response.success)
                {
                    await DisplayAlert("エラー", response.content, "OK");
                    return;
                }

                // OKサウンドを鳴らす
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "OK");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new RegisterStoreInPage(handyMenuID, listProducts);
            }
            else
            {
                // ERRORサウンドを鳴らす
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultErrorSound);
                await Application.Current.MainPage.DisplayAlert("エラー", $"E2016: {ErrorMessagesResources.E2016}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
            return;
        }
    }

    /// <summary>
    /// 箱数集計表示ボタン押下時
    /// </summary>
    private void OnTotalNumberOfBoxesBtnClicked(object sender, EventArgs e)
    {
        // ボタンの色・表示内容変更
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        numberOfBoxList.IsVisible = true;
        historyList.IsVisible = false;
    }

    /// <summary>
    /// 履歴一覧表示ボタン押下時
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        // ボタンの色・表示内容変更
        numberOfBoxListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        numberOfBoxList.IsVisible = false;
        historyList.IsVisible = true;
    }
}