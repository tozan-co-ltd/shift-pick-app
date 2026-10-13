using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
        private const int PasswordMinLength = 8;

        // デモ用アカウント（ユーザーID, パスワード）
        private static readonly Dictionary<string, string> DemoAccounts = new()
        {
            ["user@example.com"] = "password1",
            ["user@example.com"] = "password2",
            ["user1@example.com"] = "password1",
            ["user1@example.com"] = "password2",
        };

        private readonly NavigationService _navigation;

        public LoginPageViewModel(NavigationService navigation)
        {
            _navigation = navigation;
        }

        [ObservableProperty]
        private string _userId = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        /// <summary>
        /// ログインボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = string.Empty;
            string userId = UserId.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "ユーザーIDを入力してください。";
                return;
            }
            if (Password.Length < PasswordMinLength)
            {
                ErrorMessage = $"パスワードは{PasswordMinLength}文字以上で入力してください。";
                return;
            }
            if (!DemoAccounts.TryGetValue(userId, out var password) || password != Password)
            {
                ErrorMessage = "ユーザーIDまたはパスワードが正しくありません。";
                return;
            }

            // デモ：ログイン後はパスワードリセット画面へ
            await _navigation.PushAsync<PasswordReset>();
        }

        /// <summary>
        /// 「パスワードをお忘れの場合はこちら」押下
        /// </summary>
        [RelayCommand]
        private Task ForgotPassword()
        {
            return _navigation.PushAsync<PasswordReset>();
        }

        /// <summary>
        /// 設定ボタン押下
        /// </summary>
        [RelayCommand]
        private Task OpenSettings()
        {
            return _navigation.PushAsync<HandySettingPage>();
        }
    }
}
