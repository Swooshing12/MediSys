using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Services;

namespace MediSys.ViewModels
{
	public partial class LoginViewModel : ObservableObject
	{
		// Instancia estática para mantener cookies entre intentos
		private static MediSysApiService? _sharedApiService;
		private readonly AuthService _authService;

		[ObservableProperty]
		private bool showForgotPassword = false;

		[ObservableProperty]
		private string correo = "";

		[ObservableProperty]
		private string password = "";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private string errorMessage = "";

		[ObservableProperty]
		private bool showError = false;

		public LoginViewModel()
		{
			// Crear una sola instancia compartida para mantener cookies
			if (_sharedApiService == null)
				_sharedApiService = new MediSysApiService();

			_authService = new AuthService();
		}

		[RelayCommand]
		private async Task LoginAsync()
		{
			if (string.IsNullOrWhiteSpace(Correo) || string.IsNullOrWhiteSpace(Password))
			{
				ShowErrorMessage("Complete todos los campos requeridos");
				return;
			}

			if (!IsValidEmail(Correo))
			{
				ShowErrorMessage("Ingrese un formato de correo válido");
				return;
			}

			IsLoading = true;
			ErrorMessage = "";
			ShowError = false;

			try
			{
				// Usar la instancia compartida para mantener cookies
				var result = await _sharedApiService!.LoginAsync(Correo, Password);

				if (result.Success && result.Data?.Usuario != null)
				{
					var user = result.Data.Usuario;
					await _authService.SaveUserAsync(user);

					if (user.RequiereCambioPassword)
					{
						await Shell.Current.DisplayAlert("Cambio de Contraseña",
							"Debe cambiar su contraseña temporal para continuar", "OK");
						await Shell.Current.GoToAsync($"//changepassword?email={user.Correo}");
						return;
					}

					var welcomeMessage = $"¡Bienvenido, {user.Nombres}!\n\n" +
										$"👤 {user.RolDisplay}\n" +
										$"📧 {user.Correo}\n" +
										$"🆔 {user.CedulaString}";

					if (!string.IsNullOrEmpty(user.Especialidad))
					{
						welcomeMessage += $"\n🏥 {user.Especialidad}";
					}

					await Shell.Current.DisplayAlert("¡Inicio de Sesión Exitoso!", welcomeMessage, "Continuar");

					Correo = "";
					Password = "";
					ErrorMessage = "";
					ShowError = false;

					Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
					await Shell.Current.GoToAsync("//dashboard");
				}
				else
				{
					string errorMessage = result.Message ?? "Error de autenticación";

					if (errorMessage.Contains("bloqueada por múltiples intentos") ||
						errorMessage.Contains("Cuenta bloqueada"))
					{
						ShowErrorMessage("🔒 Cuenta bloqueada por seguridad.\n\nContacte al administrador para desbloquear su cuenta.");
					}
					else
					{
						ShowErrorMessage("❌ Credenciales incorrectas.\n\nVerifique su correo y contraseña.");
					}
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Error inesperado: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task ForgotPasswordAsync()
		{
			await Shell.Current.GoToAsync("//forgotpassword");
		}

		private void ShowErrorMessage(string message)
		{
			ErrorMessage = message;
			ShowError = true;
		}

		private static bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}
	}
}