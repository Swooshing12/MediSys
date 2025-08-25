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
		private readonly MediSysApiService _apiService;
		private readonly AuthService _authService;


		// Añade esta propiedad
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
			_apiService = new MediSysApiService();
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
				var result = await _apiService.LoginAsync(Correo, Password);

				if (result.Success && result.Data?.Usuario != null)
				{
					// ✅ LOGIN EXITOSO
					var user = result.Data.Usuario;
					await _authService.SaveUserAsync(user);

					if (user.RequiereCambioPassword)
					{
						await Shell.Current.DisplayAlert("Cambio de Contraseña",
							"Debe cambiar su contraseña temporal para continuar", "OK");
						return;
					}

					// 🎉 MENSAJE DE ÉXITO MEJORADO
					var welcomeMessage = $"¡Bienvenido, {user.Nombres}!\n\n" +
										$"👤 {user.RolDisplay}\n" +
										$"📧 {user.Correo}\n" +
										$"🆔 {user.CedulaString}";

					if (!string.IsNullOrEmpty(user.Especialidad))
					{
						welcomeMessage += $"\n🏥 {user.Especialidad}";
					}

					await Shell.Current.DisplayAlert("¡Inicio de Sesión Exitoso!", welcomeMessage, "Continuar");

					// TODO: Navegar al dashboard según el rol
					// await Shell.Current.GoToAsync("//dashboard");
				}
				else
				{
					// ❌ LOGIN FALLIDO
					ShowErrorMessage(result.Message);
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


		// Añade este comando
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