using MediSys.ViewModels;

namespace MediSys.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
	public ForgotPasswordPage()
	{
		InitializeComponent();
		BindingContext = new ForgotPasswordViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Focus en el campo de email al aparecer
		EmailEntry?.Focus();
	}
}