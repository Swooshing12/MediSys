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
			// No cargar datos automáticamente al inicio
			ResetUserData();
		}

		public async Task ForceReloadUserDataAsync()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🔄 Force reloading user data for shell...");

				// Limpiar datos anteriores primero
				ResetUserData();

				currentUser = await _authService.GetCurrentUserAsync();

				if (currentUser != null)
				{
					System.Diagnostics.Debug.WriteLine($"📋 Raw user data: Nombres={currentUser.Nombres}, Apellidos={currentUser.Apellidos}");
					System.Diagnostics.Debug.WriteLine($"📋 Raw user data: Rol={currentUser.Rol}, Cedula={currentUser.Cedula}");
					System.Diagnostics.Debug.WriteLine($"📋 Raw user data: TipoUsuario={currentUser.TipoUsuario}, Especialidad={currentUser.Especialidad}");

					UserFullName = !string.IsNullOrEmpty(currentUser.NombreCompleto)
						? currentUser.NombreCompleto
						: $"{currentUser.Nombres} {currentUser.Apellidos}".Trim();

					// Crear iniciales de manera más segura
					try
					{
						var nombres = currentUser.Nombres?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Usuario" };
						var apellidos = currentUser.Apellidos?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Sistema" };

						var inicial1 = nombres[0].Length > 0 ? nombres[0][0].ToString().ToUpper() : "U";
						var inicial2 = apellidos[0].Length > 0 ? apellidos[0][0].ToString().ToUpper() : "S";

						UserInitials = $"{inicial1}{inicial2}";
					}
					catch
					{
						UserInitials = "US";
					}

					UserRoleDisplay = currentUser.Rol ?? "Usuario";

					// Información adicional DETALLADA según el rol
					if (currentUser.Rol?.ToLower() == "medico" && !string.IsNullOrEmpty(currentUser.Especialidad))
					{
						UserExtraInfo = $"Dr. {currentUser.Especialidad}";
					}
					else if (currentUser.Rol?.ToLower() == "paciente")
					{
						UserExtraInfo = $"Paciente • Cédula: {currentUser.Cedula}";
					}
					else if (currentUser.Rol?.ToLower() == "administrador")
					{
						UserExtraInfo = $"Administrador • ID: {currentUser.Cedula}";
					}
					else if (currentUser.Rol?.ToLower() == "recepcionista")
					{
						UserExtraInfo = $"Recepción • ID: {currentUser.Cedula}";
					}
					else if (currentUser.Rol?.ToLower() == "enfermero")
					{
						UserExtraInfo = $"Enfermería • ID: {currentUser.Cedula}";
					}
					else
					{
						UserExtraInfo = $"Cédula: {currentUser.Cedula}";
					}

					// Configurar visibilidad según rol
					IsMedico = currentUser.Rol?.ToLower() == "medico";
					CanViewHistorial = currentUser.Rol?.ToLower() != "paciente";
					CanViewConsultaCitas = true;

					System.Diagnostics.Debug.WriteLine($"✅ Shell data updated successfully:");
					System.Diagnostics.Debug.WriteLine($"   UserFullName: {UserFullName}");
					System.Diagnostics.Debug.WriteLine($"   UserInitials: {UserInitials}");
					System.Diagnostics.Debug.WriteLine($"   UserRoleDisplay: {UserRoleDisplay}");
					System.Diagnostics.Debug.WriteLine($"   UserExtraInfo: {UserExtraInfo}");
					System.Diagnostics.Debug.WriteLine($"   IsMedico: {IsMedico}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("❌ No user data found in storage");
					ResetUserData();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Error force reloading user data: {ex.Message}");
				UserFullName = "Error cargando usuario";
				UserRoleDisplay = "Error";
				UserExtraInfo = $"Error: {ex.Message}";
				ResetUserData();
			}
		}

		private void ResetUserData()
		{
			UserFullName = "Usuario";
			UserInitials = "U";
			UserRoleDisplay = "Invitado";
			UserExtraInfo = "";
			IsMedico = false;
			CanViewHistorial = false;
			CanViewConsultaCitas = false;
		}

		[RelayCommand]
		private async Task LogoutAsync()
		{
			try
			{
				// Limpiar AuthService
				await _authService.LogoutAsync();

				// 🔥 Limpiar también Preferences
				Preferences.Remove("user_data");
				Preferences.Remove("is_logged_in");

				// Ocultar flyout y navegar al login
				Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
				await Shell.Current.GoToAsync("//login");

				System.Diagnostics.Debug.WriteLine("✅ Logout complete - all data cleared");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error during logout: {ex.Message}");
			}
		}
	}
}