using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MediSys.Models
{


	public class CitaDetallada
	{
		// Datos básicos de la cita
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";

		[JsonPropertyName("tipo_cita")]
		public string TipoCita { get; set; } = "";

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		// Datos del paciente
		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("paciente_nombres")]
		public string PacienteNombres { get; set; } = "";

		[JsonPropertyName("paciente_apellidos")]
		public string PacienteApellidos { get; set; } = "";

		[JsonPropertyName("paciente_cedula")]
		public long PacienteCedula { get; set; }

		// Datos del doctor
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("doctor_nombres")]
		public string DoctorNombres { get; set; } = "";

		[JsonPropertyName("doctor_apellidos")]
		public string DoctorApellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string TituloProfesional { get; set; } = "";

		// Especialidad y sucursal
		[JsonPropertyName("nombre_especialidad")]
		public string NombreEspecialidad { get; set; } = "";

		[JsonPropertyName("nombre_sucursal")]
		public string NombreSucursal { get; set; } = "";

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		// Tipo de cita
		[JsonPropertyName("id_tipo_cita")]
		public int IdTipoCita { get; set; }

		[JsonPropertyName("tipo_cita_nombre")]
		public string TipoCitaNombre { get; set; } = "";

		// Triaje (si existe)
		[JsonPropertyName("id_triage")]
		public int? IdTriage { get; set; }

		[JsonPropertyName("estado_triaje")]
		public string? EstadoTriaje { get; set; }

		// === PROPIEDADES CALCULADAS PARA EL BINDING ===
		public string DoctorCompleto => $"Dr. {DoctorNombres} {DoctorApellidos}".Trim();

		public DateTime FechaHoraParsed => DateTime.TryParse(FechaHora, out var fecha) ?
			fecha : DateTime.MinValue;

		// En CitaDetallada - agregar esta propiedad
		public string FechaHoraDisplay => FechaHoraParsed.ToString("dd/MM/yyyy HH:mm");
		public string HoraDisplay => FechaHoraParsed.ToString("HH:mm");
		public string FechaDisplay => FechaHoraParsed.ToString("dd/MM/yyyy");

		public bool TieneTriaje => IdTriage.HasValue;
		public bool PuedeHacerTriaje => Estado == "Confirmada" && !TieneTriaje;
	}
	// Modelo para crear triaje
	public class CrearTriajeRequest
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }


		[JsonPropertyName("id_enfermero")]
		public int IdEnfermero { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";


		[JsonPropertyName("nivel_urgencia")]
		public int NivelUrgencia { get; set; }


		[JsonPropertyName("temperatura")]
		public decimal? Temperatura { get; set; }

		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; }

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("frecuencia_respiratoria")]
		public int? FrecuenciaRespiratoria { get; set; }

		[JsonPropertyName("saturacion_oxigeno")]
		public int? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }
	}

	// Respuesta al crear triaje
	public class CrearTriajeResponse
	{
		[JsonPropertyName("id_triaje")]
		public int IdTriaje { get; set; }

		[JsonPropertyName("estado_triaje")]
		public string EstadoTriaje { get; set; } = "";

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }

		[JsonPropertyName("categoria_imc")]
		public string? CategoriaImc { get; set; }

		[JsonPropertyName("alertas")]
		public List<string>? Alertas { get; set; }

		[JsonPropertyName("tiene_alertas")]
		public bool TieneAlertas { get; set; }
	}

	// Nivel de urgencia para la UI
	public class NivelUrgencia
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = "";
		public string Descripcion { get; set; } = "";
		public string Color { get; set; } = "";
		public string Icono { get; set; } = "";
	}

	// Modelo completo de triaje existente
	public class Triaje2
	{
		[JsonPropertyName("id_triage")]
		public int IdTriage { get; set; }

		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("id_enfermero")]
		public int IdEnfermero { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("nivel_urgencia")]
		public int NivelUrgencia { get; set; }

		[JsonPropertyName("estado_triaje")]
		public string EstadoTriaje { get; set; } = "";

		[JsonPropertyName("temperatura")]
		public decimal? Temperatura { get; set; }

		[JsonPropertyName("presion_arterial")]
		public string PresionArterial { get; set; } = "";

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("frecuencia_respiratoria")]
		public int? FrecuenciaRespiratoria { get; set; }

		[JsonPropertyName("saturacion_oxigeno")]
		public int? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }

		[JsonPropertyName("observaciones")]
		public string Observaciones { get; set; } = "";

		[JsonPropertyName("enfermero_nombres")]
		public string EnfermeroNombres { get; set; } = "";

		[JsonPropertyName("enfermero_apellidos")]
		public string EnfermeroApellidos { get; set; } = "";

		// Propiedades calculadas
		public string UrgenciaDisplay => NivelUrgencia switch
		{
			1 => "🟢 Baja",
			2 => "🟡 Media",
			3 => "🟠 Alta",
			4 => "🔴 Crítica",
			5 => "🚨 Emergencia",
			_ => "Sin clasificar"
		};

		public Color UrgenciaColor => NivelUrgencia switch
		{
			1 => Colors.Green,
			2 => Colors.Orange,
			3 => Colors.OrangeRed,
			4 => Colors.Red,
			5 => Colors.Purple,
			_ => Colors.Gray
		};

		public string EnfermeroCompleto => $"{EnfermeroNombres} {EnfermeroApellidos}".Trim();
		public string ImcCalculado => Imc?.ToString("F1") ?? "N/A";
		public string TemperaturaDisplay => Temperatura?.ToString("F1") + "°C" ?? "N/A";
		public string SaturacionDisplay => SaturacionOxigeno?.ToString() + "%" ?? "N/A";

		public DateTime FechaTriajeParsed => DateTime.TryParse(FechaHora, out var fecha) ?
			fecha : DateTime.MinValue;

		public string FechaTriajeDisplay => FechaTriajeParsed.ToString("dd/MM/yyyy HH:mm");
	}
}
