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

        private static INavigation Navigation => Application.Current!.Windows[0].Page!.Navigation;

        public Task PushAsync<TPage>() where TPage : Page
        {
            return Navigation.PushAsync(_services.GetRequiredService<TPage>());
        }

        public Task PopAsync()
        {
            return Navigation.PopAsync();
        }
    }
}
