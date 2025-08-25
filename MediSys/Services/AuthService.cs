using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediSys.Models;

namespace MediSys.Services
{
	public class AuthService
	{
		private const string UserKey = "current_user";
		private const string IsLoggedInKey = "is_logged_in";

		public async Task SaveUserAsync(User user)
		{
			try
			{
				var userJson = System.Text.Json.JsonSerializer.Serialize(user);
				await SecureStorage.SetAsync(UserKey, userJson);
				await SecureStorage.SetAsync(IsLoggedInKey, "true");

				// 🔥 LOG PARA DEBUG
				System.Diagnostics.Debug.WriteLine($"✅ User saved: {user.Nombres} ({user.Rol})");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error saving user: {ex.Message}");
				throw; // Re-lanzar para que el ViewModel lo maneje
			}
		}

		public async Task<User?> GetCurrentUserAsync()
		{
			try
			{
				var userJson = await SecureStorage.GetAsync(UserKey);
				if (string.IsNullOrEmpty(userJson))
				{
					System.Diagnostics.Debug.WriteLine("ℹ️ No user found in secure storage");
					return null;
				}

				var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
				System.Diagnostics.Debug.WriteLine($"✅ User loaded: {user?.Nombres} ({user?.Rol})");
				return user;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error loading user: {ex.Message}");
				return null;
			}
		}

		public async Task<bool> IsLoggedInAsync()
		{
			try
			{
				var isLoggedIn = await SecureStorage.GetAsync(IsLoggedInKey);
				var result = isLoggedIn == "true";

				// 🔥 VERIFICAR QUE TAMBIÉN EXISTA EL USUARIO
				if (result)
				{
					var user = await GetCurrentUserAsync();
					result = user != null;
				}

				System.Diagnostics.Debug.WriteLine($"🔍 Is logged in: {result}");
				return result;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error checking login status: {ex.Message}");
				return false;
			}
		}

		public async Task LogoutAsync()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("🚪 Logging out user...");
				SecureStorage.RemoveAll();
				System.Diagnostics.Debug.WriteLine("✅ User logged out successfully");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error during logout: {ex.Message}");
				// No re-lanzar error de logout, solo logear
			}
		}

		// 🔥 MÉTODO ADICIONAL: Verificar si el token/sesión sigue válida
		public async Task<bool> IsSessionValidAsync()
		{
			try
			{
				var user = await GetCurrentUserAsync();
				if (user == null) return false;

				// Aquí podrías agregar lógica adicional como verificar expiración
				// Por ahora, si existe el usuario, la sesión es válida
				return true;
			}
			catch
			{
				return false;
			}
		}

		// 🔥 MÉTODO ADICIONAL: Limpiar solo los datos del usuario (mantener otras preferencias)
		public async Task ClearUserDataAsync()
		{
			try
			{
				SecureStorage.Remove(UserKey);
				SecureStorage.Remove(IsLoggedInKey);
				System.Diagnostics.Debug.WriteLine("🧹 User data cleared");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error clearing user data: {ex.Message}");
			}
		}
	}
}