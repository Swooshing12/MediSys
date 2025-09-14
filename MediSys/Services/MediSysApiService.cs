using MediSys.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static MediSys.Models.CitaExtensions;

namespace MediSys.Services
{
	public class MediSysApiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;
		private string _sessionId = null;
		// ✅ NUEVAS PROPIEDADES PARA JWT
		private static string? _sharedJwtToken;
		private static DateTime? _sharedTokenExpiration;
		private static User? _sharedCurrentUser;


		public MediSysApiService()
		{
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
				UseCookies = false
			};

			_httpClient = new HttpClient(handler);
			_baseUrl = "http://192.168.100.17/MenuDinamico/api";

			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "MediSys-MAUI/1.0");
			_httpClient.Timeout = TimeSpan.FromSeconds(60);

			System.Diagnostics.Debug.WriteLine("API Service initialized with MANUAL cookie handling");
		}


		private async Task<HttpResponseMessage> SendRequestWithSessionAsync(HttpRequestMessage request)
		{
			if (!string.IsNullOrEmpty(_sessionId))
			{
				request.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
			}

			var response = await _httpClient.SendAsync(request);

			// Extraer cookie de sesión de la respuesta
			if (response.Headers.Contains("Set-Cookie"))
			{
				var setCookies = response.Headers.GetValues("Set-Cookie");
				foreach (var cookie in setCookies)
				{
					var match = Regex.Match(cookie, @"PHPSESSID=([^;]+)");
					if (match.Success)
					{
						_sessionId = match.Groups[1].Value;
					}
				}
			}

			return response;
		}

		// ✅ MODIFICAR: IsTokenValid usa variables estáticas
		private bool IsTokenValid()
		{
			try
			{
				if (string.IsNullOrEmpty(_sharedJwtToken))
				{
					System.Diagnostics.Debug.WriteLine("❌ No hay JWT token compartido");
					return false;
				}

				if (!_sharedTokenExpiration.HasValue)
				{
					System.Diagnostics.Debug.WriteLine("❌ No hay fecha de expiración del token compartido");
					return false;
				}

				var now = DateTime.Now;
				var expiry = _sharedTokenExpiration.Value;
				var timeRemaining = expiry.Subtract(now);

				System.Diagnostics.Debug.WriteLine($"🕐 Token compartido expira: {expiry:yyyy-MM-dd HH:mm:ss}");
				System.Diagnostics.Debug.WriteLine($"🕐 Hora actual: {now:yyyy-MM-dd HH:mm:ss}");
				System.Diagnostics.Debug.WriteLine($"🕐 Tiempo restante: {timeRemaining.TotalMinutes:F2} minutos");

				if (expiry <= now.AddMinutes(5))
				{
					System.Diagnostics.Debug.WriteLine("❌ Token compartido expirado o por expirar");
					// Limpiar token expirado
					_sharedJwtToken = null;
					_sharedTokenExpiration = null;
					_sharedCurrentUser = null;
					return false;
				}

				System.Diagnostics.Debug.WriteLine("✅ Token compartido válido");
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error validando token compartido: {ex.Message}");
				return false;
			}
		}


		// ✅ MODIFICAR: AddAuthHeaders usa token estático
		private void AddAuthHeaders(HttpRequestMessage request)
		{
			// Agregar JWT token compartido si está disponible
			if (!string.IsNullOrEmpty(_sharedJwtToken))
			{
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sharedJwtToken);
				System.Diagnostics.Debug.WriteLine($"Added shared JWT token: Bearer {_sharedJwtToken.Substring(0, 20)}...");
			}

			// Mantener sesión PHP para compatibilidad
			if (!string.IsNullOrEmpty(_sessionId))
			{
				request.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
				System.Diagnostics.Debug.WriteLine($"Added session cookie: PHPSESSID={_sessionId}");
			}
		}

		// ✅ MÉTODO BASE PARA REQUESTS QUE REQUIEREN AUTENTICACIÓN
		private async Task<HttpRequestMessage> CreateAuthenticatedRequest(HttpMethod method, string endpoint)
		{
			var url = $"{_baseUrl}{endpoint}";
			var request = new HttpRequestMessage(method, url);

			// Agregar headers de autenticación
			AddAuthHeaders(request);

			return request;
		}

		// ✅ MÉTODO PARA HACER GET CON AUTENTICACIÓN
		private async Task<ApiResponse<T>> MakeAuthenticatedGetAsync<T>(string endpoint)
		{
			try
			{
				if (!IsTokenValid())
				{
					return new ApiResponse<T>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<T>
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

					return new ApiResponse<T>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error en la solicitud",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<T>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
					Code = 500
				};
			}
		}

		// ✅ AGREGAR ESTOS MÉTODOS A TU CLASE:
		private async Task<ApiResponse<T>> MakeAuthenticatedPostAsync<T>(string endpoint, object data)
		{
			try
			{
				if (!IsTokenValid())
				{
					return new ApiResponse<T>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);

				if (data != null)
				{
					var json = JsonSerializer.Serialize(data);
					request.Content = new StringContent(json, Encoding.UTF8, "application/json");
				}

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<T>
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

					return new ApiResponse<T>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error en la solicitud",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<T>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
					Code = 500
				};
			}
		}

		// ✅ MODIFICAR: Logout limpia variables estáticas
		public void Logout()
		{
			_sharedJwtToken = null;
			_sharedTokenExpiration = null;
			_sharedCurrentUser = null;
			_sessionId = null;

			System.Diagnostics.Debug.WriteLine("🚪 Shared JWT token and session cleared - User logged out");
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
				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = content;

				// Solo agregar sesión PHP (no JWT porque es login)
				if (!string.IsNullOrEmpty(_sessionId))
				{
					request.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
					System.Diagnostics.Debug.WriteLine($"Sending cookie: PHPSESSID={_sessionId}");
				}

				System.Diagnostics.Debug.WriteLine($"Making request to: {url}");
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

				// Procesar cookies de sesión
				if (response.Headers.Contains("Set-Cookie"))
				{
					var setCookies = response.Headers.GetValues("Set-Cookie");
					foreach (var cookie in setCookies)
					{
						System.Diagnostics.Debug.WriteLine($"Received Set-Cookie: {cookie}");
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

					// ✅ CAMBIO: GUARDAR EN VARIABLES ESTÁTICAS COMPARTIDAS
					if (apiResponse?.Data != null && !string.IsNullOrEmpty(apiResponse.Data.Token))
					{
						_sharedJwtToken = apiResponse.Data.Token;  // ✅ CAMBIO: _sharedJwtToken
						_sharedCurrentUser = apiResponse.Data.Usuario;  // ✅ AGREGAR: guardar usuario

						// Calcular expiración del token
						if (apiResponse.Data.ExpiresIn > 0)
						{
							_sharedTokenExpiration = DateTime.Now.AddSeconds(apiResponse.Data.ExpiresIn);  // ✅ CAMBIO: _sharedTokenExpiration
							System.Diagnostics.Debug.WriteLine($"✅ Shared token expiration calculated from ExpiresIn: {_sharedTokenExpiration}");
						}
						else if (!string.IsNullOrEmpty(apiResponse.Data.ExpiresAt))
						{
							if (DateTime.TryParseExact(apiResponse.Data.ExpiresAt, "yyyy-MM-dd HH:mm:ss",
								System.Globalization.CultureInfo.InvariantCulture,
								System.Globalization.DateTimeStyles.None, out DateTime expDate))
							{
								_sharedTokenExpiration = expDate;  // ✅ CAMBIO: _sharedTokenExpiration
								System.Diagnostics.Debug.WriteLine($"✅ Shared token expiration parsed from ExpiresAt: {_sharedTokenExpiration}");
							}
						}

						System.Diagnostics.Debug.WriteLine($"Shared JWT token saved: {_sharedJwtToken.Substring(0, 20)}...");
						System.Diagnostics.Debug.WriteLine($"Shared current user saved: {_sharedCurrentUser?.NombreCompleto} ({_sharedCurrentUser?.Rol})");
					}

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

		// MÉTODOS EXISTENTES (ForgotPassword, ChangePassword...)
		public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string correo)
		{
			try
			{
				var request = new ForgotPasswordRequest { Correo = correo };
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/enviar-clave-temporal");
				httpRequest.Content = content;

				var response = await SendRequestWithSessionAsync(httpRequest);
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

				var response = await SendRequestWithSessionAsync(httpRequest);
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

		// NUEVOS MÉTODOS PARA HISTORIAL CLÍNICO
		public async Task<ApiResponse<PacienteResponse>> BuscarPacienteAsync(string cedula)
		{
			try
			{
				// ✅ VERIFICAR TOKEN ANTES DE HACER LA REQUEST
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("❌ Token inválido o expirado - BuscarPaciente");
					return new ApiResponse<PacienteResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				// ✅ USAR CreateAuthenticatedRequest EN LUGAR DE URL DIRECTA
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, $"/pacientes/buscar/{cedula}");

				System.Diagnostics.Debug.WriteLine($"Buscando paciente con JWT: {cedula}");

				// ✅ USAR _httpClient.SendAsync EN LUGAR DE SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Buscar paciente response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<Dictionary<string, PacienteResponse>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse?.Success == true && apiResponse.Data?.ContainsKey("paciente") == true)
					{
						return new ApiResponse<PacienteResponse>
						{
							Success = true,
							Message = apiResponse.Message,
							Data = apiResponse.Data["paciente"]
						};
					}
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO
					System.Diagnostics.Debug.WriteLine("❌ Token expirado en BuscarPaciente - limpiando datos");
					Logout();

					return new ApiResponse<PacienteResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}

				var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return new ApiResponse<PacienteResponse>
				{
					Success = false,
					Message = errorResponse?.Message ?? "Paciente no encontrado",
					Code = (int)response.StatusCode
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error buscando paciente: {ex.Message}");
				return new ApiResponse<PacienteResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<HistorialCompletoResponse>> ObtenerHistorialAsync(string cedula, HistorialClinicoFiltros? filtros = null)
		{
			try
			{
				// ✅ VERIFICAR TOKEN ANTES DE HACER LA REQUEST
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("❌ Token inválido o expirado - ObtenerHistorial");
					return new ApiResponse<HistorialCompletoResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/historial/{cedula}/filtros";
				var queryParams = new List<string>();

				if (filtros != null)
				{
					if (!string.IsNullOrEmpty(filtros.FechaDesde))
						queryParams.Add($"fecha_desde={filtros.FechaDesde}");
					if (!string.IsNullOrEmpty(filtros.FechaHasta))
						queryParams.Add($"fecha_hasta={filtros.FechaHasta}");
					if (filtros.IdEspecialidad.HasValue)
						queryParams.Add($"id_especialidad={filtros.IdEspecialidad}");
					if (filtros.IdDoctor.HasValue)
						queryParams.Add($"id_doctor={filtros.IdDoctor}");
					if (!string.IsNullOrEmpty(filtros.Estado))
						queryParams.Add($"estado={filtros.Estado}");
					if (filtros.IdSucursal.HasValue)
						queryParams.Add($"id_sucursal={filtros.IdSucursal}");

					queryParams.Add($"pagina={filtros.Pagina}");
					queryParams.Add($"por_pagina={filtros.PorPagina}");
				}
				else
				{
					queryParams.Add("pagina=1");
					queryParams.Add("por_pagina=10");
				}

				if (queryParams.Count > 0)
					endpoint += "?" + string.Join("&", queryParams);

				System.Diagnostics.Debug.WriteLine($"🔗 Historial endpoint: {endpoint}");

				// ✅ USAR CreateAuthenticatedRequest
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);

				// ✅ USAR _httpClient.SendAsync EN LUGAR DE SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📊 Historial Status: {response.StatusCode}");

				if (response.IsSuccessStatusCode)
				{
					var options = new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
						NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
						AllowTrailingCommas = true,
						ReadCommentHandling = JsonCommentHandling.Skip,
						Converters = {
					new DecimalConverter(),
					new NullableDecimalConverter(),
					new NullableIntConverter(),
					new NullableStringConverter()
				}
					};

					var apiResponse = JsonSerializer.Deserialize<ApiResponse<HistorialCompletoResponse>>(responseContent, options);

					if (apiResponse?.Success == true && apiResponse.Data != null)
					{
						System.Diagnostics.Debug.WriteLine($"✅ Historial cargado - Página: {apiResponse.Data.Paginacion?.PaginaActual}, Citas: {apiResponse.Data.Citas?.Count}");
						return apiResponse;
					}

					return new ApiResponse<HistorialCompletoResponse>
					{
						Success = false,
						Message = apiResponse?.Message ?? "Error procesando historial"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO
					System.Diagnostics.Debug.WriteLine("❌ Token expirado en ObtenerHistorial - limpiando datos");
					Logout();

					return new ApiResponse<HistorialCompletoResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}

				var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent);
				return new ApiResponse<HistorialCompletoResponse>
				{
					Success = false,
					Message = errorResponse?.Message ?? "Error obteniendo historial",
					Code = (int)response.StatusCode
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo historial: {ex.Message}");
				return new ApiResponse<HistorialCompletoResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesPacienteAsync(string cedula)
		{
			try
			{
				// Verificar token antes de hacer la request
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerEspecialidadesPaciente");
					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				// Usar CreateAuthenticatedRequest para agregar JWT automáticamente
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, $"/especialidades/paciente/{cedula}");

				// Usar _httpClient.SendAsync en lugar de SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Especialidad>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Manejar token expirado
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerEspecialidadesPaciente - limpiando datos");
					Logout();

					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}

				var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = errorResponse?.Message ?? "Error obteniendo especialidades",
					Code = (int)response.StatusCode
				};
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<List<Doctor>>> ObtenerDoctoresPorEspecialidadPacienteAsync(int idEspecialidad, string cedula)
		{
			try
			{
				// Verificar token antes de hacer la request
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerDoctoresPorEspecialidadPaciente");
					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				// Usar CreateAuthenticatedRequest para agregar JWT automáticamente
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, $"/doctores/especialidad/{idEspecialidad}/paciente/{cedula}");

				// Usar _httpClient.SendAsync en lugar de SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Doctor>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Manejar token expirado
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerDoctoresPorEspecialidadPaciente - limpiando datos");
					Logout();

					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}

				var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = errorResponse?.Message ?? "Error obteniendo doctores",
					Code = (int)response.StatusCode
				};
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// Método fallback para casos extremos
		private ApiResponse<HistorialCompletoResponse>? CreateFallbackResponse(string jsonContent)
		{
			try
			{
				using var doc = JsonDocument.Parse(jsonContent);
				var root = doc.RootElement;

				if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
				{
					var data = root.GetProperty("data");
					var citas = new List<CitaMedica>();
					var estadisticas = new EstadisticasHistorial();

					// Intentar extraer citas
					if (data.TryGetProperty("citas_medicas", out var citasProp))
					{
						var citasJson = citasProp.GetRawText();
						var citasTemp = JsonSerializer.Deserialize<List<CitaMedica>>(citasJson, GetJsonOptions());
						if (citasTemp != null) citas = citasTemp;
					}
					else if (data.TryGetProperty("citas", out var citasProp2))
					{
						var citasJson = citasProp2.GetRawText();
						var citasTemp = JsonSerializer.Deserialize<List<CitaMedica>>(citasJson, GetJsonOptions());
						if (citasTemp != null) citas = citasTemp;
					}

					// Intentar extraer estadísticas
					if (data.TryGetProperty("estadisticas", out var estadisticasProp))
					{
						var estadisticasJson = estadisticasProp.GetRawText();
						var estadisticasTemp = JsonSerializer.Deserialize<EstadisticasHistorial>(estadisticasJson, GetJsonOptions());
						if (estadisticasTemp != null) estadisticas = estadisticasTemp;
					}

					return new ApiResponse<HistorialCompletoResponse>
					{
						Success = true,
						Message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Historial obtenido",
						Data = new HistorialCompletoResponse
						{
							Citas = citas,
							Estadisticas = estadisticas,
							FiltrosAplicados = new Dictionary<string, object>()
						}
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Fallback parsing failed: {ex.Message}");
			}

			return null;
		}

		


		// Convertidores adicionales necesarios
		public class NullableIntConverter : JsonConverter<int?>
		{
			public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
					return null;
				if (reader.TokenType == JsonTokenType.String)
				{
					return int.TryParse(reader.GetString(), out var value) ? value : null;
				}
				return reader.GetInt32();
			}

			public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
			{
				if (value.HasValue)
					writer.WriteNumberValue(value.Value);
				else
					writer.WriteNullValue();
			}
		}

		public class DecimalConverter : JsonConverter<decimal>
		{
			public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					return decimal.TryParse(reader.GetString(), out var value) ? value : 0m;
				}
				if (reader.TokenType == JsonTokenType.Number)
				{
					return reader.GetDecimal();
				}
				return 0m;
			}

			public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
			{
				writer.WriteNumberValue(value);
			}
		}

		public class NullableDecimalConverter : JsonConverter<decimal?>
		{
			public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
					return null;
				if (reader.TokenType == JsonTokenType.String)
				{
					return decimal.TryParse(reader.GetString(), out var value) ? value : null;
				}
				if (reader.TokenType == JsonTokenType.Number)
				{
					return reader.GetDecimal();
				}
				return null;
			}

			public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
			{
				if (value.HasValue)
					writer.WriteNumberValue(value.Value);
				else
					writer.WriteNullValue();
			}
		}

		public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
		{
			try
			{
				// Verificar token antes de hacer la request
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerEspecialidades");
					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Especialidad>()
					};
				}

				// Usar CreateAuthenticatedRequest para agregar JWT automáticamente
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, "/especialidades");

				// Usar _httpClient.SendAsync en lugar de SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Especialidades response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Especialidad>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse?.Success == true && apiResponse.Data != null)
					{
						System.Diagnostics.Debug.WriteLine($"Especialidades count: {apiResponse.Data.Count}");
						return apiResponse;
					}
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Manejar token expirado
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerEspecialidades - limpiando datos");
					Logout();

					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Especialidad>()
					};
				}

				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = $"Error del servidor: {response.StatusCode}",
					Data = new List<Especialidad>()
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error especialidades: {ex.Message}");
				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Data = new List<Especialidad>()
				};
			}
		}

		public async Task<ApiResponse<List<Doctor>>> ObtenerDoctoresPorEspecialidadAsync(int idEspecialidad)
		{
			try
			{
				// Verificar token antes de hacer la request
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerDoctoresPorEspecialidad");
					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				// Usar CreateAuthenticatedRequest para agregar JWT automáticamente
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, $"/doctores/especialidad/{idEspecialidad}");

				// Usar _httpClient.SendAsync en lugar de SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Doctor>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Error procesando doctores"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Manejar token expirado
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerDoctoresPorEspecialidad - limpiando datos");
					Logout();

					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}

				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = "Error obteniendo doctores",
					Code = (int)response.StatusCode
				};
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
		{
			try
			{
				// Verificar token antes de hacer la request
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerSucursales");
					return new ApiResponse<List<Sucursal>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Sucursal>()
					};
				}

				// Usar CreateAuthenticatedRequest para agregar JWT automáticamente
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, "/sucursales");

				System.Diagnostics.Debug.WriteLine($"Sucursales endpoint: /sucursales");

				// Usar _httpClient.SendAsync en lugar de SendRequestWithSessionAsync
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Sucursales Status: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"Sucursales Response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					try
					{
						var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Sucursal>>>(responseContent, GetJsonOptions());

						if (apiResponse?.Success == true && apiResponse.Data != null)
						{
							System.Diagnostics.Debug.WriteLine($"Sucursales deserializadas: {apiResponse.Data.Count}");
							foreach (var sucursal in apiResponse.Data)
							{
								System.Diagnostics.Debug.WriteLine($"Sucursal: {sucursal.IdSucursal} - {sucursal.Nombre}");
							}
							return apiResponse;
						}
						else
						{
							System.Diagnostics.Debug.WriteLine($"Sucursales API unsuccessful: {apiResponse?.Message}");
						}
					}
					catch (JsonException jsonEx)
					{
						System.Diagnostics.Debug.WriteLine($"Sucursales JSON Error: {jsonEx.Message}");
					}
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Manejar token expirado
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerSucursales - limpiando datos");
					Logout();

					return new ApiResponse<List<Sucursal>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Sucursal>()
					};
				}

				return new ApiResponse<List<Sucursal>>
				{
					Success = false,
					Message = $"Error del servidor: {response.StatusCode}",
					Data = new List<Sucursal>()
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Sucursales Exception: {ex.Message}");
				return new ApiResponse<List<Sucursal>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Data = new List<Sucursal>()
				};
			}
		}

		/// <summary>
		/// Buscar médico por cédula - MÉTODO CORREGIDO
		/// </summary>
		public async Task<ApiResponse<MedicoCompleto>> BuscarMedicoPorCedulaAsync(string cedula)
		{
			try
			{
				// ✅ VALIDACIÓN DE ENTRADA
				if (string.IsNullOrWhiteSpace(cedula))
				{
					System.Diagnostics.Debug.WriteLine("❌ Cédula vacía o nula");
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Cédula es requerida",
						Code = 400
					};
				}

				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - BuscarMedicoPorCedula");
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/doctores-api?action=buscarPorCedula&cedula={cedula}";
				System.Diagnostics.Debug.WriteLine($"🔍 Buscando médico por cédula: {cedula}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<MedicoCompleto>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en BuscarMedicoPorCedula - limpiando datos");
					Logout(); // Usar método static si lo tienes
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Médico no encontrado",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error buscando médico: {ex.Message}");
				return new ApiResponse<MedicoCompleto>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}




		/// <summary>
		/// Listar médicos con filtros y paginación
		/// </summary>
		public async Task<ApiResponse<MedicosResponse>> ListarMedicosAsync(int page = 1, int limit = 10, string search = "", int especialidad = 0)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ListarMedicos");
					return new ApiResponse<MedicosResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/doctores-api?action=listar&page={page}&limit={limit}";

				if (!string.IsNullOrEmpty(search))
					endpoint += $"&search={Uri.EscapeDataString(search)}";

				if (especialidad > 0)
					endpoint += $"&especialidad={especialidad}";

				System.Diagnostics.Debug.WriteLine($"👨‍⚕️ Listando médicos - Página: {page}, Límite: {limit}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<MedicosResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<MedicosResponse>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ListarMedicos - limpiando datos");
					Logout();
					return new ApiResponse<MedicosResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicosResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo médicos",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<MedicosResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// 🔥 TAMBIÉN AGREGAR MÉTODO PARA CREAR MÉDICO (que faltaba)
		public async Task<ApiResponse<MedicoCompleto>> CrearMedicoAsync(CrearMedicoRequest medico)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CrearMedico");
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/doctores-api?action=crear";
				var json = JsonSerializer.Serialize(medico, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				System.Diagnostics.Debug.WriteLine($"📤 Creando médico: {json}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<MedicoCompleto>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CrearMedico - limpiando datos");
					Logout();
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando médico",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<MedicoCompleto>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// ===== HORARIOS =====

		/// <summary>
		/// Obtener horarios de un médico
		/// </summary>
		public async Task<ApiResponse<HorariosResponse>> ObtenerHorariosAsync(int idDoctor, int idSucursal = 0)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerHorarios");
					return new ApiResponse<HorariosResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/doctores-api?action=obtenerHorarios&id_doctor={idDoctor}";

				if (idSucursal > 0)
					endpoint += $"&id_sucursal={idSucursal}";

				System.Diagnostics.Debug.WriteLine($"🕐 Obteniendo horarios: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Horarios response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<HorariosResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<HorariosResponse>
					{
						Success = false,
						Message = "Error procesando respuesta de horarios"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerHorarios - limpiando datos");
					Logout();
					return new ApiResponse<HorariosResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<HorariosResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo horarios",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<HorariosResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// Guardar/actualizar horarios de un médico
		/// </summary>
		/// <summary>
		/// Guardar/actualizar horarios de un médico
		/// </summary>
		public async Task<ApiResponse<object>> GuardarHorariosAsync(GuardarHorariosRequest request)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - GuardarHorarios");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/doctores-api?action=guardarHorarios";
				var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				System.Diagnostics.Debug.WriteLine($"💾 Guardando horarios: {json}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Guardar horarios response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<object>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en GuardarHorarios - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error guardando horarios",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// 1. Obtener tipos de cita disponibles
		/// </summary>
		/// <summary>
		/// 1. Obtener tipos de cita disponibles
		/// </summary>
		public async Task<ApiResponse<List<TipoCita>>> ObtenerTiposCitaAsync()
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerTiposCita");
					return new ApiResponse<List<TipoCita>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<TipoCita>()
					};
				}

				var endpoint = "/tipos-cita";

				System.Diagnostics.Debug.WriteLine($"📋 Obteniendo tipos de cita: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Tipos cita response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TipoCita>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<TipoCita>>
					{
						Success = false,
						Message = "Error procesando respuesta",
						Data = new List<TipoCita>()
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerTiposCita - limpiando datos");
					Logout();
					return new ApiResponse<List<TipoCita>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<TipoCita>()
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<List<TipoCita>>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo tipos de cita",
						Code = (int)response.StatusCode,
						Data = new List<TipoCita>()
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<TipoCita>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500,
					Data = new List<TipoCita>()
				};
			}
		}

		/// <summary>
		/// 2. Buscar paciente por cédula
		/// </summary>
		/// <summary>
		/// 2. Buscar paciente por cédula - CORREGIDO
		/// </summary>
		/// <summary>
		/// 2. Buscar paciente por cédula - CORREGIDO
		/// </summary>
		public async Task<ApiResponse<PacienteBusqueda>> BuscarPacientePorCedulaAsync(string cedula)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - BuscarPacientePorCedula");
					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				// ✅ LA URL SIGUE USANDO STRING PORQUE ASÍ LO ESPERA LA API
				var endpoint = $"/pacientes/buscar/{cedula}";

				System.Diagnostics.Debug.WriteLine($"🔍 Buscando paciente: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Paciente response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<BuscarPacienteResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse?.Success == true && apiResponse.Data?.Paciente != null)
					{
						return new ApiResponse<PacienteBusqueda>
						{
							Success = true,
							Message = apiResponse.Message ?? "Paciente encontrado",
							Data = apiResponse.Data.Paciente
						};
					}
					else
					{
						return new ApiResponse<PacienteBusqueda>
						{
							Success = false,
							Message = apiResponse?.Message ?? "Error procesando respuesta"
						};
					}
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = "Paciente no encontrado",
						Code = 404
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en BuscarPacientePorCedula - limpiando datos");
					Logout();
					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error buscando paciente",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception buscando paciente: {ex.Message}");
				return new ApiResponse<PacienteBusqueda>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// 3. Crear nuevo paciente
		/// </summary>
		/// <summary>
		/// 3. Crear nuevo paciente - CORREGIDO
		/// </summary>
		/// <summary>
		/// 3. Crear nuevo paciente - CORREGIDO CON JWT
		/// </summary>
		public async Task<ApiResponse<PacienteBusqueda>> CrearPacienteAsync(CrearPacienteRequest pacienteData)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CrearPaciente");
					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/pacientes/crear2";

				System.Diagnostics.Debug.WriteLine($"👤 Creando paciente: {_baseUrl}{endpoint}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(pacienteData)}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				request.Content = new StringContent(JsonSerializer.Serialize(pacienteData), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Crear paciente response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					// ✅ EL ENDPOINT DE CREAR PACIENTE DEVUELVE EL PACIENTE DIRECTAMENTE EN DATA
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<PacienteBusqueda>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse?.Success == true && apiResponse.Data != null)
					{
						return apiResponse;
					}
					else
					{
						return new ApiResponse<PacienteBusqueda>
						{
							Success = false,
							Message = apiResponse?.Message ?? "Error procesando respuesta"
						};
					}
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CrearPaciente - limpiando datos");
					Logout();
					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando paciente",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception creando paciente: {ex.Message}");
				return new ApiResponse<PacienteBusqueda>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// 4. Obtener especialidades disponibles en una sucursal
		/// </summary>
		/// <summary>
		/// 4. Obtener especialidades disponibles en una sucursal
		/// </summary>
		public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesPorSucursalAsync(int idSucursal)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerEspecialidadesPorSucursal");
					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Especialidad>()
					};
				}

				var endpoint = $"/especialidades/sucursal/{idSucursal}";

				System.Diagnostics.Debug.WriteLine($"🏥 Obteniendo especialidades por sucursal: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Especialidades por sucursal response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Especialidad>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Error procesando respuesta",
						Data = new List<Especialidad>()
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerEspecialidadesPorSucursal - limpiando datos");
					Logout();
					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Especialidad>()
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo especialidades",
						Code = (int)response.StatusCode,
						Data = new List<Especialidad>()
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500,
					Data = new List<Especialidad>()
				};
			}
		}
		/// <summary>
		/// 5. Obtener doctores de una especialidad en una sucursal específica
		/// </summary>
		public async Task<ApiResponse<List<Doctor>>> ObtenerDoctoresPorEspecialidadYSucursalAsync(int idEspecialidad, int idSucursal)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerDoctoresPorEspecialidadYSucursal");
					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Doctor>()
					};
				}

				var endpoint = $"/doctores/especialidad/{idEspecialidad}/sucursal/{idSucursal}";

				System.Diagnostics.Debug.WriteLine($"👨‍⚕️ Obteniendo doctores: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Doctores response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Doctor>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Error procesando respuesta",
						Data = new List<Doctor>()
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerDoctoresPorEspecialidadYSucursal - limpiando datos");
					Logout();
					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<Doctor>()
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo doctores",
						Code = (int)response.StatusCode,
						Data = new List<Doctor>()
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500,
					Data = new List<Doctor>()
				};
			}
		}

		/// <summary>
		/// 6. Obtener horarios disponibles de un doctor en una semana específica
		/// </summary>
		/// <summary>
		/// 6. Obtener horarios disponibles de un doctor en una semana específica
		/// </summary>
		public async Task<ApiResponse<HorariosDisponiblesResponse>> ObtenerHorariosDisponiblesAsync(int idDoctor, int idSucursal, string semana)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerHorariosDisponibles");
					return new ApiResponse<HorariosDisponiblesResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/horarios/disponibles?id_doctor={idDoctor}&id_sucursal={idSucursal}&semana={semana}";

				System.Diagnostics.Debug.WriteLine($"🕐 Obteniendo horarios disponibles: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Horarios disponibles response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<HorariosDisponiblesResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<HorariosDisponiblesResponse>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerHorariosDisponibles - limpiando datos");
					Logout();
					return new ApiResponse<HorariosDisponiblesResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<HorariosDisponiblesResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo horarios",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<HorariosDisponiblesResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// 7. Crear nueva cita médica
		/// </summary>
		/// <summary>
		/// 7. Crear nueva cita médica
		/// </summary>
		public async Task<ApiResponse<CitaMedica2>> CrearCitaAsync(CrearCitaRequest citaData)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CrearCita");
					return new ApiResponse<CitaMedica2>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/citas/crear";

				System.Diagnostics.Debug.WriteLine($"📅 Creando cita: {_baseUrl}{endpoint}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(citaData)}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				request.Content = new StringContent(JsonSerializer.Serialize(citaData), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Crear cita response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<CitaMedica2>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<CitaMedica2>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CrearCita - limpiando datos");
					Logout();
					return new ApiResponse<CitaMedica2>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<CitaMedica2>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando cita",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<CitaMedica2>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}
		// En MediSysApiService.cs agregar:
		public async Task<ApiResponse<object>> GuardarHorariosAsync2(GuardarHorariosRequest request)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - GuardarHorariosAsync2");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/doctores/horarios";

				System.Diagnostics.Debug.WriteLine($"💾 Guardando horarios: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<object>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en GuardarHorariosAsync2 - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error guardando horarios",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}
		public async Task<ApiResponse<object>> EditarHorarioAsync(EditarHorarioRequest request)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - EditarHorario");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/horarios";

				System.Diagnostics.Debug.WriteLine($"✏️ Editando horario: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Put, endpoint);
				httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<object>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en EditarHorario - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error editando horario",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		public async Task<ApiResponse<object>> EliminarHorarioAsync(int idHorario)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - EliminarHorario");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/horarios?id={idHorario}";

				System.Diagnostics.Debug.WriteLine($"🗑️ Eliminando horario: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Delete, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<object>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en EliminarHorario - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error eliminando horario",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// Services/MediSysApiService.cs - Agregar estos métodos

		/// <summary>
		/// Obtener citas de un paciente por fecha
		/// </summary>
		public async Task<ApiResponse<List<CitaDetallada>>> ObtenerCitasPacientePorFechaAsync(string cedula, string fecha)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerCitasPacientePorFecha");
					return new ApiResponse<List<CitaDetallada>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<CitaDetallada>()
					};
				}

				var endpoint = $"/citas/paciente/{cedula}?fecha={fecha}";

				System.Diagnostics.Debug.WriteLine($"📅 Obteniendo citas del paciente: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Citas response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CitaDetallada>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<CitaDetallada>>
					{
						Success = false,
						Message = "Respuesta inválida del servidor",
						Data = new List<CitaDetallada>()
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerCitasPacientePorFecha - limpiando datos");
					Logout();
					return new ApiResponse<List<CitaDetallada>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<CitaDetallada>()
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<List<CitaDetallada>>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo citas del paciente",
						Code = (int)response.StatusCode,
						Data = new List<CitaDetallada>()
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception obteniendo citas: {ex.Message}");
				return new ApiResponse<List<CitaDetallada>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500,
					Data = new List<CitaDetallada>()
				};
			}
		}

		/// <summary>
		/// Crear triaje para una cita
		/// </summary>
		/// <summary>
		/// Crear triaje para una cita
		/// </summary>
		public async Task<ApiResponse<CrearTriajeResponse>> CrearTriajeAsync(CrearTriajeRequest triajeData)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CrearTriaje");
					return new ApiResponse<CrearTriajeResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = "/triaje/crear";

				System.Diagnostics.Debug.WriteLine($"🏥 Creando triaje: {_baseUrl}{endpoint}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(triajeData)}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				request.Content = new StringContent(JsonSerializer.Serialize(triajeData), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Crear triaje response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<CrearTriajeResponse>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<CrearTriajeResponse>
					{
						Success = false,
						Message = "Respuesta inválida del servidor"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CrearTriaje - limpiando datos");
					Logout();
					return new ApiResponse<CrearTriajeResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<CrearTriajeResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando triaje",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception creando triaje: {ex.Message}");
				return new ApiResponse<CrearTriajeResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}
		/// <summary>
		/// Obtener triaje existente de una cita
		/// </summary>
		/// <summary>
		/// Obtener triaje existente de una cita
		/// </summary>
		public async Task<ApiResponse<Triaje2>> ObtenerTriajePorCitaAsync(int idCita)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerTriajePorCita");
					return new ApiResponse<Triaje2>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/triaje/cita/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo triaje: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<Triaje2>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<Triaje2>
					{
						Success = false,
						Message = "Respuesta inválida del servidor"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerTriajePorCita - limpiando datos");
					Logout();
					return new ApiResponse<Triaje2>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<Triaje2>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Triaje no encontrado",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<Triaje2>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}
		// Services/MediSysApiService.cs - AGREGAR ESTOS MÉTODOS AL FINAL

		/// <summary>
		/// Obtener citas del doctor para consulta médica
		/// </summary>
		public async Task<ApiResponse<List<CitaConsultaMedica>>> ObtenerCitasConsultaDoctorAsync(string cedulaDoctor, string fecha, string estado = "Confirmada")
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerCitasConsultaDoctor");
					return new ApiResponse<List<CitaConsultaMedica>>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401,
						Data = new List<CitaConsultaMedica>()
					};
				}

				var endpoint = $"/consultas/doctor/{cedulaDoctor}";
				var queryParams = new List<string> { $"fecha={fecha}" };

				if (!string.IsNullOrEmpty(estado) && estado != "Todas")
				{
					queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
				}

				if (queryParams.Any())
				{
					endpoint += "?" + string.Join("&", queryParams);
				}

				System.Diagnostics.Debug.WriteLine($"🩺 Obteniendo citas doctor: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Citas doctor response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CitaConsultaMedica>>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<List<CitaConsultaMedica>>
					{
						Success = false,
						Message = "Error procesando respuesta",
						Data = new List<CitaConsultaMedica>()
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerCitasConsultaDoctor - limpiando datos");
					Logout();
					return new ApiResponse<List<CitaConsultaMedica>>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401,
						Data = new List<CitaConsultaMedica>()
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<List<CitaConsultaMedica>>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo citas del doctor",
						Code = (int)response.StatusCode,
						Data = new List<CitaConsultaMedica>()
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo citas doctor: {ex.Message}");
				return new ApiResponse<List<CitaConsultaMedica>>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500,
					Data = new List<CitaConsultaMedica>()
				};
			}
		}

		/// <summary>
		/// Crear o actualizar consulta médica
		/// </summary>
		/// <summary>
		/// Crear o actualizar consulta médica
		/// </summary>
		public async Task<ApiResponse<object>> CrearActualizarConsultaMedicaAsync(int idCita, ConsultaMedicaRequest consultaData)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CrearActualizarConsultaMedica");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/consultas/cita/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🩺 Creando/actualizando consulta: {_baseUrl}{endpoint}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos consulta: {JsonSerializer.Serialize(consultaData)}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				request.Content = new StringContent(JsonSerializer.Serialize(consultaData), Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Consulta response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<object>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CrearActualizarConsultaMedica - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error procesando consulta médica",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error procesando consulta: {ex.Message}");
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		/// <summary>
		/// Obtener detalle completo de una consulta médica
		/// </summary>
		/// <summary>
		/// Obtener detalle completo de una consulta médica
		/// </summary>
		public async Task<ApiResponse<DetalleConsulta>> ObtenerDetalleConsultaAsync(int idCita)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerDetalleConsulta");
					return new ApiResponse<DetalleConsulta>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				var endpoint = $"/consultas/detalle/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo detalle consulta: {_baseUrl}{endpoint}");

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Detalle consulta response: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<ApiResponse<DetalleConsulta>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return apiResponse ?? new ApiResponse<DetalleConsulta>
					{
						Success = false,
						Message = "Error procesando respuesta"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerDetalleConsulta - limpiando datos");
					Logout();
					return new ApiResponse<DetalleConsulta>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<DetalleConsulta>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Consulta no encontrada",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo detalle: {ex.Message}");
				return new ApiResponse<DetalleConsulta>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		// ✅ NUEVO MÉTODO para obtener información de cita (sin requerir consulta médica)
		// ✅ NUEVO MÉTODO para obtener información de cita (sin requerir consulta médica)
		public async Task<ApiResponse<ConsultaDetalleResponse>> ObtenerInformacionCitaAsync(int idCita)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ObtenerInformacionCita");
					return new ApiResponse<ConsultaDetalleResponse>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo información de cita ID: {idCita}");

				var endpoint = $"/citas/informacion/{idCita}";

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
				var response = await _httpClient.SendAsync(request);
				var content = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📡 Respuesta HTTP: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"📄 Contenido: {content}");

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<ApiResponse<ConsultaDetalleResponse>>(content, GetJsonOptions());
					System.Diagnostics.Debug.WriteLine($"✅ Información de cita obtenida exitosamente");
					return result ?? new ApiResponse<ConsultaDetalleResponse>
					{
						Success = false,
						Message = "Respuesta vacía del servidor"
					};
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ObtenerInformacionCita - limpiando datos");
					Logout();
					return new ApiResponse<ConsultaDetalleResponse>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"❌ Error HTTP {response.StatusCode}: {content}");
					return new ApiResponse<ConsultaDetalleResponse>
					{
						Success = false,
						Message = $"Error del servidor: {response.StatusCode}",
						Code = (int)response.StatusCode
					};
				}
			}
			catch (HttpRequestException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Error de conexión al servidor",
					Code = 500
				};
			}
			catch (TaskCanceledException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Timeout: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Tiempo de espera agotado",
					Code = 408
				};
			}
			catch (JsonException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error JSON: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Error procesando respuesta del servidor",
					Code = 500
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}",
					Code = 500
				};
			}
		}


		/// <summary>
		/// Actualizar estado de una cita
		/// </summary>
		// En MediSysApiService.cs - CORREGIR EL MÉTODO
		/// <summary>
		/// Actualizar estado de una cita
		/// </summary>
		public async Task<ApiResponse<object>> ActualizarEstadoCitaAsync(int idCita, string nuevoEstado)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - ActualizarEstadoCita");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				System.Diagnostics.Debug.WriteLine($"Actualizando estado de cita {idCita} a: {nuevoEstado}");

				var requestData = new { estado = nuevoEstado };
				var json = JsonSerializer.Serialize(requestData, GetJsonOptions());
				var endpoint = $"/citas/{idCita}/estado";

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var request = await CreateAuthenticatedRequest(HttpMethod.Put, endpoint);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Respuesta actualizar estado: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"Contenido: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
					return result ?? new ApiResponse<object> { Success = false, Message = "Respuesta vacía" };
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en ActualizarEstadoCita - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					try
					{
						var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
						return new ApiResponse<object>
						{
							Success = false,
							Message = errorResponse?.Message ?? $"Error del servidor: {response.StatusCode}",
							Code = (int)response.StatusCode
						};
					}
					catch
					{
						return new ApiResponse<object>
						{
							Success = false,
							Message = $"Error del servidor: {response.StatusCode}",
							Code = (int)response.StatusCode
						};
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error actualizando estado de cita: {ex.Message}");
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}
		public class NullableStringConverter : JsonConverter<string>
		{
			public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
				{
					return null;
				}
				return reader.GetString();
			}

			public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
			{
				if (value == null)
				{
					writer.WriteNullValue();
				}
				else
				{
					writer.WriteStringValue(value);
				}
			}
		}

		// ✅ CAMBIAR CONTRASEÑA PARA USUARIO LOGUEADO
		// ✅ CAMBIAR CONTRASEÑA PARA USUARIO LOGUEADO
		public async Task<ApiResponse<object>> CambiarPasswordLogueadoAsync(CambiarPasswordRequest request)
		{
			try
			{
				// ✅ VERIFICAR TOKEN COMPARTIDO
				if (!IsTokenValid())
				{
					System.Diagnostics.Debug.WriteLine("Token inválido o expirado - CambiarPasswordLogueado");
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Token expirado. Inicie sesión nuevamente.",
						Code = 401
					};
				}

				System.Diagnostics.Debug.WriteLine($"Cambiando contraseña para usuario ID: {request.IdUsuario}");
				var json = JsonSerializer.Serialize(request, GetJsonOptions());
				var endpoint = "/auth/change-password-logged";

				// ✅ USAR CreateAuthenticatedRequest CON TOKEN COMPARTIDO
				var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, endpoint);
				httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Respuesta cambio password: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"Contenido: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
					return result ?? new ApiResponse<object> { Success = false, Message = "Respuesta vacía" };
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// ✅ MANEJAR TOKEN EXPIRADO CON LOGOUT
					System.Diagnostics.Debug.WriteLine("Token expirado en CambiarPasswordLogueado - limpiando datos");
					Logout();
					return new ApiResponse<object>
					{
						Success = false,
						Message = "Sesión expirada. Por favor, inicie sesión nuevamente.",
						Code = 401
					};
				}
				else
				{
					// Tratar de extraer mensaje de error del contenido
					try
					{
						var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
						return new ApiResponse<object>
						{
							Success = false,
							Message = errorResponse?.Message ?? $"Error del servidor: {response.StatusCode}",
							Code = (int)response.StatusCode
						};
					}
					catch
					{
						return new ApiResponse<object>
						{
							Success = false,
							Message = $"Error del servidor: {response.StatusCode}",
							Code = (int)response.StatusCode
						};
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cambiando contraseña: {ex.Message}");
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Code = 500
				};
			}
		}

		private JsonSerializerOptions GetJsonOptions()
		{
			return new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				NumberHandling = JsonNumberHandling.AllowReadingFromString,
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip
			};
		}
		private T? DeserializeResponse<T>(string jsonContent) where T : class
		{
			try
			{
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					NumberHandling = JsonNumberHandling.AllowReadingFromString
				};

				return JsonSerializer.Deserialize<T>(jsonContent, options);
			}
			catch (JsonException ex)
			{
				System.Diagnostics.Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"JSON Content: {jsonContent}");
				return null;
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}