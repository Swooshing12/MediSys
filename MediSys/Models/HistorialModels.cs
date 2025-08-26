using System.Text.Json.Serialization;

namespace MediSys.Models
{
	public class PacienteBusquedaRequest
	{
		public string Cedula { get; set; } = "";
	}

	public class PacienteResponse
	{
		[JsonPropertyName("cedula")]
		public int Cedula { get; set; }

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = "";

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = "";

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = "";

		[JsonPropertyName("fecha_nacimiento")]
		public string FechaNacimiento { get; set; } = "";

		[JsonPropertyName("edad")]
		public int Edad { get; set; }

		[JsonPropertyName("tipo_sangre")]
		public string? TipoSangre { get; set; }

		[JsonPropertyName("alergias")]
		public string? Alergias { get; set; }

		[JsonPropertyName("antecedentes_medicos")]
		public string? AntecedentesMedicos { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		public string NombreCompleto => $"{Nombres} {Apellidos}";
	}

	public class HistorialClinicoFiltros
	{
		public string? FechaDesde { get; set; }
		public string? FechaHasta { get; set; }
		public int? IdEspecialidad { get; set; }
		public int? IdDoctor { get; set; }
		public string? Estado { get; set; }
		public int? IdSucursal { get; set; }
	}

	public class CitaMedica
	{
		// Campos básicos de la cita
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string? Motivo { get; set; }

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";

		[JsonPropertyName("modalidad_cita")]
		public string? ModalidadCita { get; set; }

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		[JsonPropertyName("enlace_virtual")]
		public string? EnlaceVirtual { get; set; }

		[JsonPropertyName("tipo_cita")]
		public string? TipoCita { get; set; }

		// Doctor (objeto anidado)
		[JsonPropertyName("doctor")]
		public DoctorInfo? Doctor { get; set; }

		// Especialidad (objeto anidado)
		[JsonPropertyName("especialidad")]
		public EspecialidadInfo? Especialidad { get; set; }

		// Sucursal (objeto anidado)
		[JsonPropertyName("sucursal")]
		public SucursalInfo? Sucursal { get; set; }

		// Consulta médica (objeto anidado)
		[JsonPropertyName("consulta_medica")]
		public ConsultaMedicaInfo? ConsultaMedica { get; set; }

		// Triaje (objeto anidado)
		[JsonPropertyName("triaje")]
		public TriajeInfo? Triaje { get; set; }

		// Estados
		[JsonPropertyName("tiene_consulta")]
		public bool TieneConsulta { get; set; }

		[JsonPropertyName("tiene_triaje")]
		public bool TieneTriaje { get; set; }

		[JsonPropertyName("esta_completada")]
		public bool EstaCompletada { get; set; }

		// Propiedades calculadas para compatibilidad con XAML
		public string DoctorNombres => Doctor?.Nombres ?? "";
		public string DoctorApellidos => Doctor?.Apellidos ?? "";
		public string DoctorCompleto => Doctor != null ? $"Dr. {Doctor.NombreCompleto}" : "";
		public string EspecialidadNombre => Especialidad?.Nombre ?? "";
		public string SucursalNombre => Sucursal?.Nombre ?? "";
		public string MotivoConsulta => ConsultaMedica?.MotivoConsulta ?? Motivo;
		public string? Diagnostico => ConsultaMedica?.Diagnostico;
		public string? Tratamiento => ConsultaMedica?.Tratamiento;
		public string? Sintomatologia => ConsultaMedica?.Sintomatologia;
		public string? ConsultaObservaciones => ConsultaMedica?.Observaciones;

		// Datos de triaje corregidos según BD
		public string? NivelUrgencia => Triaje?.NivelUrgencia?.ToString();
		public string? Temperatura => Triaje?.SignosVitales?.Temperatura?.ToString("F1");
		public string? PresionArterial => Triaje?.SignosVitales?.PresionArterial;
		public string? FrecuenciaCardiaca => Triaje?.SignosVitales?.FrecuenciaCardiaca?.ToString();
		public string? Peso => Triaje?.SignosVitales?.Peso?.ToString("F2");
		public string? Altura => Triaje?.SignosVitales?.Altura?.ToString();

		public string EstadoColor => Estado switch
		{
			"Completada" => "#27AE60",
			"Confirmada" => "#3498DB",
			"Programada" or "Pendiente" => "#3498DB",
			"Cancelada" => "#E74C3C",
			"No asistió" => "#95A5A6",
			_ => "#BDC3C7"
		};
	}

	public class DoctorInfo
	{
		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		public string NombreCompleto => $"{Nombres} {Apellidos}";
	}

	public class EspecialidadInfo
	{
		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		[JsonPropertyName("nombre")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("descripcion")]
		public string? Descripcion { get; set; }
	}

	public class SucursalInfo
	{
		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

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

	public class ConsultaMedicaInfo
	{
		[JsonPropertyName("id_consulta")]
		public int IdConsulta { get; set; }

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

	// Modelos de apoyo corregidos
	public class TriajeInfo
	{
		[JsonPropertyName("id_triage")]
		public int? IdTriage { get; set; }

		[JsonPropertyName("nivel_urgencia")]
		public int? NivelUrgencia { get; set; }  // tinyint en BD

		[JsonPropertyName("signos_vitales")]
		public SignosVitales? SignosVitales { get; set; }

		[JsonPropertyName("observaciones")]
		public string? Observaciones { get; set; }
	}
	public class SignosVitales
	{
		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }  // decimal(5,2) en BD

		[JsonPropertyName("altura")]
		public int? Altura { get; set; }    // `talla` int en BD

		[JsonPropertyName("presion_arterial")]
		public string? PresionArterial { get; set; } // varchar(10)

		[JsonPropertyName("temperatura")]
		public decimal? Temperatura { get; set; }  // decimal(4,1)

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }  // int

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }  // decimal(4,2)
	}


	public class EstadisticasHistorial
	{
		[JsonPropertyName("total_citas")]
		public int TotalCitas { get; set; }

		[JsonPropertyName("citas_completadas")]
		public int CitasCompletadas { get; set; }

		[JsonPropertyName("citas_pendientes")]
		public int CitasPendientes { get; set; }

		[JsonPropertyName("citas_canceladas")]
		public int CitasCanceladas { get; set; }
	}

	public class HistorialCompletoResponse
	{
		[JsonPropertyName("citas")]
		public List<CitaMedica> Citas { get; set; } = new();

		[JsonPropertyName("estadisticas")]
		public EstadisticasHistorial Estadisticas { get; set; } = new();

		[JsonPropertyName("filtros_aplicados")]
		public Dictionary<string, object>? FiltrosAplicados { get; set; }
	}
	// Modelo para respuesta básica (sin filtros)
	public class HistorialBasicoResponse
	{
		[JsonPropertyName("citas_medicas")]
		public List<CitaMedica>? CitasMedicas { get; set; }

		[JsonPropertyName("estadisticas")]
		public EstadisticasHistorial? Estadisticas { get; set; }

		[JsonPropertyName("paciente")]
		public PacienteResponse? Paciente { get; set; }

		[JsonPropertyName("historial_clinico")]
		public object? HistorialClinico { get; set; }
	}

	public class Especialidad
	{
		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		[JsonPropertyName("nombre_especialidad")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("descripcion")]
		public string? Descripcion { get; set; }
	}

	public class Doctor
	{
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		public string NombreCompleto => $"Dr. {Nombres} {Apellidos}";
	}

	public class Sucursal
	{
		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("nombre_sucursal")]
		public string Nombre { get; set; } = "";

		[JsonPropertyName("direccion")]
		public string? Direccion { get; set; }

		[JsonPropertyName("telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("email")]
		public string? Email { get; set; }
	}
}