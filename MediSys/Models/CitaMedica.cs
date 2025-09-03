using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace MediSys.Models
{
	// ===== MODELO PARA CITAS DEL DOCTOR =====
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

		[JsonPropertyName("tipo_cita")]
		public string TipoCita { get; set; } = "";

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		// Datos del paciente
		[JsonPropertyName("paciente")]
		public PacienteInfo Paciente { get; set; } = new();

		// Datos del doctor
		[JsonPropertyName("doctor")]
		public DoctorInfo2 Doctor { get; set; } = new();

		// Sucursal
		[JsonPropertyName("sucursal")]
		public string Sucursal { get; set; } = "";

		// Triaje (si existe)
		[JsonPropertyName("triaje")]
		public TriajeInfo2? Triaje { get; set; }

		// Estado de la consulta
		[JsonPropertyName("tiene_consulta")]
		public bool TieneConsulta { get; set; }

		[JsonPropertyName("tiene_historial")]
		public bool TieneHistorial { get; set; }

		[JsonPropertyName("puede_consultar")]
		public bool PuedeConsultar { get; set; }

		[JsonPropertyName("consulta_preview")]
		public ConsultaPreview? ConsultaPreview { get; set; }

		// Propiedades calculadas
		public DateTime FechaHoraParsed => DateTime.TryParse(FechaHora, out var fecha) ? fecha : DateTime.MinValue;
		public string FechaDisplay => FechaHoraParsed.ToString("dd/MM/yyyy");
		public string HoraDisplay => FechaHoraParsed.ToString("HH:mm");
		public bool EsUrgente => Triaje?.NivelUrgencia >= 3;
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

	public class PacienteInfo
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("cedula")]
		public long Cedula { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";
	}

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
	}

	public class TriajeInfo2
	{
		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; }

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("temperatura")]
		public string? Temperatura { get; set; }

		[JsonPropertyName("saturacion_oxigeno")]
		public int? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public string? Peso { get; set; }

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }

		[JsonPropertyName("nivel_urgencia")]
		public int? NivelUrgencia { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("estado")]
		public string? Estado { get; set; }
	}

	public class ConsultaPreview
	{
		[JsonPropertyName("diagnostico")]
		public string? Diagnostico { get; set; }

		[JsonPropertyName("tratamiento")]
		public string? Tratamiento { get; set; }
	}

	// Models/ConsultaMedicaModels.cs - AGREGAR SI NO EXISTE
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

	// ===== DETALLE COMPLETO DE CONSULTA MÉDICA =====
	public class DetalleConsultaMedica
	{
		[JsonPropertyName("consulta")]
		public ConsultaInfo Consulta { get; set; } = new();

		[JsonPropertyName("cita")]
		public CitaInfo Cita { get; set; } = new();

		[JsonPropertyName("paciente")]
		public PacienteDetallado Paciente { get; set; } = new();

		[JsonPropertyName("doctor")]
		public DoctorDetallado Doctor { get; set; } = new();

		[JsonPropertyName("sucursal")]
		public string Sucursal { get; set; } = "";

		[JsonPropertyName("triaje")]
		public TriajeDetallado? Triaje { get; set; }
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
		public string? Observaciones { get; set; } // Receta médica

		[JsonPropertyName("fecha_seguimiento")]
		public string? FechaSeguimiento { get; set; }
	}

	public class CitaInfo
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
	}

	public class PacienteDetallado
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = "";

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";
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

	public class TriajeDetallado
	{
		[JsonPropertyName("signos_vitales")]
		public SignosVitalesDetallado SignosVitales { get; set; } = new();

		[JsonPropertyName("evaluacion")]
		public EvaluacionTriaje Evaluacion { get; set; } = new();
	}

	public class SignosVitalesDetallado
	{
		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; }

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("temperatura")]
		public string? Temperatura { get; set; }

		[JsonPropertyName("saturacion_oxigeno")]
		public int? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public string? Peso { get; set; }

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }
	}

	public class EvaluacionTriaje
	{
		[JsonPropertyName("nivel_urgencia")]
		public int? NivelUrgencia { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }
	}

}
