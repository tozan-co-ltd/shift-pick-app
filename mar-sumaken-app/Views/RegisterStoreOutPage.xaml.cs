using Java.Util;
using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using mar_sumaken_app.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace mar_sumaken_app.Views;

using static mar_sumaken_app.Models.ShipmentScheduleModel;

public partial class RegisterStoreOutPage : ContentPage
{
    public DateTime selectedDate;
    public List<D_ShipmentSchedule> lstDShipmentSchedules;
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
        // 進捗を数える
        int sumProgressBar = dShipmentScheduleListSearch.Sum(obj => obj.NumberOfBoxesRemaining);
        UpdateProgressBar(sumProgressBar);
        collectionView.ItemsSource = dShipmentScheduleListSearch;
        collectionViewHistory.ItemsSource = dShipmentScheduleListSearch;
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
    private async void UpdateProgressBar(int sumProgressBar)
    {
        try
        {
            progressBarValueLabel.Text = "10/"+sumProgressBar.ToString();
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