// ViewModels/MisCitasViewModel.cs - VERSION CORREGIDA
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using MediSys.Views.Dashboard;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MediSys.ViewModels
{
	public partial class MisCitasViewModel : ObservableObject
	{
		private readonly MediSysApiService _apiService;

		private readonly AuthService _authService;


		[ObservableProperty]
		private ObservableCollection<CitaConsultaMedica> citas = new();

		[ObservableProperty]
		private bool isLoading;

		[ObservableProperty]
		private bool isRefreshing;

		[ObservableProperty]
		private DateTime fechaSeleccionada = DateTime.Today;

		[ObservableProperty]
		private string estadoFiltro = "Confirmada";

		[ObservableProperty]
		private string mensajeVacio = "No hay citas programadas para esta fecha";

		[ObservableProperty]
		private bool tieneCitas;

		// Estadísticas rápidas
		[ObservableProperty]
		private int totalCitas;

		[ObservableProperty]
		private int citasPendientes;

		[ObservableProperty]
		private int citasCompletadas;

		// Usuario actual
		[ObservableProperty]
		private User? usuarioActual;

		public List<string> EstadosDisponibles { get; } = new()
		{
			"Confirmada",
			"Pendiente",
			"En Proceso",
			"Completada",
			"Todas"
		};

		public MisCitasViewModel(MediSysApiService apiService, AuthService authService)
		{
			_apiService = apiService;
			_authService = authService;
		}

		// ✅ CONSTRUCTOR ALTERNATIVO si se crea manualmente
		public MisCitasViewModel(MediSysApiService apiService)
		{
			_apiService = apiService;
			_authService = new AuthService(); // Crear instancia manual
		}

		[RelayCommand]
		private async Task Inicializar()
		{
			await CargarUsuarioActual();
			if (UsuarioActual != null)
			{
				await CargarCitas();
			}
		}

		private async Task CargarUsuarioActual()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🔍 Cargando usuario actual usando AuthService...");

				// ✅ USAR AuthService en lugar de SecureStorage directo
				UsuarioActual = await _authService.GetCurrentUserAsync();

				if (UsuarioActual != null)
				{
					System.Diagnostics.Debug.WriteLine($"✅ Usuario cargado exitosamente:");
					System.Diagnostics.Debug.WriteLine($"  - Nombres: {UsuarioActual.Nombres}");
					System.Diagnostics.Debug.WriteLine($"  - Apellidos: {UsuarioActual.Apellidos}");
					System.Diagnostics.Debug.WriteLine($"  - Cédula: {UsuarioActual.Cedula}");
					System.Diagnostics.Debug.WriteLine($"  - Rol: {UsuarioActual.Rol}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("❌ No hay usuario en AuthService");
					await Shell.Current.DisplayAlert("Sesión Expirada", "Debes iniciar sesión nuevamente", "OK");
					await Shell.Current.GoToAsync("//login");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error cargando usuario: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error accediendo a los datos de usuario", "OK");
				await Shell.Current.GoToAsync("//login");
			}
		}

		[RelayCommand]
		private async Task CargarCitas()
		{
			if (IsLoading) return;

			try
			{
				IsLoading = true;

				// Verificar que tengamos usuario
				if (UsuarioActual == null)
				{
					System.Diagnostics.Debug.WriteLine("❌ UsuarioActual es null, intentando recargar...");
					await CargarUsuarioActual();

					if (UsuarioActual == null)
					{
						System.Diagnostics.Debug.WriteLine("❌ No se pudo cargar el usuario, abortando");
						return;
					}
				}

				// Verificar que el usuario sea médico
				if (UsuarioActual.Rol?.ToLower() != "medico")
				{
					System.Diagnostics.Debug.WriteLine($"❌ Usuario no es médico. Rol: '{UsuarioActual.Rol}'");
					await Shell.Current.DisplayAlert("Error", "Solo los médicos pueden acceder a esta funcionalidad", "OK");
					return;
				}

				var cedula = UsuarioActual.Cedula.ToString();
				var fechaFormateada = FechaSeleccionada.ToString("yyyy-MM-dd");

				System.Diagnostics.Debug.WriteLine($"🩺 Llamando API con parámetros:");
				System.Diagnostics.Debug.WriteLine($"  - Cédula doctor: {cedula}");
				System.Diagnostics.Debug.WriteLine($"  - Fecha: {fechaFormateada}");
				System.Diagnostics.Debug.WriteLine($"  - Estado filtro: {EstadoFiltro}");

				var response = await _apiService.ObtenerCitasConsultaDoctorAsync(cedula, fechaFormateada, EstadoFiltro);

				System.Diagnostics.Debug.WriteLine($"📥 Respuesta de la API:");
				System.Diagnostics.Debug.WriteLine($"  - Success: {response?.Success}");
				System.Diagnostics.Debug.WriteLine($"  - Message: {response?.Message}");
				System.Diagnostics.Debug.WriteLine($"  - Data is null: {response?.Data == null}");
				System.Diagnostics.Debug.WriteLine($"  - Data count: {response?.Data?.Count ?? 0}");

				if (response?.Success == true && response.Data != null)
				{
					Citas.Clear();
					var citasOrdenadas = response.Data.OrderBy(c => c.FechaHoraParsed).ToList();

					foreach (var cita in citasOrdenadas)
					{
						Citas.Add(cita);
						System.Diagnostics.Debug.WriteLine($"📋 Cita agregada: {cita.Paciente.NombreCompleto} - {cita.HoraDisplay}");
					}

					TieneCitas = Citas.Any();
					ActualizarEstadisticas();

					MensajeVacio = EstadoFiltro == "Todas"
						? $"No hay citas para el {FechaSeleccionada:dd/MM/yyyy}"
						: $"No hay citas '{EstadoFiltro}' para el {FechaSeleccionada:dd/MM/yyyy}";

					System.Diagnostics.Debug.WriteLine($"✅ {Citas.Count} citas cargadas exitosamente");
				}
				else
				{
					Citas.Clear();
					TieneCitas = false;
					MensajeVacio = response?.Message ?? "No se encontraron citas";
					System.Diagnostics.Debug.WriteLine($"❌ Sin citas o error: {response?.Message}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Excepción en CargarCitas:");
				System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");

				Citas.Clear();
				TieneCitas = false;
				MensajeVacio = "Error de conexión";
				await Shell.Current.DisplayAlert("Error", $"Error al cargar las citas: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
				IsRefreshing = false;
			}
		}

		[RelayCommand]
		private async Task RefrescarCitas()
		{
			IsRefreshing = true;
			await CargarCitas();
		}

		[RelayCommand]
		private async Task CambiarFecha()
		{
			System.Diagnostics.Debug.WriteLine($"📅 Cambiando fecha a: {FechaSeleccionada:yyyy-MM-dd}");
			await CargarCitas();
		}

		[RelayCommand]
		private async Task CambiarEstado()
		{
			System.Diagnostics.Debug.WriteLine($"📊 Cambiando estado a: {EstadoFiltro}");
			await CargarCitas();
		}

		[RelayCommand]
		private async Task IniciarConsulta(CitaConsultaMedica cita)
		{
			if (cita == null) return;

			try
			{
				System.Diagnostics.Debug.WriteLine($"🩺 Iniciando consulta para cita: {cita.IdCita}");
				var response = await _apiService.ActualizarEstadoCitaAsync(cita.IdCita, "En Proceso");

				if (response?.Success == true)
				{
					await Shell.Current.GoToAsync($"consulta-medica?idCita={cita.IdCita}");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error", response?.Message ?? "No se pudo cambiar el estado de la cita", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error iniciando consulta: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		// En MisCitasViewModel.cs - CORREGIR ESTE MÉTODO
		// En MisCitasViewModel o donde sea que navegues a la vista
		[RelayCommand]
		private async Task VerDetalleCita(CitaConsultaMedica cita)
		{
			try
			{
				var detallePage = new DetalleCitaMedicaPage(cita);
				await Shell.Current.Navigation.PushModalAsync(new NavigationPage(detallePage));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error abriendo detalle: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error abriendo detalle: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task EditarConsulta(CitaConsultaMedica cita)
		{
			if (cita?.TieneConsulta != true) return;

			try
			{
				System.Diagnostics.Debug.WriteLine($"✏️ Editando consulta para cita: {cita.IdCita}");

				// Navegar a editar consulta existente
				await Shell.Current.GoToAsync($"consulta-medica?idCita={cita.IdCita}&modo=editar");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error navegando a editar: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task MarcarComoNoAsistio(CitaConsultaMedica cita)
		{
			if (cita == null) return;

			try
			{
				var confirmar = await Shell.Current.DisplayAlert(
					"Confirmar",
					$"¿Marcar como 'No asistió' la cita de {cita.Paciente.NombreCompleto}?",
					"Sí", "Cancelar");

				if (!confirmar) return;

				System.Diagnostics.Debug.WriteLine($"❌ Marcando como no asistió: {cita.IdCita}");

				var response = await _apiService.ActualizarEstadoCitaAsync(cita.IdCita, "No asistió");

				if (response?.Success == true)
				{
					await CargarCitas(); // Recargar para actualizar la lista
					await Shell.Current.DisplayAlert("Éxito", "Cita marcada como 'No asistió'", "OK");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error", response?.Message ?? "Error actualizando la cita", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error marcando no asistió: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task FiltrarUrgentes()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🚨 Filtrando citas urgentes");

				// Filtrar solo citas urgentes (nivel 3+) de las ya cargadas
				var citasOriginales = Citas.ToList();
				var citasUrgentes = citasOriginales.Where(c => c.EsUrgente).ToList();

				Citas.Clear();
				foreach (var cita in citasUrgentes)
				{
					Citas.Add(cita);
				}

				TieneCitas = Citas.Any();
				MensajeVacio = "No hay citas urgentes para esta fecha";
				ActualizarEstadisticas();

				System.Diagnostics.Debug.WriteLine($"🚨 {Citas.Count} citas urgentes encontradas");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error filtrando urgentes: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task LimpiarFiltros()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🔄 Limpiando filtros");

				EstadoFiltro = "Confirmada";
				FechaSeleccionada = DateTime.Today;
				await CargarCitas();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error limpiando filtros: {ex.Message}");
			}
		}

		private void ActualizarEstadisticas()
		{
			TotalCitas = Citas.Count;
			CitasPendientes = Citas.Count(c => c.Estado is "Pendiente" or "Confirmada");
			CitasCompletadas = Citas.Count(c => c.Estado == "Completada");

			System.Diagnostics.Debug.WriteLine($"📊 Estadísticas: Total={TotalCitas}, Pendientes={CitasPendientes}, Completadas={CitasCompletadas}");
		}

		// Método helper para debug
		public void DebugCitas()
		{
			System.Diagnostics.Debug.WriteLine($"🔍 DEBUG - Total citas: {Citas.Count}");
			foreach (var cita in Citas)
			{
				System.Diagnostics.Debug.WriteLine($"  - {cita.Paciente.NombreCompleto} ({cita.Estado}) - {cita.HoraDisplay}");
			}
		}
	}
}