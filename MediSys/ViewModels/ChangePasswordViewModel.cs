using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Services;

namespace MediSys.ViewModels
{
	[QueryProperty(nameof(Email), "email")]
	public partial class ChangePasswordViewModel : ObservableObject
	{
		private readonly MediSysApiService _apiService;

		[ObservableProperty]
		private string email = "";

		[ObservableProperty]
		private string currentPassword = "";

		[ObservableProperty]
		private string newPassword = "";

		[ObservableProperty]
		private string confirmPassword = "";

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

		[ObservableProperty]
		private string instructionMessage = "";

		public ChangePasswordViewModel()
		{
			_apiService = new MediSysApiService();
			UpdateInstructionMessage();
		}

		partial void OnEmailChanged(string value)
		{
			UpdateInstructionMessage();
		}

		private void UpdateInstructionMessage()
		{
			if (!string.IsNullOrEmpty(Email))
			{
				InstructionMessage = $"Ingresa la clave temporal enviada a:\n{Email}\n\nLuego establece tu nueva contraseña segura.";
			}
			else
			{
				InstructionMessage = "Ingresa la clave temporal que recibiste por email y establece tu nueva contraseña.";
			}
		}

		[RelayCommand]
		private async Task ChangePasswordAsync()
		{
			// Validaciones
			if (string.IsNullOrWhiteSpace(Email))
			{
				ShowErrorMessage("Email requerido. Regrese al login e intente nuevamente.");
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentPassword))
			{
				ShowErrorMessage("Ingrese la contraseña temporal");
				return;
			}

			if (string.IsNullOrWhiteSpace(NewPassword))
			{
				ShowErrorMessage("Ingrese la nueva contraseña");
				return;
			}

			if (string.IsNullOrWhiteSpace(ConfirmPassword))
			{
				ShowErrorMessage("Confirme la nueva contraseña");
				return;
			}

			if (NewPassword != ConfirmPassword)
			{
				ShowErrorMessage("Las contraseñas no coinciden");
				return;
			}

			if (!IsValidPassword(NewPassword))
			{
				ShowErrorMessage("La contraseña no cumple con los requisitos mínimos");
				return;
			}

			if (CurrentPassword == NewPassword)
			{
				ShowErrorMessage("La nueva contraseña debe ser diferente a la temporal");
				return;
			}

			IsLoading = true;
			ErrorMessage = "";
			ShowError = false;

			try
			{
				System.Diagnostics.Debug.WriteLine($"🔄 Changing password for: {Email}");

				var result = await _apiService.ChangePasswordAsync(Email, CurrentPassword, NewPassword, ConfirmPassword);

				if (result.Success && result.Data != null)
				{
					// ✅ ÉXITO - Mostrar pantalla de confirmación
					SuccessMessage = result.Data.MensajeUsuario ??
						"Tu contraseña ha sido cambiada exitosamente. Ya puedes iniciar sesión con tu nueva contraseña.";

					ShowSuccess = true;
					ShowForm = false;

					System.Diagnostics.Debug.WriteLine("✅ Password changed successfully");
				}
				else
				{
					// ❌ ERROR
					ShowErrorMessage(result.Message);
					System.Diagnostics.Debug.WriteLine($"❌ Password change failed: {result.Message}");
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Error inesperado: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"💥 Password change exception: {ex}");
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

		private static bool IsValidPassword(string password)
		{
			if (string.IsNullOrEmpty(password) || password.Length < 8)
				return false;

			bool hasUpper = password.Any(char.IsUpper);
			bool hasLower = password.Any(char.IsLower);
			bool hasDigit = password.Any(char.IsDigit);
			bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

			return hasUpper && hasLower && hasDigit && hasSpecial;
		}
	}
}