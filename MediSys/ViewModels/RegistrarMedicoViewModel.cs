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

		// ===== SUCURSALES Y HORARIOS =====
		[ObservableProperty]
		private ObservableCollection<Especialidad> especialidades = new();

		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursales = new();

		[ObservableProperty]
		private ObservableCollection<Sucursal> sucursalesSeleccionadas = new();

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
			// 🔥 SUSCRIBIRSE A CAMBIOS EN HORARIOS Y SUCURSALES
			Horarios.CollectionChanged += (s, e) => {
				System.Diagnostics.Debug.WriteLine($"🔥 Horarios.CollectionChanged: Count = {Horarios.Count}");
				ShowHorarios = Horarios.Count > 0;
				OnPropertyChanged(nameof(ShowHorarios));
				ValidarDatos();
			};

			SucursalesSeleccionadas.CollectionChanged += (s, e) => {
				System.Diagnostics.Debug.WriteLine($"🔥 SucursalesSeleccionadas.CollectionChanged: Count = {SucursalesSeleccionadas.Count}");
				ValidarDatos();
			};

			_ = CargarDatosInicialesAsync();
		}

		private async Task CargarDatosInicialesAsync()
		{
			IsLoading = true;
			try
			{
				// Cargar especialidades
				var especialidadesResult = await ApiService.ObtenerEspecialidadesAsync();
				if (especialidadesResult.Success && especialidadesResult.Data != null)
				{
					Especialidades.Clear();
					foreach (var especialidad in especialidadesResult.Data)
					{
						Especialidades.Add(especialidad);
					}
				}

				// Cargar sucursales
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

		[RelayCommand]
		private void AgregarSucursal(Sucursal sucursal)
		{
			if (sucursal != null && !SucursalesSeleccionadas.Contains(sucursal))
			{
				System.Diagnostics.Debug.WriteLine($"🔥 Agregando sucursal: {sucursal.Nombre}");
				SucursalesSeleccionadas.Add(sucursal);
				// ValidarDatos() se llama automáticamente por la suscripción
			}
		}

		[RelayCommand]
		private void RemoverSucursal(Sucursal sucursal)
		{
			if (sucursal != null && SucursalesSeleccionadas.Contains(sucursal))
			{
				System.Diagnostics.Debug.WriteLine($"🔥 Removiendo sucursal: {sucursal.Nombre}");
				SucursalesSeleccionadas.Remove(sucursal);

				// Remover horarios de esa sucursal
				var horariosARemover = Horarios.Where(h => h.IdSucursal == sucursal.IdSucursal).ToList();
				foreach (var horario in horariosARemover)
				{
					System.Diagnostics.Debug.WriteLine($"🔥 Removiendo horario: {horario.HorarioDisplay}");
					Horarios.Remove(horario);
				}
				// ValidarDatos() se llama automáticamente por la suscripción
			}
		}

		[RelayCommand]
		private async Task AgregarHorarioAsync()
		{
			if (SucursalesSeleccionadas.Count == 0)
			{
				await Shell.Current.DisplayAlert("Error", "Primero seleccione al menos una sucursal", "OK");
				return;
			}

			var modalPage = new Views.Modals.AgregarHorarioModalPage(SucursalesSeleccionadas.ToList());

			// 🔥 CORREGIR EL EVENTO PARA QUE FUNCIONE BIEN
			modalPage.HorarioGuardado += (sender, horario) =>
			{
				System.Diagnostics.Debug.WriteLine($"🔥 EVENTO HorarioGuardado disparado: {horario.HorarioDisplay}");

				// 🔥 USAR DISPATCHER PARA ASEGURAR QUE SE EJECUTE EN EL HILO PRINCIPAL
				MainThread.BeginInvokeOnMainThread(() =>
				{
					Horarios.Add(horario);
					System.Diagnostics.Debug.WriteLine($"🔥 Horario agregado. Total: {Horarios.Count}");

					// Esto se llama automáticamente por la suscripción, pero por las dudas lo forzamos
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
				// ShowHorarios y ValidarDatos() se actualizan automáticamente
			}
		}

		[RelayCommand]
		private async Task GuardarMedicoAsync()
		{
			// 🔥 VALIDACIÓN FINAL ANTES DE ENVIAR
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
				if (EspecialidadSeleccionada == null)
					erroresList.Add("- Especialidad");
				if (SucursalesSeleccionadas.Count == 0)
					erroresList.Add("- Al menos una sucursal");
				if (Horarios.Count == 0)
					erroresList.Add("- Al menos un horario");

				await Shell.Current.DisplayAlert("Campos Requeridos",
					$"Complete los siguientes campos:\n\n{string.Join("\n", erroresList)}\n\n" +
					$"Estado actual:\n" +
					$"• Horarios: {Horarios.Count}\n" +
					$"• Sucursales: {SucursalesSeleccionadas.Count}",
					"OK");
				return;
			}
			// 🔹 Aquí agregamos logs para ver los valores actuales
			System.Diagnostics.Debug.WriteLine($"DEBUG: Cedula='{Cedula}'");
			System.Diagnostics.Debug.WriteLine($"DEBUG: Nombres='{Nombres}', Apellidos='{Apellidos}'");
			System.Diagnostics.Debug.WriteLine($"DEBUG: Username='{GenerarUsername()}'");
			System.Diagnostics.Debug.WriteLine($"DEBUG: Sexo='{Sexo}', Especialidad='{EspecialidadSeleccionada?.Nombre ?? "null"}'");
			System.Diagnostics.Debug.WriteLine($"DEBUG: SucursalesSeleccionadas.Count={SucursalesSeleccionadas.Count}, Horarios.Count={Horarios.Count}");

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
					Sucursales = SucursalesSeleccionadas.Select(s => s.IdSucursal).ToList(),
					Horarios = Horarios.Select(h => new CrearHorarioRequest
					{
						IdSucursal = h.IdSucursal,
						DiaSemana = h.DiaSemana,
						HoraInicio = h.HoraInicio,
						HoraFin = h.HoraFin,
						DuracionCita = h.DuracionCita
					}).ToList()
				};

				System.Diagnostics.Debug.WriteLine($"🚀 Enviando request con {request.Horarios.Count} horarios");

				var result = await ApiService.CrearMedicoAsync(request);

				if (result.Success)
				{
					await Shell.Current.DisplayAlert("¡Éxito!",
						$"Médico {Nombres} {Apellidos} creado exitosamente con {Horarios.Count} horarios asignados",
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

		// 🔥 MÉTODO DE VALIDACIÓN CORREGIDO
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

			if (EspecialidadSeleccionada == null)
				errores.Add("Debe seleccionar una especialidad");

			if (SucursalesSeleccionadas.Count == 0)
				errores.Add("Debe seleccionar al menos una sucursal");

			if (Horarios.Count == 0)
				errores.Add("Debe agregar al menos un horario de atención");

			var previousCanSave = CanSave;
			CanSave = errores.Count == 0;

			// 🔥 DEBUG MEJORADO
			System.Diagnostics.Debug.WriteLine($"🔍 ValidarDatos:");
			System.Diagnostics.Debug.WriteLine($"   - Horarios.Count: {Horarios.Count}");
			System.Diagnostics.Debug.WriteLine($"   - Sucursales.Count: {SucursalesSeleccionadas.Count}");
			System.Diagnostics.Debug.WriteLine($"   - Especialidad: {EspecialidadSeleccionada?.Nombre ?? "null"}");
			System.Diagnostics.Debug.WriteLine($"   - Sexo: {Sexo}");
			System.Diagnostics.Debug.WriteLine($"   - CanSave: {previousCanSave} → {CanSave}");
			System.Diagnostics.Debug.WriteLine($"   - Errores: {errores.Count}");

			// 🔥 NOTIFICAR CAMBIO SOLO SI CAMBIÓ
			if (previousCanSave != CanSave)
			{
				OnPropertyChanged(nameof(CanSave));
			}
		}

		private string GenerarUsername()
		{
			var baseUsername = $"{Nombres?.ToLower().Replace(" ", "")}.{Apellidos?.ToLower().Replace(" ", "")}".Trim('.');

			if (string.IsNullOrWhiteSpace(baseUsername))
				baseUsername = $"user{Guid.NewGuid().ToString("N")[..6]}"; // fallback

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
			SucursalesSeleccionadas.Clear();
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

		public string HorarioDisplay => $"🏢 {NombreSucursal} - {DiaSemanaTexto}: {HoraInicio} a {HoraFin} ({DuracionCita} min)";
	}
}