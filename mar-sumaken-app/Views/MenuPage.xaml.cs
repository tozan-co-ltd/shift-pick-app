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
    public List<M_HandyMenuModel> listHandyMenu;
    private HandySettingModel.HandySetting _handySetting;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
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
            var settings = await _dbService.GetSetting();
            _handySetting = settings.Last();
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
                await PerformNavigation(pageName.HandyMenuName, pageName.HandyMenuID);
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
    private async Task PerformNavigation(string title, int handyMenuID)
    {
        try
        {
            var latestMastersPostBody = new LatestMastersModel.LatestMastersPostBody();
            var listProducts = new List<M_ProductModel>();
            latestMastersPostBody.CompanyCode = _handySetting.CompanyCode;
            latestMastersPostBody.CompanyPassword = _handySetting.CompanyPassword;
            latestMastersPostBody.UserID = _loginUserSqlLite.UserID;
            var jsonDataSend = JsonConvert.SerializeObject(latestMastersPostBody);

            // ネットワーク接続チェック
            if (!NetworkCheck.CheckNetworkConnection())
            {
                await DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
                Application.Current.MainPage = new MenuPage();
                return;
            }

            // API接続
            var response = await App.API.PostMethod(jsonDataSend, _handySetting.HandyApiUrl, "LatestMasters/Fetch");
            if (response.success)
            {
                // APIからコンテンツを取得
                var contentFromApi = JsonConvert.DeserializeObject<LatestMastersModel.LatestMastersPostBackContent>(response.content);
                var listlatestMastersSqlLite =  new List<M_SupplierKanbanModel>();
                listlatestMastersSqlLite = contentFromApi.M_SupplierKanbanList;
                listProducts = contentFromApi.M_ProductList;
                var listShipmentScheduleQRCode = contentFromApi.M_ShipmentScheduleQRCodeList;
                // ローカルDBの古いメニューを削除
                await _dbService.ALLDeleteMSupplierKanbanAsync();
                foreach (var latestMastersSqlLite in listlatestMastersSqlLite)
                    await _dbService.SaveMSupplierKanbanAsync(latestMastersSqlLite);

                var listRHandyMenuSupplierKanbanSqlLite = new List<R_HandyMenuSupplierKanbanModel>();
                listRHandyMenuSupplierKanbanSqlLite = contentFromApi.R_HandyMenuSupplierKanbanList;
                // ローカルDBの古いハンディメニューを削除
                await _dbService.ALLDeleteRHandyMenuSupplierKanbanAsync();
                foreach (var rHandyMenuSupplierKanbanSqlLite in listRHandyMenuSupplierKanbanSqlLite)
                    await _dbService.SaveRHandyMenuSupplierKanbanAsync(rHandyMenuSupplierKanbanSqlLite);

                // ローカルDBの古い出荷指示QRコーを削除
                await _dbService.ALLDeleteMShipmentScheduleQRCodeAsync();
                foreach (var shipmentScheduleQRCodeSqlLite in listShipmentScheduleQRCode)
                    await _dbService.SaveMShipmentScheduleQRCodeAsync(shipmentScheduleQRCodeSqlLite);
            }
            else
            {
                await DisplayAlert("エラー", response.content, "OK");
                Application.Current.MainPage = new MenuPage();
                return;
            }

            // 入庫
            if (title == Const.C_REGISTER_STORE_IN_PAGE)
            {
                Application.Current.MainPage = new RegisterStoreInPage(handyMenuID, listProducts);
            }
            else if (title == Const.C_REGISTER_BULK_STORE_IN_PAGE)
                Application.Current.MainPage = new RegisterBulkStoreInPage(handyMenuID, listProducts);

            // 出庫
            else if (title == Const.C_SEARCH_SHIPPING_INSTRUCTION_IN_PAGE)
            {
                // API接続
                var listCompanyResult = await GetListCompanyFromApi();
                if (listCompanyResult.success) 
                    Application.Current.MainPage = new SearchShippingInstructionListPage(handyMenuID, listCompanyResult.mCompanySqlLite, listProducts);
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
            await DisplayAlert("エラー", "E3003: ネットワークに接続できませんでした。", "OK");
            Application.Current.MainPage = new MenuPage();
            return (mCompanySqlLite, false);
        }

        // API接続
        var response = await App.API.PostMethod(jsonDataSend, _handySetting.HandyApiUrl, "M_Company/FetchDelivery");
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

