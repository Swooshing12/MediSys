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
			var userJson = System.Text.Json.JsonSerializer.Serialize(user);
			await SecureStorage.SetAsync(UserKey, userJson);
			await SecureStorage.SetAsync(IsLoggedInKey, "true");
		}

		public async Task<User?> GetCurrentUserAsync()
		{
			try
			{
				var userJson = await SecureStorage.GetAsync(UserKey);
				if (string.IsNullOrEmpty(userJson))
					return null;

				return System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> IsLoggedInAsync()
		{
			try
			{
				var isLoggedIn = await SecureStorage.GetAsync(IsLoggedInKey);
				return isLoggedIn == "true";
			}
			catch
			{
				return false;
			}
		}

		public async Task LogoutAsync()
		{
			SecureStorage.RemoveAll();
		}
	}
}