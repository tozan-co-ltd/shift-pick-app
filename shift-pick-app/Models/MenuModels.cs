namespace shift_pick_app.Models
{
    /// <summary>
    /// 予約状況の件数（例：予約 3 件）。SW003・SW006 の上段カード
    /// </summary>
    public record QuickStatusItem(ReservationStatus Status, int Count)
    {
        public string Label => Status.ToLabel();
    }

    /// <summary>
    /// メニューの機能ボタン
    /// </summary>
    public record MainMenuItem(string Title);
}
