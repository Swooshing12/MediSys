using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using MediSys.Views.Dashboard;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class HistorialClinicoViewModel : ObservableObject
	{
		private static MediSysApiService? _sharedApiService;

		// Instancia compartida para mantener la sesión
		private MediSysApiService ApiService
		{
			get
			{
				if (_sharedApiService == null)
					_sharedApiService = new MediSysApiService();
				return _sharedApiService;
			}
		}

		[ObservableProperty]
		private string cedulaBusqueda = "";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool showResults = false;

		[ObservableProperty]
		private bool showFilters = false;

		[ObservableProperty]
		private PacienteResponse? pacienteEncontrado = null;

		[ObservableProperty]
		private ObservableCollection<CitaMedica> citas = new();

		[ObservableProperty]
		private EstadisticasHistorial? estadisticas = null;

		[ObservableProperty]
		private ObservableCollection<Especialidad> especialidades = new();

		[ObservableProperty]
		private ObservableCollection<Doctor> doctores = new();

		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursales = new();

		// Filtros
		[ObservableProperty]
		private DateTime? fechaDesde = null;

		[ObservableProperty]
		private DateTime? fechaHasta = null;

		[ObservableProperty]
		private Especialidad? especialidadSeleccionada = null;

		[ObservableProperty]
		private Doctor? doctorSeleccionado = null;

		[ObservableProperty]
		private string? estadoSeleccionado = null;

		[ObservableProperty]
		private Sucursal? sucursalSeleccionada = null;

		public List<string> Estados { get; } = new() { "Programada", "Completada", "Cancelada", "No asistió" };

		public HistorialClinicoViewModel()
		{
			_ = CargarDatosParaFiltrosAsync();
		}

		private async Task CargarDatosParaFiltrosAsync()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Cargando especialidades...");
				var especialidadesResult = await ApiService.ObtenerEspecialidadesAsync();

				System.Diagnostics.Debug.WriteLine($"Especialidades result: Success={especialidadesResult.Success}, Count={especialidadesResult.Data?.Count ?? 0}");

				if (especialidadesResult.Success && especialidadesResult.Data != null)
				{
					Especialidades.Clear();
					foreach (var especialidad in especialidadesResult.Data)
					{
						System.Diagnostics.Debug.WriteLine($"Adding especialidad: {especialidad.Nombre}");
						Especialidades.Add(especialidad);
					}
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Error especialidades: {especialidadesResult.Message}");
				}

				// Similar para sucursales...
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando filtros: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task CargarDoctoresPorEspecialidadAsync()
		{
			if (EspecialidadSeleccionada == null) return;

			try
			{
				var result = await ApiService.ObtenerDoctoresPorEspecialidadAsync(EspecialidadSeleccionada.IdEspecialidad);
				if (result.Success && result.Data != null)
				{
					Doctores.Clear();
					foreach (var doctor in result.Data)
					{
						Doctores.Add(doctor);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando doctores: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task BuscarHistorialAsync()
		{
			if (string.IsNullOrWhiteSpace(CedulaBusqueda))
			{
				await Shell.Current.DisplayAlert("Error", "Ingrese la cédula del paciente", "OK");
				return;
			}

			IsLoading = true;
			ShowResults = false;

			try
			{
				var pacienteResult = await ApiService.BuscarPacienteAsync(CedulaBusqueda.Trim());

				if (!pacienteResult.Success || pacienteResult.Data == null)
				{
					await Shell.Current.DisplayAlert("Paciente no encontrado",
						pacienteResult.Message ?? "No se encontró un paciente con esa cédula", "OK");
					return;
				}

				PacienteEncontrado = pacienteResult.Data;

				var filtros = new HistorialClinicoFiltros
				{
					FechaDesde = FechaDesde?.ToString("yyyy-MM-dd"),
					FechaHasta = FechaHasta?.ToString("yyyy-MM-dd"),
					IdEspecialidad = EspecialidadSeleccionada?.IdEspecialidad,
					IdDoctor = DoctorSeleccionado?.IdDoctor,
					Estado = EstadoSeleccionado,
					IdSucursal = SucursalSeleccionada?.IdSucursal
				};

				var historialResult = await ApiService.ObtenerHistorialAsync(CedulaBusqueda.Trim(), filtros);

				if (historialResult.Success && historialResult.Data != null)
				{
					Citas.Clear();
					foreach (var cita in historialResult.Data.Citas)
					{
						Citas.Add(cita);
					}

					Estadisticas = historialResult.Data.Estadisticas;
					ShowResults = true;
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						historialResult.Message ?? "Error obteniendo historial clínico", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}



		[RelayCommand]
		private async Task VerDetalleCitaAsync(CitaMedica cita)
		{
			if (cita == null) return;

			var detallePage = new DetalleCitaModalPage(cita);
			await Shell.Current.Navigation.PushModalAsync(detallePage);
		}



		[RelayCommand]
		private void LimpiarFiltros()
		{
			FechaDesde = null;
			FechaHasta = null;
			EspecialidadSeleccionada = null;
			DoctorSeleccionado = null;
			EstadoSeleccionado = null;
			SucursalSeleccionada = null;
			Doctores.Clear();
		}

		[RelayCommand]
		private void ToggleFilters()
		{
			ShowFilters = !ShowFilters;
		}

	}
}