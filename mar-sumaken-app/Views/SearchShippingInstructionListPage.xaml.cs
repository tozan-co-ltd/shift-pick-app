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
    List<string> receiverList = new List<string> { "会社名1", "会社名2" };
    ObservableCollection<string> placeList = new ObservableCollection<string> { "E5", "T5" };
    string selectedTripOption = "1"; // デフォルトでは、便は 1 です
    private readonly LocalDbService _dbService;
    bool hasOnSearchClick = false; // 検索クリック
    List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    int selectedCompanyID;
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
    private void SetSearchShippingInstruction()
    {
        receiver.ItemsSource = listMCompany.Select(c => c.CompanyName).ToList();
        receiver.SelectedItem = listMCompany.Select(c => c.CompanyName).ToList().FirstOrDefault();

        place.ItemsSource = placeList;
        place.SelectedItem = "E5";
        datePicker.Date = CustomDatePicker.SetDisplayDatePicker();
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
    private void OnPlaceChanged(object sender, EventArgs e)
    {
        place.SelectedItem = (string)place.SelectedItem;
    }

    /// <summary>
    /// 納入会社名を変更するのを処理
    /// </summary>
    private void OnReceiverChanged(object sender, EventArgs e)
    {
        // 会社IDを取得
        int selectedIndex = receiver.SelectedIndex;
        if (selectedIndex != -1)
        {
            selectedCompanyID = listMCompany[selectedIndex].CompanyID;
        }
        receiver.SelectedItem = (string)receiver.SelectedItem;
    }

    /// <summary>
    /// 便を変更するのを処理
    /// </summary>
    private void OnTripButtonClicked(object sender, EventArgs e)
    {
        trip1.BackgroundColor = Colors.Gray;
        trip2.BackgroundColor = Colors.Gray;
        trip3.BackgroundColor = Colors.Gray;
        trip4.BackgroundColor = Colors.Gray;

        Button selectedTrip = (Button)sender;
        selectedTrip.BackgroundColor = Color.FromHex("#008577");
        selectedTripOption = selectedTrip.Text;
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
                DeliveryTimeClass = selectedTripOption,
                DeliveryLocation = (string)place.SelectedItem,selectedTripOption
            };

            var jsonDataSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

            // APIにリクエストを送る
            var response = await App.API.PostMethod(jsonDataSend, "https://wbtzn/sumaken-api-MRq2xg5_test/api/", "ShipmentSchedule/Find");
            if (response.success)
            {
                // APIからコンテンツを取得
                var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduleModel.ShipmentScheduelResponseBody>(response.content);
                var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList;

                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, (string)receiver.SelectedItem,selectedDate, selectedTripOption, (string)place.SelectedItem, listMCompany, shipmentScheduleList, listProducts);
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