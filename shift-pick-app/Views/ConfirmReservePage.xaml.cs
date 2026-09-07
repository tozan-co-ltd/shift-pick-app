using shift_pick_app.Models;
using shift_pick_app.ViewModels;

namespace shift_pick_app.Views;

public partial class ConfirmReservePage : ContentPage
{
	public ConfirmReservePage()
	{
		InitializeComponent();
        BindingContext = new ConfirmReservePageViewModel();
	}
    public async void OnReserveClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new RegisterStoreInPage(0, new List<M_ProductModel>());
    }

    /// <summary>
    /// �߂�{�^������
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
       
        Application.Current.MainPage = new MenuPage();
    }
}