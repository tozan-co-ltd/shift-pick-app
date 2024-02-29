using System;
using System.Collections.ObjectModel;
using mar_sumaken_app.Common;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Newtonsoft.Json;
using Plugin.Maui.Audio;

namespace mar_sumaken_app.Views;

public partial class MenuPage : FlyoutPage
{
    private readonly LocalDbService _dbService;
    private LoginModel.LoginUserSqlLite _loginUserSqlLite;
    private List<M_HandyMenuModel> listHandyMenu;
    bool isItemTapped = false;
    public List<LoginModel.LoginUserSqlLite> loginUsers;

    /// <summary>
    /// メニュー画面表示
    /// </summary>
    public MenuPage()
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        // メニューの設定
        SetMenuPage();
        // メニューのアイテムをタップ
        myListView.ItemTapped += OnItemTapped;
    }

    /// <summary>
    /// メニューの設定
    /// </summary>
    private async void SetMenuPage()
    {
        try
        {
            // メニュー一覧を取得
            listHandyMenu = await _dbService.GetMenuAsync();
            if (listHandyMenu.Count > 0)
                myListView.ItemsSource = listHandyMenu;
            else
                await DisplayAlert("エラー", "E2000: メニューが登録されていません。", "閉じる");

            // ローカルDBのユーザー情報取得
            loginUsers = await _dbService.GetLoginAsync();
            _loginUserSqlLite = loginUsers.Last();
            companyCode.Text = _loginUserSqlLite.CompanyCode;
            loginId.Text = _loginUserSqlLite.LoginID;
            userName.Text = _loginUserSqlLite.UserName;
            depoName.Text = _loginUserSqlLite.DepoName;
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
        }
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブ
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ((FlyoutPage)this).IsPresentedChanged += OnIsPresentedChanged;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの読み込みを解除
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
    /// メニュー名押下時
    /// </summary>
    private async void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        try
        {
            if (!isItemTapped && (e.Item is M_HandyMenuModel pageName))
            {
                isItemTapped = true;
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                await PerformNavigation(pageName.HandyMenuName);
                isItemTapped = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
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
            // 入庫
            if (title == Const.C_REGISTER_STORE_IN_PAGE)
                Application.Current.MainPage = new RegisterStoreInPage();

            // まとめ入庫
            else if (title == Const.C_REGISTER_BULK_STORE_IN_PAGE)
                Application.Current.MainPage = new RegisterBulkStoreInPage();

            // 出庫
            else if (title == Const.C_SEARCH_SHIPPING_INSTRUCTION_IN_PAGE)
            {
                // API接続
                var listCompanyResult = await GetListCompanyFromApi();
                if (listCompanyResult.success) 
                    Application.Current.MainPage = new SearchShippingInstructionListPage(listCompanyResult.mCompanySqlLite);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 出荷指示リスト検索画面用の納入先リスト取得
    /// </summary>
    /// <returns>納入先リスト</returns>
    public async Task<(List<M_CompanyModel> mCompanySqlLite, bool success)> GetListCompanyFromApi()
    {
        var mCompanySqlLite = new List<M_CompanyModel>();
        var mCompanyPostBody = new
        {
            CompanyCode = _loginUserSqlLite.CompanyCode,
            CompanyPassword = _loginUserSqlLite.CompanyPassword,
        };
        var jsonDataSend = JsonConvert.SerializeObject(mCompanyPostBody);

        // ネットワーク接続チェック
        if (!NetworkCheck.CheckNetworkConnection())
        {
            await DisplayAlert("エラー", "E3003: ネットワークが利用できません。", "OK");
            Application.Current.MainPage = new MenuPage();
            return (mCompanySqlLite, false);
        }

        // API接続
        var response = await App.API.PostMethod(jsonDataSend, "https://wbtzn/sumaken-api-MRq2xg5_test/api/", "M_Company/FetchDelivery");
        if (response.success)
        { 
            // APIからコンテンツを取得
            mCompanySqlLite = JsonConvert.DeserializeObject<List<M_CompanyModel>>(response.content);
            return (mCompanySqlLite, true);
        }
        else
        {
            await DisplayAlert("エラー", "E2000: 接続先URLにアクセスできません。", "OK");
            Application.Current.MainPage = new MenuPage();
            return (mCompanySqlLite, false);
        }
    }

    /// <summary>
    /// ログアウトボタン押下
    /// </summary>
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // ログインユーザーのローカルDBのデータ削除
        await _dbService.DeleteUserLogin(_loginUserSqlLite);

        // ログイン画面表示
        Application.Current.MainPage = new LoadingPage();
        await Task.Delay(200);
        Application.Current.MainPage = new LoginPage();
    }
}

