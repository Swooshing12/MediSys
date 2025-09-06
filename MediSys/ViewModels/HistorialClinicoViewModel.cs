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

		// ✅ NUEVAS PROPIEDADES PARA PAGINACIÓN
		[ObservableProperty]
		private PaginacionInfo? paginacionInfo;

		[ObservableProperty]
		private bool tienePaginaAnterior = false;

		[ObservableProperty]
		private bool tienePaginaSiguiente = false;

		[ObservableProperty]
		private string textoPaginacion = "";

		[ObservableProperty]
		private bool mostrarControlesPaginacion = false;

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
				System.Diagnostics.Debug.WriteLine($"🔍 Buscando historial para cédula: {CedulaBusqueda.Trim()}");

				// 1. Buscar paciente primero
				var pacienteResult = await ApiService.BuscarPacienteAsync(CedulaBusqueda.Trim());

				if (!pacienteResult.Success || pacienteResult.Data == null)
				{
					await Shell.Current.DisplayAlert("Paciente no encontrado",
						pacienteResult.Message ?? "No se encontró un paciente con esa cédula", "OK");
					return;
				}

				PacienteEncontrado = pacienteResult.Data;
				System.Diagnostics.Debug.WriteLine($"✅ Paciente encontrado: {PacienteEncontrado.NombreCompleto}");

				// ✅ 2. REINICIAR PAGINACIÓN AL BUSCAR NUEVO PACIENTE
				ReiniciarPaginacion();

				// 3. Buscar historial desde la primera página
				await CargarHistorialConFiltros();

				ShowResults = true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda: {ex.Message}");
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

				System.Diagnostics.Debug.WriteLine("🔧 Aplicando filtros automáticamente...");
				IsLoading = true;

				// ✅ REINICIAR PAGINACIÓN AL APLICAR FILTROS
				ReiniciarPaginacion();

				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error aplicando filtros: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		// ✅ MÉTODO INTERNO MEJORADO PARA CARGAR HISTORIAL CON PAGINACIÓN
		private async Task CargarHistorialConFiltros()
		{
			try
			{
				// ✅ CREAR FILTROS INCLUYENDO PAGINACIÓN
				var filtros = new HistorialClinicoFiltros
				{
					FechaDesde = FechaDesde.ToString("yyyy-MM-dd"),
					FechaHasta = FechaHasta.ToString("yyyy-MM-dd"),
					IdEspecialidad = EspecialidadSeleccionada?.IdEspecialidad,
					IdDoctor = DoctorSeleccionado?.IdDoctor,
					Estado = EstadoSeleccionado == "Todas" ? null : EstadoSeleccionado,
					IdSucursal = SucursalSeleccionada?.IdSucursal,
					// ✅ USAR PAGINACIÓN ACTUAL O COMENZAR EN PÁGINA 1
					Pagina = PaginacionInfo?.PaginaActual ?? 1,
					PorPagina = 10 // ✅ 10 CITAS POR PÁGINA
				};

				System.Diagnostics.Debug.WriteLine($"📊 Filtros aplicados: Página={filtros.Pagina}, Desde={filtros.FechaDesde}, Hasta={filtros.FechaHasta}, Especialidad={EspecialidadSeleccionada?.Nombre}, Estado={filtros.Estado}");

				var historialResult = await ApiService.ObtenerHistorialAsync(CedulaBusqueda.Trim(), filtros);

				if (historialResult.Success && historialResult.Data != null)
				{
					// ✅ ACTUALIZAR CITAS (REEMPLAZAR, NO AGREGAR)
					Citas.Clear();
					if (historialResult.Data.Citas != null)
					{
						foreach (var cita in historialResult.Data.Citas)
						{
							Citas.Add(cita);
						}
					}

					// ✅ ACTUALIZAR ESTADÍSTICAS
					Estadisticas = historialResult.Data.Estadisticas ?? new EstadisticasHistorial();

					// ✅ ACTUALIZAR INFORMACIÓN DE PAGINACIÓN
					PaginacionInfo = historialResult.Data.Paginacion;
					ActualizarEstadoPaginacion();

					System.Diagnostics.Debug.WriteLine($"✅ Historial cargado: {Citas.Count} citas en página {PaginacionInfo?.PaginaActual}/{PaginacionInfo?.TotalPaginas}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo historial: {historialResult.Message}");
					Citas.Clear();
					Estadisticas = new EstadisticasHistorial();
					PaginacionInfo = null;
					ActualizarEstadoPaginacion();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error cargando historial: {ex.Message}");
				throw;
			}
		}

		// ✅ NUEVOS MÉTODOS PARA PAGINACIÓN
		private void ReiniciarPaginacion()
		{
			PaginacionInfo = new PaginacionInfo
			{
				PaginaActual = 1,
				PorPagina = 10
			};
			ActualizarEstadoPaginacion();
		}

		private void ActualizarEstadoPaginacion()
		{
			if (PaginacionInfo != null && PaginacionInfo.TotalRegistros > 0)
			{
				TienePaginaAnterior = PaginacionInfo.TieneAnterior;
				TienePaginaSiguiente = PaginacionInfo.TieneSiguiente;
				TextoPaginacion = $"Página {PaginacionInfo.PaginaActual} de {PaginacionInfo.TotalPaginas} • Mostrando {PaginacionInfo.Desde}-{PaginacionInfo.Hasta} de {PaginacionInfo.TotalRegistros} registros";
				MostrarControlesPaginacion = PaginacionInfo.TotalPaginas > 1;
			}
			else
			{
				TienePaginaAnterior = false;
				TienePaginaSiguiente = false;
				TextoPaginacion = "";
				MostrarControlesPaginacion = false;
			}

			System.Diagnostics.Debug.WriteLine($"📄 Estado paginación: {TextoPaginacion}");
		}

		// ✅ COMANDOS DE NAVEGACIÓN DE PÁGINAS
		[RelayCommand]
		private async Task IrPaginaAnterior()
		{
			if (PaginacionInfo == null || !PaginacionInfo.TieneAnterior || IsLoading)
				return;

			IsLoading = true;

			try
			{
				System.Diagnostics.Debug.WriteLine($"⬅️ Ir a página anterior: {PaginacionInfo.PaginaActual - 1}");
				PaginacionInfo.PaginaActual--;
				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error ir a página anterior: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error al cargar la página anterior", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task IrPaginaSiguiente()
		{
			if (PaginacionInfo == null || !PaginacionInfo.TieneSiguiente || IsLoading)
				return;

			IsLoading = true;

			try
			{
				System.Diagnostics.Debug.WriteLine($"➡️ Ir a página siguiente: {PaginacionInfo.PaginaActual + 1}");
				PaginacionInfo.PaginaActual++;
				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error ir a página siguiente: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error al cargar la página siguiente", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task IrPrimeraPagina()
		{
			if (PaginacionInfo == null || PaginacionInfo.PaginaActual == 1 || IsLoading)
				return;

			IsLoading = true;

			try
			{
				System.Diagnostics.Debug.WriteLine("⏮️ Ir a primera página");
				PaginacionInfo.PaginaActual = 1;
				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error ir a primera página: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error al cargar la primera página", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task IrUltimaPagina()
		{
			if (PaginacionInfo == null || PaginacionInfo.PaginaActual == PaginacionInfo.TotalPaginas || IsLoading)
				return;

			IsLoading = true;

			try
			{
				System.Diagnostics.Debug.WriteLine($"⏭️ Ir a última página: {PaginacionInfo.TotalPaginas}");
				PaginacionInfo.PaginaActual = PaginacionInfo.TotalPaginas;
				await CargarHistorialConFiltros();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error ir a última página: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error al cargar la última página", "OK");
			}
			finally
			{
				IsLoading = false;
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
				System.Diagnostics.Debug.WriteLine("🧹 Limpiando filtros...");

				FechaDesde = DateTime.Today.AddMonths(-6);
				FechaHasta = DateTime.Today;
				EspecialidadSeleccionada = null;
				DoctorSeleccionado = null;
				EstadoSeleccionado = null;
				SucursalSeleccionada = null;

				Doctores.Clear();

				// ✅ REINICIAR PAGINACIÓN AL LIMPIAR FILTROS
				if (ShowResults && !string.IsNullOrWhiteSpace(CedulaBusqueda))
				{
					ReiniciarPaginacion();
					await CargarHistorialConFiltros();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error limpiando filtros: {ex.Message}");
			}
		}

		[RelayCommand]
		private void ToggleFilters()
		{
			ShowFilters = !ShowFilters;
			System.Diagnostics.Debug.WriteLine($"🔧 Filtros mostrados: {ShowFilters}");
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
			// ✅ AGREGAR INFORMACIÓN DE PAGINACIÓN AL DEBUG
			System.Diagnostics.Debug.WriteLine($"PaginaActual: {PaginacionInfo?.PaginaActual ?? 0}");
			System.Diagnostics.Debug.WriteLine($"TotalPaginas: {PaginacionInfo?.TotalPaginas ?? 0}");
			System.Diagnostics.Debug.WriteLine($"TotalRegistros: {PaginacionInfo?.TotalRegistros ?? 0}");
			System.Diagnostics.Debug.WriteLine($"MostrarControlesPaginacion: {MostrarControlesPaginacion}");
			System.Diagnostics.Debug.WriteLine("=====================================");
		}
	}
}