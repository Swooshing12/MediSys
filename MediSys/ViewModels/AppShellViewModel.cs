using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;

namespace MediSys.ViewModels
{
	public partial class AppShellViewModel : ObservableObject
	{
		private readonly AuthService _authService;

		[ObservableProperty]
		private string userFullName = "Cargando...";

		[ObservableProperty]
		private string userInitials = "U";

		[ObservableProperty]
		private string userRoleDisplay = "Verificando...";

		[ObservableProperty]
		private string userExtraInfo = "";

		// 🔥 ESTAS ERAN LAS QUE FALTABAN
		[ObservableProperty]
		private bool isMedico = false;

		[ObservableProperty]
		private bool canViewHistorial = false;

		[ObservableProperty]
		private bool canViewConsultaCitas = false;

		private User? currentUser;

		public AppShellViewModel()
		{
			_authService = new AuthService();
		}

		public async Task LoadUserDataAsync()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🔄 Loading user data...");

				currentUser = await _authService.GetCurrentUserAsync();

				if (currentUser != null)
				{
					UserFullName = currentUser.NombreCompleto;
					UserInitials = currentUser.Iniciales;
					UserRoleDisplay = currentUser.RolDisplay;

					// Información adicional según el rol
					if (currentUser.TipoUsuario == "doctor" && !string.IsNullOrEmpty(currentUser.Especialidad))
					{
						UserExtraInfo = currentUser.Especialidad;
					}
					else
					{
						UserExtraInfo = $"ID: {currentUser.CedulaString}";
					}

					// 🔥 CONFIGURAR VISIBILIDAD SEGÚN ROL
					IsMedico = currentUser.Rol == "Medico";
					CanViewHistorial = currentUser.Rol != "Paciente";
					CanViewConsultaCitas = currentUser.Rol != "Paciente";

					System.Diagnostics.Debug.WriteLine($"✅ User data loaded: {currentUser.NombreCompleto} ({currentUser.Rol})");
					System.Diagnostics.Debug.WriteLine($"   IsMedico: {IsMedico}");
					System.Diagnostics.Debug.WriteLine($"   CanViewHistorial: {CanViewHistorial}");
					System.Diagnostics.Debug.WriteLine($"   CanViewConsultaCitas: {CanViewConsultaCitas}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("❌ No user data found - redirecting to login");

					// Limpiar datos
					UserFullName = "Usuario";
					UserInitials = "U";
					UserRoleDisplay = "Invitado";
					UserExtraInfo = "";
					IsMedico = false;
					CanViewHistorial = false;
					CanViewConsultaCitas = false;

					await Shell.Current.GoToAsync("//login");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Error loading user data: {ex.Message}");
				UserFullName = "Error cargando usuario";
				UserRoleDisplay = "Error";
				UserExtraInfo = ex.Message;
				IsMedico = false;
				CanViewHistorial = false;
				CanViewConsultaCitas = false;
			}
		}

		[RelayCommand]
		private async Task LogoutAsync()
		{
			var confirm = await Shell.Current.DisplayAlert(
				"Cerrar Sesión",
				"¿Está seguro que desea salir del sistema?",
				"Sí", "Cancelar");

			if (confirm)
			{
				System.Diagnostics.Debug.WriteLine("🚪 Logging out user...");

				// Limpiar datos
				await _authService.LogoutAsync();

				// Resetear propiedades
				UserFullName = "Usuario";
				UserInitials = "U";
				UserRoleDisplay = "Invitado";
				UserExtraInfo = "";
				IsMedico = false;
				CanViewHistorial = false;
				CanViewConsultaCitas = false;

				// Navegar al login y deshabilitar flyout
				Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
				await Shell.Current.GoToAsync("//login");

				System.Diagnostics.Debug.WriteLine("✅ User logged out successfully");
			}
		}
	}
}