using System.Text.Json.Serialization;

namespace MediSys.Models
{
	// ===== MODELO PRINCIPAL PARA DETALLE DE CONSULTA =====
	public class ConsultaDetalleResponse
	{
		[JsonPropertyName("cita")]
		public CitaInfo? Cita { get; set; }

		[JsonPropertyName("paciente")]
		public PacienteInfo3? Paciente { get; set; }

		[JsonPropertyName("doctor")]
		public DoctorInfo3? Doctor { get; set; }

		[JsonPropertyName("sucursal")]
		public SucursalInfo3? Sucursal { get; set; }

		[JsonPropertyName("especialidad")]
		public EspecialidadInfo3? Especialidad { get; set; }

		[JsonPropertyName("triaje")]
		public TriajeInfo3? Triaje { get; set; }

		[JsonPropertyName("consulta_medica")]
		public ConsultaMedicaInfo3? ConsultaMedica { get; set; }

		[JsonPropertyName("consulta")]
		public ConsultaMedicaInfo3? Consulta { get; set; } // Alias para compatibilidad

		[JsonPropertyName("historial")]
		public HistorialInfo3? Historial { get; set; }
	}

	// ===== MODELOS DE APOYO =====
	public class CitaInfo
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string? FechaHora { get; set; }

		[JsonPropertyName("motivo")]
		public string? Motivo { get; set; }

		[JsonPropertyName("estado")]
		public string? Estado { get; set; }

		[JsonPropertyName("tipo")]
		public string? Tipo { get; set; }

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		[JsonPropertyName("modalidad")]
		public ModalidadCita? Modalidad { get; set; }

		[JsonPropertyName("tiene_consulta")]
		public bool TieneConsulta { get; set; }
	}

	public class ModalidadCita3
	{
		[JsonPropertyName("tipo")]
		public string? Tipo { get; set; }

		[JsonPropertyName("nombre")]
		public string? Nombre { get; set; }

		[JsonPropertyName("enlace_virtual")]
		public string? EnlaceVirtual { get; set; }

		[JsonPropertyName("sala_virtual")]
		public string? SalaVirtual { get; set; }
	}

	public class PacienteInfo3
	{
		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("nombres")]
		public string? Nombres { get; set; }

		[JsonPropertyName("apellidos")]
		public string? Apellidos { get; set; }

		[JsonPropertyName("cedula")]
		public long? Cedula { get; set; }

		[JsonPropertyName("correo")]
		public string? Correo { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("fecha_nacimiento")]
		public string? FechaNacimiento { get; set; }

		[JsonPropertyName("edad")]
		public int? Edad { get; set; }

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
		public string? NombreCompleto { get; set; }
	}

	public class DoctorInfo3
	{
		[JsonPropertyName("nombres")]
		public string? Nombres { get; set; }

		[JsonPropertyName("apellidos")]
		public string? Apellidos { get; set; }

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		[JsonPropertyName("especialidad")]
		public string? Especialidad { get; set; }

		[JsonPropertyName("nombre_completo")]
		public string? NombreCompleto { get; set; }
	}

	public class SucursalInfo3
	{
		[JsonPropertyName("nombre_sucursal")]
		public string? NombreSucursal { get; set; }

		[JsonPropertyName("direccion")]
		public string? Direccion { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("horario_atencion")]
		public string? HorarioAtencion { get; set; }
	}

	public class EspecialidadInfo3
	{
		[JsonPropertyName("nombre_especialidad")]
		public string? NombreEspecialidad { get; set; }

		[JsonPropertyName("descripcion")]
		public string? Descripcion { get; set; }
	}

	public class TriajeInfo3
	{
		[JsonPropertyName("id_triage")]
		public int? IdTriage { get; set; }

		[JsonPropertyName("fecha_triaje")]
		public string? FechaTriaje { get; set; }

		[JsonPropertyName("completado")]
		public bool Completado { get; set; }

		[JsonPropertyName("signos_vitales")]
		public SignosVitalesInfo? SignosVitales { get; set; }

		[JsonPropertyName("evaluacion")]
		public EvaluacionTriaje? Evaluacion { get; set; }
	}

	public class SignosVitalesInfo
	{
		[JsonPropertyName("temperatura")]
		public decimal? Temperatura { get; set; }  // ✅ Cambiado a decimal

		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; }  // ✅ Mantener string (ej: "120/80")

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }  // ✅ Cambiado a int

		[JsonPropertyName("frecuencia_respiratoria")]
		public int? FrecuenciaRespiratoria { get; set; }  // ✅ Cambiado a int

		[JsonPropertyName("saturacion_oxigeno")]
		public decimal? SaturacionOxigeno { get; set; }  // ✅ Cambiado a decimal

		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }  // ✅ Cambiado a decimal

		[JsonPropertyName("talla")]
		public int? Talla { get; set; }  // ✅ Cambiado a int (en cm)

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }  // ✅ Cambiado a decimal

		// ✅ PROPIEDADES HELPER PARA MOSTRAR COMO STRING SI NECESITAS
		public string TemperaturaDisplay => Temperatura?.ToString("F1") + "°C";
		public string FrecuenciaCardiacaDisplay => FrecuenciaCardiaca?.ToString() + " bpm";
		public string SaturacionOxigenoDisplay => SaturacionOxigeno?.ToString("F1") + "%";
		public string PesoDisplay => Peso?.ToString("F1") + " kg";
		public string TallaDisplay => Talla?.ToString() + " cm";
		public string ImcDisplay => Imc?.ToString("F2");
	}

	public class EvaluacionTriaje3
	{
		[JsonPropertyName("nivel_urgencia")]
		public string? NivelUrgencia { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }
	}

	public class ConsultaMedicaInfo3
	{
		[JsonPropertyName("existe")]
		public bool Existe { get; set; }

		[JsonPropertyName("id_consulta")]
		public int? IdConsulta { get; set; }

		[JsonPropertyName("motivo_consulta")]
		public string? MotivoConsulta { get; set; }

		[JsonPropertyName("sintomatologia")]
		public string? Sintomatologia { get; set; }

		[JsonPropertyName("diagnostico")]
		public string? Diagnostico { get; set; }

		[JsonPropertyName("tratamiento")]
		public string? Tratamiento { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }

		[JsonPropertyName("fecha_seguimiento")]
		public string? FechaSeguimiento { get; set; }
	}

	public class HistorialInfo3
	{
		[JsonPropertyName("tiene_historial")]
		public bool TieneHistorial { get; set; }

		[JsonPropertyName("id_historial")]
		public int? IdHistorial { get; set; }
	}
}