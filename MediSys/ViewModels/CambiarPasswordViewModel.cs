using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using MediSys.Views.Dashboard;
using System.Text.RegularExpressions;

namespace MediSys.ViewModels
{
	public partial class CambiarPasswordViewModel : ObservableObject, IActivatable
	{
		private readonly MediSysApiService _apiService;
		private readonly AuthService _authService;
		private User _currentUser;

		[ObservableProperty]
		private string passwordActual = "";

		[ObservableProperty]
		private string passwordNueva = "";

		[ObservableProperty]
		private string confirmarPassword = "";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool showValidaciones = false;

		[ObservableProperty]
		private bool showValidacionCoincidencia = false;

		[ObservableProperty]
		private bool showIndicadorSeguridad = false;

		// Validaciones individuales
		[ObservableProperty]
		private string validacionLongitud = "";

		[ObservableProperty]
		private string validacionMayuscula = "";

		[ObservableProperty]
		private string validacionMinuscula = "";

		[ObservableProperty]
		private string validacionCaracterEspecial = "";

		[ObservableProperty]
		private string validacionCoincidencia = "";

		[ObservableProperty]
		private string nivelSeguridadPassword = "";

		// Colores para validaciones
		[ObservableProperty]
		private Color colorValidacionLongitud = Colors.Gray;

		[ObservableProperty]
		private Color colorValidacionMayuscula = Colors.Gray;

		[ObservableProperty]
		private Color colorValidacionMinuscula = Colors.Gray;

		[ObservableProperty]
		private Color colorValidacionCaracterEspecial = Colors.Gray;

		[ObservableProperty]
		private Color colorValidacionCoincidencia = Colors.Gray;

		[ObservableProperty]
		private Color colorSeguridadPassword = Colors.Gray;

		// Propiedades calculadas
		public bool PuedeCambiarPassword =>
			!string.IsNullOrWhiteSpace(PasswordActual) &&
			ValidarPasswordCompleta(PasswordNueva) &&
			PasswordNueva == ConfirmarPassword &&
			PasswordActual != PasswordNueva &&
			!IsLoading;

		public CambiarPasswordViewModel()
		{
			_apiService = new MediSysApiService();
			_authService = new AuthService();

			System.Diagnostics.Debug.WriteLine("CambiarPasswordViewModel inicializado");
		}

		public void OnActivated()
		{
			_ = CargarUsuarioActual();
		}

		private async Task CargarUsuarioActual()
		{
			try
			{
				_currentUser = await _authService.GetCurrentUserAsync();

				if (_currentUser == null)
				{
					await Shell.Current.DisplayAlert("Error", "No se pudo obtener la información del usuario", "OK");
					await Shell.Current.GoToAsync("..");
				}

				System.Diagnostics.Debug.WriteLine($"Usuario actual cargado: {_currentUser?.NombreCompleto}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cargando usuario: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error cargando información del usuario", "OK");
			}
		}

		[RelayCommand]
		private async Task CambiarPassword()
		{
			if (!PuedeCambiarPassword)
			{
				await Shell.Current.DisplayAlert("Error", "Por favor verifica todos los campos", "OK");
				return;
			}

			IsLoading = true;

			try
			{
				var request = new CambiarPasswordRequest
				{
					IdUsuario = _currentUser.IdUsuario,
					PasswordActual = PasswordActual,
					PasswordNueva = PasswordNueva,
					ConfirmarPassword = ConfirmarPassword
				};

				var response = await _apiService.CambiarPasswordLogueadoAsync(request);

				if (response.Success)
				{
					await Shell.Current.DisplayAlert("Éxito",
						"Contraseña actualizada exitosamente.\n\nTu sesión se cerrará y deberás iniciar sesión con tu nueva contraseña.",
						"Entendido");

					// Cerrar sesión automáticamente
					await _authService.LogoutAsync();

					// Navegar al login
					await Shell.Current.GoToAsync("//login");
				}
				else
				{
					await Shell.Current.DisplayAlert("Error", response.Message ?? "Error cambiando contraseña", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cambiando contraseña: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "Error inesperado cambiando contraseña", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task Cancelar()
		{
			var confirmar = await Shell.Current.DisplayAlert("Cancelar",
				"¿Estás seguro de que deseas cancelar el cambio de contraseña?",
				"Sí", "No");

			if (confirmar)
			{
				await Shell.Current.Navigation.PopModalAsync(); // Cerrar modal
			}
		}

		public void ValidarPasswordEnTiempoReal()
		{
			if (string.IsNullOrEmpty(PasswordNueva))
			{
				ShowValidaciones = false;
				ShowIndicadorSeguridad = false;
				return;
			}

			ShowValidaciones = true;
			ShowIndicadorSeguridad = true;

			// Validar longitud
			bool longitudOK = PasswordNueva.Length >= 8;
			ValidacionLongitud = longitudOK ? "✓ Mínimo 8 caracteres" : "✗ Mínimo 8 caracteres";
			ColorValidacionLongitud = longitudOK ? Colors.Green : Colors.Red;

			// Validar mayúscula
			bool mayusculaOK = Regex.IsMatch(PasswordNueva, @"[A-Z]");
			ValidacionMayuscula = mayusculaOK ? "✓ Contiene mayúscula" : "✗ Falta mayúscula";
			ColorValidacionMayuscula = mayusculaOK ? Colors.Green : Colors.Red;

			// Validar minúscula
			bool minusculaOK = Regex.IsMatch(PasswordNueva, @"[a-z]");
			ValidacionMinuscula = minusculaOK ? "✓ Contiene minúscula" : "✗ Falta minúscula";
			ColorValidacionMinuscula = minusculaOK ? Colors.Green : Colors.Red;

			// Validar carácter especial
			bool especialOK = Regex.IsMatch(PasswordNueva, @"[!@#$%^&*(),.?""{}|<>]");
			ValidacionCaracterEspecial = especialOK ? "✓ Contiene carácter especial" : "✗ Falta carácter especial";
			ColorValidacionCaracterEspecial = especialOK ? Colors.Green : Colors.Red;

			// Calcular nivel de seguridad
			CalcularNivelSeguridad(longitudOK, mayusculaOK, minusculaOK, especialOK);

			// Notificar cambio en PuedeCambiarPassword
			OnPropertyChanged(nameof(PuedeCambiarPassword));
		}

		public void ValidarCoincidenciaPassword()
		{
			if (string.IsNullOrEmpty(ConfirmarPassword))
			{
				ShowValidacionCoincidencia = false;
				return;
			}

			ShowValidacionCoincidencia = true;

			bool coincide = PasswordNueva == ConfirmarPassword;
			ValidacionCoincidencia = coincide ? "✓ Las contraseñas coinciden" : "✗ Las contraseñas no coinciden";
			ColorValidacionCoincidencia = coincide ? Colors.Green : Colors.Red;

			OnPropertyChanged(nameof(PuedeCambiarPassword));
		}

		private void CalcularNivelSeguridad(bool longitud, bool mayuscula, bool minuscula, bool especial)
		{
			int puntos = 0;
			if (longitud) puntos++;
			if (mayuscula) puntos++;
			if (minuscula) puntos++;
			if (especial) puntos++;

			switch (puntos)
			{
				case 4:
					NivelSeguridadPassword = "🔒 Seguridad: FUERTE";
					ColorSeguridadPassword = Color.FromArgb("#059669"); // Verde
					break;
				case 3:
					NivelSeguridadPassword = "🔓 Seguridad: MEDIA";
					ColorSeguridadPassword = Color.FromArgb("#D97706"); // Naranja
					break;
				case 2:
					NivelSeguridadPassword = "⚠️ Seguridad: DÉBIL";
					ColorSeguridadPassword = Color.FromArgb("#DC2626"); // Rojo
					break;
				default:
					NivelSeguridadPassword = "❌ Seguridad: MUY DÉBIL";
					ColorSeguridadPassword = Color.FromArgb("#7F1D1D"); // Rojo oscuro
					break;
			}
		}

		private bool ValidarPasswordCompleta(string password)
		{
			if (string.IsNullOrEmpty(password)) return false;

			return password.Length >= 8 &&
				   Regex.IsMatch(password, @"[A-Z]") &&
				   Regex.IsMatch(password, @"[a-z]") &&
				   Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]");
		}
	}
}