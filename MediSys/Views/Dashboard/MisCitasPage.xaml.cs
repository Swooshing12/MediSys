using MediSys.ViewModels;
using MediSys.Services;

namespace MediSys.Views.Dashboard;

public partial class MisCitasPage : ContentPage
{
	private MisCitasViewModel _viewModel;

	public MisCitasPage()
	{
		InitializeComponent();

		// ✅ CREAR SERVICIOS MANUALMENTE
		var apiService = Application.Current?.Handler?.MauiContext?.Services?.GetService<MediSysApiService>();
		var authService = new AuthService(); // O también via DI si está registrado

		if (apiService != null)
		{
			_viewModel = new MisCitasViewModel(apiService, authService);
			BindingContext = _viewModel;
		}

		System.Diagnostics.Debug.WriteLine("🩺 MisCitasPage initialized with AuthService");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		try
		{
			if (_viewModel?.InicializarCommand?.CanExecute(null) == true)
			{
				await _viewModel.InicializarCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"❌ Error en OnAppearing: {ex.Message}");
			await DisplayAlert("Error", $"Error cargando datos: {ex.Message}", "OK");
		}
	}

	// Resto de métodos igual...
	private async void OnFechaChanged(object sender, DateChangedEventArgs e)
	{
		if (_viewModel?.CambiarFechaCommand?.CanExecute(null) == true)
			await _viewModel.CambiarFechaCommand.ExecuteAsync(null);
	}

	private async void OnEstadoChanged(object sender, EventArgs e)
	{
		if (_viewModel?.CambiarEstadoCommand?.CanExecute(null) == true)
			await _viewModel.CambiarEstadoCommand.ExecuteAsync(null);
	}
}