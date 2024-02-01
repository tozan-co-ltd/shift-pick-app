using mar_sumaken_app.Common;
using mar_sumaken_app.ViewModels;
using System.Collections.ObjectModel;
using mar_sumaken_app.Models;
using mar_sumaken_app.Services;
using System.Linq.Expressions;

namespace mar_sumaken_app.Views;

public partial class RegisterBulkStoreInPage : ContentPage
{
    int count = 0;
    bool isItemTapped = false;
    private ObservableCollection<ProductDemo> _items;
    private ObservableCollection<ProductDemo> _itemsNotDelete;
    private ObservableCollection<ProductDemo> _itemsHistory;
    private readonly LocalDbService _dbService;
    private ScanAndSound _scanAndSound;
    private bool _IsScanPageActive = false;
    string productNumValue = string.Empty;
    string uniformNumValue = string.Empty;
    string keyValue = string.Empty;
    string resString = string.Empty;
    string clipboardContent = string.Empty;
    public RegisterBulkStoreInPage()
    {
        InitializeComponent();
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        _dbService = new LocalDbService();
        _items = new ObservableCollection<ProductDemo>();
        _itemsNotDelete = new ObservableCollection<ProductDemo>();
        _itemsHistory = new ObservableCollection<ProductDemo>();
        CounterBtn.Text = $"{count}";
        numberOfBoxListBtn.BackgroundColor = Colors.Teal;
        historyListBtn.BackgroundColor = Colors.Gray;
        scanTextResult.Text = string.Empty;
        enteredNumberLabel.Text = string.Empty;
        BindingContext = new RegisterBulkStoreInPageViewModel();
    }

    /// <summary>
    /// 画面をが開いたらイベントをサブスクライブします
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _IsScanPageActive = true;
    }

    /// <summary>
    /// ページが表示されない場合はイベントの購読を解除する
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _IsScanPageActive = false;
    }

    /// <summary>
    /// 戻る
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
        if (!isItemTapped)
        {
            isItemTapped = true;
            bool result = await Application.Current.MainPage.DisplayAlert("確認", "読み取ったデータは削除さ   れますが、戻ってよろしいですか。", "OK", "キャンセル");
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
            if (_IsScanPageActive)
            {
                // サウンドのパスを取得する
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _scanAndSound);
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _scanAndSound);

                clipboardContent = Clipboard.GetTextAsync().Result;
                if (clipboardContent != string.Empty)
                {
                    string[] substrings = clipboardContent.Split(' ');
                    if (substrings.Length > 3)
                    {
                        resString = substrings[0].Trim();
                        if (resString.Length > 8)
                        {
                            productNumValue = resString.Substring(resString.Length - 8).ToString();
                            uniformNumValue = resString.Substring(21, 2);
                            keyValue = resString.Substring(4, 11);
                        }
                        // 既存の製品に同じ製品番号があるかチェックする
                        ProductDemo duplicateProduct = _itemsNotDelete.FirstOrDefault(p => p.product_num == productNumValue && p.key == keyValue);
                        if (duplicateProduct != null)
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
                        numberInputModal.IsVisible = true;
                        bulkStockContent.IsVisible = false;
                    }
                    else
                    {
                        // エラーサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1000:QRコードが正しくありません。";
                        scanTextResult.TextColor = Colors.Red;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "閉じる");
            return;
        }
    }

    /// <summary>
    /// 数字ボタンをタップしたのを処理する
    /// </summary>
    private void OnNumberButtonClicked(object sender, EventArgs e)
    {
        // 数値ボタンのクリックを処理する
        var button = (Button)sender;
        if (button.Text != string.Empty || int.Parse(button.Text) == 0)
            errorLabel.Text = string.Empty;

        enteredNumberLabel.Text += button.Text;
    }

    /// <summary>
    /// タップした数字を消す
    /// </summary>
    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Handle delete button click
        if (!string.IsNullOrEmpty(enteredNumberLabel.Text))
        {
            errorLabel.Text = string.Empty;
            enteredNumberLabel.Text = enteredNumberLabel.Text.Substring(0, enteredNumberLabel.Text.Length - 1);
        }
    }

    /// <summary>
    /// オーケーボタンを押すとチェックして処理
    /// </summary>
    private async void OnOkButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // サウンドのパスを取得する
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _scanAndSound);
            string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _scanAndSound);

            if (enteredNumberLabel.Text.Length > 3)
            {
                errorLabel.Text = "最大3桁まで入力可能です。";
                // エラーサウンド
                SoundPlayer.PlaySound(resultErrorSound);
            }
            else
            {
                if (enteredNumberLabel.Text == string.Empty)
                {
                    errorLabel.Text = "0以上で入力してください。";
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                }
                else if (enteredNumberLabel.Text != string.Empty && int.Parse(enteredNumberLabel.Text) == 0)
                {
                    errorLabel.Text = "0以上で入力してください。";
                    // エラーサウンド
                    SoundPlayer.PlaySound(resultErrorSound);
                }
                else
                {
                    // 既存の製品に同じ製品番号があるかチェックする
                    ProductDemo existingProduct = _items.FirstOrDefault(p => p.product_num == productNumValue);
                    // 既存の製品に同じ製品番号とキーがあるかチェックする 
                    ProductDemo duplicateProduct = _itemsNotDelete.FirstOrDefault(p => p.product_num == productNumValue && p.key == keyValue);
                    if (duplicateProduct == null)
                    {
                        count++;
                        if (count == 1)
                            CounterBtn.Text = $"{count}";
                        else
                            CounterBtn.Text = $"{count}";

                        if (existingProduct != null)
                        {
                            _items.Add(new ProductDemo
                            {
                                Id = 2,
                                product_num = productNumValue,
                                uniform_num = uniformNumValue,
                                key = keyValue,
                                box_num = ((enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0) + existingProduct.box_num
                            });
                            // 既存の製品を削除する
                            _items.Remove(existingProduct);
                        }
                        else
                        {
                            _items.Add(new ProductDemo
                            {
                                Id = 2,
                                product_num = productNumValue,
                                uniform_num = uniformNumValue,
                                key = keyValue,
                                box_num = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0
                            });
                            // 重複処理前のすべてのアイテムを取得する
                            _itemsNotDelete.Add(new ProductDemo { Id = 2, product_num = productNumValue, uniform_num = uniformNumValue, key = keyValue, box_num = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
                        }
                        _itemsHistory.Add(new ProductDemo { Id = 2, product_num = productNumValue, key = keyValue, uniform_num = uniformNumValue, box_num = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
                        // 履歴コレクションビュー
                        historyCollectionView.ItemsSource = _itemsHistory.Reverse();

                        collectionView.ItemsSource = _items.Reverse();
                    }
                    else
                    {
                        // OKサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
                        scanTextResult.Text = "E1003: 既にスキャン済みの仕入先かんばんです。";
                        scanTextResult.TextColor = Colors.Red;
                        return;
                    }

                    numberInputModal.IsVisible = false;
                    bulkStockContent.IsVisible = true;
                    enteredNumberLabel.Text = string.Empty;
                    errorLabel.Text = string.Empty;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// キャンセルボタンを押すと全て消す、画面を戻る
    /// </summary>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        enteredNumberLabel.Text = string.Empty;
        numberInputModal.IsVisible = false;
        bulkStockContent.IsVisible = true;
        errorLabel.Text = string.Empty;
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

    /// <summary>
    /// データを登録
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (_items.Count() > 0)
            {
                // サウンドのパスを取得す
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _scanAndSound);

                // OKサウンド
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("完了", "データ登録が完了しました", "閉じる");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new RegisterBulkStoreInPage();
            }
            else
            {
                // サウンドのパスを取得す
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _scanAndSound);
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
}