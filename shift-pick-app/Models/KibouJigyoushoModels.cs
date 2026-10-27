using CommunityToolkit.Mvvm.ComponentModel;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 都道府県カード
    /// </summary>
    public class Prefecture
    {
        public Prefecture(int id, string name, string image, bool isAvailable)
        {
            Id = id;
            Name = name;
            Image = image;
            IsAvailable = isAvailable;
        }

        public int Id { get; }   // 都道府県コード（JIS X 0401）
        public string Name { get; }
        public string Image { get; }
        public bool IsAvailable { get; }   // 事業所あり
    }

    /// <summary>
    /// 地方（都道府県カードのグループ見出し）
    /// </summary>
    public class PrefectureGroup : List<Prefecture>
    {
        public PrefectureGroup(string name, IEnumerable<Prefecture> prefectures) : base(prefectures)
        {
            Name = name;
        }

        public string Name { get; }
    }

    /// <summary>
    /// 希望事業所の1行
    /// </summary>
    public partial class Office : ObservableObject
    {
        public Office(OfficeInfo info)
        {
            Id = info.Id;
            Name = info.Name;
            PrefectureId = info.PrefectureId;
            City = info.City;
            Location = new Location(info.Latitude, info.Longitude);
        }

        public int Id { get; }
        public string Name { get; }
        public int PrefectureId { get; }
        public string City { get; }
        public Location Location { get; }

        // 現在地からの直線距離（現在地の取得前は null）
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DistanceText))]
        private double? _distanceKm;

        public string DistanceText => DistanceKm is double km ? $"{km:0.0}km" : "—";

        [ObservableProperty]
        private bool _isSelected;
    }
}
