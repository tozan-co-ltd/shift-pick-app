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
    private ObservableCollection<D_ReceiveModel> _items;
    private ObservableCollection<D_ReceiveModel> _itemsNotDelete;
    private ObservableCollection<D_ReceiveModel> _itemsHistory;
    private readonly LocalDbService _dbService;
    private HandySettingModel.HandySetting _handySetting;
    private bool _IsScanPageActive = false;
    string supplierProductNumber = string.Empty;
    int numberOfBoxes = 0;
    string lotNumber = string.Empty;
    string resString = string.Empty;
    string clipboardContent = string.Empty;
    public RegisterBulkStoreInPage()
    {
        InitializeComponent();
        Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        _dbService = new LocalDbService();
        _items = new ObservableCollection<D_ReceiveModel>();
        _itemsNotDelete = new ObservableCollection<D_ReceiveModel>();
        _itemsHistory = new ObservableCollection<D_ReceiveModel>();
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
            if (_items.Count() > 0)
            {
                bool result = await Application.Current.MainPage.DisplayAlert("確認", "読み取ったデータは削除されますが、戻ってよろしいですか。", "OK", "キャンセル");
                if (result)
                    Application.Current.MainPage = new MenuPage();
            }
            else
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
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                clipboardContent = Clipboard.GetTextAsync().Result;
                if (clipboardContent.Trim().IndexOf("11") != -1)
                {
                    supplierProductNumber = clipboardContent.Substring(81, 50).ToString().Trim();
                    lotNumber = clipboardContent.Substring(29, 10).Trim();
                    // 既存の製品に同じ製品番号があるかチェックする
                    D_ReceiveModel duplicateProduct = _itemsNotDelete.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();
                    if (duplicateProduct != null)
                    {
                        // エラーサウンド
                        SoundPlayer.PlaySound(resultErrorSound);
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
            string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
            string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);

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
                    D_ReceiveModel existingProduct =  _items.Where(p => p.SupplierProductNumber == supplierProductNumber).FirstOrDefault();
                    // 既存の製品に同じ製品番号とキーがあるかチェックする 
                    D_ReceiveModel duplicateProduct = _itemsNotDelete.Where(p => p.SupplierProductNumber == supplierProductNumber && p.LotNumber == lotNumber).FirstOrDefault();
                    if (duplicateProduct == null)
                    {
                        count++;
                        CounterBtn.Text = $"{count}";

                        if (existingProduct != null)
                        {
                            _items.Add(new D_ReceiveModel
                            {
                                ReceiveID = 2,
                                SupplierProductNumber = supplierProductNumber,
                                LotNumber = lotNumber,
                                Quantity = ((enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0) + existingProduct.Quantity
                            });
                            // 既存の製品を削除する
                            _items.Remove(existingProduct);
                        }
                        else
                        {
                            _items.Add(new D_ReceiveModel
                            {
                                ReceiveID = 2,
                                SupplierProductNumber = supplierProductNumber,
                                LotNumber = lotNumber,
                                Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0
                            });
                            // 重複処理前のすべてのアイテムを取得する
                            _itemsNotDelete.Add(new D_ReceiveModel { ReceiveID = 2, SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
                        }
                        _itemsHistory.Add(new D_ReceiveModel { ReceiveID = 2, SupplierProductNumber = supplierProductNumber, LotNumber = lotNumber, Quantity = (enteredNumberLabel.Text != string.Empty) ? int.Parse(enteredNumberLabel.Text) : 0 });
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
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);

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
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
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
