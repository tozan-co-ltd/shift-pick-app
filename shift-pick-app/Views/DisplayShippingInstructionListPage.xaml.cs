using System.Collections.ObjectModel;
using Newtonsoft.Json;
using static shift_pick_app.Models.ShipmentScheduleModel;
using shift_pick_app.Models;
using shift_pick_app.Common;
using shift_pick_app.Services;
using shift_pick_app.Properties;

namespace shift_pick_app.Views;

public partial class DisplayShippingInstructionListPage : ContentPage
{
    private int _itemsPerPage = 10;
    public int handyMenuID;
    public int currentPage = 1;
    public int companyID;
    private string? _handyApiUrl;
    private readonly LocalDbService _dbService;
    List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    private List<D_ShipmentSchedule> lstDShipmentSchedules = new List<D_ShipmentSchedule>();

    /// <summary>
    /// 出荷指示リスト表示
    /// </summary>
    /// <param name="menuID">menuID</param>
    /// <param name="selectedCompanyID">会社ID</param>
    /// <param name="mCompanySqlLite">会社一覧</param>
    /// <param name="deliveryName">納入先</param>
    /// <param name="selectedDate">納入指示日</param>
    /// <param name="deliveryTimeClass">納入場所</param>
    /// <param name="deliveryLocation">便</param>
    /// <param name="shipmentScheduleList">出荷一覧</param>
    /// <param name="listProductItems">プロダクト一覧</param>
    /// <param name="deliverySlipNumber">納入番号</param>
    public DisplayShippingInstructionListPage(int menuID, int selectedCompanyID, string deliveryName, DateTime selectedDate, string deliveryTimeClass, string deliveryLocation, List<M_CompanyModel> mCompanySqlLite,  ObservableCollection<D_ShipmentSchedule> shipmentScheduleList, List<M_ProductModel> listProductItems, string deliverySlipNumber)
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        selectedDeliveryNameLabel.Text = $"{deliveryName}";
        selectedDateLabel.Text = $"{selectedDate.ToString("yyyy/MM/dd")}";
        selectedDeliveryTimeClassLabel.Text = $"{deliveryTimeClass}";
        selectedDeliveryLocationLabel.Text = $"{deliveryLocation}";
        deliverySlipNumberLabel.Text = deliverySlipNumber;
        listMCompany = mCompanySqlLite;
        listProducts = listProductItems;
        handyMenuID = menuID;
        companyID = selectedCompanyID;
        // 残りが0の行が表示されない, 箱数降順
        lstDShipmentSchedules = shipmentScheduleList.Where(p => p.NumberOfBoxesRemaining != 0).OrderByDescending(p => p.NumberOfBoxes).ToList();
        // No昇順
        for (int i = 0; i < lstDShipmentSchedules.Count; i++)
            lstDShipmentSchedules[i].No = i+1;
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await SetItemsAndPagingAsync();
    }

    /// <summary>
    /// 表示とページネーション
    /// </summary>
    private async Task SetItemsAndPagingAsync()
    {
        try
        {
            //テーブルのページネーション表示
            if (lstDShipmentSchedules.Count() % _itemsPerPage == 0)
                totalPage.Text = "/" + (lstDShipmentSchedules.Count() / _itemsPerPage).ToString();
            else
                totalPage.Text = "/" + ((lstDShipmentSchedules.Count() / _itemsPerPage) + 1).ToString();

            pageNumber.Text = currentPage.ToString();

            //ボタンイベント登録
            var previousButton = new Button { };
            previousButton.Clicked += OnPreviousClicked;
            var nextButton = new Button { };
            nextButton.Clicked += OnNextClicked;

            //未検品の出荷指示数で表示切り替え
            if(lstDShipmentSchedules.Count() > 0)
            {
                shippingInstructionsList.IsVisible = true;
                noneShippingInstructions.IsVisible = false;
                DisplayItems();
            }
            else
            {
                shippingInstructionsList.IsVisible = false;
                noneShippingInstructions.IsVisible = true;
            }
        }
        catch (Exception)
        {
            await DisplayAlert("お知らせ", "データがありません", "OK");
            return;
        }
    }

    /// <summary>
    /// コレクションビューに表示データを追加
    /// </summary>
    private CollectionView DisplayItems()
    {
        try
        {
            var startIndex = (currentPage - 1) * _itemsPerPage;
            var endIndex = startIndex + _itemsPerPage;
            var pageItems = lstDShipmentSchedules.Skip(startIndex).Take(_itemsPerPage);
            collectionView.ItemsSource = pageItems;

            return collectionView;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    ///前へボタンをクリックしたときのページネーション
    /// </summary>
    private void OnPreviousClicked(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            DisplayItems();
            pageNumber.Text = currentPage.ToString();
        }
    }

    /// <summary>
    ///次へボタンをクリックしたときのページネーション
    /// </summary>
    private void OnNextClicked(object sender, EventArgs e)
    {
        var totalPages = (int)Math.Ceiling((double)lstDShipmentSchedules.Count / _itemsPerPage);
        if (currentPage < totalPages)
        {
            currentPage++;
            DisplayItems();
            pageNumber.Text = currentPage.ToString();
        }
    }

    /// <summary>
    /// 戻るボタンをクリックしたときの画面遷移処理
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new SearchShippingInstructionListPage(handyMenuID, listMCompany, listProducts);
    }

    /// <summary>
    /// 検品開始ボタンをクリックするのを処理
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        var tmp_page = this;
        try
        {
            // ローカルDBのハンディ設定情報取得
            var settings = await _dbService.GetSetting();
            if (settings.Count() > 0 && settings.Last() != null)
                _handyApiUrl = settings.Last().HandyApiUrl;

            if (!NetworkCheck.CheckNetworkConnection()) // ネットワーク接続チェック
            {
                await Application.Current.MainPage.DisplayAlert("エラー", $"E3003: {ErrorMessagesResources.E3003}", "OK");
                return;
            }
            //ロード画面表示
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);

            // ユーザー情報を取得
            var loginUsers = await _dbService.GetLoginAsync();
            var loginUserSqlLite = loginUsers.Last();
            var shipmentScheduleRequestBody = new
            {
                CompanyID = companyID,
                CompanyCode = loginUserSqlLite.CompanyCode,
                CompanyPassword = loginUserSqlLite.CompanyPassword,
                DepoID = loginUserSqlLite.DepoID,
                DeliveryDate = selectedDateLabel.Text.Replace("/", "-"),
                DeliveryTimeClass = selectedDeliveryTimeClassLabel.Text,
                DeliveryLocation = selectedDeliveryLocationLabel.Text,
            };

            var jsonDataSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

            // 最新データを取得するため、もう一回APIにリクエストを送る
            var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "ShipmentSchedule/Find");
            if (response.success)
            {
                // APIからコンテンツを取得
                var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduleModel.ShipmentScheduelResponseBody>(response.content);
                var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList; // 出荷指示
                var dShipmentList = contentFromApi.D_ShipmentList; // 出荷実績
                var mUserList = contentFromApi.M_UserList; // 管理者ユーザーリスト
                Application.Current.MainPage = new RegisterStoreOutPage(handyMenuID, companyID, (string)selectedDeliveryNameLabel.Text, selectedDateLabel.Text, selectedDeliveryTimeClassLabel.Text, selectedDeliveryLocationLabel.Text, listMCompany, shipmentScheduleList, dShipmentList, mUserList, listProducts, deliverySlipNumberLabel.Text);
            }
            else
            {
                await DisplayAlert("エラー", response.content, "OK");
                Application.Current.MainPage = tmp_page;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", $"E9999: {ErrorMessagesResources.E9999}\n" + ex.Message, "OK");
            Application.Current.MainPage = tmp_page;
            return;
        }
    }
}
