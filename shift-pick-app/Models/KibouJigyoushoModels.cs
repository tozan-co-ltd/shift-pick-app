using CommunityToolkit.Mvvm.ComponentModel;

namespace shift_pick_app.Models
{
    /// <summary>
    /// 都道府県カード
    /// </summary>
    public partial class Prefecture : ObservableObject
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

        [ObservableProperty]
        private bool _isSelected;
    }

    /// <summary>
    /// 希望事業所の1行
    /// </summary>
    public partial class Office : ObservableObject
    {
        public Office(int id, string name, string city, double distanceKm)
        {
            Id = id;
            Name = name;
            City = city;
            DistanceKm = distanceKm;
        }

        public int Id { get; }
        public string Name { get; }
        public string City { get; }
        public double DistanceKm { get; }   // デモ：現在地からの距離（GPS の代わり）

        [ObservableProperty]
        private bool _isSelected;
    }
}
