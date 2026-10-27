using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using shift_pick_app.Views.Shared;

namespace shift_pick_app.Services
{
    /// <summary>
    /// ViewModelからのポップアップ表示（確認：端末標準のアラート / 結果：ResultPopup）
    /// </summary>
    public class DialogService
    {
        private static Page CurrentPage => Application.Current!.Windows[0].Page!;

        /// <summary>
        /// 確認（はい／いいえ）。はいで true
        /// </summary>
        public Task<bool> ConfirmAsync(string title, string message, string accept = "はい", string cancel = "いいえ")
        {
            return CurrentPage.DisplayAlert(title, message, accept, cancel);
        }

        /// <summary>
        /// お知らせ（OK のみ）
        /// </summary>
        public Task AlertAsync(string title, string message)
        {
            return CurrentPage.DisplayAlert(title, message, "OK");
        }

        /// <summary>
        /// 処理結果のポップアップ。主ボタンで true、副ボタン・外側タップ・戻るで false
        /// </summary>
        public async Task<bool> ShowResultAsync(string title, string message, string primary, string secondary)
        {
            var options = new PopupOptions
            {
                Shape = new RoundRectangle { CornerRadius = 16, Stroke = Colors.Transparent, StrokeThickness = 0 },
                Shadow = null,
                PageOverlayColor = GetColor("Scrim").WithAlpha(0.32f),
            };

            // 背景色はコードで渡す：XAML の DynamicResource は表示前に解決されず、Toolkit が既定の白で上書きする（ダークモードで白くなる）
            // SurfaceContainerHigh（テーマの「ダイアログ」の面）：ダークモードは画面より明るい面にして浮いて見せる（黒い画面ではオーバーレイが効かない）
            var popup = new ResultPopup(title, message, primary, secondary) { BackgroundColor = GetColor("SurfaceContainerHigh") };
            IPopupResult<bool> result = await CurrentPage.ShowPopupAsync<bool>(popup, options);
            return result.Result;
        }

        private static Color GetColor(string key) =>
            Application.Current!.Resources.TryGetValue(key, out var value) && value is Color color ? color : Colors.Black;
    }
}
