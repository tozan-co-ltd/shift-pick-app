namespace shift_pick_app.Views.Shared;

public partial class ExpandableLabel : ContentView
{
	public static readonly BindableProperty TextProperty = BindableProperty.Create(
		nameof(Text), typeof(string), typeof(ExpandableLabel), string.Empty,
		propertyChanged: (bindable, _, newValue) => ((ExpandableLabel)bindable).ApplyText((string)newValue));

	private bool _isExpanded;

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public ExpandableLabel()
	{
		InitializeComponent();
	}

	private void ApplyText(string text)
	{
		textLabel.Text = text;
		measureLabel.Text = text;
	}

	/// <summary>
	/// S1：全文の方が高ければ（1行に収まらない）「すべて表示」を出す。文字・幅が変わるたびに再判定
	/// </summary>
	private void OnTextSizeChanged(object? sender, EventArgs e)
	{
		if (!_isExpanded)
			toggleButton.IsVisible = measureLabel.Height > textLabel.Height + 1;
	}

	/// <summary>
	/// S1 ⇔ S2 の切り替え。
	/// MaxLines を -1 にしても TailTruncation のままだと1行になるため、S2 は WordWrap にする
	/// </summary>
	private void OnToggleClicked(object? sender, EventArgs e)
	{
		_isExpanded = !_isExpanded;
		if (_isExpanded)
		{
			textLabel.MaxLines = -1;
			textLabel.LineBreakMode = LineBreakMode.WordWrap;
			toggleButton.Text = "閉じる";
		}
		else
		{
			textLabel.LineBreakMode = LineBreakMode.TailTruncation;
			textLabel.MaxLines = 1;
			toggleButton.Text = "すべて表示";
		}
	}
}
