using mar_sumaken_app.Common;
using mar_sumaken_app.ViewModels;
using System.Collections.ObjectModel;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace mar_sumaken_app.Views;

public partial class RegisterBulkStoreInPage : ContentPage
{
    int count = 0;
    bool isItemTapped = false;
    private string? _handyApiUrl;
    private bool _IsScanPageActive = false;
    string supplierProductNumber = string.Empty;
    string lotNumber = string.Empty;
    string clipboardContent = string.Empty;
    public int handyMenuID = 0;
    private HandySettingModel.HandySetting _handySetting;
    private readonly LocalDbService _dbService = new LocalDbService();
    private ObservableCollection<D_ReceiveModel> _numberOfBoxListItems = new ObservableCollection<D_ReceiveModel>();
    private ObservableCollection<D_ReceiveModel> _listItemsNoteDetele = new ObservableCollection<D_ReceiveModel>();
    private ObservableCollection<D_ReceiveModel> _numberOfBoxListItemsHistory = new ObservableCollection<D_ReceiveModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public M_SupplierKanbanModel mSupplierKanban = new M_SupplierKanbanModel();
    public List<M_SupplierKanbanModel> listMSupplierKanban = new List<M_SupplierKanbanModel>();
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode = new List<M_SupplierKanbanModel>();
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban = new List<R_HandyMenuSupplierKanbanModel>();
    public D_StoreInModel.StoreInPostBody storeInPostBody = new D_StoreInModel.StoreInPostBody();

    public RegisterBulkStoreInPage(int menuID)
    {
        InitializeComponent();
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        handyMenuID = menuID;
        CounterBtn.Text = $"{count}";
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        enteredNumberLabel.Text = string.Empty;
        BindingContext = new RegisterBulkStoreInPageViewModel();
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
        if (!isItemTapped)
        {
            isItemTapped = true;
            if (_numberOfBoxListItems.Count() > 0)
            {
                bool result = await Application.Current.MainPage.DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                if (result)
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
                clipboardContent = Clipboard.GetTextAsync().Result;

                if (string.IsNullOrEmpty(clipboardContent))
                {
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

                if (clipboardContent.Trim().IndexOf(mSupplierKanban.IdentifyString) != -1)
                {
                    var supplierProductNumberTrim = clipboardContent.Substring(mSupplierKanban.ProductNumberStartIndex - 1, mSupplierKanban.ProductNumberLength).ToString().Trim();
                    if(supplierProductNumberTrim.IndexOf("-") != -1) // -の後ろの文字を削除する
                        supplierProductNumberTrim = supplierProductNumberTrim.Substring(0, supplierProductNumberTrim.IndexOf('-'));

                    supplierProductNumber = supplierProductNumberTrim;
                    lotNumber = clipboardContent.Substring(mSupplierKanban.LotStartIndex-1, mSupplierKanban.LotLength).Trim();
                    // 既存の製品に同じ製品番号があるかチェックする
                    D_ReceiveModel duplicateProduct = _listItemsNoteDetele.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();
                    if (duplicateProduct != null)
                    {
                        // エラーサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1003: 既にスキャン済みの仕入先かんばんです。";
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
                else
                {
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                    scanTextResult.Text = "E1000:QRコードが正しくありません。";
                    scanTextResult.TextColor = Colors.Red;
                    d_HandyErrorMessageModel = SetHandyErrorMessage(d_HandyErrorMessageModel, loginUser, clipboardContent, scanTextResult.Text);
                    listDHandyErrorMessage.Add(d_HandyErrorMessageModel); 
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
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
    /// 数字ボタンをタップしたのを処理する
    /// </summary>
    private void OnNumberButtonClicked(object sender, EventArgs e)
    {
        // 数値ボタンのクリックを処理する
        var button = (Button)sender;
        if (button.Text != string.Empty || int.Parse(button.Text) == 0)
            errorLabel.Text = string.Empty;

        enteredNumberLabel.Text += button.Text;
    }

    /// <summary>
    /// タップした数字を消す
    /// </summary>
    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Handle delete button click
        if (!string.IsNullOrEmpty(enteredNumberLabel.Text))
        {
            errorLabel.Text = string.Empty;
            enteredNumberLabel.Text = enteredNumberLabel.Text.Substring(0, enteredNumberLabel.Text.Length - 1);
        }
    }

    /// <summary>
    /// オーケーボタンを押すとチェックして処理
    /// </summary>
    private async void OnOkButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var loginUser = await _dbService.GetLoginAsync();
            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);

            if (enteredNumberLabel.Text.Length > 3)
            {
                errorLabel.Text = "最大3桁まで入力可能です。";
                // エラーサウンド
                SoundPlayer.PlaySound(resultErrorSound);
            }
            else
            {
                if (enteredNumberLabel.Text == string.Empty)
                {
                    errorLabel.Text = "0以上で入力してください。";
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                }
                else if (enteredNumberLabel.Text != string.Empty && int.Parse(enteredNumberLabel.Text) == 0)
                {
                    errorLabel.Text = "0以上で入力してください。";
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                }
                else
                {
                    // 既存の製品に同じ製品番号があるかチェックする
                    D_ReceiveModel existingProduct =  _numberOfBoxListItems.Where(p => p.SupplierProductNumber == supplierProductNumber).FirstOrDefault();
                    // 既存の製品に同じ製品番号とキーがあるかチェックする 
                    D_ReceiveModel duplicateProduct = _listItemsNoteDetele.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();
                    var scanResultModel = new ScanReceiveModel();
                    var d_ScanResultModel = new D_ScanResultModel();
                    var d_ReceiveModel = new D_ReceiveModel();
                    count++;
                    CounterBtn.Text = $"{count}";
                    int quantity = 0; //箱数
                    if (existingProduct != null)
                    {
                        quantity = ((enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0) + existingProduct.Quantity;
                        _numberOfBoxListItems.Add(new D_ReceiveModel { SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = ((enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0) + existingProduct.Quantity });
                        // 既存の製品を削除する
                        _numberOfBoxListItems.Remove(existingProduct);
                        // 読取実績のデータを設定
                        scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                        scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, loginUser.Last().UserName, quantity, mSupplierKanban.CompanyID);
                    }
                    else
                    {
                        quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0;
                        _numberOfBoxListItems.Add(new D_ReceiveModel { SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
                        // 重複処理前のすべてのアイテムを取得する
                        _listItemsNoteDetele.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });

                        // 読取実績のデータを設定
                        scanResultModel.d_ScanResultModel = SetScanResultModel(d_ScanResultModel, loginUser.Last().DepoID, clipboardContent, loginUser.Last().UserName, mSupplierKanban.SupplierKanbanID);
                        scanResultModel.d_ReceiveModel = SetDReceiveModel(d_ReceiveModel, supplierProductNumber, loginUser.Last().UserName, quantity, mSupplierKanban.CompanyID);
                    }
                    //  仕入先かんばんデータを設定
                    listScanReceive.Add(scanResultModel);
                    _numberOfBoxListItemsHistory.Add(new D_ReceiveModel {SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
                    // 履歴コレクションビュー
                    historyCollectionView.ItemsSource = _numberOfBoxListItemsHistory.Reverse();
                    collectionView.ItemsSource = _numberOfBoxListItems.Reverse();
                    numberInputModal.IsVisible = false;
                    bulkStockContent.IsVisible = true;
                    enteredNumberLabel.Text = string.Empty;
                    errorLabel.Text = string.Empty;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    ///  ハンディメッセージを設定
    /// </summary>
    /// <param name="d_ScanResultModel">読取実績モデル</param>
    /// <param name="depoId">デーポID</param>
    /// <param name="clipboardContent">クリップボードの内容</param>
    /// <param name="userName">ユーザー名</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoId,  string clipboardContent, string userName, int supplierKanbanID)
    {
        d_ScanResultModel.DepoID = depoId;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.SupplierKanbanID = supplierKanbanID;
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
    /// <param name="boxCout">箱数</param>
    /// <returns></returns>
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, string userName, int boxCout, int companyID)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.ReceiveDatetime = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.Quantity = boxCout;
        d_ReceiveModel.ScanedAt = DateTime.Now;
        d_ReceiveModel.CreatedAt = DateTime.Now;
        d_ReceiveModel.CreatedBy = userName;

        return d_ReceiveModel;
    }

    /// <summary>
    /// キャンセルボタンを押すと全て消す、画面を戻る
    /// </summary>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        enteredNumberLabel.Text = string.Empty;
        numberInputModal.IsVisible = false;
        bulkStockContent.IsVisible = true;
        errorLabel.Text = string.Empty;
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

    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (_numberOfBoxListItems.Count() > 0)
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
                // サウンドのパスを取得す
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);

                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました。", "閉じる");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new RegisterBulkStoreInPage(handyMenuID);
            }
            else
            {
                // サウンドのパスを取得す
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
}
