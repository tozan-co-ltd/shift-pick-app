using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;
using ProductDemo = mar_sumaken_app.Models.ProductDemo;

public partial class RegisterStoreOutPage : ContentPage
{
    public DateTime selectedDate;
    private ObservableCollection<ProductDemo> items;
    private ObservableCollection<ProductDemo> itemsHistory;
    public string chosedTrip = string.Empty;
    private int itemsPerPage = 10;
    public int currentPage = 1;
    public int currentHistoryPage = 1;
    public string currentDate = string.Empty; 
    public string selectedTrip = string.Empty;
    public string chosedPlace = string.Empty;
    private readonly LocalDbService _dbService;
    private ScanAndSound scanAndSound;
    public RegisterStoreOutPage(string currentDate, string selectedTrip, ObservableCollection<ProductDemo> itemsData, string place)
    {
		InitializeComponent();
        selectedDateLabel.Text = $"{currentDate}";
        selectedTripLabel.Text = $"{selectedTrip}";
        selectedDate = DateTime.Parse(currentDate);
        chosedPlace = $"{place}";
        chosedTrip = $"{selectedTrip}";
        selectedPlaceLabel.Text = $"{place}";
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        _dbService = new LocalDbService();

        UpdateProgressBar();

        ShowShippingList(itemsData);

        ShowHistoryList(itemsData);
    }

    /// <summary>
    /// 出荷指示リスト表示のデータを表示
    /// </summary>
    /// <param name="itemsData">ObservableCollection<ProductDemo></param>
    private async void ShowShippingList(ObservableCollection<ProductDemo> itemsData)
    {
        try
        {
            // 出荷指示リスト表示
            items = itemsData;
            collectionView = DisplayItems();

            if (items.Count() % itemsPerPage == 0)
                totalPage.Text = "/" + (items.Count() / itemsPerPage).ToString();
            else
                totalPage.Text = "/" + ((items.Count() / itemsPerPage) + 1).ToString();

            pageNumber.Text = currentPage.ToString();

            var previousButton = new Button { };
            previousButton.Clicked += OnPreviousClicked;

            var nextButton = new Button { };
            nextButton.Clicked += OnNextClicked;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 履歴一覧表示のデータを表示
    /// </summary>
    /// <param name="itemsData">ObservableCollection<ProductDemo></param>
    private async void ShowHistoryList(ObservableCollection<ProductDemo> itemsData)
    {
        try
        {
            // 履歴一覧表示
            itemsHistory = itemsData;
            collectionViewHistory = DisplayItemsHistory();

            if (items.Count() % itemsPerPage == 0)
                totalHistoryPage.Text = "/" + (itemsHistory.Count() / itemsPerPage).ToString();
            else
                totalHistoryPage.Text = "/" + ((itemsHistory.Count() / itemsPerPage) + 1).ToString();

            historyPageNumber.Text = currentHistoryPage.ToString();

            var previousHistoryButton = new Button { };
            previousHistoryButton.Clicked += HistoryOnPreviousClicked;

            var nextHistoryButton = new Button { };
            nextHistoryButton.Clicked += HistoryOnNextClicked;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 出荷指示リストのコレクションビューに表示データを追加
    /// </summary>
    private CollectionView DisplayItems()
    {
        try
        {
            var startIndex = (currentPage - 1) * itemsPerPage;
            var endIndex = startIndex + itemsPerPage;
            var pageItems = items.Skip(startIndex).Take(itemsPerPage);

            collectionView.ItemsSource = pageItems;
            return collectionView;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 履歴一覧表のコレクションビューに表示データを追加
    /// </summary>
    private CollectionView DisplayItemsHistory()
    {
        try
        {
            var startIndex = (currentHistoryPage - 1) * itemsPerPage;
            var endIndex = startIndex + itemsPerPage;
            var pageItems = items.Skip(startIndex).Take(itemsPerPage);

            collectionViewHistory.ItemsSource = pageItems;
            return collectionViewHistory;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 戻るボタンをクリックするのを処理
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new DisplayShippingInstructionListPage(selectedDate, chosedTrip, chosedPlace);
    }

    /// <summary>
    /// プログレスバーを更新する
    /// </summary>
    private async void UpdateProgressBar()
    {
        try
        {
            progressBar.Progress = 0.5;
            progressBar.ProgressColor = Color.FromHex("#008080");

            if (progressBar.Progress == 1)
            {
                completeLabel.Text = "完了";
                completeLabel.TextColor = Color.FromHex("#F44E4B");
                progressBar.Progress = 1;
                progressBar.ProgressColor = Color.FromHex("#F44E4B");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 出荷指示リストで前へボタンをクリックするのを処理
    /// </summary>
    private void OnPreviousClicked(object sender, EventArgs e)
    {
        try
        {
            if (currentPage > 1)
            {
                currentPage--;
                DisplayItems();
                pageNumber.Text = currentPage.ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 出荷指示リストで次へボタンをクリックするのを処理
    /// </summary>
    private void OnNextClicked(object sender, EventArgs e)
    {
        try
        {
            var totalPages = (int)Math.Ceiling((double)items.Count / itemsPerPage);
            if (currentPage < totalPages)
            {
                currentPage++;
                DisplayItems();
                pageNumber.Text = currentPage.ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 履歴一覧表で前へボタンをクリックするのを処理
    /// </summary>
    private void HistoryOnPreviousClicked(object sender, EventArgs e)
    {
        try
        {
            if (currentHistoryPage > 1)
            {
                currentHistoryPage--;
                DisplayItemsHistory();
                historyPageNumber.Text = currentHistoryPage.ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 履歴一覧表で次へボタンをクリックするのを処理
    /// </summary>
    private void HistoryOnNextClicked(object sender, EventArgs e)
    {
        try
        {
            var totalPages = (int)Math.Ceiling((double)itemsHistory.Count / itemsPerPage);
            if (currentHistoryPage < totalPages)
            {
                currentHistoryPage++;
                DisplayItemsHistory();
                historyPageNumber.Text = currentHistoryPage.ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnShippingListBtnClicked(object sender, EventArgs e)
    {
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        shippingList.IsVisible = true;
        historyList.IsVisible = false;
    }

    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        shippingListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        shippingList.IsVisible = false;
        historyList.IsVisible = true;
    }

    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, scanAndSound);
            // OKサウンド
            SoundPlayer.PlaySound(resultOkSound);
            await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "OK");
        }
        catch (Exception)
        {
            throw;
        }
    }
}