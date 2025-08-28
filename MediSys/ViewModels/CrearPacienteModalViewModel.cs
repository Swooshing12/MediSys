using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class CrearPacienteModalViewModel : ObservableObject
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

		public event EventHandler<PacienteBusqueda>? PacienteCreado;
		public event EventHandler? CerrarModal;

		// ===== PROPIEDADES - CORREGIDAS =====
		[ObservableProperty]
		private string cedulaPaciente = "";

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

		[ObservableProperty]
		private bool isLoading = false;

		// ===== LISTAS ESTÁTICAS =====
		public List<string> SexosDisponibles { get; } = new() { "M", "F" };
		public List<string> TiposSangre { get; } = new()
		{
			"A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Desconocido"
		};
		public List<string> NacionalidadesDisponibles { get; } = new()
		{
			"Ecuatoriana", "Colombiana", "Peruana", "Venezolana", "Argentina",
			"Chilena", "Brasileña", "Mexicana", "Española", "Estadounidense", "Otra"
		};

		public CrearPacienteModalViewModel(string cedula)
		{
			CedulaPaciente = cedula;
		}

		[RelayCommand]
		private async Task CrearPacienteAsync()
		{
			if (!ValidarDatos())
				return;

			try
			{
				IsLoading = true;

				// Convertir cédula string a long
				if (!long.TryParse(CedulaPaciente.Trim(), out long cedulaLong))
				{
					await Shell.Current.DisplayAlert("Error", "La cédula debe contener solo números", "OK");
					return;
				}

				var pacienteData = new CrearPacienteRequest
				{
					Cedula = cedulaLong,
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

				System.Diagnostics.Debug.WriteLine($"Enviando datos del paciente: {System.Text.Json.JsonSerializer.Serialize(pacienteData)}");

				var result = await ApiService.CrearPacienteAsync(pacienteData);

				if (result.Success && result.Data != null)
				{
					await Shell.Current.DisplayAlert("¡Paciente Registrado!",
						$"✅ {result.Data.NombreCompleto} ha sido registrado exitosamente", "OK");

					// ✅ NOTIFICAR QUE EL PACIENTE FUE CREADO
					PacienteCreado?.Invoke(this, result.Data);

					// ✅ CERRAR EL MODAL
					CerrarModal?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error registrando el paciente", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error creando paciente: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void Cancelar()
		{
			CerrarModal?.Invoke(this, EventArgs.Empty);
		}

		private bool ValidarDatos()
		{
			var errores = new List<string>();

			if (string.IsNullOrWhiteSpace(NombresPaciente))
				errores.Add("• Los nombres son obligatorios");

			if (string.IsNullOrWhiteSpace(ApellidosPaciente))
				errores.Add("• Los apellidos son obligatorios");

			if (string.IsNullOrWhiteSpace(TelefonoPaciente))
				errores.Add("• El teléfono es obligatorio");

			if (!string.IsNullOrWhiteSpace(CorreoPaciente) && !CorreoPaciente.Contains("@"))
				errores.Add("• El formato del correo es inválido");

			if (FechaNacimientoPaciente > DateTime.Now.AddYears(-1))
				errores.Add("• La fecha de nacimiento no es válida");

			if (errores.Any())
			{
				Shell.Current.DisplayAlert("Datos Incompletos",
					"Por favor corrija los siguientes errores:\n\n" + string.Join("\n", errores), "OK");
				return false;
			}

			return true;
		}
	}
}