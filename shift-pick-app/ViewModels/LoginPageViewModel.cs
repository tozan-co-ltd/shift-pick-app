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

        [ObservableProperty]
        private bool _isErrorVisible = false;

        // パスワードを隠す（初期値）/ 表示する
        [ObservableProperty]
        private bool _isPasswordHidden = true;

        /// <summary>
        /// パスワード欄の目のアイコン押下：表示 / 非表示の切り替え
        /// </summary>
        [RelayCommand]
        private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

        /// <summary>
        /// ログインボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
            string userId = UserId.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "ユーザーIDを入力してください。";
                IsErrorVisible = true;
                return;
            }
            if (Password.Length < Const.C_PASSWORD_MIN_LENGTH)
            {
                ErrorMessage = $"パスワードは{Const.C_PASSWORD_MIN_LENGTH}文字以上で入力してください。";
                IsErrorVisible = true;
                return;
            }
            if (!DemoAccounts.IsValid(userId, Password))
            {
                ErrorMessage = "ユーザーIDまたはパスワードが正しくありません。";
                IsErrorVisible = true;
                return;
            }

            // デモ：22222222 はパスワード変更済み扱いでメニューへ、それ以外は初回パスワード変更（ケース1）へ
            await RunBusyAsync(ProcessingMessage, () =>
            {
                if (Password == "22222222")
                {
                    _navigation.SetRoot<MenuPage>(page => page.SetUser(userId));
                    return Task.CompletedTask;
                }
                return _navigation.PushAsync<PasswordReset>(page => page.StartFirstLogin(userId));
            });
        }

        /// <summary>
        /// 「パスワードをお忘れの場合はこちら」押下
        /// </summary>
        [RelayCommand]
        private Task ForgotPassword()
        {
            return RunBusyAsync(LoadingMessage, () => _navigation.PushAsync<PasswordReset>());
        }
    }
}
