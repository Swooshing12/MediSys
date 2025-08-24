using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediSys.Models;
using System.Text.Json;

namespace MediSys.Services
{
	public class MediSysApiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public MediSysApiService()
		{
			_httpClient = new HttpClient();
			_baseUrl = "http://192.168.100.16/MenuDinamico/api"; // 🔥 TU URL LOCAL

			// Configurar headers
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MediSys-MAUI/1.0");

			// Timeout de 30 segundos
			_httpClient.Timeout = TimeSpan.FromSeconds(30);
		}

		public async Task<ApiResponse<LoginResponse>> LoginAsync(string correo, string password)
		{
			try
			{
				var loginRequest = new LoginRequest
				{
					Correo = correo,
					Password = password
				};

				var json = JsonSerializer.Serialize(loginRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					// Login exitoso
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = "Error al procesar respuesta del servidor"
					};
				}
				else
				{
					// Error de login - puede incluir bloqueo por intentos
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<LoginResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error de autenticación",
						Code = (int)response.StatusCode,
						Details = errorResponse?.Details
					};
				}
			}
			catch (TaskCanceledException)
			{
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = "Tiempo de espera agotado. Verifique su conexión.",
					Code = 408
				};
			}
			catch (HttpRequestException)
			{
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = "Error de conexión. Verifique que el servidor esté disponible.",
					Code = 500
				};
			}
			catch (Exception ex)
			{
				return new ApiResponse<LoginResponse>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
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