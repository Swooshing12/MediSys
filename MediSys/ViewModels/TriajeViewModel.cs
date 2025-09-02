using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class TriajeViewModel : ObservableObject
	{
		private readonly MediSysApiService _apiService;

		private readonly AuthService _authService;

		[ObservableProperty]
		private bool isLoading = false;

		// Búsqueda de paciente
		[ObservableProperty]
		private string cedulaBusqueda = "";

		[ObservableProperty]
		private bool pacienteEncontrado = false;

		[ObservableProperty]
		private PacienteBusqueda? pacienteSeleccionado;

		// Citas del paciente
		[ObservableProperty]
		private ObservableCollection<CitaDetallada> citasDelDia = new();

		[ObservableProperty]
		private CitaDetallada? citaSeleccionada;

		[ObservableProperty]
		private bool mostrarCitas = false;

		[ObservableProperty]
		private bool mostrarFormularioTriaje = false;

		// Signos vitales
		[ObservableProperty]
		private string temperatura = "";

		[ObservableProperty]
		private string presionSistolica = "";

		[ObservableProperty]
		private string presionDiastolica = "";

		[ObservableProperty]
		private string frecuenciaCardiaca = "";

		[ObservableProperty]
		private string frecuenciaRespiratoria = "";

		[ObservableProperty]
		private string saturacionOxigeno = "";

		// Medidas antropométricas
		[ObservableProperty]
		private string peso = "";

		[ObservableProperty]
		private string talla = "";

		[ObservableProperty]
		private decimal? imcCalculado;

		[ObservableProperty]
		private string categoriaImc = "";

		// Nivel de urgencia
		[ObservableProperty]
		private ObservableCollection<NivelUrgencia> nivelesUrgencia = new();

		[ObservableProperty]
		private NivelUrgencia? nivelUrgenciaSeleccionado;

		[ObservableProperty]
		private string observaciones = "";

		// Estados de validación
		[ObservableProperty]
		private bool puedeGuardarTriaje = false;


		[ObservableProperty]
		private ObservableCollection<string> alertasSignosVitales = new();

		public TriajeViewModel(MediSysApiService apiService, AuthService authService)
		{
			_apiService = apiService;
			_authService = authService;
			InicializarNivelesUrgencia();
			PropertyChanged += OnPropertyChanged;
		}

		private void InicializarNivelesUrgencia()
		{
			NivelesUrgencia = new ObservableCollection<NivelUrgencia>
			{
				new NivelUrgencia { Id = 1, Nombre = "Baja", Descripcion = "No urgente, puede esperar", Color = "#4CAF50", Icono = "🟢" },
				new NivelUrgencia { Id = 2, Nombre = "Media", Descripcion = "Atención en 30-60 minutos", Color = "#FF9800", Icono = "🟡" },
				new NivelUrgencia { Id = 3, Nombre = "Alta", Descripcion = "Atención en 15-30 minutos", Color = "#FF5722", Icono = "🟠" },
				new NivelUrgencia { Id = 4, Nombre = "Crítica", Descripcion = "Atención inmediata", Color = "#F44336", Icono = "🔴" },
				new NivelUrgencia { Id = 5, Nombre = "Emergencia", Descripcion = "Riesgo de vida", Color = "#9C27B0", Icono = "🚨" }
			};

			NivelUrgenciaSeleccionado = NivelesUrgencia.FirstOrDefault(n => n.Id == 2);
		}

		private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Peso) or nameof(Talla):
					CalcularIMC();
					break;
				case nameof(Temperatura) or nameof(FrecuenciaCardiaca) or nameof(SaturacionOxigeno):
					ValidarSignosVitales();
					break;
				case nameof(CitaSeleccionada):
					MostrarFormularioTriaje = CitaSeleccionada != null;
					ValidarFormulario();
					break;
				case nameof(PresionSistolica) or nameof(PresionDiastolica):
					ValidarSignosVitales();
					break;
			}
			ValidarFormulario();
		}

		// En TriajeViewModel.cs - Agregar debug en BuscarPacienteAsync
		[RelayCommand]
		private async Task BuscarPacienteAsync()
		{
			System.Diagnostics.Debug.WriteLine($"🔍 Iniciando búsqueda de cédula: '{CedulaBusqueda}'");

			if (string.IsNullOrWhiteSpace(CedulaBusqueda) || CedulaBusqueda.Length != 10)
			{
				await Shell.Current.DisplayAlert("Error", "Ingrese una cédula válida de 10 dígitos", "OK");
				return;
			}

			try
			{
				IsLoading = true;
				System.Diagnostics.Debug.WriteLine($"🔍 Buscando paciente con cédula: {CedulaBusqueda.Trim()}");

				var result = await _apiService.BuscarPacientePorCedulaAsync(CedulaBusqueda.Trim());

				System.Diagnostics.Debug.WriteLine($"🔍 Resultado búsqueda paciente: Success={result.Success}, Data={result.Data != null}");

				if (result.Success && result.Data != null)
				{
					PacienteSeleccionado = result.Data;
					PacienteEncontrado = true;

					System.Diagnostics.Debug.WriteLine($"✅ Paciente encontrado: {PacienteSeleccionado.NombreCompleto}");

					await CargarCitasDelDiaAsync();
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"❌ Paciente no encontrado: {result.Message}");
					PacienteEncontrado = false;
					MostrarCitas = false;
					PacienteSeleccionado = null;
					await Shell.Current.DisplayAlert("No encontrado",
						"No se encontró un paciente con esta cédula", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Exception en búsqueda: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error buscando paciente: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task CargarCitasDelDiaAsync()
		{
			if (PacienteSeleccionado == null)
			{
				System.Diagnostics.Debug.WriteLine("❌ PacienteSeleccionado es null");
				return;
			}

			try
			{
				IsLoading = true;
				var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");

				System.Diagnostics.Debug.WriteLine($"🔍 Cargando citas para cédula: {PacienteSeleccionado.Cedula}, fecha: {fechaHoy}");

				var result = await _apiService.ObtenerCitasPacientePorFechaAsync(
					PacienteSeleccionado.Cedula.ToString(), fechaHoy);

				System.Diagnostics.Debug.WriteLine($"🔍 Resultado citas: Success={result.Success}, Count={result.Data?.Count ?? 0}");

				if (result.Success && result.Data != null && result.Data.Any())
				{
					CitasDelDia.Clear();
					int citasAgregadas = 0;

					foreach (var cita in result.Data)
					{
						System.Diagnostics.Debug.WriteLine($"📋 Cita ID:{cita.IdCita}, Estado:{cita.Estado}, Triaje:{cita.IdTriage}");

						if (cita.Estado == "Confirmada" && cita.IdTriage == null)
						{
							CitasDelDia.Add(cita);
							citasAgregadas++;
						}
					}

					MostrarCitas = CitasDelDia.Count > 0;

					System.Diagnostics.Debug.WriteLine($"✅ Citas agregadas: {citasAgregadas}, MostrarCitas: {MostrarCitas}");

					if (CitasDelDia.Count == 0)
					{
						await Shell.Current.DisplayAlert("Sin citas",
							"Este paciente no tiene citas confirmadas pendientes de triaje para hoy", "OK");
					}
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"❌ No hay citas o error: {result.Message}");
					MostrarCitas = false;
					await Shell.Current.DisplayAlert("Sin citas",
						"No se encontraron citas para hoy", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Exception cargando citas: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error cargando citas: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void SeleccionarCita(CitaDetallada cita)
		{
			CitaSeleccionada = cita;
		}

		private void CalcularIMC()
		{
			if (decimal.TryParse(Peso, out decimal pesoValue) &&
				int.TryParse(Talla, out int tallaValue) &&
				pesoValue > 0 && tallaValue > 0)
			{
				decimal alturaMetros = tallaValue / 100m;
				ImcCalculado = Math.Round(pesoValue / (alturaMetros * alturaMetros), 2);

				CategoriaImc = ImcCalculado switch
				{
					< 18.5m => "Bajo peso",
					< 25m => "Peso normal",
					< 30m => "Sobrepeso",
					_ => "Obesidad"
				};
			}
			else
			{
				ImcCalculado = null;
				CategoriaImc = "";
			}
		}

		private void ValidarSignosVitales()
		{
			AlertasSignosVitales.Clear();

			// Validar temperatura
			if (decimal.TryParse(Temperatura, out decimal temp))
			{
				if (temp < 35.0m || temp > 42.0m)
					AlertasSignosVitales.Add("⚠️ Temperatura fuera del rango normal (35-42°C)");
				else if (temp < 36.0m || temp > 37.5m)
					AlertasSignosVitales.Add("⚠️ Temperatura ligeramente alterada");
			}

			// Validar frecuencia cardíaca
			if (int.TryParse(FrecuenciaCardiaca, out int fc))
			{
				if (fc < 50 || fc > 120)
					AlertasSignosVitales.Add("⚠️ Frecuencia cardíaca fuera del rango normal (50-120 lpm)");
			}

			// Validar saturación
			if (int.TryParse(SaturacionOxigeno, out int sat))
			{
				if (sat < 95)
					AlertasSignosVitales.Add("🚨 Saturación de oxígeno baja (<95%) - REQUIERE ATENCIÓN");
			}

			// Validar presión arterial
			if (!string.IsNullOrEmpty(PresionSistolica) && !string.IsNullOrEmpty(PresionDiastolica))
			{
				if (int.TryParse(PresionSistolica, out int sistolica) &&
					int.TryParse(PresionDiastolica, out int diastolica))
				{
					if (sistolica > 140 || diastolica > 90)
						AlertasSignosVitales.Add("⚠️ Presión arterial elevada (>140/90)");
					else if (sistolica < 90 || diastolica < 60)
						AlertasSignosVitales.Add("⚠️ Presión arterial baja (<90/60)");
				}
			}
		}

		private void ValidarFormulario()
		{
			PuedeGuardarTriaje = CitaSeleccionada != null &&
								NivelUrgenciaSeleccionado != null &&
								(!string.IsNullOrWhiteSpace(Temperatura) ||
								 !string.IsNullOrWhiteSpace(Peso) ||
								 !string.IsNullOrWhiteSpace(FrecuenciaCardiaca));
		}

		[RelayCommand]
		private async Task GuardarTriajeAsync()
		{
			if (CitaSeleccionada == null || NivelUrgenciaSeleccionado == null)
			{
				await Shell.Current.DisplayAlert("Error", "Debe seleccionar una cita y nivel de urgencia", "OK");
				return;
			}
			try
			{
				IsLoading = true;
				var currentUser = await _authService.GetCurrentUserAsync();

				var request = new CrearTriajeRequest
				{
					IdCita = CitaSeleccionada.IdCita,
					IdEnfermero = currentUser.IdUsuario,
					FechaHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					NivelUrgencia = NivelUrgenciaSeleccionado.Id,
					Temperatura = decimal.TryParse(Temperatura, out var temp) ? temp : null,
					PresionArterial = (!string.IsNullOrEmpty(PresionSistolica) && !string.IsNullOrEmpty(PresionDiastolica))
						? $"{PresionSistolica}/{PresionDiastolica}" : null,
					FrecuenciaCardiaca = int.TryParse(FrecuenciaCardiaca, out var fc) ? fc : null,
					FrecuenciaRespiratoria = int.TryParse(FrecuenciaRespiratoria, out var fr) ? fr : null,
					SaturacionOxigeno = int.TryParse(SaturacionOxigeno, out var sat) ? sat : null,
					Peso = decimal.TryParse(Peso, out var pesoVal) ? pesoVal : null,
					Talla = int.TryParse(Talla, out var tallaVal) ? tallaVal : null,
					Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones.Trim()
				};

				var result = await _apiService.CrearTriajeAsync(request);
				if (result.Success && result.Data != null)
				{
					string mensaje = "✅ Triaje registrado exitosamente";
					if (result.Data.TieneAlertas && result.Data.Alertas?.Any() == true)
					{
						mensaje += "\n\n⚠️ ALERTAS DETECTADAS:\n";
						mensaje += string.Join("\n", result.Data.Alertas);
					}
					if (result.Data.Imc.HasValue)
					{
						mensaje += $"\n\n📊 IMC: {result.Data.Imc:F1} ({result.Data.CategoriaImc})";
					}
					await Shell.Current.DisplayAlert("Triaje Completado", mensaje, "OK");
					LimpiarFormulario();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error", result.Message ?? "Error guardando triaje", "OK");
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
		private void LimpiarFormulario()
		{
			CedulaBusqueda = "";
			PacienteEncontrado = false;
			PacienteSeleccionado = null;
			MostrarCitas = false;
			MostrarFormularioTriaje = false;
			CitaSeleccionada = null;
			CitasDelDia.Clear();

			// Limpiar signos vitales
			Temperatura = "";
			PresionSistolica = "";
			PresionDiastolica = "";
			FrecuenciaCardiaca = "";
			FrecuenciaRespiratoria = "";
			SaturacionOxigeno = "";
			Peso = "";
			Talla = "";
			Observaciones = "";
			ImcCalculado = null;
			CategoriaImc = "";
			AlertasSignosVitales.Clear();

			NivelUrgenciaSeleccionado = NivelesUrgencia.FirstOrDefault(n => n.Id == 2);
		}

		[RelayCommand]
		private async Task CancelarAsync()
		{
			await Shell.Current.GoToAsync("..");
		}
	}
}