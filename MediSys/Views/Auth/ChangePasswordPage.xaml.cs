using MediSys.ViewModels;

namespace MediSys.Views.Auth;

public partial class ChangePasswordPage : ContentPage
{
	public ChangePasswordPage()
	{
		InitializeComponent();
		BindingContext = new ChangePasswordViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Focus en el primer campo al aparecer
		CurrentPasswordEntry?.Focus();
	}
}