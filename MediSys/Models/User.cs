using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MediSys.Models
{
	public class User
	{
		[JsonPropertyName("id_usuario")]
		public int IdUsuario { get; set; }

		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = string.Empty;

		[JsonPropertyName("username")]
		public string Username { get; set; } = string.Empty;

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = string.Empty;

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = string.Empty;

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = string.Empty;

		[JsonPropertyName("rol")]
		public string Rol { get; set; } = string.Empty;

		[JsonPropertyName("id_rol")]
		public int IdRol { get; set; }

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = string.Empty;

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = string.Empty;

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = string.Empty;

		[JsonPropertyName("requiere_cambio_password")]
		public bool RequiereCambioPassword { get; set; }

		[JsonPropertyName("fecha_creacion")]
		public string FechaCreacion { get; set; } = string.Empty;

		[JsonPropertyName("ultimo_login")]
		public string? UltimoLogin { get; set; }

		// Propiedades calculadas
		public string NombreCompleto => $"{Nombres} {Apellidos}";
		public string Iniciales => $"{(string.IsNullOrEmpty(Nombres) ? "" : Nombres[0])}{(string.IsNullOrEmpty(Apellidos) ? "" : Apellidos[0])}";
	}

	public class LoginRequest
	{
		public string Correo { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class LoginResponse
	{
		[JsonPropertyName("usuario")]
		public User Usuario { get; set; } = new();

		[JsonPropertyName("mensaje")]
		public string Mensaje { get; set; } = string.Empty;
	}
}