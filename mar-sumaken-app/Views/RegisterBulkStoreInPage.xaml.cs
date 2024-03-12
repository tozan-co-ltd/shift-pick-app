using mar_sumaken_app.Common;
using mar_sumaken_app.ViewModels;
using System.Collections.ObjectModel;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.Properties;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace mar_sumaken_app.Views;

public partial class RegisterBulkStoreInPage : ContentPage
{
    int countOfScaned = 0;
    bool isItemTapped = false;
    private string? _handyApiUrl;
    private bool _IsScanPageActive = false;
    string supplierProductNumber = string.Empty;
    string lotNumber = string.Empty;
    string clipboardContent = string.Empty;
    public int handyMenuID = 0;

    /// <summary>
    /// ハンディ設定情報
    /// </summary>
    private HandySettingModel.HandySetting _handySetting;

    private readonly LocalDbService _dbService = new LocalDbService();
    /// <summary>
    /// 箱数集計表示リスト
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _TotalNumberOfBoxesDisplay = new ObservableCollection<D_ReceiveModel>();

    /// <summary>
    /// 履歴一覧表示リスト
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _HistoryListDisplay = new ObservableCollection<D_ReceiveModel>();

    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public M_SupplierKanbanModel mSupplierKanban = new M_SupplierKanbanModel();
    public List<M_SupplierKanbanModel> listMSupplierKanban = new List<M_SupplierKanbanModel>();
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode = new List<M_SupplierKanbanModel>();
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban = new List<R_HandyMenuSupplierKanbanModel>();
    public D_StoreInModel.StoreInPostBody storeInPostBody = new D_StoreInModel.StoreInPostBody();

    /// <summary>
    /// まとめ入庫画面表示
    /// </summary>
    /// <param name="menuID"></param>
    public RegisterBulkStoreInPage(int menuID, List<M_ProductModel> listProductItems)
    {
        InitializeComponent();
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        handyMenuID = menuID;
        listProducts = listProductItems;
        CounterBtn.Text = $"{countOfScaned}";
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        enteredNumberLabel.Text = string.Empty;
        BindingContext = new RegisterBulkStoreInPageViewModel();
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
        if (!isItemTapped)
        {
            isItemTapped = true;
            if (_TotalNumberOfBoxesDisplay.Count() > 0)
            {
                bool result = await DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                if (result)
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
                clipboardContent = Clipboard.GetTextAsync().Result;

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
                var supplierProductNumberTrim = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).ToString().Trim();
                supplierProductNumber = Util.SubStringAfterDash(supplierProductNumberTrim);
                // 仕入先品番が存在をチェック
                bool existsSupplierProductNumber = listProducts.Any(product => product.SupplierProductNumber == supplierProductNumber);
                if(!existsSupplierProductNumber)
                {
                    // 識別文字列が正しくない場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2002: {string.Format(ErrorMessagesResources.E2002, "品番マスター")}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                lotNumber = clipboardContent.Substring(mSupplierKanban.LotStartIndex-1, mSupplierKanban.LotLength).Trim();

                // 履歴一覧表示リスト内の同じ品番・キーを取得
                D_ReceiveModel duplicateProduct = _HistoryListDisplay.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();
                if (duplicateProduct != null)
                {
                    // 既にスキャンしている場合はエラー
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = $"E2017: {ErrorMessagesResources.E2017}";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel);
                    return;
                }

                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
                scanTextResult.Text = "スキャンOK";
                scanTextResult.TextColor = Colors.Black;
                numberInputModal.IsVisible = true;
                bulkStockContent.IsVisible = false;
               
            }
        }
        catch (ArgumentOutOfRangeException arg)
        {
            await DisplayAlert("エラー", $"E2001: {ErrorMessagesResources.E2001}" + arg.Message, "OK");
            return;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
            return;
        }
    }

    /// <summary>
    /// ハンディメッセージを設定
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
    /// 箱数入力の数字ボタンをタップした時
    /// </summary>
    private void OnNumberButtonClicked(object sender, EventArgs e)
    {
        // 数値ボタンのクリックを処理
        var button = (Button)sender;
        if (button.Text != string.Empty || int.Parse(button.Text) == 0)
            errorLabel.Text = string.Empty;

        enteredNumberLabel.Text += button.Text;
    }

    /// <summary>
    /// 箱数入力のCボタンをタップした時
    /// </summary>
    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // タップした数字を消す
        if (!string.IsNullOrEmpty(enteredNumberLabel.Text))
        {
            errorLabel.Text = string.Empty;
            //enteredNumberLabel.Text = enteredNumberLabel.Text.Substring(0, enteredNumberLabel.Text.Length - 1);
            enteredNumberLabel.Text = string.Empty;
        }
    }

    /// <summary>
    /// 箱数入力のチェックボタンをタップした時
    /// </summary>
    private async void OnOkButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var loginUser = await _dbService.GetLoginAsync();

            // サウンドのパスを取得
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);

            // 3桁を超える場合はエラー
            if (enteredNumberLabel.Text.Length > 3)
            {
                errorLabel.Text = "最大3桁まで入力可能です。";
                SoundPlayer.PlaySound(resultErrorSound);
                return;
            }
           
            // 入力値が空または0の場合はエラー
            if (enteredNumberLabel.Text == string.Empty)
            {
                errorLabel.Text = "0以上で入力してください。";
                SoundPlayer.PlaySound(resultErrorSound);
                return;
            }
            if (enteredNumberLabel.Text != string.Empty && int.Parse(enteredNumberLabel.Text) == 0)
            {
                errorLabel.Text = "0以上で入力してください。";
                SoundPlayer.PlaySound(resultErrorSound);
                return;
            }
                countOfScaned++;
                CounterBtn.Text = $"{countOfScaned}";

                // 箱数集計表示リスト内の同じ品番を取得
                D_ReceiveModel existingProduct =  _TotalNumberOfBoxesDisplay.Where(p => p.SupplierProductNumber == supplierProductNumber).FirstOrDefault();
                // 履歴一覧表示リスト内の同じ品番・キーを取得
                D_ReceiveModel duplicateProduct = _HistoryListDisplay.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();

                var scanResultModel = new ScanReceiveModel();
                var d_ScanResultModel = new D_ScanResultModel();
                var d_ReceiveModel = new D_ReceiveModel();

                // 箱数
                int numOfBoxes = ((enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0);
                // 収容数
                int lotOfQuantity = (int)double.Parse(clipboardContent.Substring(mSupplierKanban.QuantityStartIndex - 1, mSupplierKanban.QuantityLength).Trim());

                // 箱数集計表示リスト内に同じ品番がある場合は、同一行の箱数を加算
                if (existingProduct != null)
                {
                    //加算処理
                    int numOfBoxesAccumlated = numOfBoxes + existingProduct.NumberOfBoxes;
                    _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel { SupplierProductNumber = supplierProductNumber,
                        LotNumber = lotNumber, NumberOfBoxes = numOfBoxesAccumlated ,Quantity = lotOfQuantity });
                    // 既存の製品を削除
                    _TotalNumberOfBoxesDisplay.Remove(existingProduct);
                    // 読取実績・入荷実績のデータを設定
                    scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                    scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, loginUser.Last().UserName, numOfBoxes, mSupplierKanban.CompanyID, lotOfQuantity,lotNumber);
                }
                else
                {
                    // 箱数集計表示リストに新しい行を追加                    
                    _TotalNumberOfBoxesDisplay.Add(new D_ReceiveModel { SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, 
                        NumberOfBoxes = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 ,
                        Quantity = lotOfQuantity
                    });
                    // 読取実績・入荷実績のデータを設定
                    scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                    scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, loginUser.Last().UserName, numOfBoxes, mSupplierKanban.CompanyID, lotOfQuantity, lotNumber);
                }
                //  仕入先かんばんデータを設定
                listScanReceive.Add(scanResultModel);
                _HistoryListDisplay.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, 
                    NumberOfBoxes = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 ,Quantity = lotOfQuantity});

                // 箱数集計表示リスト追加
                boxesCollectionView.ItemsSource = _TotalNumberOfBoxesDisplay.Reverse();
                // 履歴一覧表示リスト追加
                historyCollectionView.ItemsSource = _HistoryListDisplay.Reverse();
                numberInputModal.IsVisible = false;
                bulkStockContent.IsVisible = true;
                enteredNumberLabel.Text = string.Empty;
                errorLabel.Text = string.Empty;
              
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
        }
    }

    /// <summary>
    /// 読取実績モデル作成
    /// </summary>
    /// <param name="d_ScanResultModel">読取実績モデル</param>
    /// <param name="depoID">倉庫ID</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="userName">ユーザー名</param>
    /// <param name="supplierKanbanID">仕入先かんばんID</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoID,  string clipboardContent, string userName, int supplierKanbanID)
    {
        d_ScanResultModel.DepoID = depoID;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.SupplierKanbanID = supplierKanbanID;
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
    /// <param name="userName">ユーザー名</param>
    /// <param name="numOfBoxes">箱数</param>
    /// <param name="companyID">会社ID</param>
    /// <returns></returns>
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, string userName, int numOfBoxes, int companyID,int lotOfQuantity,string lotNumber)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.Quantity = lotOfQuantity * numOfBoxes;
        d_ReceiveModel.ReceiveDatetime = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.NumberOfBoxes = numOfBoxes;
        d_ReceiveModel.LotNumber = lotNumber;
        d_ReceiveModel.ScanedAt = DateTime.Now;
        d_ReceiveModel.CreatedAt = DateTime.Now;
        d_ReceiveModel.CreatedBy = userName;

        return d_ReceiveModel;
    }

    /// <summary>
    /// 箱数入力のキャンセルボタンをタップした時
    /// </summary>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        enteredNumberLabel.Text = string.Empty;
        numberInputModal.IsVisible = false;
        bulkStockContent.IsVisible = true;
        errorLabel.Text = string.Empty;
    }

    /// <summary>
    /// 箱数集計表示ボタン押下時
    /// </summary>
    private void OnTotalNumberOfBoxesBtnClicked(object sender, EventArgs e)
    {
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
        numberOfBoxListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        numberOfBoxList.IsVisible = false;
        historyList.IsVisible = true;
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
                //  仕入先かんばんデータを設定
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
                await DisplayAlert("完了", "データ登録が完了しました。", "OK");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new RegisterBulkStoreInPage(handyMenuID, listProducts);
            }
            else
            {
                // ERRORサウンドを鳴らす
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultErrorSound);
                await DisplayAlert("エラー", $"E2016: {ErrorMessagesResources.E2016}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
            return;
        }
    }
}
