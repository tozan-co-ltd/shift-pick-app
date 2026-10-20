using CommunityToolkit.Maui.Views;

namespace shift_pick_app.Views.Shared;

public partial class ResultPopup : Popup<bool>
{
    private bool _isClosing;

    public ResultPopup(string title, string message, string primary, string secondary)
    {
        InitializeComponent();
        titleLabel.Text = title;
        messageLabel.Text = message;
        primaryButton.Text = primary;
        secondaryButton.Text = secondary;
    }

    private async void OnPrimaryClicked(object? sender, EventArgs e) => await CloseOnceAsync(true);

    private async void OnSecondaryClicked(object? sender, EventArgs e) => await CloseOnceAsync(false);

    /// <summary>
    /// 連打で2回閉じないようにする
    /// </summary>
    private async Task CloseOnceAsync(bool result)
    {
        if (_isClosing)
            return;

        _isClosing = true;
        await CloseAsync(result);
    }
}
