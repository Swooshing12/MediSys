using MediSys.ViewModels;

namespace MediSys.Views.Auth;

public partial class LoginPage : ContentPage
{
	private readonly LoginViewModel _viewModel;

	public LoginPage()
	{
		InitializeComponent();
		_viewModel = new LoginViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Resetear formulario cuando aparece la página
		_viewModel?.ResetForm();

		// Focus en el primer campo de entrada después de un pequeño delay
		// para asegurar que la UI esté completamente renderizada
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
		{
			try
			{
				// Buscar el Entry del email por su nombre en el XAML
				var emailEntry = this.FindByName<Entry>("EmailEntry");
				emailEntry?.Focus();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error setting focus: {ex.Message}");
			}
		});
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Limpiar focus cuando se navega away
		try
		{
			var emailEntry = this.FindByName<Entry>("EmailEntry");
			var passwordEntry = this.FindByName<Entry>("PasswordEntry");

			emailEntry?.Unfocus();
			passwordEntry?.Unfocus();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error removing focus: {ex.Message}");
		}
	}

	// Método para limpiar recursos si es necesario
	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		// Asegurar que el formulario se resetee al navegar away
		_viewModel?.ResetForm();
	}
}