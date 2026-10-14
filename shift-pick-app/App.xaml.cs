using shift_pick_app.Common;
using shift_pick_app.Themes;
using shift_pick_app.Views;
#if __ANDROID__
using Android.Content.Res;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif
using Microsoft.Maui;
using Application = Microsoft.Maui.Controls.Application;

namespace shift_pick_app
{
    public partial class App : Application
    {
        static APIClass m_api;
        public static APIClass API
        {
            get
            {
                if (m_api == null)
                {
                    m_api = new APIClass();
                }
                return m_api;
            }
        }

        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;

            // テーマ適用（端末のLight/Dark変更時も再適用）
            ApplyTheme();
            RequestedThemeChanged += (s, e) => ApplyTheme();

            // 入力の下線を削除する
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif
            });
        }

        private ResourceDictionary? _theme;

        /// <summary>
        /// 端末のLight/Dark設定に合わせてテーマの色を差し替える
        /// </summary>
        private void ApplyTheme()
        {
            ResourceDictionary theme = RequestedTheme == AppTheme.Dark ? new DarkTheme() : new LightTheme();

            if (_theme != null)
                Resources.MergedDictionaries.Remove(_theme);
            Resources.MergedDictionaries.Add(theme);
            _theme = theme;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(_services.GetRequiredService<LoginPage>()));
        }
    }
}
