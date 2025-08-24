using MediSys.Models;
using System.Text;
using System.Text.Json;
using System.Net;

namespace MediSys.Services
{
	public class MediSysApiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public MediSysApiService()
		{
			// 🔧 CONFIGURACIÓN ESPECIAL PARA DESARROLLO LOCAL
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			_httpClient = new HttpClient(handler);
			_baseUrl = "http://192.168.100.16/MenuDinamico/api";

			// Configurar headers
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MediSys-MAUI/1.0");
			_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

			// Timeout más largo para debug
			_httpClient.Timeout = TimeSpan.FromSeconds(60);

			// 🔥 LOG PARA DEBUG
			System.Diagnostics.Debug.WriteLine($"🔗 API Service initialized with URL: {_baseUrl}");
		}

		public async Task<ApiResponse<LoginResponse>> LoginAsync(string correo, string password)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"🔄 Starting login for: {correo}");

				var loginRequest = new LoginRequest
				{
					Correo = correo,
					Password = password
				};

				var json = JsonSerializer.Serialize(loginRequest);
				System.Diagnostics.Debug.WriteLine($"📤 Request JSON: {json}");

				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var url = $"{_baseUrl}/auth/login";

				System.Diagnostics.Debug.WriteLine($"🌐 Making request to: {url}");

				var response = await _httpClient.PostAsync(url, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Response Status: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"📥 Response Content: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					// Login exitoso
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					System.Diagnostics.Debug.WriteLine("✅ Login successful!");
					return apiResponse ?? new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = "Error al procesar respuesta del servidor"
					};
				}
				else
				{
					// Error de login
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					System.Diagnostics.Debug.WriteLine($"❌ Login failed: {errorResponse?.Message}");
					return new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error de autenticación",
						Code = (int)response.StatusCode,
						Details = errorResponse?.Details
					};
				}
			}
			catch (TaskCanceledException ex)
			{
				System.Diagnostics.Debug.WriteLine($"⏱️ Timeout error: {ex.Message}");
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = "Tiempo de espera agotado. Verifique su conexión.",
					Code = 408
				};
			}
			catch (HttpRequestException ex)
			{
				System.Diagnostics.Debug.WriteLine($"🔌 HTTP error: {ex.Message}");
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = $"Error de conexión: {ex.Message}. Verifique que el servidor esté disponible en {_baseUrl}",
					Code = 500
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Unexpected error: {ex.Message}");
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
					Code = 500
				};
			}
		}

		// Método para "Olvidé mi contraseña"
		public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string correo)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"🔑 Sending forgot password for: {correo}");

				var request = new ForgotPasswordRequest { Correo = correo };
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync($"{_baseUrl}/auth/enviar-clave-temporal", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Forgot password response: {response.StatusCode} - {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<ForgotPasswordResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
					return apiResponse ?? new ApiResponse<ForgotPasswordResponse> { Success = false, Message = "Error procesando respuesta" };
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<ForgotPasswordResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error enviando clave temporal",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Forgot password error: {ex.Message}");
				return new ApiResponse<ForgotPasswordResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// Método para cambiar contraseña
		public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(string correo, string passwordActual, string passwordNueva, string confirmarPassword)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"🔄 Changing password for: {correo}");

				var request = new ChangePasswordRequest
				{
					Correo = correo,
					PasswordActual = passwordActual,
					PasswordNueva = passwordNueva,
					ConfirmarPassword = confirmarPassword
				};

				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync($"{_baseUrl}/auth/change-password", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Change password response: {response.StatusCode} - {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<ChangePasswordResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
					return apiResponse ?? new ApiResponse<ChangePasswordResponse> { Success = false, Message = "Error procesando respuesta" };
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<ChangePasswordResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error cambiando contraseña",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 Change password error: {ex.Message}");
				return new ApiResponse<ChangePasswordResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}