using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class RegistrarMedicoViewModel : ObservableObject
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

		// ===== DATOS DEL MÉDICO =====
		[ObservableProperty]
		private string cedula = "";

		[ObservableProperty]
		private string nombres = "";

		[ObservableProperty]
		private string apellidos = "";

		[ObservableProperty]
		private string correo = "";

		[ObservableProperty]
		private string sexo = "M";

		[ObservableProperty]
		private string nacionalidad = "Ecuatoriana";

		[ObservableProperty]
		private string? tituloProfesional = "";

		[ObservableProperty]
		private Especialidad? especialidadSeleccionada;

		// ✅ CORREGIDO: Igual que CrearCitaViewModel
		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursales = new();

		[ObservableProperty]
		private Sucursal? sucursalSeleccionada;

		// ✅ ESTA ES LA CLAVE: usar 'especialidades' (minúscula) como en CrearCitaViewModel
		[ObservableProperty]
		private ObservableCollection<Especialidad> especialidades = new();

		// ===== HORARIOS =====
		[ObservableProperty]
		private ObservableCollection<HorarioCrear> horarios = new();

		// ===== CONTROL DE ESTADO =====
		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool showHorarios = false;

		[ObservableProperty]
		private bool canSave = false;

		// ===== SEXOS DISPONIBLES =====
		public List<string> SexosDisponibles { get; } = new() { "M", "F" };

		public List<string> SexosDisplay { get; } = new() { "👨 Masculino", "👩 Femenino" };

		// ===== NACIONALIDADES =====
		public List<string> NacionalidadesDisponibles { get; } = new()
		{
			"Ecuatoriana", "Colombiana", "Peruana", "Venezolana", "Argentina",
			"Chilena", "Brasileña", "Mexicana", "Española", "Estadounidense", "Otra"
		};

		public RegistrarMedicoViewModel()
		{
			// 🔥 SUSCRIBIRSE A CAMBIOS
			Horarios.CollectionChanged += (s, e) => {
				System.Diagnostics.Debug.WriteLine($"🔥 Horarios.CollectionChanged: Count = {Horarios.Count}");
				ShowHorarios = Horarios.Count > 0;
				OnPropertyChanged(nameof(ShowHorarios));
				ValidarDatos();
			};

			// ✅ AGREGAR EL PropertyChanged COMO EN CrearCitaViewModel
			PropertyChanged += OnPropertyChanged;

			_ = CargarDatosInicialesAsync();
		}

		// ✅ EVENTO PropertyChanged EXACTO COMO CrearCitaViewModel
		private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(SucursalSeleccionada):
					_ = CargarEspecialidadesPorSucursalAsync();
					break;
				case nameof(EspecialidadSeleccionada):
				case nameof(Cedula):
				case nameof(Nombres):
				case nameof(Apellidos):
				case nameof(Correo):
					ValidarDatos();
					break;
			}
		}

		private async Task CargarDatosInicialesAsync()
		{
			IsLoading = true;
			try
			{
				// ✅ SOLO CARGAR SUCURSALES AL INICIO (como CrearCitaViewModel)
				var sucursalesResult = await ApiService.ObtenerSucursalesAsync();
				if (sucursalesResult.Success && sucursalesResult.Data != null)
				{
					Sucursales.Clear();
					foreach (var sucursal in sucursalesResult.Data)
					{
						Sucursales.Add(sucursal);
					}
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error cargando datos: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		// ✅ MÉTODO EXACTO COMO CrearCitaViewModel
		[RelayCommand]
		private async Task CargarEspecialidadesPorSucursalAsync()
		{
			if (SucursalSeleccionada == null)
				return;

			try
			{
				System.Diagnostics.Debug.WriteLine($"🔍 Cargando especialidades para sucursal: {SucursalSeleccionada.Nombre} (ID: {SucursalSeleccionada.IdSucursal})");

				IsLoading = true;
				var result = await ApiService.ObtenerEspecialidadesPorSucursalAsync(SucursalSeleccionada.IdSucursal);

				System.Diagnostics.Debug.WriteLine($"📥 Respuesta API: Success={result.Success}, Data={result.Data?.Count ?? 0} especialidades");

				if (result.Success && result.Data != null)
				{
					// ✅ USAR 'Especialidades' (la propiedad correcta)
					Especialidades.Clear();
					foreach (var especialidad in result.Data)
					{
						System.Diagnostics.Debug.WriteLine($"➕ Agregando especialidad: {especialidad.Nombre} (ID: {especialidad.IdEspecialidad})");
						Especialidades.Add(especialidad);
					}

					// ✅ LIMPIAR SELECCIÓN COMO EN CrearCitaViewModel
					EspecialidadSeleccionada = null;

					System.Diagnostics.Debug.WriteLine($"✅ Total especialidades cargadas: {Especialidades.Count}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron especialidades para esta sucursal");
					Especialidades.Clear();
					EspecialidadSeleccionada = null;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error cargando especialidades: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task AgregarHorarioAsync()
		{
			if (SucursalSeleccionada == null)
			{
				await Shell.Current.DisplayAlert("Error", "Primero seleccione una sucursal", "OK");
				return;
			}

			var sucursalesList = new List<Sucursal> { SucursalSeleccionada };
			var modalPage = new Views.Modals.AgregarHorarioModalPage(sucursalesList);

			modalPage.HorarioGuardado += (sender, horario) =>
			{
				System.Diagnostics.Debug.WriteLine($"🔥 EVENTO HorarioGuardado disparado: {horario.HorarioDisplay}");

				horario.IdSucursal = SucursalSeleccionada?.IdSucursal ?? 0;
				horario.NombreSucursal = SucursalSeleccionada?.Nombre ?? "";

				MainThread.BeginInvokeOnMainThread(() =>
				{
					Horarios.Add(horario);
					System.Diagnostics.Debug.WriteLine($"🔥 Horario agregado. Total: {Horarios.Count}");

					ShowHorarios = Horarios.Count > 0;
					OnPropertyChanged(nameof(ShowHorarios));
					ValidarDatos();
				});
			};

			await Shell.Current.Navigation.PushModalAsync(modalPage);
		}

		[RelayCommand]
		private void RemoverHorario(HorarioCrear horario)
		{
			if (horario != null && Horarios.Contains(horario))
			{
				System.Diagnostics.Debug.WriteLine($"🔥 Removiendo horario: {horario.HorarioDisplay}");
				Horarios.Remove(horario);
			}
		}

		[RelayCommand]
		private async Task GuardarMedicoAsync()
		{
			ValidarDatos();

			if (!CanSave)
			{
				var erroresList = new List<string>();

				if (string.IsNullOrWhiteSpace(Cedula) || Cedula.Length != 10)
					erroresList.Add("- Cédula válida (10 dígitos)");
				if (string.IsNullOrWhiteSpace(Nombres))
					erroresList.Add("- Nombres");
				if (string.IsNullOrWhiteSpace(Apellidos))
					erroresList.Add("- Apellidos");
				if (string.IsNullOrWhiteSpace(Correo) || !Correo.Contains("@"))
					erroresList.Add("- Correo válido");
				if (SucursalSeleccionada == null)
					erroresList.Add("- Sucursal");
				if (EspecialidadSeleccionada == null)
					erroresList.Add("- Especialidad");
				if (Horarios.Count == 0)
					erroresList.Add("- Al menos un horario");

				await Shell.Current.DisplayAlert("Campos Requeridos",
					$"Complete los siguientes campos:\n\n{string.Join("\n", erroresList)}",
					"OK");
				return;
			}

			System.Diagnostics.Debug.WriteLine($"DEBUG: Guardando médico...");
			System.Diagnostics.Debug.WriteLine($"DEBUG: Cedula='{Cedula}', Nombres='{Nombres}', Apellidos='{Apellidos}'");
			System.Diagnostics.Debug.WriteLine($"DEBUG: Sucursal='{SucursalSeleccionada?.Nombre}', Especialidad='{EspecialidadSeleccionada?.Nombre}'");

			IsLoading = true;

			try
			{
				var request = new CrearMedicoRequest
				{
					Cedula = Cedula,
					Username = GenerarUsername(),
					Nombres = Nombres,
					Apellidos = Apellidos,
					Correo = Correo,
					Sexo = Sexo,
					Nacionalidad = Nacionalidad,
					IdEspecialidad = EspecialidadSeleccionada?.IdEspecialidad ?? 0,
					TituloProfesional = TituloProfesional,
					Sucursales = new List<int> { SucursalSeleccionada?.IdSucursal ?? 0 },
					Horarios = Horarios.Select(h => new CrearHorarioRequest
					{
						IdSucursal = h.IdSucursal,
						DiaSemana = h.DiaSemana,
						HoraInicio = h.HoraInicio,
						HoraFin = h.HoraFin,
						DuracionCita = h.DuracionCita
					}).ToList()
				};

				var result = await ApiService.CrearMedicoAsync(request);

				if (result.Success)
				{
					await Shell.Current.DisplayAlert("¡Éxito!",
						$"Médico {Nombres} {Apellidos} creado exitosamente.\n\n" +
						$"🏥 Sucursal: {SucursalSeleccionada?.Nombre}\n" +
						$"🩺 Especialidad: {EspecialidadSeleccionada?.Nombre}\n" +
						$"⏰ Horarios: {Horarios.Count} configurados",
						"OK");

					LimpiarFormulario();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error creando el médico",
						"OK");
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

		public void ValidarDatos()
		{
			var errores = new List<string>();

			if (string.IsNullOrWhiteSpace(Cedula) || Cedula.Length != 10)
				errores.Add("Cédula debe tener 10 dígitos");

			if (string.IsNullOrWhiteSpace(Nombres))
				errores.Add("Nombres son requeridos");

			if (string.IsNullOrWhiteSpace(Apellidos))
				errores.Add("Apellidos son requeridos");

			if (string.IsNullOrWhiteSpace(Correo) || !Correo.Contains("@"))
				errores.Add("Correo electrónico válido es requerido");

			if (string.IsNullOrWhiteSpace(Sexo))
				errores.Add("Debe seleccionar el sexo");

			if (SucursalSeleccionada == null)
				errores.Add("Debe seleccionar una sucursal");

			if (EspecialidadSeleccionada == null)
				errores.Add("Debe seleccionar una especialidad");

			if (Horarios.Count == 0)
				errores.Add("Debe agregar al menos un horario de atención");

			var previousCanSave = CanSave;
			CanSave = errores.Count == 0;

			System.Diagnostics.Debug.WriteLine($"🔍 ValidarDatos:");
			System.Diagnostics.Debug.WriteLine($"   - Horarios.Count: {Horarios.Count}");
			System.Diagnostics.Debug.WriteLine($"   - Sucursal: {SucursalSeleccionada?.Nombre ?? "null"}");
			System.Diagnostics.Debug.WriteLine($"   - Especialidad: {EspecialidadSeleccionada?.Nombre ?? "null"}");
			System.Diagnostics.Debug.WriteLine($"   - CanSave: {previousCanSave} → {CanSave}");

			if (previousCanSave != CanSave)
			{
				OnPropertyChanged(nameof(CanSave));
			}
		}

		private string GenerarUsername()
		{
			var baseUsername = $"{Nombres?.ToLower().Replace(" ", "")}.{Apellidos?.ToLower().Replace(" ", "")}".Trim('.');

			if (string.IsNullOrWhiteSpace(baseUsername))
				baseUsername = $"user{Guid.NewGuid().ToString("N")[..6]}";

			return baseUsername.Substring(0, Math.Min(20, baseUsername.Length));
		}

		private void LimpiarFormulario()
		{
			Cedula = "";
			Nombres = "";
			Apellidos = "";
			Correo = "";
			Sexo = "M";
			Nacionalidad = "Ecuatoriana";
			TituloProfesional = "";
			EspecialidadSeleccionada = null;
			SucursalSeleccionada = null;
			Especialidades.Clear();
			Horarios.Clear();
			ShowHorarios = false;
			CanSave = false;
		}
	}

	// ===== MODELO PARA CREAR HORARIOS =====
	public class HorarioCrear
	{
		public int IdSucursal { get; set; }
		public string NombreSucursal { get; set; } = "";
		public int DiaSemana { get; set; }
		public string HoraInicio { get; set; } = "";
		public string HoraFin { get; set; } = "";
		public int DuracionCita { get; set; } = 30;

		public string DiaSemanaTexto => DiaSemana switch
		{
			1 => "Lunes",
			2 => "Martes",
			3 => "Miércoles",
			4 => "Jueves",
			5 => "Viernes",
			6 => "Sábado",
			7 => "Domingo",
			_ => "Desconocido"
		};

		public string HorarioDisplay => $"📅 {DiaSemanaTexto}: {HoraInicio} a {HoraFin} ({DuracionCita} min)";
	}
}