using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
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
            if (Password.Length < Const.C_PASSWORD_MIN_LENGTH)
            {
                ErrorMessage = $"パスワードは{Const.C_PASSWORD_MIN_LENGTH}文字以上で入力してください。";
                return;
            }
            if (!DemoAccounts.IsValid(userId, Password))
            {
                ErrorMessage = "ユーザーIDまたはパスワードが正しくありません。";
                return;
            }

            // デモ：22222222 はパスワード変更済み扱いでメニューへ、それ以外は初回パスワード変更（ケース1）へ
            if (Password == "22222222")
            {
                _navigation.SetRoot<MenuPage>(page => page.SetUser(userId));
                return;
            }
            await _navigation.PushAsync<PasswordReset>(page => page.StartFirstLogin(userId));
        }

        /// <summary>
        /// 「パスワードをお忘れの場合はこちら」押下
        /// </summary>
        [RelayCommand]
        private Task ForgotPassword()
        {
            return _navigation.PushAsync<PasswordReset>();
        }
    }
}
