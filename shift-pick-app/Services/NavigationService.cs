namespace shift_pick_app.Services
{
    /// <summary>
    /// ViewModelからの画面遷移（NavigationPage）。画面はDIから取得する。
    /// </summary>
    public class NavigationService
    {
        private readonly IServiceProvider _services;

        public NavigationService(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// FlyoutPage（メニュー）の場合は Detail 側の NavigationPage に積む
        /// </summary>
        private static INavigation Navigation
        {
            get
            {
                var root = Application.Current!.Windows[0].Page!;
                return root is FlyoutPage flyout ? flyout.Detail.Navigation : root.Navigation;
            }
        }

        /// <summary>
        /// 画面を積む。setup で表示前に初期値を渡せる。
        /// </summary>
        public Task PushAsync<TPage>(Action<TPage>? setup = null) where TPage : Page
        {
            var page = _services.GetRequiredService<TPage>();
            setup?.Invoke(page);
            return Navigation.PushAsync(page);
        }

        /// <summary>
        /// 画面を積み、スタックの最初の画面（メニュー）との間の画面を外す（戻るでメニューへ）
        /// </summary>
        public async Task PushOverRootAsync<TPage>(Action<TPage>? setup = null) where TPage : Page
        {
            var navigation = Navigation;
            await PushAsync(setup);
            foreach (var page in navigation.NavigationStack.Skip(1).SkipLast(1).ToList())
                navigation.RemovePage(page);
        }

        public Task PopAsync()
        {
            return Navigation.PopAsync();
        }

        /// <summary>
        /// 画面スタックを破棄して新しいルート画面にする（戻るで前の画面に戻れない）
        /// FlyoutPage はルートでのみ使えるため NavigationPage で包まない
        /// </summary>
        public void SetRoot<TPage>(Action<TPage>? setup = null) where TPage : Page
        {
            var page = _services.GetRequiredService<TPage>();
            setup?.Invoke(page);
            Application.Current!.Windows[0].Page = page is FlyoutPage ? page : new NavigationPage(page);
        }
    }
}
