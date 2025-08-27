// MediSys/Models/CitaModels.cs
using System.Text.Json.Serialization;

namespace MediSys.Models
{
	// ===== PACIENTE SEARCH =====
	public class PacienteBusqueda
	{
		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("id_usuario")]
		public int IdUsuario { get; set; }

		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = "";

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = "";

		[JsonPropertyName("telefono")]
		public string Telefono { get; set; } = "";

		[JsonPropertyName("fecha_nacimiento")]
		public string FechaNacimiento { get; set; } = "";

		[JsonPropertyName("tipo_sangre")]
		public string TipoSangre { get; set; } = "";

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = "";

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = "";

		[JsonPropertyName("alergias")]
		public string Alergias { get; set; } = "";

		[JsonPropertyName("antecedentes_medicos")]
		public string AntecedentesMedicos { get; set; } = "";

		[JsonPropertyName("contacto_emergencia")]
		public string ContactoEmergencia { get; set; } = "";

		[JsonPropertyName("telefono_emergencia")]
		public string TelefonoEmergencia { get; set; } = "";

		[JsonPropertyName("numero_seguro")]
		public string NumeroSeguro { get; set; } = "";

		public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
		public string EdadDisplay
		{
			get
			{
				if (DateTime.TryParse(FechaNacimiento, out DateTime fechaNac))
				{
					var edad = DateTime.Now.Year - fechaNac.Year;
					if (DateTime.Now < fechaNac.AddYears(edad)) edad--;
					return $"{edad} años";
				}
				return "N/A";
			}
		}
		public string SexoDisplay => Sexo switch
		{
			"M" => "👨 Masculino",
			"F" => "👩 Femenino",
			_ => Sexo
		};
	}

	public class PacienteData
	{
		[JsonPropertyName("paciente")]
		public PacienteBusqueda Paciente { get; set; } = new();

		[JsonPropertyName("historial_clinico")]
		public object? HistorialClinico { get; set; }

		[JsonPropertyName("estadisticas_rapidas")]
		public object? EstadisticasRapidas { get; set; }
	}

	// ===== CREAR PACIENTE REQUEST =====
	public class CrearPacienteRequest
	{
		[JsonPropertyName("cedula")]
		public string Cedula { get; set; } = "";

		[JsonPropertyName("nombres")]
		public string Nombres { get; set; } = "";

		[JsonPropertyName("apellidos")]
		public string Apellidos { get; set; } = "";

		[JsonPropertyName("correo")]
		public string Correo { get; set; } = "";

		[JsonPropertyName("telefono")]
		public string Telefono { get; set; } = "";

		[JsonPropertyName("fecha_nacimiento")]
		public string FechaNacimiento { get; set; } = "";

		[JsonPropertyName("sexo")]
		public string Sexo { get; set; } = "";

		[JsonPropertyName("nacionalidad")]
		public string Nacionalidad { get; set; } = "Ecuatoriana";

		[JsonPropertyName("tipo_sangre")]
		public string TipoSangre { get; set; } = "";

		[JsonPropertyName("alergias")]
		public string Alergias { get; set; } = "";

		[JsonPropertyName("antecedentes_medicos")]
		public string AntecedentesMedicos { get; set; } = "";

		[JsonPropertyName("contacto_emergencia")]
		public string ContactoEmergencia { get; set; } = "";

		[JsonPropertyName("telefono_emergencia")]
		public string TelefonoEmergencia { get; set; } = "";

		[JsonPropertyName("numero_seguro")]
		public string NumeroSeguro { get; set; } = "";
	}

	// ===== CITA MÉDICA COMPLETA =====
	public class CitaMedica2
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

		[JsonPropertyName("modalidad_cita")]
		public string ModalidadCita { get; set; } = "";

		[JsonPropertyName("notas")]
		public string Notas { get; set; } = "";

		[JsonPropertyName("cita_notas")]
		public string CitaNotas { get; set; } = "";

		[JsonPropertyName("enlace_virtual")]
		public string EnlaceVirtual { get; set; } = "";

		[JsonPropertyName("sala_virtual")]
		public string SalaVirtual { get; set; } = "";

		[JsonPropertyName("fecha_creacion")]
		public string FechaCreacion { get; set; } = "";

		[JsonPropertyName("cita_creada")]
		public string CitaCreada { get; set; } = "";

		// Datos del paciente
		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("paciente_nombres")]
		public string PacienteNombres { get; set; } = "";

		[JsonPropertyName("paciente_apellidos")]
		public string PacienteApellidos { get; set; } = "";

		[JsonPropertyName("paciente_cedula")]
		public string PacienteCedula { get; set; } = "";

		[JsonPropertyName("paciente_telefono")]
		public string PacienteTelefono { get; set; } = "";

		[JsonPropertyName("paciente_correo")]
		public string PacienteCorreo { get; set; } = "";

		[JsonPropertyName("fecha_nacimiento")]
		public string FechaNacimiento { get; set; } = "";

		[JsonPropertyName("tipo_sangre")]
		public string TipoSangre { get; set; } = "";

		[JsonPropertyName("alergias")]
		public string Alergias { get; set; } = "";

		[JsonPropertyName("contacto_emergencia")]
		public string ContactoEmergencia { get; set; } = "";

		[JsonPropertyName("telefono_emergencia")]
		public string TelefonoEmergencia { get; set; } = "";

		// Datos del doctor
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("doctor_nombres")]
		public string DoctorNombres { get; set; } = "";

		[JsonPropertyName("doctor_apellidos")]
		public string DoctorApellidos { get; set; } = "";

		[JsonPropertyName("medico_nombres")]
		public string MedicoNombres { get; set; } = "";

		[JsonPropertyName("medico_apellidos")]
		public string MedicoApellidos { get; set; } = "";

		[JsonPropertyName("titulo_profesional")]
		public string TituloProfesional { get; set; } = "";

		[JsonPropertyName("medico_titulo")]
		public string MedicoTitulo { get; set; } = "";

		// Especialidad y sucursal
		[JsonPropertyName("id_especialidad")]
		public int IdEspecialidad { get; set; }

		[JsonPropertyName("nombre_especialidad")]
		public string NombreEspecialidad { get; set; } = "";

		[JsonPropertyName("especialidad_descripcion")]
		public string EspecialidadDescripcion { get; set; } = "";

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("nombre_sucursal")]
		public string NombreSucursal { get; set; } = "";

		[JsonPropertyName("sucursal_direccion")]
		public string SucursalDireccion { get; set; } = "";

		[JsonPropertyName("sucursal_telefono")]
		public string SucursalTelefono { get; set; } = "";

		[JsonPropertyName("sucursal_email")]
		public string SucursalEmail { get; set; } = "";

		[JsonPropertyName("horario_atencion")]
		public string HorarioAtencion { get; set; } = "";

		// Tipo de cita
		[JsonPropertyName("id_tipo_cita")]
		public int IdTipoCita { get; set; }

		[JsonPropertyName("tipo_cita")]
		public string NombreTipoCita { get; set; } = "";

		[JsonPropertyName("tipo_cita")]
		public string TipoCitaNombre { get; set; } = "";


		// Consulta médica
		[JsonPropertyName("id_consulta")]
		public int? IdConsulta { get; set; }

		[JsonPropertyName("motivo_consulta")]
		public string MotivoConsulta { get; set; } = "";

		[JsonPropertyName("sintomatologia")]
		public string Sintomatologia { get; set; } = "";

		[JsonPropertyName("diagnostico")]
		public string Diagnostico { get; set; } = "";

		[JsonPropertyName("tratamiento")]
		public string Tratamiento { get; set; } = "";

		[JsonPropertyName("observaciones_medicas")]
		public string ObservacionesMedicas { get; set; } = "";

		[JsonPropertyName("consulta_observaciones")]
		public string ConsultaObservaciones { get; set; } = "";

		[JsonPropertyName("fecha_seguimiento")]
		public string FechaSeguimiento { get; set; } = "";

		// Triaje
		[JsonPropertyName("id_triage")]
		public int? IdTriage { get; set; }

		[JsonPropertyName("nivel_urgencia")]
		public int? NivelUrgencia { get; set; }

		[JsonPropertyName("estado_triaje")]
		public string EstadoTriaje { get; set; } = "";

		[JsonPropertyName("temperatura")]
		public decimal? Temperatura { get; set; }

		[JsonPropertyName("presion_arterial")]
		public string PresionArterial { get; set; } = "";

		[JsonPropertyName("frecuencia_cardiaca")]
		public int? FrecuenciaCardiaca { get; set; }

		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }

		[JsonPropertyName("talla")]
		public decimal? Talla { get; set; }

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }

		[JsonPropertyName("triaje_observaciones")]
		public string TriajeObservaciones { get; set; } = "";

		// Propiedades calculadas
		public string PacienteCompleto => $"{PacienteNombres} {PacienteApellidos}".Trim();
		public string DoctorCompleto => $"Dr. {(DoctorNombres ?? MedicoNombres)} {(DoctorApellidos ?? MedicoApellidos)}".Trim();
		public DateTime FechaHoraParsed => DateTime.TryParse(FechaHora, out var fecha) ? fecha : DateTime.MinValue;
		public string FechaDisplay => FechaHoraParsed.ToString("dd/MM/yyyy");
		public string HoraDisplay => FechaHoraParsed.ToString("HH:mm");
		public string FechaHoraDisplay => FechaHoraParsed.ToString("dd/MM/yyyy HH:mm");
		public bool EsVirtual => TipoCita?.ToLower() == "virtual" || ModalidadCita?.ToLower() == "virtual";
		public bool TieneConsulta => IdConsulta.HasValue && IdConsulta > 0;
		public bool TieneTriaje => IdTriage.HasValue && IdTriage > 0;

		public Color EstadoColor => Estado switch
		{
			"Pendiente" => Colors.Orange,
			"Confirmada" => Colors.Blue,
			"Programada" => Colors.Blue,
			"Completada" => Colors.Green,
			"Cancelada" => Colors.Red,
			"No asistió" => Colors.Gray,
			_ => Colors.Gray
		};

		public string EstadoIcon => Estado switch
		{
			"Pendiente" => "⏳",
			"Confirmada" => "✅",
			"Programada" => "📋",
			"Completada" => "🏁",
			"Cancelada" => "❌",
			"No asistió" => "👻",
			_ => "📋"
		};

		public string TipoIcon => EsVirtual ? "💻" : "🏥";
		public string UrgenciaDisplay => NivelUrgencia switch
		{
			1 => "🟢 Baja",
			2 => "🟡 Media",
			3 => "🟠 Alta",
			4 => "🔴 Crítica",
			5 => "🚨 Emergencia",
			_ => "➖ Sin triaje"
		};
	}

	// ===== CREAR CITA REQUEST =====
	public class CrearCitaRequest
	{
		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("id_sucursal")]
		public int IdSucursal { get; set; }

		[JsonPropertyName("id_tipo_cita")]
		public int IdTipoCita { get; set; } = 1; // Por defecto presencial

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("tipo_cita")]
		public string TipoCita { get; set; } = "presencial";

		[JsonPropertyName("notas")]
		public string Notas { get; set; } = "";

		[JsonPropertyName("enlace_virtual")]
		public string EnlaceVirtual { get; set; } = "";

		[JsonPropertyName("sala_virtual")]
		public string SalaVirtual { get; set; } = "";
	}

	// ===== HORARIOS DISPONIBLES =====
	public class HorariosDisponiblesResponse
	{
		[JsonPropertyName("horarios")]
		public List<HorarioDoctor> Horarios { get; set; } = new();

		[JsonPropertyName("citas_ocupadas")]
		public List<CitaOcupada> CitasOcupadas { get; set; } = new();

		[JsonPropertyName("excepciones")]
		public List<ExcepcionHorario> Excepciones { get; set; } = new();

		[JsonPropertyName("semana_inicio")]
		public string SemanaInicio { get; set; } = "";

		[JsonPropertyName("semana_fin")]
		public string SemanaFin { get; set; } = "";
	}

	public class CitaOcupada
	{
		[JsonPropertyName("fecha")]
		public string Fecha { get; set; } = "";

		[JsonPropertyName("hora")]
		public string Hora { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";
	}

	public class ExcepcionHorario
	{
		[JsonPropertyName("fecha")]
		public string Fecha { get; set; } = "";

		[JsonPropertyName("tipo")]
		public string Tipo { get; set; } = "";

		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("hora_inicio")]
		public string HoraInicio { get; set; } = "";

		[JsonPropertyName("hora_fin")]
		public string HoraFin { get; set; } = "";
	}

	// ===== TIPOS DE CITA =====
	public class TipoCita
	{
		[JsonPropertyName("id_tipo_cita")]
		public int IdTipoCita { get; set; }

		[JsonPropertyName("nombre_tipo")]
		public string NombreTipo { get; set; } = "";

		[JsonPropertyName("descripcion")]
		public string Descripcion { get; set; } = "";

		[JsonPropertyName("activo")]
		public int Activo { get; set; }

		[JsonIgnore]
		public bool ActivoBool => Activo == 1;


		[JsonPropertyName("fecha_creacion")]
		public string FechaCreacion { get; set; } = "";

		public string IconoTipo => NombreTipo.ToLower() switch
		{
			"presencial" => "🏥",
			"virtual" => "💻",
			"urgente" => "🚨",
			"emergencia" => "🚑",
			_ => "📅"
		};
	}

	// ===== SLOT DE HORARIO =====
	public class SlotHorario
	{
		public string Fecha { get; set; } = "";
		public string Hora { get; set; } = "";
		public string FechaHora { get; set; } = "";
		public bool Disponible { get; set; }
		public string Motivo { get; set; } = "";
		public string Estado { get; set; } = "";
		public int DiaSemana { get; set; }
		public DateTime FechaParsed => DateTime.TryParse(Fecha, out var fecha) ? fecha : DateTime.MinValue;
		public TimeSpan HoraParsed => TimeSpan.TryParse(Hora, out var hora) ? hora : TimeSpan.Zero;

		public Color ColorSlot => Disponible ? Colors.LightGreen : Colors.LightGray;
		public string TextoSlot => Disponible ? "✓" : "X";
		public string DiaNombre => FechaParsed.ToString("ddd");
		public string FechaCorta => FechaParsed.ToString("dd/MM");
		public string HoraCorta => HoraParsed.ToString(@"hh\:mm");
	}

	// ===== RESPUESTAS DE CITAS =====
	public class CitasResponse
	{
		[JsonPropertyName("citas")]
		public List<CitaMedica2> Citas { get; set; } = new();

		[JsonPropertyName("total")]
		public int Total { get; set; }

		[JsonPropertyName("total_registros")]
		public int TotalRegistros { get; set; }

		[JsonPropertyName("mostrando")]
		public int Mostrando { get; set; }

		[JsonPropertyName("pagina_actual")]
		public int PaginaActual { get; set; }

		[JsonPropertyName("total_paginas")]
		public int TotalPaginas { get; set; }

		[JsonPropertyName("por_pagina")]
		public int PorPagina { get; set; }

		[JsonPropertyName("tiene_siguiente")]
		public bool TieneSiguiente { get; set; }

		[JsonPropertyName("tiene_anterior")]
		public bool TieneAnterior { get; set; }
	}

	public class PacienteResponse2
	{
		[JsonPropertyName("paciente")]
		public PacienteBusqueda Paciente { get; set; } = new();

		[JsonPropertyName("encontrado")]
		public bool Encontrado { get; set; }

		[JsonPropertyName("mensaje")]
		public string Mensaje { get; set; } = "";
	}

	public class CrearCitaResponse
	{
		[JsonPropertyName("cita")]
		public CitaMedica2 Cita { get; set; } = new();

		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("mensaje")]
		public string Mensaje { get; set; } = "";

		[JsonPropertyName("password_temporal")]
		public string PasswordTemporal { get; set; } = "";
	}

	// ===== FILTROS DE CITAS =====
	public class FiltrosCitas
	{
		public DateTime? FechaDesde { get; set; }
		public DateTime? FechaHasta { get; set; }
		public int? IdEspecialidad { get; set; }
		public int? IdDoctor { get; set; }
		public string? Estado { get; set; }
		public int? IdSucursal { get; set; }
		public string? CedulaPaciente { get; set; }
		public string? NombrePaciente { get; set; }
		public string? TipoCita { get; set; }
		public int? NivelUrgencia { get; set; }
		public int Pagina { get; set; } = 1;
		public int ResultadosPorPagina { get; set; } = 20;
		public string OrdenarPor { get; set; } = "fecha_hora";
		public string Direccion { get; set; } = "asc";

		// Métodos de ayuda
		public string FechaDesdeString => FechaDesde?.ToString("yyyy-MM-dd") ?? "";
		public string FechaHastaString => FechaHasta?.ToString("yyyy-MM-dd") ?? "";
	}

	// ===== ESTADÍSTICAS =====
	public class EstadisticasCitas
	{
		[JsonPropertyName("total_citas")]
		public int TotalCitas { get; set; }

		[JsonPropertyName("citas_pendientes")]
		public int CitasPendientes { get; set; }

		[JsonPropertyName("citas_confirmadas")]
		public int CitasConfirmadas { get; set; }

		[JsonPropertyName("citas_programadas")]
		public int CitasProgramadas { get; set; }

		[JsonPropertyName("citas_completadas")]
		public int CitasCompletadas { get; set; }

		[JsonPropertyName("citas_canceladas")]
		public int CitasCanceladas { get; set; }

		[JsonPropertyName("citas_no_asistio")]
		public int CitasNoAsistio { get; set; }

		[JsonPropertyName("citas_hoy")]
		public int CitasHoy { get; set; }

		[JsonPropertyName("citas_semana")]
		public int CitasSemana { get; set; }

		[JsonPropertyName("citas_mes")]
		public int CitasMes { get; set; }

		[JsonPropertyName("citas_virtuales")]
		public int CitasVirtuales { get; set; }

		[JsonPropertyName("citas_presenciales")]
		public int CitasPresenciales { get; set; }

		[JsonPropertyName("promedio_citas_dia")]
		public decimal PromedioCitasDia { get; set; }

		[JsonPropertyName("tasa_cancelacion")]
		public decimal TasaCancelacion { get; set; }

		[JsonPropertyName("tasa_no_asistencia")]
		public decimal TasaNoAsistencia { get; set; }
	}

	// ===== HISTORIAL MÉDICO =====
	public class HistorialMedico
	{
		[JsonPropertyName("id_historial")]
		public int IdHistorial { get; set; }

		[JsonPropertyName("id_paciente")]
		public int IdPaciente { get; set; }

		[JsonPropertyName("fecha_creacion")]
		public string FechaCreacion { get; set; } = "";

		[JsonPropertyName("ultima_actualizacion")]
		public string UltimaActualizacion { get; set; } = "";

		[JsonPropertyName("paciente")]
		public PacienteBusqueda Paciente { get; set; } = new();

		[JsonPropertyName("citas")]
		public List<CitaMedica2> Citas { get; set; } = new();

		[JsonPropertyName("estadisticas")]
		public EstadisticasHistorial Estadisticas { get; set; } = new();
	}

	public class EstadisticasHistorial2
	{
		[JsonPropertyName("total_citas")]
		public int TotalCitas { get; set; }

		[JsonPropertyName("total_consultas")]
		public int TotalConsultas { get; set; }

		[JsonPropertyName("ultima_cita")]
		public string UltimaCita { get; set; } = "";

		[JsonPropertyName("proxima_cita")]
		public string ProximaCita { get; set; } = "";

		[JsonPropertyName("especialidades_visitadas")]
		public int EspecialidadesVisitadas { get; set; }

		[JsonPropertyName("doctores_consultados")]
		public int DoctoresConsultados { get; set; }

		[JsonPropertyName("citas_por_especialidad")]
		public Dictionary<string, int> CitasPorEspecialidad { get; set; } = new();

		[JsonPropertyName("citas_por_ano")]
		public Dictionary<string, int> CitasPorAno { get; set; } = new();
	}

	// ===== CONSULTA MÉDICA =====
	public class ConsultaMedica
	{
		[JsonPropertyName("id_consulta")]
		public int IdConsulta { get; set; }

		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("id_historial")]
		public int IdHistorial { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string FechaHora { get; set; } = "";

		[JsonPropertyName("fecha_consulta")]
		public string FechaConsulta { get; set; } = "";

		[JsonPropertyName("motivo_consulta")]
		public string MotivoConsulta { get; set; } = "";

		[JsonPropertyName("sintomatologia")]
		public string Sintomatologia { get; set; } = "";

		[JsonPropertyName("diagnostico")]
		public string Diagnostico { get; set; } = "";

		[JsonPropertyName("tratamiento")]
		public string Tratamiento { get; set; } = "";

		[JsonPropertyName("observaciones")]
		public string Observaciones { get; set; } = "";

		[JsonPropertyName("observaciones_medicas")]
		public string ObservacionesMedicas { get; set; } = "";

		[JsonPropertyName("fecha_seguimiento")]
		public string FechaSeguimiento { get; set; } = "";

		[JsonPropertyName("receta_medica")]
		public string RecetaMedica { get; set; } = "";

		[JsonPropertyName("examenes_solicitados")]
		public string ExamenesSolicitados { get; set; } = "";

		[JsonPropertyName("proximo_control")]
		public string ProximoControl { get; set; } = "";

		public DateTime FechaConsultaParsed => DateTime.TryParse(FechaConsulta ?? FechaHora, out var fecha) ? fecha : DateTime.MinValue;
		public string FechaConsultaDisplay => FechaConsultaParsed.ToString("dd/MM/yyyy HH:mm");
		public bool TieneSeguimiento => !string.IsNullOrEmpty(FechaSeguimiento) && DateTime.TryParse(FechaSeguimiento, out _);
	}

	// ===== TRIAJE =====
	public class Triaje
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
		public decimal? SaturacionOxigeno { get; set; }

		[JsonPropertyName("peso")]
		public decimal? Peso { get; set; }

		[JsonPropertyName("talla")]
		public decimal? Talla { get; set; }

		[JsonPropertyName("imc")]
		public decimal? Imc { get; set; }

		[JsonPropertyName("dolor_escala")]
		public int? DolorEscala { get; set; }

		[JsonPropertyName("alergias_conocidas")]
		public string AlergiasConocidas { get; set; } = "";

		[JsonPropertyName("medicamentos_actuales")]
		public string MedicamentosActuales { get; set; } = "";

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
			2 => Colors.Yellow,
			3 => Colors.Orange,
			4 => Colors.Red,
			5 => Colors.DarkRed,
			_ => Colors.Gray
		};

		public string EnfermeroCompleto => $"{EnfermeroNombres} {EnfermeroApellidos}".Trim();
		public string ImcCalculado => Imc?.ToString("F1") ?? "N/A";
		public string TemperaturaDisplay => Temperatura?.ToString("F1") + "°C" ?? "N/A";
		public string DolorDisplay => DolorEscala?.ToString() + "/10" ?? "Sin evaluar";
		public string SaturacionDisplay => SaturacionOxigeno?.ToString("F1") + "%" ?? "N/A";
		public DateTime FechaTriajeParsed => DateTime.TryParse(FechaHora, out var fecha) ? fecha : DateTime.MinValue;
		public string FechaTriajeDisplay => FechaTriajeParsed.ToString("dd/MM/yyyy HH:mm");
	}

	// ===== ACTUALIZAR CITA =====
	public class ActualizarCitaRequest
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("fecha_hora")]
		public string? FechaHora { get; set; }

		[JsonPropertyName("motivo")]
		public string? Motivo { get; set; }

		[JsonPropertyName("estado")]
		public string? Estado { get; set; }

		[JsonPropertyName("notas")]
		public string? Notas { get; set; }

		[JsonPropertyName("id_tipo_cita")]
		public int? IdTipoCita { get; set; }

		[JsonPropertyName("enlace_virtual")]
		public string? EnlaceVirtual { get; set; }

		[JsonPropertyName("sala_virtual")]
		public string? SalaVirtual { get; set; }
	}

	// ===== CANCELAR CITA =====
	public class CancelarCitaRequest
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("motivo_cancelacion")]
		public string MotivoCancelacion { get; set; } = "";

		[JsonPropertyName("notificar_paciente")]
		public bool NotificarPaciente { get; set; } = true;

		[JsonPropertyName("notificar_doctor")]
		public bool NotificarDoctor { get; set; } = true;
	}

	// ===== CONFIRMAR CITA =====
	public class ConfirmarCitaRequest
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("confirmado_por")]
		public string ConfirmadoPor { get; set; } = "";

		[JsonPropertyName("notas_confirmacion")]
		public string NotasConfirmacion { get; set; } = "";

		[JsonPropertyName("enviar_recordatorio")]
		public bool EnviarRecordatorio { get; set; } = true;
	}

	// ===== REPROGRAMAR CITA =====
	public class ReprogramarCitaRequest
	{
		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("nueva_fecha_hora")]
		public string NuevaFechaHora { get; set; } = "";

		[JsonPropertyName("motivo_reprogramacion")]
		public string MotivoReprogramacion { get; set; } = "";

		[JsonPropertyName("notificar_paciente")]
		public bool NotificarPaciente { get; set; } = true;

		[JsonPropertyName("notificar_doctor")]
		public bool NotificarDoctor { get; set; } = true;
	}

	// ===== BUSCAR CITAS =====
	public class BuscarCitasRequest
	{
		[JsonPropertyName("fecha_desde")]
		public string? FechaDesde { get; set; }

		[JsonPropertyName("fecha_hasta")]
		public string? FechaHasta { get; set; }

		[JsonPropertyName("id_paciente")]
		public int? IdPaciente { get; set; }

		[JsonPropertyName("id_doctor")]
		public int? IdDoctor { get; set; }

		[JsonPropertyName("id_especialidad")]
		public int? IdEspecialidad { get; set; }

		[JsonPropertyName("id_sucursal")]
		public int? IdSucursal { get; set; }

		[JsonPropertyName("estado")]
		public string? Estado { get; set; }

		[JsonPropertyName("tipo_cita")]
		public string? TipoCita { get; set; }

		[JsonPropertyName("cedula_paciente")]
		public string? CedulaPaciente { get; set; }

		[JsonPropertyName("nombre_paciente")]
		public string? NombrePaciente { get; set; }

		[JsonPropertyName("incluir_consultas")]
		public bool IncluirConsultas { get; set; } = false;

		[JsonPropertyName("incluir_triaje")]
		public bool IncluirTriaje { get; set; } = false;

		[JsonPropertyName("pagina")]
		public int Pagina { get; set; } = 1;

		[JsonPropertyName("por_pagina")]
		public int PorPagina { get; set; } = 20;

		[JsonPropertyName("ordenar_por")]
		public string OrdenarPor { get; set; } = "fecha_hora";

		[JsonPropertyName("direccion")]
		public string Direccion { get; set; } = "asc";
	}

	// ===== DISPONIBILIDAD =====
	public class DisponibilidadDoctor
	{
		[JsonPropertyName("id_doctor")]
		public int IdDoctor { get; set; }

		[JsonPropertyName("fecha")]
		public string Fecha { get; set; } = "";

		[JsonPropertyName("slots_disponibles")]
		public List<SlotHorario> SlotsDisponibles { get; set; } = new();

		[JsonPropertyName("slots_ocupados")]
		public List<SlotHorario> SlotsOcupados { get; set; } = new();

		[JsonPropertyName("total_slots")]
		public int TotalSlots { get; set; }

		[JsonPropertyName("slots_libres")]
		public int SlotsLibres { get; set; }

		[JsonPropertyName("porcentaje_ocupacion")]
		public decimal PorcentajeOcupacion { get; set; }

		[JsonPropertyName("primera_hora_disponible")]
		public string PrimeraHoraDisponible { get; set; } = "";

		[JsonPropertyName("ultima_hora_disponible")]
		public string UltimaHoraDisponible { get; set; } = "";
	}

	// ===== NOTIFICACIONES =====
	public class NotificacionCita
	{
		[JsonPropertyName("id_notificacion")]
		public int IdNotificacion { get; set; }

		[JsonPropertyName("id_cita")]
		public int IdCita { get; set; }

		[JsonPropertyName("tipo_notificacion")]
		public string TipoNotificacion { get; set; } = "";

		[JsonPropertyName("destinatario")]
		public string Destinatario { get; set; } = "";

		[JsonPropertyName("canal")]
		public string Canal { get; set; } = ""; // email, sms, whatsapp

		[JsonPropertyName("mensaje")]
		public string Mensaje { get; set; } = "";

		[JsonPropertyName("estado")]
		public string Estado { get; set; } = "";

		[JsonPropertyName("fecha_envio")]
		public string FechaEnvio { get; set; } = "";

		[JsonPropertyName("fecha_programada")]
		public string FechaProgramada { get; set; } = "";

		[JsonPropertyName("intentos")]
		public int Intentos { get; set; }

		[JsonPropertyName("error_mensaje")]
		public string ErrorMensaje { get; set; } = "";

		public bool FueEnviada => Estado == "enviada" || Estado == "entregada";
		public bool TieneError => Estado == "error" || Estado == "fallida";
		public string CanalIcon => Canal switch
		{
			"email" => "📧",
			"sms" => "📱",
			"whatsapp" => "📞",
			"push" => "🔔",
			_ => "📬"
		};
	}

	// ===== REPORTES Y ANALYTICS =====
	public class ReporteCitas
	{
		[JsonPropertyName("periodo")]
		public string Periodo { get; set; } = "";

		[JsonPropertyName("fecha_desde")]
		public string FechaDesde { get; set; } = "";

		[JsonPropertyName("fecha_hasta")]
		public string FechaHasta { get; set; } = "";

		[JsonPropertyName("estadisticas")]
		public EstadisticasCitas Estadisticas { get; set; } = new();

		[JsonPropertyName("citas_por_dia")]
		public Dictionary<string, int> CitasPorDia { get; set; } = new();

		[JsonPropertyName("citas_por_hora")]
		public Dictionary<string, int> CitasPorHora { get; set; } = new();

		[JsonPropertyName("citas_por_especialidad")]
		public Dictionary<string, int> CitasPorEspecialidad { get; set; } = new();

		[JsonPropertyName("citas_por_doctor")]
		public Dictionary<string, int> CitasPorDoctor { get; set; } = new();

		[JsonPropertyName("citas_por_sucursal")]
		public Dictionary<string, int> CitasPorSucursal { get; set; } = new();

		[JsonPropertyName("tendencias")]
		public List<TendenciaCita> Tendencias { get; set; } = new();

		[JsonPropertyName("top_motivos_consulta")]
		public List<MotivoConsultaStats> TopMotivosConsulta { get; set; } = new();

		[JsonPropertyName("satisfaccion_promedio")]
		public decimal SatisfaccionPromedio { get; set; }

		[JsonPropertyName("tiempo_promedio_espera")]
		public int TiempoPromedioEspera { get; set; } // en minutos

		[JsonPropertyName("puntualidad_promedio")]
		public decimal PuntualidadPromedio { get; set; }
	}

	public class TendenciaCita
	{
		[JsonPropertyName("fecha")]
		public string Fecha { get; set; } = "";

		[JsonPropertyName("total_citas")]
		public int TotalCitas { get; set; }

		[JsonPropertyName("citas_completadas")]
		public int CitasCompletadas { get; set; }

		[JsonPropertyName("citas_canceladas")]
		public int CitasCanceladas { get; set; }

		[JsonPropertyName("tasa_cumplimiento")]
		public decimal TasaCumplimiento { get; set; }

		public DateTime FechaParsed => DateTime.TryParse(Fecha, out var fecha) ? fecha : DateTime.MinValue;
	}

	public class MotivoConsultaStats
	{
		[JsonPropertyName("motivo")]
		public string Motivo { get; set; } = "";

		[JsonPropertyName("cantidad")]
		public int Cantidad { get; set; }

		[JsonPropertyName("porcentaje")]
		public decimal Porcentaje { get; set; }

		[JsonPropertyName("especialidad_mas_comun")]
		public string EspecialidadMasComun { get; set; } = "";

		[JsonPropertyName("promedio_duracion")]
		public int PromedioDuracion { get; set; } // en minutos
	}

	// ===== CONFIGURACIÓN CITAS =====
	public class ConfiguracionCitas
	{
		[JsonPropertyName("duracion_cita_default")]
		public int DuracionCitaDefault { get; set; } = 30; // minutos

		[JsonPropertyName("anticipacion_minima")]
		public int AnticipacionMinima { get; set; } = 60; // minutos

		[JsonPropertyName("anticipacion_maxima")]
		public int AnticipacionMaxima { get; set; } = 90; // días

		[JsonPropertyName("permite_reprogramacion")]
		public bool PermiteReprogramacion { get; set; } = true;

		[JsonPropertyName("tiempo_limite_reprogramacion")]
		public int TiempoLimiteReprogramacion { get; set; } = 24; // horas

		[JsonPropertyName("permite_cancelacion")]
		public bool PermiteCancelacion { get; set; } = true;

		[JsonPropertyName("tiempo_limite_cancelacion")]
		public int TiempoLimiteCancelacion { get; set; } = 2; // horas

		[JsonPropertyName("notificaciones_habilitadas")]
		public bool NotificacionesHabilitadas { get; set; } = true;

		[JsonPropertyName("recordatorios_automaticos")]
		public bool RecordatoriosAutomaticos { get; set; } = true;

		[JsonPropertyName("horarios_recordatorios")]
		public List<int> HorariosRecordatorios { get; set; } = new() { 24, 2 }; // horas antes

		[JsonPropertyName("canales_notificacion")]
		public List<string> CanalesNotificacion { get; set; } = new() { "email", "sms" };

		[JsonPropertyName("overbooking_permitido")]
		public bool OverbookingPermitido { get; set; } = false;

		[JsonPropertyName("porcentaje_overbooking")]
		public decimal PorcentajeOverbooking { get; set; } = 10;

		[JsonPropertyName("bloqueo_fines_semana")]
		public bool BloqueoFinesSemana { get; set; } = false;

		[JsonPropertyName("bloqueo_feriados")]
		public bool BloqueoFeriados { get; set; } = true;
	}

	// ===== ESTADOS DE CITA =====
	public static class EstadosCita
	{
		public const string Pendiente = "Pendiente";
		public const string Confirmada = "Confirmada";
		public const string Programada = "Programada";
		public const string EnProceso = "En proceso";
		public const string Completada = "Completada";
		public const string Cancelada = "Cancelada";
		public const string NoAsistio = "No asistió";
		public const string Reprogramada = "Reprogramada";

		public static List<string> Todos => new()
	   {
		   Pendiente, Confirmada, Programada, EnProceso,
		   Completada, Cancelada, NoAsistio, Reprogramada
	   };

		public static List<string> Activos => new()
	   {
		   Pendiente, Confirmada, Programada, EnProceso
	   };

		public static List<string> Finalizados => new()
	   {
		   Completada, Cancelada, NoAsistio
	   };
	}

	// ===== TIPOS DE NOTIFICACIÓN =====
	public static class TiposNotificacion
	{
		public const string CitaCreada = "cita_creada";
		public const string CitaConfirmada = "cita_confirmada";
		public const string CitaCancelada = "cita_cancelada";
		public const string CitaReprogramada = "cita_reprogramada";
		public const string RecordatorioCita = "recordatorio_cita";
		public const string CitaProxima = "cita_proxima";
		public const string CitaVencida = "cita_vencida";
		public const string ResultadosDisponibles = "resultados_disponibles";

		public static List<string> Todos => new()
	   {
		   CitaCreada, CitaConfirmada, CitaCancelada, CitaReprogramada,
		   RecordatorioCita, CitaProxima, CitaVencida, ResultadosDisponibles
	   };
	}

	// ===== EXTENSIONES ÚTILES =====
	public static class CitaExtensions
	{
		public static bool EsProxima(this CitaMedica2 cita, int minutosAntes = 30)
		{
			var ahora = DateTime.Now;
			var fechaCita = cita.FechaHoraParsed;
			var diferencia = fechaCita - ahora;

			return diferencia.TotalMinutes <= minutosAntes && diferencia.TotalMinutes > 0;
		}

		public static bool EsVencida(this CitaMedica2 cita)
		{
			return cita.FechaHoraParsed < DateTime.Now &&
				   (cita.Estado == EstadosCita.Pendiente || cita.Estado == EstadosCita.Confirmada);
		}

		public static bool PuedeSerCancelada(this CitaMedica2 cita, int horasLimite = 2)
		{
			var ahora = DateTime.Now;
			var fechaCita = cita.FechaHoraParsed;
			var diferencia = fechaCita - ahora;

			return diferencia.TotalHours >= horasLimite &&
				   (cita.Estado == EstadosCita.Pendiente || cita.Estado == EstadosCita.Confirmada);
		}

		public static bool PuedeSerReprogramada(this CitaMedica2 cita, int horasLimite = 24)
		{
			var ahora = DateTime.Now;
			var fechaCita = cita.FechaHoraParsed;
			var diferencia = fechaCita - ahora;

			return diferencia.TotalHours >= horasLimite &&
				   (cita.Estado == EstadosCita.Pendiente || cita.Estado == EstadosCita.Confirmada);
		}

		public static string ObtenerDescripcionEstado(this CitaMedica2 cita)
		{
			return cita.Estado switch
			{
				EstadosCita.Pendiente => "Cita agendada, pendiente de confirmación",
				EstadosCita.Confirmada => "Cita confirmada por el paciente",
				EstadosCita.Programada => "Cita programada y lista",
				EstadosCita.EnProceso => "Consulta médica en proceso",
				EstadosCita.Completada => "Consulta médica finalizada",
				EstadosCita.Cancelada => "Cita cancelada",
				EstadosCita.NoAsistio => "Paciente no asistió a la cita",
				EstadosCita.Reprogramada => "Cita reprogramada",
				_ => "Estado desconocido"
			};
		}

		public static List<string> ObtenerAccionesPermitidas(this CitaMedica2 cita)
		{
			var acciones = new List<string>();

			switch (cita.Estado)
			{
				case EstadosCita.Pendiente:
					acciones.AddRange(new[] { "confirmar", "cancelar", "reprogramar", "ver_detalles" });
					break;
				case EstadosCita.Confirmada:
					acciones.AddRange(new[] { "cancelar", "reprogramar", "iniciar_consulta", "ver_detalles" });
					break;
				case EstadosCita.Programada:
					acciones.AddRange(new[] { "iniciar_consulta", "marcar_no_asistio", "ver_detalles" });
					break;
				case EstadosCita.EnProceso:
					acciones.AddRange(new[] { "finalizar_consulta", "ver_detalles" });
					break;
				case EstadosCita.Completada:
					acciones.AddRange(new[] { "ver_detalles", "ver_consulta", "imprimir_receta" });
					break;
				case EstadosCita.Cancelada:
				case EstadosCita.NoAsistio:
					acciones.AddRange(new[] { "ver_detalles", "reagendar" });
					break;
			}

			return acciones;
		}
	}
}