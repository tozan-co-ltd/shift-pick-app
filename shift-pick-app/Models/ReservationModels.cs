using shift_pick_app.Common;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 予約のステータス
    /// </summary>
    public enum ReservationStatus
    {
        Reserved,   // 予約
        Confirmed,  // 確定
        Rejected,   // NG
    }

    public static class ReservationStatusExtensions
    {
        public static string ToLabel(this ReservationStatus status) => status switch
        {
            ReservationStatus.Confirmed => "確定",
            ReservationStatus.Rejected => "NG",
            _ => "予約",
        };
    }

    /// <summary>
    /// 予約・実績の1件（SW006）
    /// </summary>
    public record Reservation(DateTime WorkDate, string OfficeName, ReservationStatus Status)
    {
        public string WorkDateText => Util.ToJapaneseDate(WorkDate);
        public string StatusLabel => Status.ToLabel();
    }

    /// <summary>
    /// デモ用の予約データ（SW003・SW006 で共通）
    /// </summary>
    public static class DemoReservations
    {
        public static readonly List<Reservation> Items =
        [
            new(DateTime.Today, "○○事業所", ReservationStatus.Confirmed),
            new(DateTime.Today.AddDays(1), "○○事業所", ReservationStatus.Confirmed),
            new(DateTime.Today.AddDays(2), "○○○○○○○○事業所", ReservationStatus.Reserved),
            new(DateTime.Today.AddDays(3), "○○事業所", ReservationStatus.Rejected),
            new(DateTime.Today.AddDays(5), "○○事業所", ReservationStatus.Reserved),
            new(DateTime.Today.AddDays(7), "○○事業所", ReservationStatus.Reserved),
        ];

        public static readonly string PreferredOffices = string.Join("、", ["○○", "○○", "○○", "○○"]);

        /// <summary>
        /// ステータスごとの件数（予約・確定・NG の順）
        /// </summary>
        public static List<QuickStatusItem> CountByStatus() =>
        [
            new(ReservationStatus.Reserved, Count(ReservationStatus.Reserved)),
            new(ReservationStatus.Confirmed, Count(ReservationStatus.Confirmed)),
            new(ReservationStatus.Rejected, Count(ReservationStatus.Rejected)),
        ];

        private static int Count(ReservationStatus status) => Items.Count(r => r.Status == status);
    }
}
