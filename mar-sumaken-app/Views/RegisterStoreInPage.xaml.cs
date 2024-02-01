using System.Collections.ObjectModel;
using mar_sumaken_app.Models;
using mar_sumaken_app.ViewModels;
using mar_sumaken_app.Common;
using Plugin.Maui.Audio;
using mar_sumaken_app.Services;
using Microsoft.Maui.Animations;
using mar_sumaken_app.ViewModels;

namespace mar_sumaken_app.Views;

public partial class RegisterStoreInPage : ContentPage
{
    int count = 0;
    int boxCout = 1;
    bool isItemTapped = false;
    private ObservableCollection<ProductDemo> items;
    private ObservableCollection<ProductDemo> itemsNotDelete;
    private ObservableCollection<ProductDemo> itemsHistory;
    private readonly LocalDbService _dbService;
    private ScanAndSound scanAndSound;
    private bool IsScanPageActive = false;
    public RegisterStoreInPage()
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        CounterBtn.Text = $"{count}";
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        items = new ObservableCollection<ProductDemo>();
        itemsNotDelete = new ObservableCollection<ProductDemo>();
        itemsHistory = new ObservableCollection<ProductDemo>();
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        BindingContext = new RegisterStoreInPageViewModel();
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        IsScanPageActive = true;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの購読を解除する
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        IsScanPageActive = false;
    }

    /// <summary>
    /// 戻る
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
        if(!isItemTapped)
        {
            isItemTapped = true;
            bool result = await Application.Current.MainPage.DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
            if (result)
                Application.Current.MainPage = new MenuPage();

            isItemTapped = false;
        }
    }

    /// <summary>
    /// ハンディリーダーでスキャンする時、処理する
    /// </summary>
    protected async void OnClipboardContentChanged(object sender, EventArgs e)
    {
        try
        {
            if (IsScanPageActive)
            {
                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, scanAndSound);
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, scanAndSound);

                string clipboardContent = Clipboard.GetTextAsync().Result;
                if (clipboardContent != string.Empty)
                {
                    string[] substrings = clipboardContent.Split(' ');
                    if (substrings.Length > 3)
                    {
                        string resString = substrings[0].Trim();
                        string productNumValue = string.Empty;
                        string uniformNumValue = string.Empty;
                        string keyValue = string.Empty;
                        if (resString.Length > 8)
                        {
                            productNumValue = resString.Substring(resString.Length - 8).ToString();
                            uniformNumValue = resString.Substring(21, 2);
                            keyValue = resString.Substring(4, 11);
                        }

                        // 既存の製品に同じ製品番号があるかチェックする
                        ProductDemo existingProduct = items.FirstOrDefault(p => p.product_num == productNumValue);
                        // 既存の製品に同じ製品番号とキーがあるかチェックする 
                        ProductDemo duplicateProduct = itemsNotDelete.FirstOrDefault(p => p.product_num == productNumValue && p.key == keyValue);

                        if (duplicateProduct == null)
                        {
                            count++;
                            if (count == 1)
                                CounterBtn.Text = $"{count}";
                            else
                                CounterBtn.Text = $"{count}";

                            if (existingProduct != null)
                            {
                                existingProduct.box_num++;
                                items.Add(new ProductDemo { Id = 2, product_num = productNumValue, uniform_num = uniformNumValue, key = keyValue, box_num = existingProduct.box_num });
                                // 既存のデータを削除する
                                items.Remove(existingProduct);
                            }
                            else
                            {
                                items.Add(new ProductDemo { Id = 2, product_num = productNumValue, uniform_num = uniformNumValue, key = keyValue, box_num = boxCout });
                            }
                            // 重複ハンドル前のすべてのアイテムを取得する
                            itemsNotDelete.Add(new ProductDemo { Id = 2, product_num = productNumValue, uniform_num = uniformNumValue, key = keyValue, box_num = boxCout });
                        }
                        else
                        {
                            // エラーサウンド
                            SoundPlayer.PlaySound(resultErrorSound);
                            scanTextResult.Text = "E1003: 既にスキャン済みの仕入先かんばんです。";
                            scanTextResult.TextColor = Colors.Red;
                            return;
                        }

                        // OKサウンド
                        SoundPlayer.PlaySound(resultOkSound);
                        scanTextResult.Text = "スキャンOK";
                        scanTextResult.TextColor = Colors.Black;

                        itemsHistory.Add(new ProductDemo { Id = 2, product_num = productNumValue, key = keyValue, uniform_num = uniformNumValue });
                        // 履歴コレクションビュー
                        historyCollectionView.ItemsSource = itemsHistory.Reverse();
                        // ボックスリストの数
                        collectionView.ItemsSource = items.Reverse();
                    }
                    else
                    {
                        // エラーサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1000: QRコードが正しくありません。";
                        scanTextResult.TextColor = Colors.Red;
                        return;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (items.Count() > 0)
            {
                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, scanAndSound);

                // OKサウン
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "閉じる");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(300);
                Application.Current.MainPage = new RegisterStoreInPage();
            }
            else
            {
                // サウンドのパスを取得する
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, scanAndSound);
                // エラーサウンド
                SoundPlayer.PlaySound(resultErrorSound);
                await Application.Current.MainPage.DisplayAlert("お知らせ", "データがありません", "OK");
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
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        numberOfBoxList.IsVisible = true;
        historyList.IsVisible = false;
    }

    /// <summary>
    /// ボタンのカーラーを変更とコンテンツを非表示
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        numberOfBoxListBtn.BackgroundColor = Colors.Gray;
        historyListBtn.BackgroundColor = Color.FromHex("#FD5A00");
        numberOfBoxList.IsVisible = false;
        historyList.IsVisible = true;
    }
}