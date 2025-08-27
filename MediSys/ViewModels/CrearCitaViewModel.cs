using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class CrearCitaViewModel : ObservableObject
	{
		private static MediSysApiService? _sharedApiService;

		private MediSysApiService ApiService
		{
			get
			{
				if (_sharedApiService == null)
					_sharedApiService = new MediSysApiService();
				return _sharedApiService;
			}
		}

		// ===== CONTROL DE PASOS =====
		[ObservableProperty]
		private int pasoActual = 1;

		[ObservableProperty]
		private string tituloModal = "Crear Nueva Cita - Paso 1 de 4";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool canCrearCita = false;

		// ===== PASO 1: TIPOS DE CITA =====
		[ObservableProperty]
		private ObservableCollection<TipoCita> tiposCita = new();

		[ObservableProperty]
		private TipoCita? tipoCitaSeleccionado;

		// ===== PASO 2: PACIENTE =====
		[ObservableProperty]
		private string cedulaBusqueda = "";

		[ObservableProperty]
		private bool pacienteEncontrado = false;

		[ObservableProperty]
		private bool mostrarFormularioPaciente = false;

		[ObservableProperty]
		private PacienteBusqueda? pacienteSeleccionado;

		// Campos para crear paciente
		[ObservableProperty]
		private string nombresPaciente = "";

		[ObservableProperty]
		private string apellidosPaciente = "";

		[ObservableProperty]
		private string correoPaciente = "";

		[ObservableProperty]
		private string telefonoPaciente = "";

		[ObservableProperty]
		private DateTime fechaNacimientoPaciente = DateTime.Now.AddYears(-30);

		[ObservableProperty]
		private string sexoPaciente = "M";

		[ObservableProperty]
		private string tipoSangrePaciente = "";

		[ObservableProperty]
		private string nacionalidadPaciente = "Ecuatoriana";

		[ObservableProperty]
		private string contactoEmergenciaPaciente = "";

		[ObservableProperty]
		private string telefonoEmergenciaPaciente = "";

		// ===== PASO 3: MÉDICO =====
		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursales = new();

		[ObservableProperty]
		private Sucursal? sucursalSeleccionada;

		[ObservableProperty]
		private ObservableCollection<Especialidad> especialidades = new();

		[ObservableProperty]
		private Especialidad? especialidadSeleccionada;

		[ObservableProperty]
		private ObservableCollection<Doctor> doctores = new();

		[ObservableProperty]
		private Doctor? doctorSeleccionado;

		// ===== PASO 4: HORARIOS =====
		[ObservableProperty]
		private ObservableCollection<SlotHorario> slotsDisponibles = new();

		[ObservableProperty]
		private SlotHorario? slotSeleccionado;

		[ObservableProperty]
		private DateTime semanaActual = DateTime.Now;

		[ObservableProperty]
		private string tituloSemana = "";

		[ObservableProperty]
		private string motivoCita = "";

		[ObservableProperty]
		private string notasCita = "";

		// ===== DATOS ESTÁTICOS =====
		public List<string> SexosDisponibles { get; } = new() { "M", "F" };
		public List<string> TiposSangre { get; } = new() { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Desconocido" };
		public List<string> NacionalidadesDisponibles { get; } = new()
	   {
		   "Ecuatoriana", "Colombiana", "Peruana", "Venezolana", "Argentina",
		   "Chilena", "Brasileña", "Mexicana", "Española", "Estadounidense", "Otra"
	   };

		public CrearCitaViewModel()
		{
			ActualizarTituloSemana();
			_ = InicializarAsync();

			// Suscripciones a cambios
			PropertyChanged += OnPropertyChanged;
		}

		private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(PasoActual):
					ActualizarTitulo();
					break;
				case nameof(SucursalSeleccionada):
					_ = CargarEspecialidadesPorSucursalAsync();
					break;
				case nameof(EspecialidadSeleccionada):
					_ = CargarDoctoresPorEspecialidadAsync();
					break;
				case nameof(DoctorSeleccionado):
					_ = CargarHorariosDisponiblesAsync();
					break;
				case nameof(SlotSeleccionado):
				case nameof(MotivoCita):
					ValidarPuedeCrearCita();
					break;
			}
		}

		private async Task InicializarAsync()
		{
			await CargarTiposCitaAsync();
			await CargarSucursalesAsync();
		}

		private void ActualizarTitulo()
		{
			TituloModal = $"Crear Nueva Cita - Paso {PasoActual} de 4";
		}

		private void ActualizarTituloSemana()
		{
			var inicioSemana = SemanaActual.AddDays(-(int)SemanaActual.DayOfWeek + (int)DayOfWeek.Monday);
			var finSemana = inicioSemana.AddDays(6);
			TituloSemana = $"{inicioSemana:dd/MM} - {finSemana:dd/MM/yyyy}";
		}

		// ===== PASO 1: TIPOS DE CITA =====

		[RelayCommand]
		private async Task CargarTiposCitaAsync()
		{
			try
			{
				IsLoading = true;
				var result = await ApiService.ObtenerTiposCitaAsync();

				if (result.Success && result.Data != null)
				{
					TiposCita.Clear();
					foreach (var tipo in result.Data)
					{
						TiposCita.Add(tipo);
					}

					// Seleccionar presencial por defecto
					TipoCitaSeleccionado = TiposCita.FirstOrDefault(t => t.NombreTipo.ToLower() == "presencial");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error cargando tipos de cita", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task CargarSucursalesAsync()
		{
			try
			{
				IsLoading = true;
				var result = await ApiService.ObtenerSucursalesAsync();

				if (result.Success && result.Data != null)
				{
					Sucursales.Clear();
					foreach (var sucursal in result.Data)
					{
						Sucursales.Add(sucursal);
					}
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error cargando sucursales", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void ContinuarPaso1()
		{
			if (TipoCitaSeleccionado != null)
			{
				PasoActual = 2;
			}
		}

		// ===== PASO 2: BUSCAR/CREAR PACIENTE =====

		[RelayCommand]
		private async Task BuscarPacienteAsync()
		{
			if (string.IsNullOrWhiteSpace(CedulaBusqueda) || CedulaBusqueda.Length != 10)
			{
				await Shell.Current.DisplayAlert("Error", "Ingrese una cédula válida de 10 dígitos", "OK");
				return;
			}

			try
			{
				IsLoading = true;
				var result = await ApiService.BuscarPacientePorCedulaAsync(CedulaBusqueda.Trim());

				if (result.Success && result.Data != null)
				{
					// Paciente encontrado
					PacienteSeleccionado = result.Data;
					PacienteEncontrado = true;
					MostrarFormularioPaciente = false;

					await Shell.Current.DisplayAlert("Paciente Encontrado",
						$"✅ {result.Data.NombreCompleto}\n📧 {result.Data.Correo}\n📱 {result.Data.Telefono}", "OK");
				}
				else
				{
					// Paciente no encontrado
					PacienteEncontrado = false;
					MostrarFormularioPaciente = true;
					PacienteSeleccionado = null;
					LimpiarFormularioPaciente();

					await Shell.Current.DisplayAlert("Paciente No Encontrado",
						"El paciente no está registrado. Complete los datos para crearlo automáticamente.", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error buscando paciente: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task CrearPacienteAsync()
		{
			if (!ValidarDatosPaciente())
				return;

			try
			{
				IsLoading = true;

				var pacienteData = new CrearPacienteRequest
				{
					Cedula = CedulaBusqueda.Trim(),
					Nombres = NombresPaciente.Trim(),
					Apellidos = ApellidosPaciente.Trim(),
					Correo = CorreoPaciente.Trim(),
					Telefono = TelefonoPaciente.Trim(),
					FechaNacimiento = FechaNacimientoPaciente.ToString("yyyy-MM-dd"),
					Sexo = SexoPaciente,
					Nacionalidad = NacionalidadPaciente,
					TipoSangre = TipoSangrePaciente,
					ContactoEmergencia = ContactoEmergenciaPaciente.Trim(),
					TelefonoEmergencia = TelefonoEmergenciaPaciente.Trim()
				};

				var result = await ApiService.CrearPacienteAsync(pacienteData);

				if (result.Success && result.Data != null)
				{
					PacienteSeleccionado = result.Data;
					PacienteEncontrado = true;
					MostrarFormularioPaciente = false;

					await Shell.Current.DisplayAlert("Paciente Creado",
						$"✅ Paciente {result.Data.NombreCompleto} registrado exitosamente", "OK");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error creando el paciente", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void ContinuarPaso2()
		{
			if (PacienteSeleccionado != null)
			{
				PasoActual = 3;
			}
		}

		// ===== PASO 3: SELECCIÓN DE MÉDICO =====

		[RelayCommand]
		private async Task CargarEspecialidadesPorSucursalAsync()
		{
			if (SucursalSeleccionada == null)
				return;

			try
			{
				IsLoading = true;
				var result = await ApiService.ObtenerEspecialidadesPorSucursalAsync(SucursalSeleccionada.IdSucursal);

				if (result.Success && result.Data != null)
				{
					Especialidades.Clear();
					foreach (var especialidad in result.Data)
					{
						Especialidades.Add(especialidad);
					}

					// Limpiar selecciones dependientes
					EspecialidadSeleccionada = null;
					DoctorSeleccionado = null;
					Doctores.Clear();
					SlotsDisponibles.Clear();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error cargando especialidades", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task CargarDoctoresPorEspecialidadAsync()
		{
			if (EspecialidadSeleccionada == null || SucursalSeleccionada == null)
				return;

			try
			{
				IsLoading = true;
				var result = await ApiService.ObtenerDoctoresPorEspecialidadYSucursalAsync(
					EspecialidadSeleccionada.IdEspecialidad,
					SucursalSeleccionada.IdSucursal);

				if (result.Success && result.Data != null)
				{
					Doctores.Clear();
					foreach (var doctor in result.Data)
					{
						Doctores.Add(doctor);
					}

					// Limpiar selecciones dependientes
					DoctorSeleccionado = null;
					SlotsDisponibles.Clear();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error cargando doctores", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void ContinuarPaso3()
		{
			if (DoctorSeleccionado != null)
			{
				PasoActual = 4;
			}
		}

		// ===== PASO 4: HORARIOS =====

		[RelayCommand]
		private async Task CargarHorariosDisponiblesAsync()
		{
			if (DoctorSeleccionado == null || SucursalSeleccionada == null)
				return;

			try
			{
				IsLoading = true;
				var semanaFormato = SemanaActual.ToString("yyyy-MM-dd");
				var result = await ApiService.ObtenerHorariosDisponiblesAsync(
					DoctorSeleccionado.IdDoctor,
					SucursalSeleccionada.IdSucursal,
					semanaFormato);

				if (result.Success && result.Data != null)
				{
					GenerarSlotsDisponibles(result.Data);
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error cargando horarios", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task SemanaAnteriorAsync()
		{
			SemanaActual = SemanaActual.AddDays(-7);
			ActualizarTituloSemana();
			await CargarHorariosDisponiblesAsync();
		}

		[RelayCommand]
		private async Task SemanaSiguienteAsync()
		{
			SemanaActual = SemanaActual.AddDays(7);
			ActualizarTituloSemana();
			await CargarHorariosDisponiblesAsync();
		}

		[RelayCommand]
		private async Task CrearCitaAsync()
		{
			if (!CanCrearCita || SlotSeleccionado == null || PacienteSeleccionado == null ||
				DoctorSeleccionado == null || SucursalSeleccionada == null || TipoCitaSeleccionado == null)
				return;

			try
			{
				IsLoading = true;

				var citaData = new CrearCitaRequest
				{
					IdPaciente = PacienteSeleccionado.IdPaciente,
					IdDoctor = DoctorSeleccionado.IdDoctor,
					IdSucursal = SucursalSeleccionada.IdSucursal,
					IdTipoCita = TipoCitaSeleccionado.IdTipoCita,
					FechaHora = SlotSeleccionado.FechaHora,
					Motivo = MotivoCita.Trim(),
					TipoCita = TipoCitaSeleccionado.NombreTipo.ToLower(),
					Notas = NotasCita?.Trim() ?? ""
				};

				var result = await ApiService.CrearCitaAsync(citaData);

				if (result.Success && result.Data != null)
				{
					await Shell.Current.DisplayAlert("Cita Creada",
						$"✅ Cita programada exitosamente\n" +
						$"📅 {SlotSeleccionado.FechaHora:dd/MM/yyyy HH:mm}\n" +
						$"👤 {PacienteSeleccionado.NombreCompleto}\n" +
						$"👨‍⚕️ Dr. {DoctorSeleccionado.Nombres} {DoctorSeleccionado.Apellidos}",
						"OK");

					// Volver al dashboard
					await Shell.Current.GoToAsync("..");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error creando la cita", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task VolverAsync()
		{
			await Shell.Current.GoToAsync("..");
		}

		// ===== MÉTODOS AUXILIARES =====

		private void GenerarSlotsDisponibles(HorariosDisponiblesResponse horarios)
		{
			SlotsDisponibles.Clear();

			if (horarios.Horarios == null || !horarios.Horarios.Any())
				return;

			var inicioSemana = SemanaActual.AddDays(-(int)SemanaActual.DayOfWeek + (int)DayOfWeek.Monday);

			for (int dia = 0; dia < 7; dia++)
			{
				var fechaActual = inicioSemana.AddDays(dia);
				var diaSemana = (int)fechaActual.DayOfWeek;
				if (diaSemana == 0) diaSemana = 7; // Domingo = 7

				// Buscar horarios para este día
				var horariosDelDia = horarios.Horarios.Where(h => h.DiaSemana == diaSemana).ToList();

				foreach (var horario in horariosDelDia)
				{
					var horaInicio = TimeSpan.Parse(horario.HoraInicio);
					var horaFin = TimeSpan.Parse(horario.HoraFin);
					var duracion = TimeSpan.FromMinutes(horario.DuracionCita);

					var horaActual = horaInicio;
					while (horaActual < horaFin)
					{
						var fechaHoraSlot = fechaActual.Date + horaActual;

						// Verificar si está disponible
						var estaOcupado = horarios.CitasOcupadas?.Any(c =>
							c.Fecha == fechaActual.ToString("yyyy-MM-dd") &&
							c.Hora == horaActual.ToString(@"hh\:mm\:ss")) ?? false;

						// Verificar excepciones
						var tieneExcepcion = horarios.Excepciones?.Any(e =>
							e.Fecha == fechaActual.ToString("yyyy-MM-dd") &&
							(e.Tipo == "no_laborable" || e.Tipo == "vacaciones" || e.Tipo == "feriado")) ?? false;

						// Solo agregar slots futuros
						if (fechaHoraSlot > DateTime.Now)
						{
							var slot = new SlotHorario
							{
								Fecha = fechaActual.ToString("yyyy-MM-dd"),
								Hora = horaActual.ToString(@"hh\:mm"),
								FechaHora = fechaHoraSlot.ToString("yyyy-MM-dd HH:mm:ss"),
								Disponible = !estaOcupado && !tieneExcepcion,
								Motivo = estaOcupado ? "Ocupado" : (tieneExcepcion ? "No disponible" : "")
							};

							SlotsDisponibles.Add(slot);
						}

						horaActual = horaActual.Add(duracion);
					}
				}
			}
		}

		private bool ValidarDatosPaciente()
		{
			var errores = new List<string>();

			if (string.IsNullOrWhiteSpace(NombresPaciente))
				errores.Add("Los nombres son requeridos");

			if (string.IsNullOrWhiteSpace(ApellidosPaciente))
				errores.Add("Los apellidos son requeridos");

			if (string.IsNullOrWhiteSpace(CorreoPaciente) || !CorreoPaciente.Contains("@"))
				errores.Add("Ingrese un correo válido");

			if (string.IsNullOrWhiteSpace(TelefonoPaciente) || TelefonoPaciente.Length < 10)
				errores.Add("Ingrese un teléfono válido");

			if (errores.Any())
			{
				Shell.Current.DisplayAlert("Datos Incompletos",
					"Complete los siguientes campos:\n• " + string.Join("\n• ", errores), "OK");
				return false;
			}

			return true;
		}

		private void ValidarPuedeCrearCita()
		{
			CanCrearCita = SlotSeleccionado != null &&
						  !string.IsNullOrWhiteSpace(MotivoCita) &&
						  PacienteSeleccionado != null &&
						  DoctorSeleccionado != null &&
						  SucursalSeleccionada != null &&
						  TipoCitaSeleccionado != null;
		}

		private void LimpiarFormularioPaciente()
		{
			NombresPaciente = "";
			ApellidosPaciente = "";
			CorreoPaciente = "";
			TelefonoPaciente = "";
			FechaNacimientoPaciente = DateTime.Now.AddYears(-30);
			SexoPaciente = "M";
			TipoSangrePaciente = "";
			NacionalidadPaciente = "Ecuatoriana";
			ContactoEmergenciaPaciente = "";
			TelefonoEmergenciaPaciente = "";
		}
	}
}