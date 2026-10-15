namespace shift_pick_app.Services
{
    /// <summary>
    /// ViewModelからのポップアップ表示（端末標準のアラート）
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
    }
}
