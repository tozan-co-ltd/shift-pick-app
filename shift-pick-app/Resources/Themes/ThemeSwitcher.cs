namespace shift_pick_app.Themes;

/// <summary>
/// デモ用テーマ切替。テーマ確定後は採用テーマのLight/Dark切替のみ残す。
/// </summary>
public static class ThemeSwitcher
{
    public static readonly string[] Names = ["Shonan+ カラーマッチ", "Shonan+ 自動生成"];

    public static int Current { get; private set; }

    private static ResourceDictionary? _applied;

    /// <summary>
    /// 指定テーマを端末のLight/Dark設定に合わせて適用する
    /// </summary>
    public static void Apply(int index)
    {
        var app = Application.Current!;
        bool isDark = app.RequestedTheme == AppTheme.Dark;

        ResourceDictionary theme = (index, isDark) switch
        {
            (0, false) => new Theme1Light(),
            (0, true) => new Theme1Dark(),
            (1, false) => new Theme2Light(),
            (1, true) => new Theme2Dark(),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        var merged = app.Resources.MergedDictionaries;
        if (_applied != null)
            merged.Remove(_applied);
        merged.Add(theme);

        _applied = theme;
        Current = index;
    }
}
