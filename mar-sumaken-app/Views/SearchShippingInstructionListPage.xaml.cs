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

    public SearchShippingInstructionListPage()
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        // 出荷指示リスト検索設定
        SetSearchShippingInstruction();
    }

    /// <summary>
    /// 出荷指示リスト検索設定
    /// </summary>
    private void SetSearchShippingInstruction()
    {
        receiver.ItemsSource = receiverList;
        receiver.SelectedItem = "会社名1";

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
                await Application.Current.MainPage.DisplayAlert("エラー", "E3003 ネットワークが利用できません。", "OK");
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
                CompanyID = 4,
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
                Application.Current.MainPage = new DisplayShippingInstructionListPage((string)receiver.SelectedItem,selectedDate, selectedTripOption, (string)place.SelectedItem, shipmentScheduleList);
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