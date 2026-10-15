using Plugin.Maui.Calendar.Interfaces;

namespace shift_pick_app.Models
{
    public sealed class NoDotRegisteredDays<T> : List<T>, IMultiEventDay
    {
        public NoDotRegisteredDays(IEnumerable<T> items) : base(items) { }

        // Plugin.Maui.Calendar 2.0.x でイベントのドットを非表示にするため、意図的に色を空にしている
        public IReadOnlyList<Color> Colors => [];
    }
}
