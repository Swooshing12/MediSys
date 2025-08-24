using System.Text.Json.Serialization;

namespace MediSys.Models
{
	public class User
	{
		[JsonPropertyName("id_usuario")]
		public int IdUsuario { get; set; }

		[JsonPropertyName("cedula")]
		public long Cedula { get; set; } // 🔥 CAMBIO: long en lugar de string

		[JsonPropertyName("username")]
		public string Username { get; set; } = string.Empty;

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = string.Empty;

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = string.Empty;

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = string.Empty;

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = string.Empty;

		[JsonPropertyName("rol")]
		public string Rol { get; set; } = string.Empty;

		[JsonPropertyName("tipo_usuario")]
		public string TipoUsuario { get; set; } = string.Empty;

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = string.Empty;

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = string.Empty;

		[JsonPropertyName("id_paciente")]
		public int? IdPaciente { get; set; } // 🔥 CAMBIO: nullable int

		[JsonPropertyName("id_doctor")]
		public int? IdDoctor { get; set; } // 🔥 CAMBIO: nullable int

		[JsonPropertyName("especialidad")]
		public string? Especialidad { get; set; } // 🔥 CAMBIO: nullable string

		[JsonPropertyName("fecha_registro")]
		public string FechaRegistro { get; set; } = string.Empty;

		// 🔥 NUEVAS PROPIEDADES QUE PUEDEN VENIR DEL API
		[JsonPropertyName("requiere_cambio_password")]
		public bool RequiereCambioPassword { get; set; } = false;

		[JsonPropertyName("estado")]
		public string? Estado { get; set; }

		[JsonPropertyName("fecha_creacion")]
		public string? FechaCreacion { get; set; }

		[JsonPropertyName("ultimo_login")]
		public string? UltimoLogin { get; set; }

		// Propiedades calculadas para UI
		public string CedulaString => Cedula.ToString();
		public string Iniciales => $"{(string.IsNullOrEmpty(Nombres) ? "" : Nombres[0])}{(string.IsNullOrEmpty(Apellidos) ? "" : Apellidos[0])}";
		public string RolDisplay => Rol switch
		{
			"Medico" => "👨‍⚕️ Médico",
			"Paciente" => "🧑‍🤝‍🧑 Paciente",
			"Administrador" => "⚙️ Administrador",
			"Recepcionista" => "📋 Recepcionista",
			"Enfermero" => "👩‍⚕️ Enfermero",
			_ => Rol
		};
	}

	public class LoginRequest
	{
		[JsonPropertyName("correo")]
		public string Correo { get; set; } = string.Empty;

		[JsonPropertyName("password")]
		public string Password { get; set; } = string.Empty;
	}

	public class LoginResponse
	{
		[JsonPropertyName("usuario")]
		public User Usuario { get; set; } = new();

		[JsonPropertyName("mensaje")]
		public string? Mensaje { get; set; }
	}

	public class ForgotPasswordRequest
	{
		[JsonPropertyName("correo")]
		public string Correo { get; set; } = string.Empty;
	}

	public class ChangePasswordRequest
	{
		[JsonPropertyName("correo")]
		public string Correo { get; set; } = string.Empty;

		[JsonPropertyName("password_actual")]
		public string PasswordActual { get; set; } = string.Empty;

		[JsonPropertyName("password_nueva")]
		public string PasswordNueva { get; set; } = string.Empty;

		[JsonPropertyName("confirmar_password")]
		public string ConfirmarPassword { get; set; } = string.Empty;
	}

	public class ForgotPasswordResponse
	{
		[JsonPropertyName("clave_temporal_generada")]
		public bool ClaveTemporalGenerada { get; set; }

		[JsonPropertyName("correo_enviado")]
		public bool CorreoEnviado { get; set; }

		[JsonPropertyName("mensaje_usuario")]
		public string MensajeUsuario { get; set; } = string.Empty;
	}

	public class ChangePasswordResponse
	{
		[JsonPropertyName("password_changed")]
		public bool PasswordChanged { get; set; }

		[JsonPropertyName("usuario_activado")]
		public bool UsuarioActivado { get; set; }

		[JsonPropertyName("mensaje_usuario")]
		public string MensajeUsuario { get; set; } = string.Empty;
	}
}