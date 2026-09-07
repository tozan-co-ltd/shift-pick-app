using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.ViewModels;
using shift_pick_app.Properties;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace shift_pick_app.Views;

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
        handyMenuID = menuID;
        listProducts = listProductItems;
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
        d_ReceiveModel.ReceiveDate = DateTime.Now;
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
    public D_HandyErrorMessageModel SetHandyErrorMessage(List<LoginModel.LoginUserSqlLite> loginUser, string clipboardContent, string scanTextError)
    {
        D_HandyErrorMessageModel d_HandyErrorMessageModel = new();
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
                await Application.Current.MainPage.DisplayAlert("完了", CreateRegisteredMessage(response.content), "OK");
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
            await DisplayAlert("エラー", $"E9999: {ErrorMessagesResources.E9999}\n" + ex.Message, "OK");
            return;
        }
    }
    /// <summary>
    /// データ登録完了時のメッセージをレスポンスによって変える
    /// </summary>
    /// <param name="responceContent"></param>
    /// <returns></returns>
    private string CreateRegisteredMessage(string responceContent)
    {
        string defaultMessage = "データ登録が完了しました。";
        try
        {
            // contentをJSONオブジェクトにデシリアライズ
            var storeInResponseContent = JsonConvert.DeserializeObject<D_StoreInModel.StoreInResponceContent>(responceContent);
            if (storeInResponseContent != null)
            {                
                string strDuplicateProduct = string.Empty;
                if (storeInResponseContent.ScanReceive != null)
                {
                    strDuplicateProduct = "【今回登録されなかった品番】 \n";
                    //重複したかんばんシリアルと納入先品番を羅列                    
                    var duplicateProduct = storeInResponseContent.ScanReceive
                    .Select(p => p.d_ReceiveModel).Select(p => new {p.SupplierProductNumber , p.MainProductKey,p.FirstSubProductKey,p.SecondSubProductKey }).ToList();
                    // 結果の表示
                    foreach (var product in duplicateProduct)
                    {
                        strDuplicateProduct += $"仕入先品番: {product.SupplierProductNumber} \n" 
                                            +  $"メインキー: { product.MainProductKey}" + "\n" 
                                            +  $"サブキー１: {product.FirstSubProductKey}" + "\n"
                                            +  $"サブキー２: {product.SecondSubProductKey}" + "\n"
                                            +  "-------------------------------" + "\n"; 
                    }
                }
                return defaultMessage + '\n' + '\n' + storeInResponseContent.Message + '\n' + '\n' + strDuplicateProduct;
            }
            return defaultMessage;
        }
        catch (JsonException)
        {
            return defaultMessage;
        }
    }

    /// <summary>
    /// 箱数集計表示ボタン押下時
    /// </summary>
    private void OnTotalNumberOfBoxesBtnClicked(object sender, EventArgs e)
    {
        // ボタンの色・表示内容変更
        
    }

    /// <summary>
    /// 履歴一覧表示ボタン押下時
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        // ボタンの色・表示内容変更
        Application.Current.MainPage = new RegisterReservePage();
    }
}