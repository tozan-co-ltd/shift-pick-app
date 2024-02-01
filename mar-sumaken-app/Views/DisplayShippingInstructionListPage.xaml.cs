using mar_sumaken_app.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using ProductDemo = mar_sumaken_app.Models.ProductDemo;
using Newtonsoft.Json;

namespace mar_sumaken_app.Views;

public partial class DisplayShippingInstructionListPage : ContentPage
{
    private ObservableCollection<ProductDemo> _items;
    private int _itemsPerPage = 10;
    public int currentPage = 1;
    public string currentDate = string.Empty;
    public string slectedTrip = string.Empty;
    public string selectedPlace = string.Empty;
    public DisplayShippingInstructionListPage(DateTime selectedDate, string selectedOption, string place)
	{
		InitializeComponent();
        selectedDateLabel.Text = $"{selectedDate.ToString("yyyy/MM/dd")}";
        selectedTripLabel.Text = $"{selectedOption}";
        currentDate = $"{selectedDate.ToShortDateString()}";
        slectedTrip = $"{selectedOption}";
        selectedPlaceLabel.Text = $"{place}";
        selectedPlace = $"{place}";
        
        _items = new ObservableCollection<ProductDemo>();
        // sample data
        for (int i = 1; i <= 25; i++)
        {
            ProductDemo productDemo = new ProductDemo
            {
                Id= i,
                //No = $"{i}",
                product_num = $"24240{i}",
                uniform_num = $"69{i}",
                contain_num = $"2{i}",
                box_num = i
            };

            _items.Add(productDemo);
        }

        // 箱数降順
        _items = new ObservableCollection<ProductDemo>(_items.OrderByDescending(p => p.box_num));
        // No昇順
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].No = $"{i+1}";
        }

        // APIからデータを取得
        //_items = GetDataFromApi(_items);
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
            //_items = await GetDataFromApi();
            if (_items.Count() % _itemsPerPage == 0)
                totalPage.Text = "/" + (_items.Count() / _itemsPerPage).ToString();
            else
                totalPage.Text = "/" + ((_items.Count() / _itemsPerPage) + 1).ToString();

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
    /// APIからデータを取得
    /// </summary>
    public static async Task<ObservableCollection<ProductDemo>> GetDataFromApi()
    {
        try
        {
            var productDemo = new ObservableCollection<ProductDemo>();
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:7297/GetListProduct");
            var jsonResult = await response.Content.ReadAsStringAsync();
            productDemo = JsonConvert.DeserializeObject<ObservableCollection<ProductDemo>>(jsonResult);

            return productDemo;
        }
        catch (Exception)
        {
            throw;
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
            var pageItems = _items.Skip(startIndex).Take(_itemsPerPage);

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
        var totalPages = (int)Math.Ceiling((double)_items.Count / _itemsPerPage);
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
        Application.Current.MainPage = new SearchShippingInstructionListPage();
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
            Application.Current.MainPage = new RegisterStoreOutPage(currentDate, slectedTrip, _items, selectedPlace);
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }
}