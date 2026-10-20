namespace shift_pick_app.Models
{
    /// <summary>
    /// デモ用の一時的な切り替え（ステークホルダー確認用、決定後に削除）
    /// </summary>
    public static class DemoSettings
    {
        // SW003 お知らせ：true = データあり / false = データなし（「通知はありません。」）
        public static bool HasNews { get; set; } = true;

        // SW008 都道府県カード：true = 名前入り画像のみ / false = 名前なし画像 + ラベル
        public static bool PrefectureImageWithName { get; set; } = true;

        // SW008 都道府県カードの並べ方：true = 地方ごと（見出し付き・3列）/ false = 設計書どおり（見出しなし、3列は同じ）
        public static bool PrefectureByRegion { get; set; } = true;

        // SW007 カレンダー：true = 独自の MonthCalendar / false = Plugin.Maui.Calendar
        public static bool CustomCalendar { get; set; } = true;
    }
}
