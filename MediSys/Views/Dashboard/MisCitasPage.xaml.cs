// Views/Dashboard/MisCitasPage.xaml.cs - VERSIÓN COMPLETA
using MediSys.ViewModels;
using MediSys.Services;

namespace MediSys.Views.Dashboard;

public partial class MisCitasPage : ContentPage
{
	private MisCitasViewModel _viewModel;

	public MisCitasPage()
	{
		InitializeComponent();

		// CREAR SERVICIOS MANUALMENTE
		var apiService = Application.Current?.Handler?.MauiContext?.Services?.GetService<MediSysApiService>();
		var authService = new AuthService();

		if (apiService != null)
		{
			_viewModel = new MisCitasViewModel(apiService, authService);
			BindingContext = _viewModel;
		}
		else
		{
			// FALLBACK: crear apiService manualmente si no se encuentra via DI
			_viewModel = new MisCitasViewModel(new MediSysApiService(), authService);
			BindingContext = _viewModel;
		}

		System.Diagnostics.Debug.WriteLine("MisCitasPage initialized with AuthService");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		try
		{
			System.Diagnostics.Debug.WriteLine("MisCitasPage OnAppearing - Iniciando carga");

			if (_viewModel?.InicializarCommand?.CanExecute(null) == true)
			{
				await _viewModel.InicializarCommand.ExecuteAsync(null);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("ERROR: No se pudo ejecutar InicializarCommand");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"ERROR en OnAppearing: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
			await DisplayAlert("Error", $"Error cargando datos: {ex.Message}", "OK");
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		System.Diagnostics.Debug.WriteLine("MisCitasPage OnDisappearing");
	}

	// EVENTO: Cuando cambia la fecha
	private async void OnFechaChanged(object sender, DateChangedEventArgs e)
	{
		try
		{
			System.Diagnostics.Debug.WriteLine($"Fecha cambiada a: {e.NewDate:yyyy-MM-dd}");

			if (_viewModel?.CambiarFechaCommand?.CanExecute(null) == true)
			{
				await _viewModel.CambiarFechaCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"ERROR cambiando fecha: {ex.Message}");
		}
	}

	// EVENTO: Cuando cambia el estado del filtro
	private async void OnEstadoChanged(object sender, EventArgs e)
	{
		try
		{
			if (sender is Picker picker)
			{
				var estadoSeleccionado = picker.SelectedItem?.ToString();
				System.Diagnostics.Debug.WriteLine($"Estado cambiado a: {estadoSeleccionado}");
			}

			if (_viewModel?.CambiarEstadoCommand?.CanExecute(null) == true)
			{
				await _viewModel.CambiarEstadoCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"ERROR cambiando estado: {ex.Message}");
		}
	}

	// MÉTODO ADICIONAL: Para debugging manual
	private async void OnDebugButtonClicked(object sender, EventArgs e)
	{
		try
		{
			if (_viewModel != null)
			{
				System.Diagnostics.Debug.WriteLine("=== DEBUG MANUAL ===");
				System.Diagnostics.Debug.WriteLine($"Citas Count: {_viewModel.Citas.Count}");
				System.Diagnostics.Debug.WriteLine($"TieneCitas: {_viewModel.TieneCitas}");
				System.Diagnostics.Debug.WriteLine($"IsLoading: {_viewModel.IsLoading}");
				System.Diagnostics.Debug.WriteLine($"Usuario: {_viewModel.UsuarioActual?.Nombres}");

				// Forzar refresh
				if (_viewModel.RefrescarCitasCommand.CanExecute(null))
				{
					await _viewModel.RefrescarCitasCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"ERROR en debug: {ex.Message}");
		}
	}
}