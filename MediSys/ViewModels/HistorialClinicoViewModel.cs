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

		// ===== PROPIEDADES PRINCIPALES =====
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

		// ===== PROPIEDADES PARA FILTROS =====
		[ObservableProperty]
		private ObservableCollection<Especialidad> especialidades = new();

		[ObservableProperty]
		private ObservableCollection<Doctor> doctores = new();

		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursales = new();

		[ObservableProperty]
		private ObservableCollection<string> estados = new();

		// ===== FILTROS SELECCIONADOS =====
		// ===== FILTROS SELECCIONADOS =====
		[ObservableProperty]
		private DateTime fechaDesde = DateTime.Today.AddMonths(-1); // ✅ DateTime, no DateTime?

		[ObservableProperty]
		private DateTime fechaHasta = DateTime.Today; // ✅ DateTime, no DateTime?

		[ObservableProperty]
		private Especialidad? especialidadSeleccionada = null;

		[ObservableProperty]
		private Doctor? doctorSeleccionado = null;

		[ObservableProperty]
		private string? estadoSeleccionado = null;

		[ObservableProperty]
		private Sucursal? sucursalSeleccionada = null;



		// ===== CONSTRUCTOR =====
		public HistorialClinicoViewModel()
		{
			// Inicializar estados
			Estados.Add("Todas");
			Estados.Add("Pendiente");
			Estados.Add("Confirmada");
			Estados.Add("En Proceso");
			Estados.Add("Completada");
			Estados.Add("Cancelada");
			Estados.Add("No asistió");

			System.Diagnostics.Debug.WriteLine("HistorialClinicoViewModel inicializado");
		}

		// ===== COMANDO PARA INICIALIZAR DATOS =====
		[RelayCommand]
		private async Task Inicializar()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Inicializando HistorialClinicoViewModel...");

				await CargarEspecialidades();
				await CargarSucursales();

				System.Diagnostics.Debug.WriteLine("Inicialización completada");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error en inicialización: {ex.Message}");
			}
		}

		// ===== COMANDO PRINCIPAL DE BÚSQUEDA =====
		[RelayCommand]
		private async Task BuscarHistorial()
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
				System.Diagnostics.Debug.WriteLine($"Buscando historial para cédula: {CedulaBusqueda.Trim()}");

				// 1. Buscar paciente primero
				var pacienteResult = await ApiService.BuscarPacienteAsync(CedulaBusqueda.Trim());

				if (!pacienteResult.Success || pacienteResult.Data == null)
				{
					await Shell.Current.DisplayAlert("Paciente no encontrado",
						pacienteResult.Message ?? "No se encontró un paciente con esa cédula", "OK");
					return;
				}

				PacienteEncontrado = pacienteResult.Data;
				System.Diagnostics.Debug.WriteLine($"Paciente encontrado: {PacienteEncontrado.NombreCompleto}");

				// 2. Buscar historial con filtros
				await CargarHistorialConFiltros();

				ShowResults = true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error en búsqueda: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		// ===== COMANDO PARA APLICAR FILTROS =====
		[RelayCommand]
		private async Task AplicarFiltros()
		{
			try
			{
				if (!ShowResults || string.IsNullOrWhiteSpace(CedulaBusqueda))
					return;

				System.Diagnostics.Debug.WriteLine("Aplicando filtros automáticamente...");
				IsLoading = true;

				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error aplicando filtros: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		// ===== MÉTODO INTERNO PARA CARGAR HISTORIAL CON FILTROS =====
		// ===== MÉTODO INTERNO PARA CARGAR HISTORIAL CON FILTROS =====
		private async Task CargarHistorialConFiltros()
		{
			try
			{
				var filtros = new HistorialClinicoFiltros
				{
					FechaDesde = FechaDesde.ToString("yyyy-MM-dd"), // ✅ CORREGIDO
					FechaHasta = FechaHasta.ToString("yyyy-MM-dd"), // ✅ CORREGIDO
					IdEspecialidad = EspecialidadSeleccionada?.IdEspecialidad,
					IdDoctor = DoctorSeleccionado?.IdDoctor,
					Estado = EstadoSeleccionado == "Todas" ? null : EstadoSeleccionado,
					IdSucursal = SucursalSeleccionada?.IdSucursal
				};

				System.Diagnostics.Debug.WriteLine($"Filtros aplicados: Desde={filtros.FechaDesde}, Hasta={filtros.FechaHasta}, Especialidad={EspecialidadSeleccionada?.Nombre}, Estado={filtros.Estado}");

				var historialResult = await ApiService.ObtenerHistorialAsync(CedulaBusqueda.Trim(), filtros);

				if (historialResult.Success && historialResult.Data != null)
				{
					// Actualizar citas
					Citas.Clear();
					foreach (var cita in historialResult.Data.Citas)
					{
						Citas.Add(cita);
					}

					// Actualizar estadísticas
					Estadisticas = historialResult.Data.Estadisticas;

					System.Diagnostics.Debug.WriteLine($"Historial cargado: {Citas.Count} citas encontradas");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Error obteniendo historial: {historialResult.Message}");
					Citas.Clear();
					Estadisticas = new EstadisticasHistorial();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando historial: {ex.Message}");
				throw;
			}
		}

		// ===== COMANDO PARA CARGAR DOCTORES POR ESPECIALIDAD =====
		[RelayCommand]
		private async Task CargarDoctoresPorEspecialidad()
		{
			try
			{
				if (EspecialidadSeleccionada != null)
				{
					System.Diagnostics.Debug.WriteLine($"Cargando doctores para especialidad: {EspecialidadSeleccionada.Nombre}");

					// Limpiar doctor seleccionado
					DoctorSeleccionado = null;

					var result = await ApiService.ObtenerDoctoresPorEspecialidadAsync(EspecialidadSeleccionada.IdEspecialidad);

					if (result.Success && result.Data != null)
					{
						Doctores.Clear();
						foreach (var doctor in result.Data)
						{
							Doctores.Add(doctor);
						}

						System.Diagnostics.Debug.WriteLine($"Doctores cargados: {Doctores.Count}");
					}
					else
					{
						System.Diagnostics.Debug.WriteLine($"Error cargando doctores: {result.Message}");
						Doctores.Clear();
					}
				}
				else
				{
					// Si no hay especialidad, limpiar doctores
					Doctores.Clear();
					DoctorSeleccionado = null;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando doctores: {ex.Message}");
				Doctores.Clear();
			}
		}

		// ===== COMANDOS DE NAVEGACIÓN =====
		[RelayCommand]
		private async Task VerDetalleCita(CitaMedica cita)
		{
			if (cita == null) return;

			try
			{
				System.Diagnostics.Debug.WriteLine($"Navegando a detalle de cita ID: {cita.IdCita}");

				var detallePage = new DetalleCitaModalPage(cita);
				await Shell.Current.Navigation.PushModalAsync(detallePage);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error navegando a detalle: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "No se pudo abrir el detalle de la cita", "OK");
			}
		}

		// ===== COMANDOS DE FILTROS =====
		[RelayCommand]
		private async Task LimpiarFiltros()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Limpiando filtros...");

				FechaDesde = DateTime.Today.AddMonths(-6);
				FechaHasta = DateTime.Today;
				EspecialidadSeleccionada = null;
				DoctorSeleccionado = null;
				EstadoSeleccionado = null;
				SucursalSeleccionada = null;

				Doctores.Clear();

				// Re-aplicar búsqueda si hay resultados mostrados
				if (ShowResults && !string.IsNullOrWhiteSpace(CedulaBusqueda))
				{
					await CargarHistorialConFiltros();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error limpiando filtros: {ex.Message}");
			}
		}

		[RelayCommand]
		private void ToggleFilters()
		{
			ShowFilters = !ShowFilters;
			System.Diagnostics.Debug.WriteLine($"Filtros mostrados: {ShowFilters}");
		}

		// ===== MÉTODOS PRIVADOS PARA CARGAR DATOS =====
		private async Task CargarEspecialidades()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Cargando especialidades...");

				var especialidadesResult = await ApiService.ObtenerEspecialidadesAsync();

				if (especialidadesResult.Success && especialidadesResult.Data != null)
				{
					Especialidades.Clear();
					foreach (var especialidad in especialidadesResult.Data)
					{
						Especialidades.Add(especialidad);
					}

					System.Diagnostics.Debug.WriteLine($"Especialidades cargadas: {Especialidades.Count}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Error cargando especialidades: {especialidadesResult.Message}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando especialidades: {ex.Message}");
			}
		}

		private async Task CargarSucursales()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Cargando sucursales...");

				var sucursalesResult = await ApiService.ObtenerSucursalesAsync();

				if (sucursalesResult.Success && sucursalesResult.Data != null)
				{
					Sucursales.Clear();
					foreach (var sucursal in sucursalesResult.Data)
					{
						Sucursales.Add(sucursal);
					}

					System.Diagnostics.Debug.WriteLine($"Sucursales cargadas: {Sucursales.Count}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Error cargando sucursales: {sucursalesResult.Message}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando sucursales: {ex.Message}");
			}
		}

		// ===== MÉTODO PARA DEBUGGING =====
		public void LogCurrentState()
		{
			System.Diagnostics.Debug.WriteLine("=== ESTADO ACTUAL DEL VIEWMODEL ===");
			System.Diagnostics.Debug.WriteLine($"CedulaBusqueda: {CedulaBusqueda}");
			System.Diagnostics.Debug.WriteLine($"ShowResults: {ShowResults}");
			System.Diagnostics.Debug.WriteLine($"ShowFilters: {ShowFilters}");
			System.Diagnostics.Debug.WriteLine($"IsLoading: {IsLoading}");
			System.Diagnostics.Debug.WriteLine($"Citas Count: {Citas.Count}");
			System.Diagnostics.Debug.WriteLine($"Especialidades Count: {Especialidades.Count}");
			System.Diagnostics.Debug.WriteLine($"Doctores Count: {Doctores.Count}");
			System.Diagnostics.Debug.WriteLine($"EspecialidadSeleccionada: {EspecialidadSeleccionada?.Nombre ?? "null"}");
			System.Diagnostics.Debug.WriteLine($"EstadoSeleccionado: {EstadoSeleccionado ?? "null"}");
			System.Diagnostics.Debug.WriteLine("=====================================");
		}
	}
}