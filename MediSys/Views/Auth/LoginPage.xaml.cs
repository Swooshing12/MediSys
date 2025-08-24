using MediSys.ViewModels;

namespace MediSys.Views.Auth;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		BindingContext = new LoginViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Focus en el campo de email al aparecer
		EmailEntry.Focus();
	}
}