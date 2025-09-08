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
			if (_viewModel.EspecialidadSeleccionada != null)
			{
				// Limpiar doctor seleccionado
				_viewModel.DoctorSeleccionado = null;

				// Solo cargar doctores si ya hay un paciente buscado
				if (!string.IsNullOrWhiteSpace(_viewModel.CedulaBusqueda) && _viewModel.PacienteEncontrado != null)
				{
					await _viewModel.CargarDoctoresPorEspecialidadCommand.ExecuteAsync(null);
				}
			}
			else
			{
				// Si no hay especialidad, limpiar doctores
				_viewModel.Doctores.Clear();
				_viewModel.DoctorSeleccionado = null;
			}

			// Si hay resultados mostrados, aplicar filtros
			if (_viewModel.ShowResults)
			{
				await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error en OnEspecialidadChanged: {ex.Message}");
		}
	}

	private async void OnDoctorChanged(object sender, EventArgs e)
	{
		try
		{
			if (_viewModel.ShowResults)
			{
				await _viewModel.AplicarFiltrosCommand.ExecuteAsync(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error en OnDoctorChanged: {ex.Message}");
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