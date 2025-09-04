using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;


namespace MediSys.Models
{
	// ===== MODELO PRINCIPAL PARA CITAS DEL DOCTOR =====
	public class CitaConsultaMedica
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";

		// ✅ AGREGAR ESTA PROPIEDAD CALCULADA
		public bool PuedeMarcarNoAsistio
		{
			get
			{
				// Solo se puede marcar como "No Asistió" si NO está completada
				return !string.Equals(Estado, "Completada", StringComparison.OrdinalIgnoreCase) &&
					   !string.Equals(Estado, "Cancelada", StringComparison.OrdinalIgnoreCase) &&
					   !string.Equals(Estado, "No Asistió", StringComparison.OrdinalIgnoreCase);
			}
		}

		[JsonPropertyName("tipo_cita")]
		public string TipoCita { get; set; } = "";

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		[JsonPropertyName("enlace_virtual")]
		public string? EnlaceVirtual { get; set; }

		[JsonPropertyName("sala_virtual")]
		public string? SalaVirtual { get; set; }

		// Datos del paciente - ACTUALIZADOS
		[JsonPropertyName("paciente")]
		public PacienteInfo Paciente { get; set; } = new();

		// Datos del doctor
		[JsonPropertyName("doctor")]
		public DoctorInfo2 Doctor { get; set; } = new();

		// Sucursal - ACTUALIZADA
		[JsonPropertyName("sucursal")]
		public SucursalInformation Sucursal { get; set; } = new();

		// Especialidad
		[JsonPropertyName("especialidad")]
		public EspecialidadInfo2 Especialidad { get; set; } = new();

		// Tipo de cita info
		[JsonPropertyName("tipo_cita_info")]
		public TipoCitaInfo TipoCitaInfo { get; set; } = new();

		// Triaje (si existe) - ACTUALIZADO
		[JsonPropertyName("triaje")]
		public TriajeInfo2? Triaje { get; set; }

		// Consulta médica (si existe)
		[JsonPropertyName("consulta_medica")]
		public ConsultaMedicaInfo2? ConsultaMedica { get; set; }

		// Estados y capacidades
		[JsonPropertyName("tiene_triaje")]
		public bool TieneTriaje { get; set; }

		[JsonPropertyName("tiene_consulta")]
		public bool TieneConsulta { get; set; }

		[JsonPropertyName("tiene_historial")]
		public bool TieneHistorial { get; set; }

		[JsonPropertyName("puede_consultar")]
		public bool PuedeConsultar { get; set; }

		[JsonPropertyName("es_urgente")]
		public bool EsUrgente { get; set; }

		// Propiedades calculadas
		public DateTime FechaHoraParsed => DateTime.TryParse(FechaHora, out var fecha) ? fecha : DateTime.MinValue;
		public string FechaDisplay => FechaHoraParsed.ToString("dd/MM/yyyy");
		public string HoraDisplay => FechaHoraParsed.ToString("HH:mm");
		public bool EsVirtual => TipoCita?.ToLower() == "virtual";

		public Color EstadoColor => Estado switch
		{
			"Pendiente" => Colors.Orange,
			"Confirmada" => Colors.Blue,
			"En Proceso" => Colors.Purple,
			"Completada" => Colors.Green,
			"Cancelada" => Colors.Red,
			_ => Colors.Gray
		};

		public string EstadoIcon => Estado switch
		{
			"Pendiente" => "⏳",
			"Confirmada" => "✅",
			"En Proceso" => "🩺",
			"Completada" => "🏁",
			"Cancelada" => "❌",
			_ => "📋"
		};
	}



	// ===== PACIENTE INFO COMPLETA =====
	public class PacienteInfo
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("cedula")]
		public long Cedula { get; set; }

		[JsonPropertyName("correo")]
		public string? Correo { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("fecha_nacimiento")]
		public string? FechaNacimiento { get; set; }

		[JsonPropertyName("sexo")]
		public string? Sexo { get; set; }

		[JsonPropertyName("nacionalidad")]
		public string? Nacionalidad { get; set; }

		[JsonPropertyName("tipo_sangre")]
		public string? TipoSangre { get; set; }

		[JsonPropertyName("alergias")]
		public string? Alergias { get; set; }

		[JsonPropertyName("contacto_emergencia")]
		public string? ContactoEmergencia { get; set; }

		[JsonPropertyName("telefono_emergencia")]
		public string? TelefonoEmergencia { get; set; }

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";

		// Propiedades calculadas
		public string SexoDisplay => Sexo switch
		{
			"M" => "Masculino",
			"F" => "Femenino",
			_ => "No especificado"
		};
		public int? Edad
		{
			get
			{
				if (DateTime.TryParse(FechaNacimiento, out var fechaNac))
				{
					var hoy = DateTime.Today;
					var edad = hoy.Year - fechaNac.Year;
					if (fechaNac.Date > hoy.AddYears(-edad)) edad--;
					return edad;
				}
				return null;
			}
		}
	}

	// ===== SUCURSAL INFO =====
	public class SucursalInformation
	{
		[JsonPropertyName("nombre")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("direccion")]
		public string? Direccion { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("horario_atencion")]
		public string? HorarioAtencion { get; set; }
	}

	// ===== ESPECIALIDAD INFO =====
	public class EspecialidadInfo2
	{
		[JsonPropertyName("nombre")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("descripcion")]
		public string? Descripcion { get; set; }
	}

	// ===== TIPO CITA INFO =====
	public class TipoCitaInfo
	{
		[JsonPropertyName("codigo")]
		public string Codigo { get; set; } = "";

		[JsonPropertyName("nombre")]
		public string Nombre { get; set; } = "";
	}

	// ===== DOCTOR INFO =====
	public class DoctorInfo2
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		[JsonPropertyName("especialidad")]
		public string Especialidad { get; set; } = "";

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";
	}

	// ===== TRIAJE INFO COMPLETA (CORREGIDA) =====
	public class TriajeInfo2
	{
		[JsonPropertyName("id_triage")]
		public int IdTriage { get; set; }

		[JsonPropertyName("signos_vitales")]
		public SignosVitales2 SignosVitales { get; set; } = new();

		[JsonPropertyName("evaluacion")]
		public EvaluacionTriaje Evaluacion { get; set; } = new();

		// Propiedades de acceso directo para compatibilidad con binding
		public decimal? Temperatura => SignosVitales?.TemperaturaDecimal ??
									   (decimal.TryParse(SignosVitales?.Temperatura, out var temp) ? temp : null);

		public string? PresionArterial => SignosVitales?.PresionArterial;

		public int? FrecuenciaCardiaca => SignosVitales?.FrecuenciaCardiaca;

		public int? FrecuenciaRespiratoria => SignosVitales?.FrecuenciaRespiratoria;

		public int? SaturacionOxigeno => SignosVitales?.SaturacionOxigeno;

		public decimal? Peso => SignosVitales?.PesoDecimal ??
							   (decimal.TryParse(SignosVitales?.Peso, out var peso) ? peso : null);

		public int? Talla => SignosVitales?.Talla;

		public decimal? IMC => SignosVitales?.IMCDecimal ??
									(decimal.TryParse(SignosVitales?.IMC,out var imc) ? imc : null);

		public int? NivelUrgencia => Evaluacion?.NivelUrgencia;

		public string? Observaciones => Evaluacion?.Observaciones;

		public string? FechaTriaje => Evaluacion?.FechaTriaje;
	}

	// ===== SIGNOS VITALES CORREGIDOS =====
	public class SignosVitales2
	{
		[JsonPropertyName("temperatura")]
		public string? Temperatura { get; set; }

		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; }

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("frecuencia_respiratoria")]
		public int? FrecuenciaRespiratoria { get; set; }

		[JsonPropertyName("saturacion_oxigeno")]
		public int? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public string? Peso { get; set; }

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }

		[JsonPropertyName("imc")]
		public string? IMC { get; set; }

		// Propiedades calculadas para conversión de tipos
		[JsonIgnore]
		public decimal? TemperaturaDecimal =>
			decimal.TryParse(Temperatura, out var temp) ? temp : null;

		[JsonIgnore]
		public decimal? PesoDecimal =>
			decimal.TryParse(Peso, out var peso) ? peso : null;

		[JsonIgnore]
		public decimal? IMCDecimal =>
			decimal.TryParse(IMC, out var imc) ? imc : null;
	}

	

	
	// ===== CONVERTER FLEXIBLE PARA STRING/NUMBER A DECIMAL =====
	public class FlexibleDecimalConverter : JsonConverter<decimal?>
	{
		public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.String)
			{
				var stringValue = reader.GetString();
				if (string.IsNullOrEmpty(stringValue))
					return null;

				if (decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalResult))
					return decimalResult;

				return null;
			}

			if (reader.TokenType == JsonTokenType.Number)
			{
				if (reader.TryGetDecimal(out var numberResult))
					return numberResult;

				// Fallback para double/float
				if (reader.TryGetDouble(out var doubleResult))
					return (decimal)doubleResult;
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

	public class EvaluacionTriaje
	{
		[JsonPropertyName("nivel_urgencia")]
		public int? NivelUrgencia { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("fecha_triaje")]
		public string? FechaTriaje { get; set; }

		// Propiedades calculadas
		public string NivelUrgenciaTexto => NivelUrgencia switch
		{
			1 => "Baja",
			2 => "Media",
			3 => "Alta",
			4 => "Crítica",
			5 => "Emergencia",
			_ => "No especificado"
		};

		public Color NivelUrgenciaColor => NivelUrgencia switch
		{
			1 => Colors.Green,
			2 => Colors.Orange,
			3 => Colors.Red,
			4 => Colors.DarkRed,
			5 => Colors.Purple,
			_ => Colors.Gray
		};
	}

	// ===== CONSULTA MÉDICA INFO =====
	public class ConsultaMedicaInfo2
	{
		[JsonPropertyName("id_consulta")]
		public int IdConsulta { get; set; }

		[JsonPropertyName("motivo_consulta")]
		public string MotivoConsulta { get; set; } = "";

		[JsonPropertyName("sintomatologia")]
		public string? Sintomatologia { get; set; }

		[JsonPropertyName("diagnostico")]
		public string Diagnostico { get; set; } = "";

		[JsonPropertyName("tratamiento")]
		public string? Tratamiento { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("fecha_seguimiento")]
		public string? FechaSeguimiento { get; set; }

		[JsonPropertyName("fecha_consulta")]
		public string? FechaConsulta { get; set; }
	}

	// ===== DETALLE COMPLETO DE CONSULTA (para la página de detalles) =====
	public class DetalleConsulta
	{
		[JsonPropertyName("consulta")]
		public ConsultaInfo Consulta { get; set; } = new();

		[JsonPropertyName("cita")]
		public CitaInfoDetallada Cita { get; set; } = new();

		[JsonPropertyName("paciente")]
		public PacienteInfo Paciente { get; set; } = new();

		[JsonPropertyName("doctor")]
		public DoctorDetallado Doctor { get; set; } = new();

		[JsonPropertyName("sucursal")]
		public SucursalInformation Sucursal { get; set; } = new();

		[JsonPropertyName("especialidad")]
		public EspecialidadInfo2 Especialidad { get; set; } = new();

		[JsonPropertyName("triaje")]
		public TriajeInfo2? Triaje { get; set; }

		[JsonPropertyName("historial")]
		public HistorialInfo Historial { get; set; } = new();
	}

	public class ConsultaInfo
	{
		[JsonPropertyName("id_consulta")]
		public int IdConsulta { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo_consulta")]
		public string MotivoConsulta { get; set; } = "";

		[JsonPropertyName("sintomatologia")]
		public string? Sintomatologia { get; set; }

		[JsonPropertyName("diagnostico")]
		public string Diagnostico { get; set; } = "";

		[JsonPropertyName("tratamiento")]
		public string? Tratamiento { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("fecha_seguimiento")]
		public string? FechaSeguimiento { get; set; }
	}

	public class CitaInfoDetallada
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo_original")]
		public string MotivoOriginal { get; set; } = "";

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";

		[JsonPropertyName("tipo")]
		public string Tipo { get; set; } = "";

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		[JsonPropertyName("modalidad")]
		public ModalidadCita Modalidad { get; set; } = new();
	}

	public class ModalidadCita
	{
		[JsonPropertyName("tipo")]
		public string Tipo { get; set; } = "";

		[JsonPropertyName("nombre")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("enlace_virtual")]
		public string? EnlaceVirtual { get; set; }

		[JsonPropertyName("sala_virtual")]
		public string? SalaVirtual { get; set; }
	}

	public class DoctorDetallado
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		[JsonPropertyName("especialidad")]
		public string Especialidad { get; set; } = "";

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";
	}

	public class HistorialInfo
	{
		[JsonPropertyName("tiene_historial")]
		public bool TieneHistorial { get; set; }

		[JsonPropertyName("id_historial")]
		public int? IdHistorial { get; set; }
	}

	// ===== MODELO DE REQUEST =====
	public class ConsultaMedicaRequest
	{
		[JsonPropertyName("motivo_consulta")]
		public string MotivoConsulta { get; set; } = "";

		[JsonPropertyName("sintomatologia")]
		public string? Sintomatologia { get; set; }

		[JsonPropertyName("diagnostico")]
		public string Diagnostico { get; set; } = "";

		[JsonPropertyName("tratamiento")]
		public string? Tratamiento { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("fecha_seguimiento")]
		public string? FechaSeguimiento { get; set; }
	}
}