using CommunityToolkit.Mvvm.ComponentModel;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 都道府県カード
    /// </summary>
    public class Prefecture
    {
        public Prefecture(string name, string image, bool isNameVisible)
        {
            Name = name;
            Image = image;
            IsNameVisible = isNameVisible;
        }

        public string Name { get; }
        public string Image { get; }
        public bool IsNameVisible { get; }
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
            City = info.City;
            DistanceKm = info.DistanceKm;
        }

        public int Id { get; }
        public string Name { get; }
        public string City { get; }
        public double DistanceKm { get; }   // デモ：現在地からの距離（GPS の代わり）
        public string DistanceText => $"{DistanceKm:0.0}km";

        [ObservableProperty]
        private bool _isSelected;
    }
}
