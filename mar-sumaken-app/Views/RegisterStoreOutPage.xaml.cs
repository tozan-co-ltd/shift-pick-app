using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;

using static mar_sumaken_app.Models.ShipmentScheduleModel;

public partial class RegisterStoreOutPage : ContentPage
{
    public DateTime selectedDate;
    private List<D_ShipmentSchedule> _dShipmentScheduleList;
    private List<D_ShipmentSchedule> _dShipmentScheduleHistoryList;
    public List<D_ShipmentSchedule> lstDShipmentSchedules;
    private int _itemsPerPage = 10;
    public int currentPage = 1;
    public int currentHistoryPage = 1;
    public string currentDate = string.Empty; 
    private readonly LocalDbService _dbService;
    private HandySettingModel.HandySetting _handySetting;
    public RegisterStoreOutPage(string receiver, string currentDate, string selectedTrip, string place, List<D_ShipmentSchedule> dShipmentScheduleListSearch)
    {
        InitializeComponent();
        selectedReceiverLable.Text = $"{receiver}";
        selectedDateLabel.Text = $"{currentDate}";
        selectedTripLabel.Text = $"{selectedTrip}";
        selectedDate = DateTime.Parse(currentDate);
        selectedPlaceLabel.Text = $"{place}";
        shippingListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        lstDShipmentSchedules = dShipmentScheduleListSearch;
        _dbService = new LocalDbService();

        UpdateProgressBar();

        ShowShippingList(lstDShipmentSchedules);

        ShowHistoryList(lstDShipmentSchedules);
    }

    /// <summary>
    /// 出荷指示リスト表示のデータを表示
    /// </summary>
    /// <param name="lstDShipmentSchedules">List<D_ShipmentSchedule></param>
    private async void ShowShippingList(List<D_ShipmentSchedule> lstDShipmentSchedules)
    {
        try
        {
            // 出荷指示リスト表示
            _dShipmentScheduleList = lstDShipmentSchedules;
            collectionView = DisplayItems();

            if (_dShipmentScheduleList.Count() % _itemsPerPage == 0)
                totalPage.Text = "/" + (_dShipmentScheduleList.Count() / _itemsPerPage).ToString();
            else
                totalPage.Text = "/" + ((_dShipmentScheduleList.Count() / _itemsPerPage) + 1).ToString();

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
    /// <param name="lstDShipmentSchedules">List<D_ShipmentSchedule></param>
    private async void ShowHistoryList(List<D_ShipmentSchedule> lstDShipmentSchedules)
    {
        try
        {
            // 履歴一覧表示
            _dShipmentScheduleHistoryList = lstDShipmentSchedules;
            collectionViewHistory = DisplayItemsHistory();

            if (_dShipmentScheduleList.Count() % _itemsPerPage == 0)
                totalHistoryPage.Text = "/" + (_dShipmentScheduleHistoryList.Count() / _itemsPerPage).ToString();
            else
                totalHistoryPage.Text = "/" + ((_dShipmentScheduleHistoryList.Count() / _itemsPerPage) + 1).ToString();

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
            var startIndex = (currentPage - 1) * _itemsPerPage;
            var endIndex = startIndex + _itemsPerPage;
            var pageItems = _dShipmentScheduleList.Skip(startIndex).Take(_itemsPerPage);

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
            var startIndex = (currentHistoryPage - 1) * _itemsPerPage;
            var endIndex = startIndex + _itemsPerPage;
            var pageItems = _dShipmentScheduleList.Skip(startIndex).Take(_itemsPerPage);

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
        Application.Current.MainPage = new DisplayShippingInstructionListPage(selectedReceiverLable.Text, selectedDate, selectedTripLabel.Text, selectedPlaceLabel.Text, lstDShipmentSchedules);
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
            var totalPages = (int)Math.Ceiling((double)_dShipmentScheduleList.Count / _itemsPerPage);
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
            var totalPages = (int)Math.Ceiling((double)_dShipmentScheduleHistoryList.Count / _itemsPerPage);
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
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
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