using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace mar_sumaken_app.Views;

public partial class SearchShippingInstructionListPage : ContentPage
{
    bool isItemTapped = false;
    ObservableCollection<string> placeList = new ObservableCollection<string> { "E5", "T5" };
    string chosedDeliveryTimeClass = "1"; // デフォルトでは、便は 1 です
    private readonly LocalDbService _dbService;
    bool hasOnSearchClick = false; // 検索クリック
    List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    int selectedCompanyID;
    private string? _handyApiUrl;
    public int handyMenuID = 0;

    public SearchShippingInstructionListPage(int menuID, List<M_CompanyModel> mCompanySqlLite, List<M_ProductModel> listProductItems)
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        listMCompany = mCompanySqlLite;
        listProducts = listProductItems;
        handyMenuID = menuID;
        // 出荷指示リスト検索設定
        SetSearchShippingInstruction();
    }

    /// <summary>
    /// 出荷指示リスト検索設定
    /// </summary>
    private async void SetSearchShippingInstruction()
    {
        // ローカルDBのハンディ設定情報取得
        var settings = await _dbService.GetSetting();
        if (settings.Count() > 0 && settings.Last() != null)
            _handyApiUrl = settings.Last().HandyApiUrl;

        deliveryName.ItemsSource = listMCompany.Select(c => c.CompanyName).ToList();
        deliveryName.SelectedItem = listMCompany.Select(c => c.CompanyName).ToList().FirstOrDefault();

        deliveryLocation.ItemsSource = placeList;
        deliveryLocation.SelectedItem = "E5";
        datePicker.Date = Util.GetNextday(DateTime.Now);
    }

    /// <summary>
    /// 戻る
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (!isItemTapped)
        {
            isItemTapped = true;
            Application.Current.MainPage = new MenuPage();
            isItemTapped = false;
        }
    }

    /// <summary>
    /// 納入場所を変更するのを処理
    /// </summary>
    private void OnDeliveryLocationChanged(object sender, EventArgs e)
    {
        deliveryLocation.SelectedItem = (string)deliveryLocation.SelectedItem;
    }

    /// <summary>
    /// 納入会社名を変更するのを処理
    /// </summary>
    private void OnDeliveryNameChanged(object sender, EventArgs e)
    {
        // 会社IDを取得
        int selectedIndex = deliveryName.SelectedIndex;
        if (selectedIndex != -1)
        {
            selectedCompanyID = listMCompany[selectedIndex].CompanyID;
        }
        deliveryName.SelectedItem = (string)deliveryName.SelectedItem;
    }

    /// <summary>
    /// 便を変更するのを処理
    /// </summary>
    private void OnDeliveryTimeClassClicked(object sender, EventArgs e)
    {
        deliveryTimeClass1.BackgroundColor = Colors.Gray;
        deliveryTimeClass2.BackgroundColor = Colors.Gray;
        deliveryTimeClass3.BackgroundColor = Colors.Gray;
        deliveryTimeClass4.BackgroundColor = Colors.Gray;

        Button selectedDeliveryTimeClass = (Button)sender;
        selectedDeliveryTimeClass.BackgroundColor = Color.FromHex("#008577");
        chosedDeliveryTimeClass = selectedDeliveryTimeClass.Text;
    }

    /// <summary>
    /// 検索する
    /// </summary>
    private async void OnSearchButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (hasOnSearchClick)
                return;

            if (!NetworkCheck.CheckNetworkConnection()) // ネットワーク接続チェック
            {
                await Application.Current.MainPage.DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
                return;
            }
            //ロード画面表示
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);

            hasOnSearchClick = true;
            // ユーザー情報を取得
            var loginUsers = await _dbService.GetLoginAsync();
            var loginUserSqlLite = loginUsers.Last();
            DateTime selectedDate = datePicker.Date;

            var shipmentScheduleSqlLite = new ShipmentScheduleModel.ShipmentScheduleRequestBody();

            var shipmentScheduleRequestBody = new
            {
                CompanyID = selectedCompanyID,
                CompanyCode = loginUserSqlLite.CompanyCode,
                CompanyPassword = loginUserSqlLite.CompanyPassword,
                DepoID = loginUserSqlLite.DepoID,
                DeliveryDate = selectedDate,
                DeliveryTimeClass = chosedDeliveryTimeClass,
                DeliveryLocation = (string)deliveryLocation.SelectedItem,
            };

            var jsonDataSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

            // APIにリクエストを送る
            var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "ShipmentSchedule/Find");
            if (response.success)
            {
                // APIからコンテンツを取得
                var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduleModel.ShipmentScheduelResponseBody>(response.content);
                var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList;
                // 納入書番号
                var deliverySlipNumber = shipmentScheduleList.FirstOrDefault().DeliverySlipNumber;            
                Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, selectedCompanyID, (string)deliveryName.SelectedItem,selectedDate, chosedDeliveryTimeClass, (string)deliveryLocation.SelectedItem, listMCompany, shipmentScheduleList, listProducts, deliverySlipNumber);
            }
            else
                await DisplayAlert("エラー", response.content, "OK");

            hasOnSearchClick = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }
}
