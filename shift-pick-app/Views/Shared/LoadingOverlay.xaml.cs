namespace shift_pick_app.Views.Shared;

public partial class LoadingOverlay : ContentView
{
	public static readonly BindableProperty MessageProperty = BindableProperty.Create(
		nameof(Message), typeof(string), typeof(LoadingOverlay), string.Empty,
		propertyChanged: (bindable, _, newValue) => ((LoadingOverlay)bindable).ApplyMessage((string)newValue));

	/// <summary>
	/// スピナーの下のヒント（空なら非表示）
	/// </summary>
	public string Message
	{
		get => (string)GetValue(MessageProperty);
		set => SetValue(MessageProperty, value);
	}

	public LoadingOverlay()
	{
		InitializeComponent();
	}

	private void ApplyMessage(string message)
	{
		messageLabel.Text = message;
		messageLabel.IsVisible = !string.IsNullOrEmpty(message);
	}
}
