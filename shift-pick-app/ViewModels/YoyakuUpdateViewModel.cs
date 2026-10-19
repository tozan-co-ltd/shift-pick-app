using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class YoyakuUpdateViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        public YoyakuUpdateViewModel(NavigationService navigation, DialogService dialog)
        {
            _navigation = navigation;
            _dialog = dialog;
        }

        /// <summary>
        /// SW004 で選択した予約
        /// </summary>
        [ObservableProperty]
        private List<YoyakuSlot> _slots = [];

        /// <summary>
        /// 登録ボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Register()
        {
            bool confirmed = await _dialog.ConfirmAsync("応募登録", "予約が確定したらキャンセルできません。\n登録しますか？");
            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                // デモ：登録・メール送信の代わり
                await Task.Delay(1000);
            }
            finally
            {
                IsBusy = false;
            }

            bool openList = await _dialog.ConfirmAsync("登録しました",
                                                       "登録内容をメールで送信しました。\n予約状況を確認しますか？",
                                                       "予約状況へ", "閉じる");
            if (openList)
            {
                await _navigation.PopToRootAsync(animated: false);
                await _navigation.PushAsync<YoyakuList>();
                return;
            }
            await _navigation.PopAsync();
        }
    }
}
