using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;

public partial class RegisterStoreInPage : ContentPage
{
    int count = 0;
    int quantity = 1;
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

    private ObservableCollection<D_ReceiveModel> _historyList = new ObservableCollection<D_ReceiveModel>();
    private readonly LocalDbService _dbService = new LocalDbService();
    public D_StoreInModel.StoreInPostBody storeInPostBody = new D_StoreInModel.StoreInPostBody();
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
    public RegisterStoreInPage(int menuID)
	{
		InitializeComponent();
        CounterBtn.Text = $"{count}";
        handyMenuID = menuID;
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
                    scanTextResult.Text = "E2015: QRコードの読取ができませんでした。再度スキャンしてください。";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                // ローカルの仕入先かんばん情報取得(この画面で読めるかんばんのみ)
                listMSupplierKanban = await _dbService.GetMSupplierKanbanAsync();
                listRHandyMenuSupplierKanban = await _dbService.GetRHandyMenuSupplierKanbanByHandyMenuID(handyMenuID);
                listMSupplierKanbanForCheckQRCode = await _dbService.GetMSupplierKanbanByHandySupplierKanbanID(listRHandyMenuSupplierKanban.FirstOrDefault().SupplierKanbanID);
                mSupplierKanban = listMSupplierKanbanForCheckQRCode.FirstOrDefault();

                // スキャンデータチェック
                string[] substrings = clipboardContent.Split(' ');
                // 識別文字列チェック
                if (substrings.First().Trim().IndexOf(mSupplierKanban.IdentifyString) != -1)
                {
                    // 文字列をトリム
                    string supplierProductNumber = string.Empty;
                    int numberOfBoxes = 0;
                    string mainProductKey = string.Empty;
                    // 品番桁数
                    supplierProductNumber = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).Trim();
                    // 収容数
                    numberOfBoxes = int.Parse(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim());
                    // キー
                    mainProductKey = clipboardContent.Substring(mSupplierKanban.MainProductKeyStartIndex - 1, mSupplierKanban.MainProductKeyLength).Trim();

                    // 箱数集計表示リスト内の同じ品番を取得
                    D_ReceiveModel existingProduct = _TotalNumberOfBoxesDisplay.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber);

                    // 履歴一覧表示リスト内の同じ品番・キーを取得
                    D_ReceiveModel duplicateProduct = _historyList.FirstOrDefault(p => p.SupplierProductNumber == supplierProductNumber && p.MainProductKey == mainProductKey);

                    var scanResultModel = new ScanReceiveModel();
                    var d_ScanResultModel = new D_ScanResultModel();
                    var d_ReceiveModel = new D_ReceiveModel();

                    if (duplicateProduct == null)
                    {
                        count++;
                        CounterBtn.Text = $"{count}";

                        // 箱数集計表示リスト内に同じ品番がある場合は、同一行の箱数を加算
                        if (existingProduct != null)
                        {
                            existingProduct.Quantity++;
                            _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = existingProduct.Quantity });
                            // 既存のデータを削除
                            _TotalNumberOfBoxesDisplay.Remove(existingProduct);
                            // 読取実績・入荷実績のデータを設定
                            scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numberOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                            scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, numberOfBoxes, loginUser.Last().UserName,  existingProduct.Quantity, mSupplierKanban.CompanyID);
                        }
                        else
                        {
                            // 箱数集計表示リストに新しい行を追加
                            _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = quantity });
                            // 読取実績・入荷実績のデータを設定
                            scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, numberOfBoxes, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                            scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, numberOfBoxes, loginUser.Last().UserName, quantity, mSupplierKanban.CompanyID);
                        }
                        // 重複ハンドル前のすべてのアイテムを取得
                        _historyList.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, NumberOfBoxes = numberOfBoxes, MainProductKey = mainProductKey, Quantity = quantity });
                    }
                    else
                    {
                        // 既にスキャンしている場合はエラー
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1003: 既にスキャン済みの仕入先かんばんです。";
                        scanTextResult.TextColor = Colors.Red;
                        d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                        listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                        return;
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
                    _HistoryListDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, MainProductKey = mainProductKey, NumberOfBoxes = numberOfBoxes });
                    historyCollectionView.ItemsSource = _HistoryListDisplay.Reverse();
                }
                else
                {
                    // 識別文字列が正しくない場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = "E2001: QRコードが正しくありません。正しいQRコードを読み取ってください。";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }
            }
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
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, int numberOfBoxes, string userName, int quantity , int companyID)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.ReceiveDatetime = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.NumberOfBoxes = numberOfBoxes;
        d_ReceiveModel.Quantity = quantity;
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
                    await DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
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
                await Task.Delay(300);
                Application.Current.MainPage = new RegisterStoreInPage(handyMenuID);
            }
            else
            {
                // ERRORサウンドを鳴らす
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultErrorSound);
                await Application.Current.MainPage.DisplayAlert("エラー", "E2016: 読取データがありません。", "OK");
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