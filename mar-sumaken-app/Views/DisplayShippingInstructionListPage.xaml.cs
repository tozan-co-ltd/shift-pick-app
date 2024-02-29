using mar_sumaken_app.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using ProductDemo = mar_sumaken_app.Models.D_ReceiveModel;
using Newtonsoft.Json;
using static mar_sumaken_app.Models.ShipmentScheduleModel;
using mar_sumaken_app.Models;

namespace mar_sumaken_app.Views;

public partial class DisplayShippingInstructionListPage : ContentPage
{
    private List<D_ShipmentSchedule> lstDShipmentSchedules;
    private int _itemsPerPage = 10;
    public int currentPage = 1;
    public string currentDate = string.Empty;
    public string slectedTrip = string.Empty;
    public string selectedPlace = string.Empty;
    public string selectedReceiver = string.Empty;
    List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();

    public DisplayShippingInstructionListPage(string receiver, DateTime selectedDate, string selectedOption, string place, List<M_CompanyModel> mCompanySqlLite,  List<D_ShipmentSchedule> shipmentScheduleList, List<M_ProductModel> listProductItems)
	{
		InitializeComponent();
        selectedReceiverLable.Text = $"{receiver}";
        selectedDateLabel.Text = $"{selectedDate.ToString("yyyy/MM/dd")}";
        selectedTripLabel.Text = $"{selectedOption}";
        currentDate = $"{selectedDate.ToShortDateString()}";
        selectedPlaceLabel.Text = $"{place}";
        listMCompany = mCompanySqlLite;
        listProducts = listProductItems;
        lstDShipmentSchedules = new List<D_ShipmentSchedule>();

        // 箱数降順
        lstDShipmentSchedules = new List<D_ShipmentSchedule>(shipmentScheduleList.OrderByDescending(p => p.NumberOfBoxes));
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
            if (lstDShipmentSchedules.Count() % _itemsPerPage == 0)
                totalPage.Text = "/" + (lstDShipmentSchedules.Count() / _itemsPerPage).ToString();
            else
                totalPage.Text = "/" + ((lstDShipmentSchedules.Count() / _itemsPerPage) + 1).ToString();

            pageNumber.Text = currentPage.ToString();

            var previousButton = new Button { };
            previousButton.Clicked += OnPreviousClicked;

            var nextButton = new Button { };
            nextButton.Clicked += OnNextClicked;

            DisplayItems();
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
    ///前へボタンをクリックするのを処理
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
    /// 次へボタンをクリックするのを処理
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
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new SearchShippingInstructionListPage(listMCompany, listProducts);
    }

    /// <summary>
    /// 検品開始ボタンをクリックするのを処理
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);
            Application.Current.MainPage = new RegisterStoreOutPage((string)selectedReceiverLable.Text, selectedDateLabel.Text, selectedTripLabel.Text, selectedPlaceLabel.Text, listMCompany, lstDShipmentSchedules, listProducts);
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }
}