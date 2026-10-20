namespace shift_pick_app.Views.Shared;

/// <summary>
/// ボタンの文字の左にアイコン（Font Awesome 6 Solid）を付ける
/// 使い方：&lt;Button Text="登録" shared:ButtonIcon.Glyph="&amp;#xe09a;" /&gt;
/// アイコンの色・大きさはボタンの文字に合わせる（無効時の色も追従）
/// </summary>
public static class ButtonIcon
{
	public static readonly BindableProperty GlyphProperty = BindableProperty.CreateAttached(
		"Glyph", typeof(string), typeof(ButtonIcon), null,
		propertyChanged: (bindable, _, newValue) => ApplyGlyph((Button)bindable, (string?)newValue));

	public static string? GetGlyph(BindableObject view) => (string?)view.GetValue(GlyphProperty);

	public static void SetGlyph(BindableObject view, string? value) => view.SetValue(GlyphProperty, value);

	private static void ApplyGlyph(Button button, string? glyph)
	{
		if (string.IsNullOrEmpty(glyph))
		{
			button.ImageSource = null;
			return;
		}

		var icon = new FontImageSource { Glyph = glyph, FontFamily = "FontAwesome6Solid" };
		icon.SetBinding(FontImageSource.ColorProperty, static (Button b) => b.TextColor, source: button);
		icon.SetBinding(FontImageSource.SizeProperty, static (Button b) => b.FontSize, source: button);
		button.ImageSource = icon;
		button.ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 8);
	}
}
