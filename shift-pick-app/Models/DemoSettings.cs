namespace shift_pick_app.Models
{
    /// <summary>
    /// デモ用の一時的な切り替え（ステークホルダー確認用、決定後に削除）
    /// </summary>
    public static class DemoSettings
    {
        // SW008 都道府県カード：true = 名前入り画像のみ / false = 名前なし画像 + ラベル
        public static bool PrefectureImageWithName { get; set; }
    }
}
