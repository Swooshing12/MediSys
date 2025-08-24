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
	public partial class ForgotPasswordViewModel : ObservableObject
	{
		private readonly MediSysApiService _apiService;

		[ObservableProperty]
		private string email = "";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private string errorMessage = "";

		[ObservableProperty]
		private bool showError = false;

		[ObservableProperty]
		private string successMessage = "";

		[ObservableProperty]
		private bool showSuccess = false;

		[ObservableProperty]
		private bool showForm = true;

		public ForgotPasswordViewModel()
		{
			_apiService = new MediSysApiService();
		}

		[RelayCommand]
		private async Task SendRecoveryAsync()
		{
			if (string.IsNullOrWhiteSpace(Email))
			{
				ShowErrorMessage("Ingrese su correo electrónico");
				return;
			}

			if (!IsValidEmail(Email))
			{
				ShowErrorMessage("Ingrese un formato de correo válido");
				return;
			}

			IsLoading = true;
			ErrorMessage = "";
			ShowError = false;

			try
			{
				System.Diagnostics.Debug.WriteLine($"🔑 Sending recovery email to: {Email}");

				var result = await _apiService.ForgotPasswordAsync(Email);

				if (result.Success && result.Data != null)
				{
					// ✅ ÉXITO - Mostrar pantalla de confirmación
					SuccessMessage = result.Data.MensajeUsuario ??
						"Si el correo existe en nuestro sistema, recibirás una clave temporal en unos minutos.";

					ShowSuccess = true;
					ShowForm = false;

					System.Diagnostics.Debug.WriteLine("✅ Recovery email sent successfully");
				}
				else
				{
					// ❌ ERROR
					ShowErrorMessage(result.Message);
					System.Diagnostics.Debug.WriteLine($"❌ Recovery failed: {result.Message}");
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Error inesperado: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"💥 Recovery exception: {ex}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task BackToLoginAsync()
		{
			await Shell.Current.GoToAsync("//login");
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