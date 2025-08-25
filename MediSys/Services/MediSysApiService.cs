using MediSys.Models;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Text.RegularExpressions;

namespace MediSys.Services
{
	public class MediSysApiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;
		private string _sessionId = null; // Manejar sesión manualmente

		public MediSysApiService()
		{
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
				UseCookies = false // Deshabitar cookies automáticas
			};

			_httpClient = new HttpClient(handler);
			_baseUrl = "http://192.168.100.16/MenuDinamico/api";

			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MediSys-MAUI/1.0");
			_httpClient.Timeout = TimeSpan.FromSeconds(60);

			System.Diagnostics.Debug.WriteLine("API Service initialized with MANUAL cookie handling");
		}

		public async Task<ApiResponse<LoginResponse>> LoginAsync(string correo, string password)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"Starting login for: {correo}");
				System.Diagnostics.Debug.WriteLine($"Current session ID: {_sessionId ?? "NULL"}");

				var loginRequest = new LoginRequest
				{
					Correo = correo,
					Password = password
				};

				var json = JsonSerializer.Serialize(loginRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var url = $"{_baseUrl}/auth/login";

				// Crear request manual
				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = content;

				// Agregar cookie de sesión manualmente si existe
				if (!string.IsNullOrEmpty(_sessionId))
				{
					request.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
					System.Diagnostics.Debug.WriteLine($"Sending cookie: PHPSESSID={_sessionId}");
				}

				System.Diagnostics.Debug.WriteLine($"Making request to: {url}");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

				// Extraer cookie de sesión de la respuesta
				if (response.Headers.Contains("Set-Cookie"))
				{
					var setCookies = response.Headers.GetValues("Set-Cookie");
					foreach (var cookie in setCookies)
					{
						System.Diagnostics.Debug.WriteLine($"Received Set-Cookie: {cookie}");

						// Extraer PHPSESSID
						var match = Regex.Match(cookie, @"PHPSESSID=([^;]+)");
						if (match.Success)
						{
							_sessionId = match.Groups[1].Value;
							System.Diagnostics.Debug.WriteLine($"Extracted session ID: {_sessionId}");
						}
					}
				}

				System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					System.Diagnostics.Debug.WriteLine("Login successful!");
					return apiResponse ?? new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = "Error al procesar respuesta del servidor"
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					System.Diagnostics.Debug.WriteLine($"Login failed: {errorResponse?.Message}");
					return new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error de autenticación",
						Code = (int)response.StatusCode,
						Details = errorResponse?.Details
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string correo)
		{
			try
			{
				var request = new ForgotPasswordRequest { Correo = correo };
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/enviar-clave-temporal");
				httpRequest.Content = content;

				if (!string.IsNullOrEmpty(_sessionId))
				{
					httpRequest.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
				}

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<ForgotPasswordResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return apiResponse ?? new ApiResponse<ForgotPasswordResponse> { Success = false, Message = "Error procesando respuesta" };
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return new ApiResponse<ForgotPasswordResponse> { Success = false, Message = errorResponse?.Message ?? "Error enviando clave temporal", Code = (int)response.StatusCode };
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<ForgotPasswordResponse> { Success = false, Message = $"Error: {ex.Message}", Code = 500 };
			}
		}

		public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(string correo, string passwordActual, string passwordNueva, string confirmarPassword)
		{
			try
			{
				var request = new ChangePasswordRequest { Correo = correo, PasswordActual = passwordActual, PasswordNueva = passwordNueva, ConfirmarPassword = confirmarPassword };
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/change-password");
				httpRequest.Content = content;

				if (!string.IsNullOrEmpty(_sessionId))
				{
					httpRequest.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
				}

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<ChangePasswordResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return apiResponse ?? new ApiResponse<ChangePasswordResponse> { Success = false, Message = "Error procesando respuesta" };
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return new ApiResponse<ChangePasswordResponse> { Success = false, Message = errorResponse?.Message ?? "Error cambiando contraseña", Code = (int)response.StatusCode };
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<ChangePasswordResponse> { Success = false, Message = $"Error: {ex.Message}", Code = 500 };
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}