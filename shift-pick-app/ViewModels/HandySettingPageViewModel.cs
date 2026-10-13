using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Services;
using shift_pick_app.Themes;

namespace shift_pick_app.ViewModels
{
    public partial class HandySettingPageViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;

        public HandySettingPageViewModel(NavigationService navigation)
        {
            _navigation = navigation;
            _selectedThemeIndex = ThemeSwitcher.Current;
        }

        public string[] ThemeNames => ThemeSwitcher.Names;

        /// <summary>
        /// 接続先
        /// </summary>
        [ObservableProperty]
#if DEBUG
        private string _apiUrl = "https://wbtzn/sumaken-api-MRq2xg5_test/api/";
#else
        private string _apiUrl = "https://www.tozan.co.jp/sumaken-api-MRq2xg5/api/";
#endif

        /// <summary>
        /// テーマ（デモ用）
        /// </summary>
        [ObservableProperty]
        private int _selectedThemeIndex;

        partial void OnSelectedThemeIndexChanged(int value)
        {
            if (value >= 0)
                ThemeSwitcher.Apply(value);
        }

        /// <summary>
        /// 戻るボタン押下
        /// </summary>
        [RelayCommand]
        private Task Back()
        {
            return _navigation.PopAsync();
        }

        /// <summary>
        /// 保存ボタン押下
        /// </summary>
        [RelayCommand]
        private Task Save()
        {
            // 保存処理をここに追加
            return _navigation.PopAsync();
        }
    }
}
