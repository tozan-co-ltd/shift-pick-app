using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
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
        /// 登録完了時の処理（SW004 の選択解除）
        /// </summary>
        public Action? OnRegistered { get; set; }

        /// <summary>
        /// 戻る（ナビゲーションバー・端末の戻るボタン共通、登録後の「閉じる」）→ SW004
        /// </summary>
        [RelayCommand]
        private Task Back() => RunBusyAsync(LoadingMessage, _navigation.PopAsync);

        /// <summary>
        /// 登録ボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Register()
        {
            bool confirmed = await _dialog.ConfirmAsync("応募登録", Util.PhraseJpText("|予約が|確定したら|キャンセル|できません。|登録しますか？"));
            if (!confirmed)
                return;

            await RunBusyAsync(ProcessingMessage, async () =>
            {
                // デモ：登録・メール送信の代わり
                await Task.Delay(1000);
                OnRegistered?.Invoke();
            });

            bool openList = await _dialog.ShowResultAsync("登録しました",
                                                          Util.PhraseJpText("|登録内容を|メールで|送信しました。\n|予約状況を|確認しますか？"),
                                                          "予約状況へ", "閉じる");
            if (openList)
            {
                // SW006 を開き、SW004・SW005 を外す（戻るでメニューへ）
                await RunBusyAsync(LoadingMessage, () => _navigation.PushOverRootAsync<YoyakuList>());
                return;
            }
            await Back();
        }
    }
}
