using System.Text.Json;

namespace MediSys.Services
{
	public class CedulaValidationService
	{
		private readonly HttpClient _httpClient;

		public CedulaValidationService()
		{
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			_httpClient = new HttpClient(handler);
			_httpClient.Timeout = TimeSpan.FromSeconds(30);

			// Headers para que funcione bien
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
		}

		public async Task<CedulaValidationResponse> ValidarCedulaAsync(string cedula)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10)
				{
					return new CedulaValidationResponse
					{
						Success = false,
						Message = "La cédula debe tener exactamente 10 dígitos"
					};
				}

				System.Diagnostics.Debug.WriteLine($"🔍 Validando cédula directamente: {cedula}");

				// ✅ DIRECTO AL API DE ECUADOR
				var url = $"https://sifae.agrocalidad.gob.ec/SIFAEBack/index.php?ruta=datos_demograficos/{cedula}";

				var response = await _httpClient.GetAsync(url);
				var responseContent = await response.Content.ReadAsStringAsync();

				System.Diagnostics.Debug.WriteLine($"📥 Respuesta API Ecuador: {responseContent}");

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<CedulaApiResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse?.Estado == "OK" && apiResponse.Resultado?.Length > 0)
					{
						var datos = apiResponse.Resultado[0];

						// Procesar y separar nombres y apellidos
						var (nombres, apellidos) = ProcesarNombreCompleto(datos.Nombre);

						return new CedulaValidationResponse
						{
							Success = true,
							Message = "Datos encontrados exitosamente",
							Data = new DatosCedulaValidada
							{
								Cedula = datos.Cedula,
								Nombres = nombres,
								Apellidos = apellidos,
								FechaNacimiento = ParsearFecha(datos.FechaNacimiento),
								EstadoCivil = datos.EstadoCivil,
								Profesion = datos.Profesion,
								LugarNacimiento = datos.LugarNacimiento
							}
						};
					}
					else
					{
						return new CedulaValidationResponse
						{
							Success = false,
							Message = "No se encontraron datos para esta cédula en el Registro Civil"
						};
					}
				}
				else
				{
					return new CedulaValidationResponse
					{
						Success = false,
						Message = $"Error consultando el Registro Civil (Código: {response.StatusCode})"
					};
				}
			}
			catch (HttpRequestException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error HTTP validando cédula: {ex.Message}");
				return new CedulaValidationResponse
				{
					Success = false,
					Message = "Error de conexión. Verifique su internet e intente nuevamente."
				};
			}
			catch (TaskCanceledException ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Timeout validando cédula: {ex.Message}");
				return new CedulaValidationResponse
				{
					Success = false,
					Message = "La consulta tardó demasiado. Intente nuevamente."
				};
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error general validando cédula: {ex.Message}");
				return new CedulaValidationResponse
				{
					Success = false,
					Message = $"Error inesperado: {ex.Message}"
				};
			}
		}

		private (string nombres, string apellidos) ProcesarNombreCompleto(string nombreCompleto)
		{
			try
			{
				// El nombre viene como: "CUNALATA GRANIZO RONALD SANTIAGO"
				// En Ecuador generalmente es: APELLIDO1 APELLIDO2 NOMBRE1 NOMBRE2
				var partes = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if (partes.Length >= 4)
				{
					// Primeros 2 = apellidos, resto = nombres
					var apellidos = string.Join(" ", partes.Take(2));
					var nombres = string.Join(" ", partes.Skip(2));
					return (CapitalizarNombre(nombres), CapitalizarNombre(apellidos));
				}
				else if (partes.Length >= 2)
				{
					// Si hay menos de 4 partes, dividir por la mitad
					var mitad = partes.Length / 2;
					var apellidos = string.Join(" ", partes.Take(mitad));
					var nombres = string.Join(" ", partes.Skip(mitad));
					return (CapitalizarNombre(nombres), CapitalizarNombre(apellidos));
				}
				else
				{
					return (CapitalizarNombre(nombreCompleto), "");
				}
			}
			catch
			{
				return (CapitalizarNombre(nombreCompleto), "");
			}
		}

		private string CapitalizarNombre(string nombre)
		{
			if (string.IsNullOrWhiteSpace(nombre))
				return "";

			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower());
		}

		private DateTime? ParsearFecha(string fecha)
		{
			try
			{
				// Formato que viene: "09/03/2007"
				if (DateTime.TryParseExact(fecha, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime resultado))
				{
					return resultado;
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}

	// ===== MODELOS PARA LA RESPUESTA =====
	public class CedulaValidationResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = "";
		public DatosCedulaValidada? Data { get; set; }
	}

	public class DatosCedulaValidada
	{
		public string Cedula { get; set; } = "";
		public string Nombres { get; set; } = "";
		public string Apellidos { get; set; } = "";
		public DateTime? FechaNacimiento { get; set; }
		public string EstadoCivil { get; set; } = "";
		public string Profesion { get; set; } = "";
		public string LugarNacimiento { get; set; } = "";
	}

	// ===== MODELOS PARA EL API DE ECUADOR =====
	public class CedulaApiResponse
	{
		public string Estado { get; set; } = "";
		public CedulaApiDatos[] Resultado { get; set; } = Array.Empty<CedulaApiDatos>();
	}

	public class CedulaApiDatos
	{
		public string Cedula { get; set; } = "";
		public string Nombre { get; set; } = "";
		public string FechaNacimiento { get; set; } = "";
		public string EstadoCivil { get; set; } = "";
		public string Profesion { get; set; } = "";
		public string LugarNacimiento { get; set; } = "";
		public string CondicionCiudadano { get; set; } = "";
		public string FechaExpedicion { get; set; } = "";
		public string FechaExpiracion { get; set; } = "";
		public string LugarInscripcionNacimiento { get; set; } = "";
		public string AnioInscripcionNacimiento { get; set; } = "";
		public string Conyuge { get; set; } = "";
		public string Apellido { get; set; } = "";
	}
}