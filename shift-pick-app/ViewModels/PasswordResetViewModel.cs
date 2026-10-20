using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Services;
using shift_pick_app.Views;

namespace shift_pick_app.ViewModels
{
    public partial class PasswordResetViewModel : BaseViewModel
    {
        /// <summary>
        /// 画面の状態
        /// </summary>
        public enum ResetStep
        {
            FirstLogin,     // ケース1：初回ログイン（初期パスワード → 新規パスワード）
            EnterUserId,    // ケース2-1：パスワードを忘れた（ユーザーID入力）
            NewPassword,    // ケース2-2：ユーザーID確認後（新規パスワード）
        }

        private readonly NavigationService _navigation;

        public PasswordResetViewModel(NavigationService navigation)
        {
            _navigation = navigation;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUserIdEditable))]
        [NotifyPropertyChangedFor(nameof(IsUserIdReadOnly))]
        [NotifyPropertyChangedFor(nameof(IsInitialPasswordVisible))]
        [NotifyPropertyChangedFor(nameof(IsNewPasswordVisible))]
        [NotifyPropertyChangedFor(nameof(SubmitText))]
        private ResetStep _step = ResetStep.EnterUserId;

        [ObservableProperty]
        private string _userId = string.Empty;

        [ObservableProperty]
        private string _initialPassword = string.Empty;

        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool IsUserIdEditable => Step == ResetStep.EnterUserId;
        public bool IsUserIdReadOnly => !IsUserIdEditable;
        public bool IsInitialPasswordVisible => Step == ResetStep.FirstLogin;
        public bool IsNewPasswordVisible => Step != ResetStep.EnterUserId;
        public string SubmitText => Step == ResetStep.EnterUserId ? "送信" : "登録";

        // 新規パスワードの下のヒント（例：8文字以上）
        public string PasswordRuleHint => $"{Const.C_PASSWORD_MIN_LENGTH}文字以上";

        /// <summary>
        /// ケース1：ログイン画面から初回ログインとして開く
        /// </summary>
        public void StartFirstLogin(string userId)
        {
            UserId = userId;
            Step = ResetStep.FirstLogin;
        }

        /// <summary>
        /// 送信／登録ボタン押下
        /// </summary>
        [RelayCommand]
        private async Task Submit()
        {
            ErrorMessage = string.Empty;

            switch (Step)
            {
                case ResetStep.EnterUserId:
                    ConfirmUserId();
                    break;

                case ResetStep.FirstLogin:
                    if (!DemoAccounts.IsValid(UserId, InitialPassword))
                    {
                        ErrorMessage = "初期パスワードが正しくありません。";
                        return;
                    }
                    await RegisterNewPassword();
                    break;

                case ResetStep.NewPassword:
                    await RegisterNewPassword();
                    break;
            }
        }

        /// <summary>
        /// 戻るボタン・端末の戻るボタン押下 → SW001
        /// </summary>
        [RelayCommand]
        private Task Back()
        {
            return RunBusyAsync(LoadingMessage, _navigation.PopAsync);
        }

        private void ConfirmUserId()
        {
            string userId = UserId.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "ユーザーIDを入力してください。";
                return;
            }
            if (!DemoAccounts.Exists(userId))
            {
                ErrorMessage = "ユーザーIDが見つかりません。";
                return;
            }

            UserId = userId;
            Step = ResetStep.NewPassword;
        }

        private Task RegisterNewPassword()
        {
            if (NewPassword.Length < Const.C_PASSWORD_MIN_LENGTH)
            {
                ErrorMessage = $"パスワードは{Const.C_PASSWORD_MIN_LENGTH}文字以上で入力してください。";
                return Task.CompletedTask;
            }
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "新規パスワードと確認用パスワードが一致しません。";
                return Task.CompletedTask;
            }

            // デモ：新しいパスワードでログインした扱いでメニューへ（戻れないようにスタックを置き換える）
            return RunBusyAsync(ProcessingMessage, () =>
            {
                _navigation.SetRoot<MenuPage>(page => page.SetUser(UserId));
                return Task.CompletedTask;
            });
        }
    }
}
