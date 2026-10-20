using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        // LoadingOverlay のヒント
        protected const string LoadingMessage = "読み込み中…";     // 画面遷移・表示切り替え など
        protected const string ProcessingMessage = "処理中…";      // ログイン・登録 など

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyMessage = string.Empty;

        [ObservableProperty]
        private string _title;

        /// <summary>
        /// オーバーレイ（ヒント付き）を表示して処理する。表示中は再実行・戻るを受け付けない
        /// </summary>
        protected async Task RunBusyAsync(string message, Func<Task> work)
        {
            if (IsBusy)
                return;

            BusyMessage = message;
            IsBusy = true;
            try
            {
                // 画面生成・表示切り替えは UI スレッドを止めるため、先にオーバーレイを描画させる
                await Task.Delay(50);
                await work();
                // 表示切り替えの描画（UI スレッド）が終わるまでオーバーレイを残す
                await Task.Delay(50);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
