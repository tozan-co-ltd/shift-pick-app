namespace shift_pick_app.Models
{
    /// <summary>
    /// 事業所の基本情報
    /// </summary>
    /// <param name="Id">事業所ID</param>
    /// <param name="Name">事業所名</param>
    /// <param name="City">都市名</param>
    /// <param name="DistanceKm">現在地からの距離（GPS の代わり）</param>
    public record OfficeInfo(int Id, string Name, string City, double DistanceKm);

    /// <summary>
    /// デモ用の事業所と希望事業所（SW004・SW006・SW008 で共通、アプリ再起動でリセット）
    /// </summary>
    public static class DemoOffices
    {
        public static readonly List<OfficeInfo> All =
        [
            new(1, "○○A事業所", "○○市", 5.2),
            new(2, "○○B事業所", "○○市", 1.3),
            new(3, "○○○○○○○○C事業所", "○○市", 8.0),
            new(4, "○○D事業所", "○○市", 3.4),
            new(5, "○○E事業所", "○○市", 12.5),
            new(6, "○○F事業所", "○○市", 9.1),
            new(7, "○○G事業所", "○○市", 6.7),
            new(8, "○○H事業所", "○○市", 15.0),
            new(9, "○○○○I事業所", "○○市", 37.4),
            new(10, "○○K事業所", "○○市", 50.2),
            new(11, "○○L事業所", "○○市", 135.8),
        ];

        /// <summary>
        /// 希望事業所の事業所ID（SW008 の登録で置き換え）
        /// </summary>
        public static List<int> PreferredIds { get; set; } = [1, 3, 4, 6];

        public static OfficeInfo Get(int id) => All.First(o => o.Id == id);

        /// <summary>
        /// 希望事業所の表示（○○A、○○B、…）。未登録は空文字
        /// </summary>
        public static string PreferredText => ToText(All.Where(o => PreferredIds.Contains(o.Id)).Select(o => o.Name));

        /// <summary>
        /// 事業所名を「、」区切りにする（「事業所」は省略）
        /// </summary>
        public static string ToText(IEnumerable<string> officeNames) =>
            string.Join("、", officeNames.Select(n => n.Replace("事業所", "")));
    }
}
