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

		// ===== PROPIEDADES PARA LA NUEVA UI - DEFINIDAS MANUALMENTE =====
		private string _progresoTexto = "Seleccione el tipo de cita";
		public string ProgresoTexto
		{
			get => _progresoTexto;
			set => SetProperty(ref _progresoTexto, value);
		}

		private bool _puedeCrearCita = false;
		public bool PuedeCrearCita
		{
			get => _puedeCrearCita;
			set => SetProperty(ref _puedeCrearCita, value);
		}

		private string _estadoTipoCita = "";
		public string EstadoTipoCita
		{
			get => _estadoTipoCita;
			set => SetProperty(ref _estadoTipoCita, value);
		}

		// ===== PROPIEDADES EXISTENTES CON [ObservableProperty] =====
		[ObservableProperty]
		private int pasoActual = 1;

		[ObservableProperty]
		private string tituloModal = "Crear Nueva Cita - Paso 1 de 4";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool canCrearCita = false;

		[ObservableProperty]
		private ObservableCollection<TipoCita> tiposCita = new();

		[ObservableProperty]
		private TipoCita? tipoCitaSeleccionado;

		[ObservableProperty]
		private string cedulaBusqueda = "";

		[ObservableProperty]
		private bool pacienteEncontrado = false;

		[ObservableProperty]
		private bool mostrarFormularioPaciente = false;

		[ObservableProperty]
		private PacienteBusqueda? pacienteSeleccionado;

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

		// ===== NUEVAS PROPIEDADES PARA CITAS VIRTUALES =====
		[ObservableProperty]
		private bool mostrarOpcionesVirtuales = false;

		[ObservableProperty]
		private ObservableCollection<PlataformaVirtual> plataformasVirtuales = new();

		[ObservableProperty]
		private PlataformaVirtual? plataformaSeleccionada;

		[ObservableProperty]
		private string salaVirtual = "";

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
			InicializarPlataformasVirtuales();
			_ = InicializarAsync();
			PropertyChanged += OnPropertyChanged;
		}

		private void InicializarPlataformasVirtuales()
		{
			PlataformasVirtuales = new ObservableCollection<PlataformaVirtual>
			{
				new PlataformaVirtual
				{
					Codigo = "zoom",
					Nombre = "Zoom",
					Icono = "📹",
					Descripcion = "Videollamada con Zoom - Fácil y confiable"
				},
				new PlataformaVirtual
				{
					Codigo = "meet",
					Nombre = "Google Meet",
					Icono = "📱",
					Descripcion = "Google Meet - Gratis para todos"
				},
				new PlataformaVirtual
				{
					Codigo = "teams",
					Nombre = "Microsoft Teams",
					Icono = "💼",
					Descripcion = "Microsoft Teams - Ideal para empresas"
				},
			
			};
		}

		private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(TipoCitaSeleccionado):
					if (TipoCitaSeleccionado != null)
					{
						EstadoTipoCita = "Completado";
						ProgresoTexto = "Busque al paciente por su cédula";

						// Detectar si es cita virtual
						MostrarOpcionesVirtuales = TipoCitaSeleccionado.IdTipoCita == 2;

						if (MostrarOpcionesVirtuales)
						{
							// Seleccionar Zoom por defecto para citas virtuales
							PlataformaSeleccionada = PlataformasVirtuales.FirstOrDefault(p => p.Codigo == "zoom");
						}
						else
						{
							// Limpiar selecciones virtuales si cambia a presencial
							PlataformaSeleccionada = null;
							SalaVirtual = "";
						}
					}
					break;
				case nameof(PacienteSeleccionado):
					if (PacienteSeleccionado != null)
					{
						ProgresoTexto = "Seleccione médico y sucursal";
					}
					break;
				case nameof(DoctorSeleccionado):
					if (DoctorSeleccionado != null)
					{
						ProgresoTexto = "Elija fecha y hora disponible";
						_ = CargarHorariosDisponiblesAsync();
					}
					break;
				case nameof(SlotSeleccionado):
					if (SlotSeleccionado != null)
					{
						ProgresoTexto = "Complete los detalles de la cita";
					}
					ValidarPuedeCrearCita();
					break;
				case nameof(MotivoCita):
					ValidarPuedeCrearCita();
					break;
				case nameof(SucursalSeleccionada):
					_ = CargarEspecialidadesPorSucursalAsync();
					break;
				case nameof(EspecialidadSeleccionada):
					_ = CargarDoctoresPorEspecialidadAsync();
					break;
				case nameof(PasoActual):
					ActualizarTitulo();
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
		private async Task BuscarPacienteAsync()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(CedulaBusqueda))
				{
					await Shell.Current.DisplayAlert("Error", "Ingrese una cédula válida", "OK");
					return;
				}

				IsLoading = true;
				var cedula = CedulaBusqueda.Trim();

				var result = await ApiService.BuscarPacientePorCedulaAsync(cedula);

				if (result.Success && result.Data != null)
				{
					// ✅ PACIENTE ENCONTRADO
					PacienteSeleccionado = result.Data;
					PacienteEncontrado = true;
					MostrarFormularioPaciente = false; // Ocultar formulario de crear paciente

					await Shell.Current.DisplayAlert("Paciente Encontrado",
						$"✅ {result.Data.NombreCompleto}\n📧 {result.Data.Correo}\n📞 {result.Data.Telefono}",
						"OK");
				}
				else
				{
					// ❌ PACIENTE NO ENCONTRADO - ANALIZAR EL TIPO DE ERROR
					PacienteSeleccionado = null;
					PacienteEncontrado = false;

					string mensajeError = result.Message ?? "Error buscando paciente";

					// ✅ VERIFICAR SI ES ERROR DE CÉDULA EXISTENTE CON OTRO ROL
					if (mensajeError.Contains("ya está registrada como") ||
						mensajeError.Contains("ya está registrada en el sistema"))
					{
						// 🚫 CÉDULA PERTENECE A OTRO USUARIO (médico, enfermero, etc.)
						MostrarFormularioPaciente = false; // NO mostrar botón de crear

						await Shell.Current.DisplayAlert("Cédula Ya Registrada",
							$"❌ {mensajeError}\n\n" +
							"No se puede crear un paciente con esta cédula porque ya pertenece a otro usuario del sistema.",
							"Entendido");
					}
					else
					{
						// ✅ CÉDULA NO EXISTE EN EL SISTEMA - PUEDE CREAR PACIENTE
						MostrarFormularioPaciente = true; // Mostrar formulario de crear paciente

						await Shell.Current.DisplayAlert("Paciente No Encontrado",
							$"No se encontró un paciente con la cédula: {cedula}\n\n" +
							"Puede registrar un nuevo paciente con esta cédula.",
							"OK");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error buscando paciente: {ex.Message}");

				PacienteSeleccionado = null;
				PacienteEncontrado = false;
				MostrarFormularioPaciente = false; // Por seguridad, no mostrar formulario en caso de error

				await Shell.Current.DisplayAlert("Error", "Error inesperado buscando paciente", "OK");
			}
			finally
			{
				IsLoading = false;
				ValidarPuedeCrearCita(); // Revalidar estado del formulario
			}
		}

		[RelayCommand]
		private async Task AbrirModalCrearPacienteAsync()
		{
			if (string.IsNullOrWhiteSpace(CedulaBusqueda))
			{
				await Shell.Current.DisplayAlert("Error", "Ingrese una cédula válida", "OK");
				return;
			}

			try
			{
				var modalPage = new Views.Modals.CrearPacienteModalPage(CedulaBusqueda.Trim());
				modalPage.PacienteCreado += OnPacienteCreado;
				await Shell.Current.Navigation.PushModalAsync(modalPage);
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error abriendo modal: {ex.Message}", "OK");
			}
		}

		private void OnPacienteCreado(object? sender, PacienteBusqueda paciente)
		{
			PacienteSeleccionado = paciente;
			PacienteEncontrado = true;
			MostrarFormularioPaciente = false;

			if (sender is Views.Modals.CrearPacienteModalPage modalPage)
			{
				modalPage.PacienteCreado -= OnPacienteCreado;
			}
		}

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

					EspecialidadSeleccionada = null;
					DoctorSeleccionado = null;
					Doctores.Clear();
					SlotsDisponibles.Clear();

					System.Diagnostics.Debug.WriteLine($"✅ Se encontraron {Especialidades.Count} especialidades para {SucursalSeleccionada.Nombre}");
				}
				else
				{
					Especialidades.Clear();
					EspecialidadSeleccionada = null;

					System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron especialidades para {SucursalSeleccionada.Nombre}");
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

					DoctorSeleccionado = null;
					SlotsDisponibles.Clear();
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
			if (PacienteSeleccionado == null || PacienteSeleccionado.IdPaciente <= 0)
			{
				await Shell.Current.DisplayAlert("Error",
					"No hay un paciente seleccionado válido.", "OK");
				return;
			}

			if (DoctorSeleccionado == null || SucursalSeleccionada == null ||
				TipoCitaSeleccionado == null || SlotSeleccionado == null)
			{
				await Shell.Current.DisplayAlert("Error",
					"Complete todos los datos requeridos.", "OK");
				return;
			}

			if (string.IsNullOrWhiteSpace(MotivoCita))
			{
				await Shell.Current.DisplayAlert("Error",
					"El motivo de la cita es obligatorio", "OK");
				return;
			}

			// Validación específica para citas virtuales
			if (TipoCitaSeleccionado.IdTipoCita == 2 && PlataformaSeleccionada == null)
			{
				await Shell.Current.DisplayAlert("Error",
					"Debe seleccionar una plataforma virtual", "OK");
				return;
			}

			try
			{
				IsLoading = true;

				var fechaHoraCompleta = $"{SlotSeleccionado.FechaHora}";

				var citaData = new CrearCitaRequest
				{
					IdPaciente = PacienteSeleccionado.IdPaciente,
					IdDoctor = DoctorSeleccionado.IdDoctor,
					IdSucursal = SucursalSeleccionada.IdSucursal,
					IdTipoCita = TipoCitaSeleccionado.IdTipoCita,
					FechaHora = fechaHoraCompleta,
					Motivo = MotivoCita.Trim(),
					Notas = NotasCita?.Trim(),

					// Datos para citas virtuales
					PlataformaVirtual = PlataformaSeleccionada?.Codigo,
					SalaVirtual = string.IsNullOrWhiteSpace(SalaVirtual) ? null : SalaVirtual.Trim()
				};

				// DEBUG: Mostrar datos que se envían
				System.Diagnostics.Debug.WriteLine($"🔍 ENVIANDO CITA:");
				System.Diagnostics.Debug.WriteLine($"  - Tipo Cita: {citaData.IdTipoCita}");
				System.Diagnostics.Debug.WriteLine($"  - Plataforma: {citaData.PlataformaVirtual}");
				System.Diagnostics.Debug.WriteLine($"  - Sala: {citaData.SalaVirtual}");

				var result = await ApiService.CrearCitaAsync(citaData);

				if (result.Success && result.Data != null)
				{
					// DEBUG: Mostrar respuesta
					System.Diagnostics.Debug.WriteLine($"🔍 RESPUESTA:");
					System.Diagnostics.Debug.WriteLine($"  - Enlace Virtual: {result.Data.EnlaceVirtual}");
					System.Diagnostics.Debug.WriteLine($"  - Plataforma: {result.Data.PlataformaVirtual}");

					string mensajeExito = "✅ Cita creada exitosamente";

					// Información adicional para citas virtuales
					if (TipoCitaSeleccionado.IdTipoCita == 2)
					{
						mensajeExito += $"\n\n📹 Plataforma: {PlataformaSeleccionada?.Nombre}";

						if (!string.IsNullOrEmpty(result.Data.EnlaceVirtual))
						{
							mensajeExito += $"\n🔗 Enlace generado automáticamente";
						}

						mensajeExito += "\n💌 Se enviaron las credenciales por correo";
					}

					mensajeExito += $"\n\n📅 Fecha y hora: {SlotSeleccionado.FechaHora}" +
								   $"\n👤 Paciente: {PacienteSeleccionado.NombreCompleto}" +
								   $"\n🩺 Doctor: Dr. {DoctorSeleccionado.Nombres} {DoctorSeleccionado.Apellidos}" +
								   $"\n🏥 Sucursal: {SucursalSeleccionada.Nombre}";

					await Shell.Current.DisplayAlert("¡Éxito!", mensajeExito, "OK");

					ResetFormulario();
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
				System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void SeleccionarDoctor(Doctor doctor)
		{
			DoctorSeleccionado = doctor;
		}

		[RelayCommand]
		private void SeleccionarTipoCita(object parametro)
		{
			if (int.TryParse(parametro.ToString(), out int tipoId))
			{
				TipoCitaSeleccionado = TiposCita?.FirstOrDefault(t => t.IdTipoCita == tipoId);
			}
		}

		[RelayCommand]
		private async Task CancelarAsync()
		{
			var confirmacion = await Shell.Current.DisplayAlert("Cancelar",
				"¿Desea cancelar la creación de la cita?", "Sí", "No");

			if (confirmacion)
			{
				ResetFormulario();
				await Shell.Current.GoToAsync("..");
			}
		}

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
				if (diaSemana == 0) diaSemana = 7;

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

						var estaOcupado = horarios.CitasOcupadas?.Any(c =>
							c.Fecha == fechaActual.ToString("yyyy-MM-dd") &&
							c.Hora == horaActual.ToString(@"hh\:mm\:ss")) ?? false;

						var tieneExcepcion = horarios.Excepciones?.Any(e =>
							e.Fecha == fechaActual.ToString("yyyy-MM-dd") &&
							(e.Tipo == "no_laborable" || e.Tipo == "vacaciones" || e.Tipo == "feriado")) ?? false;

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

		private void ValidarPuedeCrearCita()
		{
			bool validacionBasica = TipoCitaSeleccionado != null &&
									PacienteSeleccionado != null &&
									DoctorSeleccionado != null &&
									SucursalSeleccionada != null &&
									SlotSeleccionado != null &&
									!string.IsNullOrWhiteSpace(MotivoCita);

			// Validación adicional para citas virtuales
			bool validacionVirtual = true;
			if (TipoCitaSeleccionado?.IdTipoCita == 2)
			{
				validacionVirtual = PlataformaSeleccionada != null;
			}

			PuedeCrearCita = validacionBasica && validacionVirtual;
		}

		private async Task ResetFormulario()
		{
			PasoActual = 1;
			TituloModal = "Crear Nueva Cita - Paso 1 de 4";
			ProgresoTexto = "Seleccione el tipo de cita";
			PuedeCrearCita = false;
			EstadoTipoCita = "";

			CedulaBusqueda = "";
			PacienteEncontrado = false;
			MostrarFormularioPaciente = false;
			PacienteSeleccionado = null;

			SucursalSeleccionada = null;
			EspecialidadSeleccionada = null;
			DoctorSeleccionado = null;
			SlotSeleccionado = null;

			// Limpiar opciones virtuales
			MostrarOpcionesVirtuales = false;
			PlataformaSeleccionada = null;
			SalaVirtual = "";

			TiposCita.Clear();
			Sucursales.Clear();
			Especialidades.Clear();
			Doctores.Clear();
			SlotsDisponibles.Clear();

			SemanaActual = DateTime.Now;
			ActualizarTituloSemana();

			MotivoCita = "";
			NotasCita = "";

			await CargarTiposCitaAsync();
			await CargarSucursalesAsync();
		}
	}
}