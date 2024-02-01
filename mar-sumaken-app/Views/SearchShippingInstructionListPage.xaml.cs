using mar_sumaken_app.Common;
using mar_sumaken_app.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace mar_sumaken_app.Views;

public partial class SearchShippingInstructionListPage : ContentPage
{
    bool isItemTapped = false;
    ObservableCollection<string> items = new ObservableCollection<string> { "E5", "T5" };
    string selectedTripOption = "1"; // デフォルトでは、便は 1 です
    public SearchShippingInstructionListPage()
	{
		InitializeComponent();
        place.ItemsSource = items;
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
            DateTime selectedDate = datePicker.Date;

            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);
            Application.Current.MainPage = new DisplayShippingInstructionListPage(selectedDate, selectedTripOption, (string)place.SelectedItem);
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }
}