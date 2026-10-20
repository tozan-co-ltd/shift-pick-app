using shift_pick_app.Models;

namespace shift_pick_app.Views.Shared;

public partial class StatusPill : ContentView
{
	public static readonly BindableProperty StatusProperty = BindableProperty.Create(
		nameof(Status), typeof(ReservationStatus), typeof(StatusPill), ReservationStatus.Reserved,
		propertyChanged: (bindable, _, _) => ((StatusPill)bindable).ApplyStatus());

	public static readonly BindableProperty TextProperty = BindableProperty.Create(
		nameof(Text), typeof(string), typeof(StatusPill), string.Empty,
		propertyChanged: (bindable, _, newValue) => ((StatusPill)bindable).label.Text = (string)newValue);

	public ReservationStatus Status
	{
		get => (ReservationStatus)GetValue(StatusProperty);
		set => SetValue(StatusProperty, value);
	}

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public StatusPill()
	{
		InitializeComponent();
		ApplyStatus();
	}

	/// <summary>
	/// ステータスに合わせてテーマの色とアイコンを設定する
	/// </summary>
	private void ApplyStatus()
	{
		(string background, string text, string glyph) = Status switch
		{
			ReservationStatus.Confirmed => ("SuccessContainer", "OnSuccessContainer", ""),   // circle-check
			ReservationStatus.Rejected => ("ErrorContainer", "OnErrorContainer", ""),        // circle-xmark
			_ => ("SurfaceContainerHighest", "OnSurfaceVariant", ""),                      // hourglass-half
		};
		pill.SetDynamicResource(BackgroundColorProperty, background);
		label.SetDynamicResource(Label.TextColorProperty, text);
		icon.SetDynamicResource(Label.TextColorProperty, text);
		icon.Text = glyph;
	}
}
