using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Globalization;

namespace MediSys.ViewModels
{
	public partial class PerfilViewModel : ObservableObject
	{
		[ObservableProperty]
		private User usuario = new();

		[ObservableProperty]
		private bool isLoading = false;

		public PerfilViewModel()
		{
			CargarPerfil();
		}

		// Propiedades calculadas para la UI
		public string SexoDisplay => Usuario.Sexo switch
		{
			"M" => "👨 Masculino",
			"F" => "👩 Femenino",
			_ => Usuario.Sexo
		};

		public string TipoUsuarioDisplay => Usuario.TipoUsuario switch
		{
			"doctor" => "👨‍⚕️ Médico",
			"paciente" => "🧑‍🤝‍🧑 Paciente",
			"admin" => "⚙️ Administrador",
			_ => Usuario.TipoUsuario
		};

		public string FechaRegistroFormateada
		{
			get
			{
				if (DateTime.TryParse(Usuario.FechaRegistro, out DateTime fecha))
				{
					return fecha.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
				}
				return Usuario.FechaRegistro;
			}
		}

		public bool MostrarInfoProfesional => Usuario.TipoUsuario == "doctor" && !string.IsNullOrEmpty(Usuario.Especialidad);

		public Color AvatarBackgroundColor => Usuario.Rol switch
		{
			"Medico" => Colors.DodgerBlue,
			"Paciente" => Colors.SeaGreen,
			"Administrador" => Colors.OrangeRed,
			"Recepcionista" => Colors.Purple,
			"Enfermero" => Colors.DeepPink,
			_ => Colors.Gray
		};

		public Color RolBackgroundColor => Usuario.Rol switch
		{
			"Medico" => Colors.DodgerBlue,
			"Paciente" => Colors.SeaGreen,
			"Administrador" => Colors.OrangeRed,
			"Recepcionista" => Colors.Purple,
			"Enfermero" => Colors.DeepPink,
			_ => Colors.Gray
		};

		[RelayCommand]
		private void CargarPerfil()
		{
			try
			{
				// Obtener el usuario desde las preferencias (guardado en el login)
				var userDataJson = Preferences.Get("user_data", string.Empty);

				if (!string.IsNullOrEmpty(userDataJson))
				{
					var userData = System.Text.Json.JsonSerializer.Deserialize<User>(userDataJson, new System.Text.Json.JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (userData != null)
					{
						Usuario = userData;
						System.Diagnostics.Debug.WriteLine($"Perfil cargado: {Usuario.NombreCompleto}");

						// Notificar cambios en las propiedades calculadas
						OnPropertyChanged(nameof(SexoDisplay));
						OnPropertyChanged(nameof(TipoUsuarioDisplay));
						OnPropertyChanged(nameof(FechaRegistroFormateada));
						OnPropertyChanged(nameof(MostrarInfoProfesional));
						OnPropertyChanged(nameof(AvatarBackgroundColor));
						OnPropertyChanged(nameof(RolBackgroundColor));
					}
				}
				else
				{
					Shell.Current.DisplayAlert("Error", "No se pudo cargar la información del perfil", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando perfil: {ex.Message}");
				Shell.Current.DisplayAlert("Error", $"Error cargando perfil: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task CambiarContrasenaAsync()
		{
			try
			{
				await Shell.Current.DisplayAlert("Cambiar Contraseña", "Esta funcionalidad estará disponible pronto", "OK");
				// TODO: Implementar navegación a página de cambio de contraseña
				// await Shell.Current.GoToAsync("//cambiarcontrasena");
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task EditarPerfilAsync()
		{
			try
			{
				await Shell.Current.DisplayAlert("Editar Perfil", "Esta funcionalidad estará disponible pronto", "OK");
				// TODO: Implementar navegación a página de edición de perfil
				// await Shell.Current.GoToAsync("//editarperfil");
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task CerrarSesionAsync()
		{
			try
			{
				bool confirm = await Shell.Current.DisplayAlert(
					"Cerrar Sesión",
					$"¿Está seguro que desea cerrar la sesión de {Usuario.Nombres}?",
					"Sí, cerrar sesión",
					"Cancelar");

				if (confirm)
				{
					// Limpiar datos de usuario
					Preferences.Remove("user_data");
					Preferences.Remove("is_logged_in");

					// Navegar al login
					await Shell.Current.GoToAsync("//login");

					await Shell.Current.DisplayAlert("Sesión Cerrada", "Sesión cerrada correctamente", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error cerrando sesión: {ex.Message}", "OK");
			}
		}
	}
}