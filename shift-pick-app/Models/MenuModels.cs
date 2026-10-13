namespace shift_pick_app.Models
{
    /// <summary>
    /// メニュー上段の予約状況（例：予約 3 件）
    /// </summary>
    public record QuickStatusItem(string Label, int Count);

    /// <summary>
    /// メニューの機能ボタン
    /// </summary>
    public record MainMenuItem(string Title);
}
