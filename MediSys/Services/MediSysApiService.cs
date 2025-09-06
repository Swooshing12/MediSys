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

		public MediSysApiService()
		{
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
				UseCookies = false
			};

			_httpClient = new HttpClient(handler);
			_baseUrl = "http://192.168.100.11/MenuDinamico/api";

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

		// MÉTODOS EXISTENTES DE AUTENTICACIÓN
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

				if (!string.IsNullOrEmpty(_sessionId))
				{
					request.Headers.Add("Cookie", $"PHPSESSID={_sessionId}");
					System.Diagnostics.Debug.WriteLine($"Sending cookie: PHPSESSID={_sessionId}");
				}

				System.Diagnostics.Debug.WriteLine($"Making request to: {url}");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

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
				var url = $"{_baseUrl}/pacientes/buscar/{cedula}";
				var request = new HttpRequestMessage(HttpMethod.Get, url);

				System.Diagnostics.Debug.WriteLine($"Buscando paciente: {cedula}");

				var response = await SendRequestWithSessionAsync(request);
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
				var url = $"{_baseUrl}/historial/{cedula}/filtros";
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
				}

				if (queryParams.Count > 0)
					url += "?" + string.Join("&", queryParams);

				System.Diagnostics.Debug.WriteLine($"Historial URL: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Historial Status: {response.StatusCode}");

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

					try
					{
						// Primero verificar si es el endpoint de filtros o el básico
						if (responseContent.Contains("filtros_aplicados"))
						{
							// Es el endpoint con filtros - deserializar normal
							var apiResponse = JsonSerializer.Deserialize<ApiResponse<HistorialCompletoResponse>>(responseContent, options);

							if (apiResponse?.Success == true && apiResponse.Data != null)
							{
								System.Diagnostics.Debug.WriteLine($"Historial con filtros - Citas: {apiResponse.Data.Citas?.Count ?? 0}");
								return apiResponse;
							}
						}
						else
						{
							// Es búsqueda básica - usar estructura simplificada
							var basicResponse = JsonSerializer.Deserialize<ApiResponse<HistorialBasicoResponse>>(responseContent, options);

							if (basicResponse?.Success == true && basicResponse.Data != null)
							{
								// Convertir a HistorialCompletoResponse
								var historialCompleto = new HistorialCompletoResponse
								{
									Citas = basicResponse.Data.CitasMedicas ?? new List<CitaMedica>(),
									Estadisticas = basicResponse.Data.Estadisticas ?? new EstadisticasHistorial(),
									FiltrosAplicados = new Dictionary<string, object>()
								};

								var apiResponse = new ApiResponse<HistorialCompletoResponse>
								{
									Success = true,
									Message = basicResponse.Message,
									Data = historialCompleto
								};

								System.Diagnostics.Debug.WriteLine($"Historial básico convertido - Citas: {historialCompleto.Citas.Count}");
								return apiResponse;
							}
						}
					}
					catch (JsonException jsonEx)
					{
						System.Diagnostics.Debug.WriteLine($"JSON Error: {jsonEx.Message}");
						System.Diagnostics.Debug.WriteLine($"Trying fallback deserialization...");

						// Fallback: crear respuesta manualmente parseando lo básico
						try
						{
							var fallbackData = CreateFallbackResponse(responseContent);
							if (fallbackData != null)
							{
								return fallbackData;
							}
						}
						catch (Exception fallbackEx)
						{
							System.Diagnostics.Debug.WriteLine($"Fallback failed: {fallbackEx.Message}");
						}

						return new ApiResponse<HistorialCompletoResponse>
						{
							Success = false,
							Message = $"Error procesando datos: {jsonEx.Message}",
							Data = new HistorialCompletoResponse()
						};
					}
				}

				return new ApiResponse<HistorialCompletoResponse>
				{
					Success = false,
					Message = $"Error del servidor: {response.StatusCode}",
					Data = new HistorialCompletoResponse()
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error general: {ex.Message}");
				return new ApiResponse<HistorialCompletoResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}",
					Data = new HistorialCompletoResponse()
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
				var url = $"{_baseUrl}/especialidades";
				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				var url = $"{_baseUrl}/doctores/especialidad/{idEspecialidad}";
				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				var url = $"{_baseUrl}/sucursales";
				System.Diagnostics.Debug.WriteLine($"Sucursales URL: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				if (string.IsNullOrWhiteSpace(cedula))
				{
					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = "Cédula es requerida"
					};
				}

				// 🔥 USAR EL NUEVO ENDPOINT buscarPorCedula
				var url = $"{_baseUrl}/doctores-api?action=buscarPorCedula&cedula={cedula}";

				System.Diagnostics.Debug.WriteLine($"🔍 Buscando médico por cédula: {cedula}");
				System.Diagnostics.Debug.WriteLine($"🔗 URL: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Médico no encontrado"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error buscando médico: {ex.Message}");
				return new ApiResponse<MedicoCompleto>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
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
				var url = $"{_baseUrl}/doctores-api?action=listar&page={page}&limit={limit}";

				if (!string.IsNullOrEmpty(search))
					url += $"&search={Uri.EscapeDataString(search)}";

				if (especialidad > 0)
					url += $"&especialidad={especialidad}";

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicosResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo médicos"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<MedicosResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		// 🔥 TAMBIÉN AGREGAR MÉTODO PARA CREAR MÉDICO (que faltaba)
		public async Task<ApiResponse<MedicoCompleto>> CrearMedicoAsync(CrearMedicoRequest medico)
		{
			try
			{
				var url = $"{_baseUrl}/doctores-api?action=crear";
				var json = JsonSerializer.Serialize(medico, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				System.Diagnostics.Debug.WriteLine($"📤 Creando médico: {json}");

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<MedicoCompleto>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando médico"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<MedicoCompleto>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
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
				var url = $"{_baseUrl}/doctores-api?action=obtenerHorarios&id_doctor={idDoctor}";

				if (idSucursal > 0)
					url += $"&id_sucursal={idSucursal}";

				System.Diagnostics.Debug.WriteLine($"🕐 Obteniendo horarios: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<HorariosResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error obteniendo horarios"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<HorariosResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Guardar/actualizar horarios de un médico
		/// </summary>
		public async Task<ApiResponse<object>> GuardarHorariosAsync(GuardarHorariosRequest request)
		{
			try
			{
				var url = $"{_baseUrl}/doctores-api?action=guardarHorarios";
				var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				System.Diagnostics.Debug.WriteLine($"💾 Guardando horarios: {json}");

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
				httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(httpRequest);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error guardando horarios"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// 1. Obtener tipos de cita disponibles
		/// </summary>
		public async Task<ApiResponse<List<TipoCita>>> ObtenerTiposCitaAsync()
		{
			try
			{
				var url = $"{_baseUrl}/tipos-cita";

				System.Diagnostics.Debug.WriteLine($"📋 Obteniendo tipos de cita: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Error procesando respuesta"
					};
				}
				else
				{
					return new ApiResponse<List<TipoCita>>
					{
						Success = false,
						Message = "Error obteniendo tipos de cita"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<TipoCita>>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
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
				// ✅ LA URL SIGUE USANDO STRING PORQUE ASÍ LO ESPERA LA API
				var url = $"{_baseUrl}/pacientes/buscar/{cedula}";

				System.Diagnostics.Debug.WriteLine($"🔍 Buscando paciente: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Paciente no encontrado"
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
						Message = errorResponse?.Message ?? "Error buscando paciente"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception buscando paciente: {ex.Message}");
				return new ApiResponse<PacienteBusqueda>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// 3. Crear nuevo paciente
		/// </summary>
		/// <summary>
		/// 3. Crear nuevo paciente - CORREGIDO
		/// </summary>
		public async Task<ApiResponse<PacienteBusqueda>> CrearPacienteAsync(CrearPacienteRequest pacienteData)
		{
			try
			{
				var url = $"{_baseUrl}/pacientes/crear2";

				System.Diagnostics.Debug.WriteLine($"👤 Creando paciente: {url}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(pacienteData)}");

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = new StringContent(JsonSerializer.Serialize(pacienteData), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<PacienteBusqueda>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando paciente"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception creando paciente: {ex.Message}");
				return new ApiResponse<PacienteBusqueda>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// 4. Obtener especialidades disponibles en una sucursal
		/// </summary>
		public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesPorSucursalAsync(int idSucursal)
		{
			try
			{
				var url = $"{_baseUrl}/especialidades/sucursal/{idSucursal}";

				System.Diagnostics.Debug.WriteLine($"🏥 Obteniendo especialidades por sucursal: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Error procesando respuesta"
					};
				}
				else
				{
					return new ApiResponse<List<Especialidad>>
					{
						Success = false,
						Message = "Error obteniendo especialidades"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Especialidad>>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
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
				var url = $"{_baseUrl}/doctores/especialidad/{idEspecialidad}/sucursal/{idSucursal}";

				System.Diagnostics.Debug.WriteLine($"👨‍⚕️ Obteniendo doctores: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Error procesando respuesta"
					};
				}
				else
				{
					return new ApiResponse<List<Doctor>>
					{
						Success = false,
						Message = "Error obteniendo doctores"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<List<Doctor>>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// 6. Obtener horarios disponibles de un doctor en una semana específica
		/// </summary>
		public async Task<ApiResponse<HorariosDisponiblesResponse>> ObtenerHorariosDisponiblesAsync(int idDoctor, int idSucursal, string semana)
		{
			try
			{
				var url = $"{_baseUrl}/horarios/disponibles?id_doctor={idDoctor}&id_sucursal={idSucursal}&semana={semana}";

				System.Diagnostics.Debug.WriteLine($"🕐 Obteniendo horarios disponibles: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					return new ApiResponse<HorariosDisponiblesResponse>
					{
						Success = false,
						Message = "Error obteniendo horarios"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<HorariosDisponiblesResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// 7. Crear nueva cita médica
		/// </summary>
		public async Task<ApiResponse<CitaMedica2>> CrearCitaAsync(CrearCitaRequest citaData)
		{
			try
			{
				var url = $"{_baseUrl}/citas/crear";

				System.Diagnostics.Debug.WriteLine($"📅 Creando cita: {url}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(citaData)}");

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = new StringContent(JsonSerializer.Serialize(citaData), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<CitaMedica2>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando cita"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<CitaMedica2>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		// En MediSysApiService.cs agregar:
		public async Task<ApiResponse<object>> GuardarHorariosAsync2(GuardarHorariosRequest request)
		{
			try
			{
				var url = $"{_baseUrl}/doctores/horarios";

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
				httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return apiResponse ?? new ApiResponse<object> { Success = false, Message = "Error" };
			}
			catch (Exception ex)
			{
				return new ApiResponse<object> { Success = false, Message = ex.Message };
			}
		}

		public async Task<ApiResponse<object>> EditarHorarioAsync(EditarHorarioRequest request)
		{
			try
			{
				var url = $"{_baseUrl}/horarios";

				var httpRequest = new HttpRequestMessage(HttpMethod.Put, url);
				httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return apiResponse ?? new ApiResponse<object> { Success = false, Message = "Error" };
			}
			catch (Exception ex)
			{
				return new ApiResponse<object> { Success = false, Message = ex.Message };
			}
		}

		public async Task<ApiResponse<object>> EliminarHorarioAsync(int idHorario)
		{
			try
			{
				var url = $"{_baseUrl}/horarios?id={idHorario}";

				var request = new HttpRequestMessage(HttpMethod.Delete, url);
				var response = await SendRequestWithSessionAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return apiResponse ?? new ApiResponse<object> { Success = false, Message = "Error" };
			}
			catch (Exception ex)
			{
				return new ApiResponse<object> { Success = false, Message = ex.Message };
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
				var url = $"{_baseUrl}/citas/paciente/{cedula}?fecha={fecha}";

				System.Diagnostics.Debug.WriteLine($"📅 Obteniendo citas del paciente: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Respuesta inválida del servidor"
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
						Message = errorResponse?.Message ?? "Error obteniendo citas del paciente"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception obteniendo citas: {ex.Message}");
				return new ApiResponse<List<CitaDetallada>>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Crear triaje para una cita
		/// </summary>
		public async Task<ApiResponse<CrearTriajeResponse>> CrearTriajeAsync(CrearTriajeRequest triajeData)
		{
			try
			{
				var url = $"{_baseUrl}/triaje/crear";

				System.Diagnostics.Debug.WriteLine($"🏥 Creando triaje: {url}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(triajeData)}");

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = new StringContent(JsonSerializer.Serialize(triajeData), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<CrearTriajeResponse>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error creando triaje"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Exception creando triaje: {ex.Message}");
				return new ApiResponse<CrearTriajeResponse>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Obtener triaje existente de una cita
		/// </summary>
		public async Task<ApiResponse<Triaje2>> ObtenerTriajePorCitaAsync(int idCita)
		{
			try
			{
				var url = $"{_baseUrl}/triaje/cita/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo triaje: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					return new ApiResponse<Triaje2>
					{
						Success = false,
						Message = "Triaje no encontrado"
					};
				}
			}
			catch (Exception ex)
			{
				return new ApiResponse<Triaje2>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
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
				var url = $"{_baseUrl}/consultas/doctor/{cedulaDoctor}";
				var queryParams = new List<string> { $"fecha={fecha}" };

				if (!string.IsNullOrEmpty(estado) && estado != "Todas")
				{
					queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
				}

				if (queryParams.Any())
				{
					url += "?" + string.Join("&", queryParams);
				}

				System.Diagnostics.Debug.WriteLine($"🩺 Obteniendo citas doctor: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
						Message = "Error procesando respuesta"
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
						Message = errorResponse?.Message ?? "Error obteniendo citas del doctor"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo citas doctor: {ex.Message}");
				return new ApiResponse<List<CitaConsultaMedica>>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Crear o actualizar consulta médica
		/// </summary>
		public async Task<ApiResponse<object>> CrearActualizarConsultaMedicaAsync(int idCita, ConsultaMedicaRequest consultaData)
		{
			try
			{
				var url = $"{_baseUrl}/consultas/cita/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🩺 Creando/actualizando consulta: {url}");
				System.Diagnostics.Debug.WriteLine($"📤 Datos consulta: {JsonSerializer.Serialize(consultaData)}");

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				request.Content = new StringContent(JsonSerializer.Serialize(consultaData), Encoding.UTF8, "application/json");

				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<object>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Error procesando consulta médica"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error procesando consulta: {ex.Message}");
				return new ApiResponse<object>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}
		
		/// <summary>
		/// Obtener detalle completo de una consulta médica
		/// </summary>
		public async Task<ApiResponse<DetalleConsulta>> ObtenerDetalleConsultaAsync(int idCita)
		{
			try
			{
				var url = $"{_baseUrl}/consultas/detalle/{idCita}";

				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo detalle consulta: {url}");

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return new ApiResponse<DetalleConsulta>
					{
						Success = false,
						Message = errorResponse?.Message ?? "Consulta no encontrada"
					};
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo detalle: {ex.Message}");
				return new ApiResponse<DetalleConsulta>
				{
					Success = false,
					Message = $"Error: {ex.Message}"
				};
			}
		}

		// ✅ NUEVO MÉTODO para obtener información de cita (sin requerir consulta médica)
		public async Task<ApiResponse<ConsultaDetalleResponse>> ObtenerInformacionCitaAsync(int idCita)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo información de cita ID: {idCita}");

				var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/citas/informacion/{idCita}");
				var response = await SendRequestWithSessionAsync(request);
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
				else
				{
					System.Diagnostics.Debug.WriteLine($"❌ Error HTTP {response.StatusCode}: {content}");
					return new ApiResponse<ConsultaDetalleResponse>
					{
						Success = false,
						Message = $"Error del servidor: {response.StatusCode}"
					};
				}
			}
			catch (HttpRequestException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Error de conexión al servidor"
				};
			}
			catch (TaskCanceledException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Timeout: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Tiempo de espera agotado"
				};
			}
			catch (JsonException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error JSON: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = "Error procesando respuesta del servidor"
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
				return new ApiResponse<ConsultaDetalleResponse>
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}"
				};
			}
		}


		/// <summary>
		/// Actualizar estado de una cita
		/// </summary>
		// En MediSysApiService.cs - CORREGIR EL MÉTODO
		public async Task<ApiResponse<object>> ActualizarEstadoCitaAsync(int idCita, string nuevoEstado)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"Actualizando estado de cita {idCita} a: {nuevoEstado}");

				var requestData = new { estado = nuevoEstado };
				var json = JsonSerializer.Serialize(requestData, GetJsonOptions());
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// ✅ USAR PUT EN LUGAR DE POST
				var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/citas/{idCita}/estado")
				{
					Content = content
				};

				var response = await SendRequestWithSessionAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Respuesta actualizar estado: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"Contenido: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
					return result ?? new ApiResponse<object> { Success = false, Message = "Respuesta vacía" };
				}
				else
				{
					try
					{
						var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
						return new ApiResponse<object>
						{
							Success = false,
							Message = errorResponse?.Message ?? $"Error del servidor: {response.StatusCode}"
						};
					}
					catch
					{
						return new ApiResponse<object>
						{
							Success = false,
							Message = $"Error del servidor: {response.StatusCode}"
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
					Message = $"Error: {ex.Message}"
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
		public async Task<ApiResponse<object>> CambiarPasswordLogueadoAsync(CambiarPasswordRequest request)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"Cambiando contraseña para usuario ID: {request.IdUsuario}");

				var json = JsonSerializer.Serialize(request, GetJsonOptions());
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/change-password-logged")
				{
					Content = content
				};

				var response = await SendRequestWithSessionAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"Respuesta cambio password: {response.StatusCode}");
				System.Diagnostics.Debug.WriteLine($"Contenido: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
					return result ?? new ApiResponse<object> { Success = false, Message = "Respuesta vacía" };
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
							Message = errorResponse?.Message ?? $"Error del servidor: {response.StatusCode}"
						};
					}
					catch
					{
						return new ApiResponse<object>
						{
							Success = false,
							Message = $"Error del servidor: {response.StatusCode}"
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
					Message = $"Error: {ex.Message}"
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