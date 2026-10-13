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

        public App()
        {
            InitializeComponent();

            // テーマ適用（端末のLight/Dark変更時も再適用）
            ThemeSwitcher.Apply(0);
            RequestedThemeChanged += (s, e) => ThemeSwitcher.Apply(ThemeSwitcher.Current);

            //MainPage = new AppShell();            
            MainPage = new LoginPage();
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
    }
}
