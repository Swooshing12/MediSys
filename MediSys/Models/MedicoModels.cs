using System.Text.Json.Serialization;
using System.ComponentModel;

namespace MediSys.Models
{
	// ===== MÉDICO PRINCIPAL =====
	public class MedicoCompleto
	{
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("id_usuario")]
		public int IdUsuario { get; set; }

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		[JsonPropertyName("cedula")]
		public int Cedula { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; } = "";

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

		[JsonPropertyName("id_estado")]
		public int IdEstado { get; set; }

		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		[JsonPropertyName("nombre_especialidad")]
		public string NombreEspecialidad { get; set; } = "";

		[JsonPropertyName("nombre_completo")]
		public string NombreCompleto { get; set; } = "";

		[JsonPropertyName("estado_texto")]
		public string EstadoTexto { get; set; } = "";

		[JsonPropertyName("sucursales")]
		public List<SucursalAsignada> Sucursales { get; set; } = new();

		[JsonPropertyName("total_horarios")]
		public int TotalHorarios { get; set; }

		// Propiedades calculadas para UI
		public string CedulaString => Cedula.ToString();
		public string Iniciales => $"{(string.IsNullOrEmpty(Nombres) ? "" : Nombres[0])}{(string.IsNullOrEmpty(Apellidos) ? "" : Apellidos[0])}";
		public Color EstadoColor => IdEstado switch
		{
			1 => Colors.Green,  // Activo
			2 => Colors.Red,    // Inactivo
			3 => Colors.Orange, // Pendiente
			4 => Colors.DarkRed, // Bloqueado
			_ => Colors.Gray
		};
		public string EspecialidadDisplay => $"🩺 {NombreEspecialidad}";
		public string SucursalesTexto => string.Join(", ", Sucursales.Select(s => s.NombreSucursal));
	}

	// ===== HORARIOS =====
	public class HorarioDoctor
	{
		[JsonPropertyName("id_horario")]
		public int IdHorario { get; set; }

		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("dia_semana")]
		public int DiaSemana { get; set; } // 1-7 (Lunes-Domingo)

		[JsonPropertyName("hora_inicio")]
		public string HoraInicio { get; set; } = "";

		[JsonPropertyName("hora_fin")]
		public string HoraFin { get; set; } = "";

		[JsonPropertyName("duracion_cita")]
		public int DuracionCita { get; set; } = 30; // Por defecto 30 minutos

		[JsonPropertyName("activo")]
		public int Activo { get; set; } = 1;

		[JsonPropertyName("fecha_creacion")]
		public string? FechaCreacion { get; set; }

		[JsonPropertyName("nombre_sucursal")]
		public string NombreSucursal { get; set; } = "";

		[JsonPropertyName("nombre_dia")]
		public string NombreDia { get; set; } = "";

		[JsonPropertyName("horario_completo")]
		public string HorarioCompleto { get; set; } = "";

		[JsonPropertyName("citas_estimadas_dia")]
		public int CitasEstimadasDia { get; set; }

		// Propiedades calculadas
		public string DiaSemanaTexto => DiaSemana switch
		{
			1 => "Lunes",
			2 => "Martes",
			3 => "Miércoles",
			4 => "Jueves",
			5 => "Viernes",
			6 => "Sábado",
			7 => "Domingo",
			_ => "Desconocido"
		};

		public string HorarioDisplay => $"{DiaSemanaTexto}: {HoraInicio} - {HoraFin}";
		public string DuracionDisplay => $"{DuracionCita} min/cita";
		public string CitasEstimadasDisplay => $"~{CitasEstimadasDia} citas";
		public Color DiaSemanaColor => DiaSemana <= 5 ? Colors.Blue : Colors.Orange; // Semana vs fin de semana
	}

	// ===== SUCURSAL ASIGNADA =====
	public class SucursalAsignada
	{
		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("nombre_sucursal")]
		public string NombreSucursal { get; set; } = "";
	}

	// ===== HORARIOS AGRUPADOS POR SUCURSAL =====
	public class HorariosPorSucursal
	{
		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("nombre_sucursal")]
		public string NombreSucursal { get; set; } = "";

		[JsonPropertyName("horarios")]
		public List<HorarioDoctor> Horarios { get; set; } = new();

		[JsonPropertyName("estadisticas")]
		public EstadisticasSucursal Estadisticas { get; set; } = new();
	}

	public class EstadisticasSucursal
	{
		[JsonPropertyName("total_horarios")]
		public int TotalHorarios { get; set; }

		[JsonPropertyName("horas_semanales")]
		public double HorasSemanales { get; set; }

		[JsonPropertyName("citas_estimadas_semana")]
		public int CitasEstimadasSemana { get; set; }
	}

	// ===== RESPUESTAS DEL API =====
	public class MedicosResponse
	{
		[JsonPropertyName("doctores")]
		public List<MedicoCompleto> Doctores { get; set; } = new();

		[JsonPropertyName("pagination")]
		public PaginationInfo Pagination { get; set; } = new();
	}

	public class HorariosResponse
	{
		[JsonPropertyName("horarios_raw")]
		public List<HorarioDoctor> HorariosRaw { get; set; } = new();

		[JsonPropertyName("horarios_por_sucursal")]
		public List<HorariosPorSucursal> HorariosPorSucursal { get; set; } = new();

		[JsonPropertyName("estadisticas")]
		public EstadisticasGenerales Estadisticas { get; set; } = new();

		[JsonPropertyName("info_duraciones")]
		public InfoDuraciones InfoDuraciones { get; set; } = new();
	}

	public class EstadisticasGenerales
	{
		[JsonPropertyName("total_horarios")]
		public int TotalHorarios { get; set; }

		[JsonPropertyName("total_horas_semanales")]
		public double TotalHorasSemanales { get; set; }

		[JsonPropertyName("total_citas_estimadas_semana")]
		public int TotalCitasEstimadasSemana { get; set; }

		[JsonPropertyName("duraciones_utilizadas")]
		public List<int> DuracionesUtilizadas { get; set; } = new();
	}

	public class InfoDuraciones
	{
		[JsonPropertyName("duracion_por_defecto")]
		public int DuracionPorDefecto { get; set; } = 30;

		[JsonPropertyName("duraciones_permitidas")]
		public List<int> DuracionesPermitidas { get; set; } = new();

		[JsonPropertyName("duraciones_en_uso")]
		public List<int> DuracionesEnUso { get; set; } = new();
	}

	// ===== REQUEST MODELS =====
	public class CrearMedicoRequest
	{
		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = "";

		[JsonPropertyName("username")]
		public string Username { get; set; } = "";

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = "";

		[JsonPropertyName("password")]
		public string Password { get; set; } = "";

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = "";

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = "";

		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		[JsonPropertyName("titulo_profesional")]
		public string? TituloProfesional { get; set; }

		[JsonPropertyName("sucursales")]
		public List<int> Sucursales { get; set; } = new();

		[JsonPropertyName("horarios")]
		public List<CrearHorarioRequest> Horarios { get; set; } = new();
	}

	public class CrearHorarioRequest
	{
		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("dia_semana")]
		public int DiaSemana { get; set; }

		[JsonPropertyName("hora_inicio")]
		public string HoraInicio { get; set; } = "";

		[JsonPropertyName("hora_fin")]
		public string HoraFin { get; set; } = "";

		[JsonPropertyName("duracion_cita")]
		public int DuracionCita { get; set; } = 30;
	}

	public class GuardarHorariosRequest
	{
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("horarios")]
		public List<CrearHorarioRequest> Horarios { get; set; } = new();
	}

	// ===== BÚSQUEDA POR CÉDULA =====
	public class BuscarMedicoRequest
	{
		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = "";
	}

	public class EditarHorarioRequest
	{
		[JsonPropertyName("id_horario")]
		public int IdHorario { get; set; }

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("dia_semana")]
		public int DiaSemana { get; set; }

		[JsonPropertyName("hora_inicio")]
		public string HoraInicio { get; set; } = "";

		[JsonPropertyName("hora_fin")]
		public string HoraFin { get; set; } = "";

		[JsonPropertyName("duracion_cita")]
		public int DuracionCita { get; set; }
	}

	// ===== PAGINACIÓN =====
	public class PaginationInfo
	{
		[JsonPropertyName("current_page")]
		public int CurrentPage { get; set; }

		[JsonPropertyName("per_page")]
		public int PerPage { get; set; }

		[JsonPropertyName("total")]
		public int Total { get; set; }

		[JsonPropertyName("total_pages")]
		public int TotalPages { get; set; }

		[JsonPropertyName("has_next")]
		public bool HasNext { get; set; }

		[JsonPropertyName("has_prev")]
		public bool HasPrev { get; set; }
	}
}