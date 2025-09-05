using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class HistorialClinicoPage : ContentPage
{
	private HistorialClinicoViewModel _viewModel;

	public HistorialClinicoPage()
	{
		InitializeComponent();
		_viewModel = new HistorialClinicoViewModel();
		BindingContext = _viewModel;

		System.Diagnostics.Debug.WriteLine("HistorialClinicoPage inicializada");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		try
		{
			System.Diagnostics.Debug.WriteLine("HistorialClinicoPage OnAppearing");

			// Inicializar datos si es necesario
			if (_viewModel?.InicializarCommand?.CanExecute(null) == true)
			{
				await _viewModel.InicializarCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
		}
	}

	// ===== EVENTOS PARA AUTO-APLICAR FILTROS =====
	private async void OnFechaDesdeChanged(object sender, DateChangedEventArgs e)
	{
		try
		{
			if (_viewModel != null && _viewModel.ShowResults)
			{
				System.Diagnostics.Debug.WriteLine($"Fecha desde cambiada a: {e.NewDate:yyyy-MM-dd}");

				// Auto-aplicar filtros cuando hay resultados mostrados
				if (_viewModel.AplicarFiltrosCommand?.CanExecute(null) == true)
				{
					await Task.Delay(300); // Pequeña pausa para UX
					await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error cambiando fecha desde: {ex.Message}");
		}
	}

	private async void OnFechaHastaChanged(object sender, DateChangedEventArgs e)
	{
		try
		{
			if (_viewModel != null && _viewModel.ShowResults)
			{
				System.Diagnostics.Debug.WriteLine($"Fecha hasta cambiada a: {e.NewDate:yyyy-MM-dd}");

				if (_viewModel.AplicarFiltrosCommand?.CanExecute(null) == true)
				{
					await Task.Delay(300);
					await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error cambiando fecha hasta: {ex.Message}");
		}
	}

	private async void OnEspecialidadChanged(object sender, EventArgs e)
	{
		try
		{
			if (sender is Picker picker && _viewModel != null)
			{
				System.Diagnostics.Debug.WriteLine($"Especialidad cambiada");

				// Cargar doctores de la especialidad seleccionada
				if (_viewModel.CargarDoctoresPorEspecialidadCommand?.CanExecute(null) == true)
				{
					await _viewModel.CargarDoctoresPorEspecialidadCommand.ExecuteAsync(null);
				}

				// Auto-aplicar filtros si hay resultados
				if (_viewModel.ShowResults && _viewModel.AplicarFiltrosCommand?.CanExecute(null) == true)
				{
					await Task.Delay(300);
					await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error cambiando especialidad: {ex.Message}");
		}
	}

	private async void OnDoctorChanged(object sender, EventArgs e)
	{
		try
		{
			if (_viewModel != null && _viewModel.ShowResults)
			{
				System.Diagnostics.Debug.WriteLine("Doctor seleccionado cambiado");

				if (_viewModel.AplicarFiltrosCommand?.CanExecute(null) == true)
				{
					await Task.Delay(300);
					await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error cambiando doctor: {ex.Message}");
		}
	}

	private async void OnEstadoChanged(object sender, EventArgs e)
	{
		try
		{
			if (_viewModel != null && _viewModel.ShowResults)
			{
				System.Diagnostics.Debug.WriteLine("Estado seleccionado cambiado");

				if (_viewModel.AplicarFiltrosCommand?.CanExecute(null) == true)
				{
					await Task.Delay(300);
					await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error cambiando estado: {ex.Message}");
		}
	}
}