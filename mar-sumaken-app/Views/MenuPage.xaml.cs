using System.Collections.ObjectModel;
using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Plugin.Maui.Audio;
using MenuItem = mar_sumaken_app.Models.MenuItem;

namespace mar_sumaken_app.Views;

public partial class MenuPage : FlyoutPage
{
    private readonly LocalDbService _dbService;
    private Login.LoginUserSqlLite _loginUserSqlLite;
    bool isItemTapped = false;
    public MenuPage()
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        // メニューの設定
        SetMenuPage();

        // メニュー一覧
        myListView.ItemsSource = new ObservableCollection<MenuItem>
        {
            new MenuItem {Id=1, Title = "入庫" },
            new MenuItem {Id=2, Title = "まとめ入庫" },
            new MenuItem {Id = 3,  Title = "出庫" },
        };

        //　メニューのアイテムをタップ
        myListView.ItemTapped += OnItemTapped;
    }

    /// <summary>
    /// メニューの設定
    /// </summary>
    private async void SetMenuPage()
    {
        try
        {
            var loginUsers = await _dbService.GetLoginAsync();
            _loginUserSqlLite = loginUsers.Last();
            companyCode.Text = _loginUserSqlLite.CompanyCode;
            loginId.Text = _loginUserSqlLite.LoginID;
            userName.Text = _loginUserSqlLite.UserName;
            companyName.Text = _loginUserSqlLite.CompanyName;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ((FlyoutPage)this).IsPresentedChanged += OnIsPresentedChanged;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの購読を解除する
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ((FlyoutPage)this).IsPresentedChanged -= OnIsPresentedChanged;
    }

    /// <summary>
    /// フライアウトが開いているかどうかを確認
    /// フライアウトの状態に基づいて FlyoutPage.Detail を有効化または無効
    /// </summary>
    private void OnIsPresentedChanged(object sender, EventArgs e)
    {
        bool isFlyoutOpen = ((FlyoutPage)sender).IsPresented;

        ((FlyoutPage)this).Detail.IsEnabled = !isFlyoutOpen;
    }

    /// <summary>
    /// アイテムがタップされた時、対応するページに移動
    /// </summary>
    private async void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        try
        {
            if (!isItemTapped && (e.Item is MenuItem pageName))
            {
                isItemTapped = true;
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                await PerformNavigation(pageName.Title);
                isItemTapped = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// ナビゲーション
    /// </summary>
    /// <param name="title">string</param>
    /// <returns>対応画面</returns>
    private async Task PerformNavigation(string title)
    {
        try
        {
            if (title == "入庫")
                Application.Current.MainPage = new RegisterStoreInPage();
            else if (title == "まとめ入庫")
                Application.Current.MainPage = new RegisterBulkStoreInPage();
            else if (title == "出庫")
                Application.Current.MainPage = new SearchShippingInstructionListPage();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(200);
        Application.Current.MainPage = new LoginPage();
    }
}

