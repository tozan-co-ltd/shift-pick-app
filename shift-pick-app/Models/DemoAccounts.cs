namespace shift_pick_app.Models
{
    /// <summary>
    /// デモ用アカウント（ログイン・パスワードリセットで共通）
    /// </summary>
    public static class DemoAccounts
    {
        private static readonly (string UserId, string Password)[] Accounts =
        [
            ("user@example.com", "password1"),
            ("user@example.com", "password2"),
            ("user1@example.com", "password1"),
            ("user1@example.com", "password2"),
        ];

        public static bool Exists(string userId)
        {
            return Accounts.Any(a => a.UserId == userId);
        }

        public static bool IsValid(string userId, string password)
        {
            return Accounts.Contains((userId, password));
        }
    }
}
