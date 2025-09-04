// ViewModels/DetalleCitaMedicaViewModel.cs - VERSIÓN COMPLETAMENTE NUEVA Y CORREGIDA
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using System.Text.Json;

namespace MediSys.ViewModels
{
	public partial class DetalleCitaMedicaViewModel : ObservableObject
	{
		// ===== PROPIEDAD PRINCIPAL =====
		[ObservableProperty]
		private CitaConsultaMedica cita;

		// ===== PROPIEDADES CALCULADAS PARA EL BINDING =====

		// Información básica
		public string IdCita => Cita?.IdCita.ToString() ?? "Sin ID";
		public string EstadoCita => Cita?.Estado ?? "Sin estado";
		public string FechaHoraDisplay => Cita?.FechaDisplay + " - " + Cita?.HoraDisplay ?? "Sin fecha";
		public string MotivoCita => Cita?.Motivo ?? "Sin motivo especificado";
		public Color EstadoColor => Cita?.EstadoColor ?? Colors.Gray;
		public string EstadoIcon => Cita?.EstadoIcon ?? "📋";

		// Información del paciente
		public string PacienteNombreCompleto => Cita?.Paciente?.NombreCompleto ?? "Sin nombre";
		public string PacienteCedula => Cita?.Paciente?.Cedula.ToString() ?? "Sin cédula";
		public string PacienteEdad => $"{Cita?.Paciente?.Edad ?? 0} años";
		public string PacienteSexo => Cita?.Paciente?.SexoDisplay ?? "No especificado";
		public string PacienteTipoSangre => Cita?.Paciente?.TipoSangre ?? "No especificado";
		public string PacienteAlergias => Cita?.Paciente?.Alergias ?? "Ninguna registrada";
		public string PacienteTelefono => Cita?.Paciente?.Telefono ?? "Sin teléfono";
		public string PacienteCorreo => Cita?.Paciente?.Correo ?? "Sin correo";

		// Información del doctor
		public string DoctorNombreCompleto => Cita?.Doctor?.NombreCompleto ?? "Sin doctor asignado";
		public string DoctorTitulo => Cita?.Doctor?.TituloProfesional ?? "Sin título";
		public string DoctorEspecialidad => Cita?.Doctor?.Especialidad ?? "Sin especialidad";

		// Información de sucursal
		public string SucursalNombre => Cita?.Sucursal?.Nombre ?? "Sin sucursal";
		public string SucursalDireccion => Cita?.Sucursal?.Direccion ?? "Sin dirección";
		public string SucursalTelefono => Cita?.Sucursal?.Telefono ?? "Sin teléfono";

		// Información de especialidad
		public string EspecialidadNombre => Cita?.Especialidad?.Nombre ?? "Sin especialidad";
		public string EspecialidadDescripcion => Cita?.Especialidad?.Descripcion ?? "Sin descripción";

		// Estados para mostrar secciones
		public bool TieneTriaje => Cita?.TieneTriaje == true && Cita?.Triaje != null;
		public bool TieneConsulta => Cita?.TieneConsulta == true && Cita?.ConsultaMedica != null;
		public bool PuedeConsultar => Cita?.PuedeConsultar == true;
		public bool EsUrgente => Cita?.EsUrgente == true;

		// ===== DATOS DEL TRIAJE (con validaciones nullas) =====
		public bool MostrarTriaje => TieneTriaje;

		// Signos vitales
		public string TriajeTemperatura => Cita?.Triaje?.SignosVitales?.Temperatura ?? "No registrada";
		public string TriajePresionArterial => Cita?.Triaje?.SignosVitales?.PresionArterial ?? "No registrada";
		public string TriajeFrecuenciaCardiaca => Cita?.Triaje?.SignosVitales?.FrecuenciaCardiaca?.ToString() ?? "No registrada";
		public string TriajeFrecuenciaRespiratoria => Cita?.Triaje?.SignosVitales?.FrecuenciaRespiratoria?.ToString() ?? "No registrada";
		public string TriajeSaturacionOxigeno => Cita?.Triaje?.SignosVitales?.SaturacionOxigeno?.ToString() ?? "No registrada";
		public string TriajePeso => Cita?.Triaje?.SignosVitales?.Peso ?? "No registrado";
		public string TriajeTalla => Cita?.Triaje?.SignosVitales?.Talla?.ToString() ?? "No registrada";
		public string TriajeIMC => Cita?.Triaje?.SignosVitales?.IMC ?? "No calculado";

		// Evaluación de triaje
		public string TriajeNivelUrgencia => Cita?.Triaje?.Evaluacion?.NivelUrgenciaTexto ?? "No especificado";
		public Color TriajeColorUrgencia => Cita?.Triaje?.Evaluacion?.NivelUrgenciaColor ?? Colors.Gray;
		public string TriajeObservaciones => Cita?.Triaje?.Evaluacion?.Observaciones ?? "Sin observaciones";
		public string TriajeFecha => Cita?.Triaje?.Evaluacion?.FechaTriaje ?? "Sin fecha";

		// ===== DATOS DE LA CONSULTA MÉDICA =====
		public bool MostrarConsulta => TieneConsulta;
		public string ConsultaMotivoConsulta => Cita?.ConsultaMedica?.MotivoConsulta ?? "Sin motivo";
		public string ConsultaSintomatologia => Cita?.ConsultaMedica?.Sintomatologia ?? "Sin síntomas registrados";
		public string ConsultaDiagnostico => Cita?.ConsultaMedica?.Diagnostico ?? "Sin diagnóstico";
		public string ConsultaTratamiento => Cita?.ConsultaMedica?.Tratamiento ?? "Sin tratamiento prescrito";
		public string ConsultaObservaciones => Cita?.ConsultaMedica?.Observaciones ?? "Sin observaciones";
		public string ConsultaFechaSeguimiento => Cita?.ConsultaMedica?.FechaSeguimiento ?? "Sin seguimiento programado";

		// ===== CONSTRUCTOR =====
		public DetalleCitaMedicaViewModel(CitaConsultaMedica cita)
		{
			this.Cita = cita;

			// Debug completo
			System.Diagnostics.Debug.WriteLine($"🔧 DetalleCitaMedicaViewModel creado para cita ID: {cita.IdCita}");
			System.Diagnostics.Debug.WriteLine($"📊 Paciente: {PacienteNombreCompleto}");
			System.Diagnostics.Debug.WriteLine($"🩺 Doctor: {DoctorNombreCompleto}");
			System.Diagnostics.Debug.WriteLine($"🏥 Sucursal: {SucursalNombre}");
			System.Diagnostics.Debug.WriteLine($"🔬 Tiene triaje: {TieneTriaje}");
			System.Diagnostics.Debug.WriteLine($"📋 Tiene consulta: {TieneConsulta}");

			if (TieneTriaje)
			{
				System.Diagnostics.Debug.WriteLine($"🌡️ Temperatura: {TriajeTemperatura}");
				System.Diagnostics.Debug.WriteLine($"❤️ Presión: {TriajePresionArterial}");
				System.Diagnostics.Debug.WriteLine($"⚠️ Nivel urgencia: {TriajeNivelUrgencia}");
			}

			// Notificar cambios de todas las propiedades calculadas
			OnPropertyChanged(nameof(IdCita));
			OnPropertyChanged(nameof(EstadoCita));
			OnPropertyChanged(nameof(FechaHoraDisplay));
			OnPropertyChanged(nameof(PacienteNombreCompleto));
			OnPropertyChanged(nameof(DoctorNombreCompleto));
			OnPropertyChanged(nameof(TieneTriaje));
			OnPropertyChanged(nameof(TieneConsulta));
			OnPropertyChanged(nameof(MostrarTriaje));
			OnPropertyChanged(nameof(MostrarConsulta));
		}

		// ===== COMMANDS =====
		[RelayCommand]
		private async Task CerrarModal()
		{
			try
			{
				await Shell.Current.Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error cerrando modal: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task IniciarConsulta()
		{
			try
			{
				if (PuedeConsultar)
				{
					await Shell.Current.Navigation.PopModalAsync();
					await Shell.Current.GoToAsync($"consulta-medica?idCita={Cita.IdCita}");
				}
				else
				{
					await Shell.Current.DisplayAlert("Aviso", "Esta cita ya tiene una consulta médica registrada", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error iniciando consulta: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task VerConsulta()
		{
			try
			{
				if (TieneConsulta)
				{
					await Shell.Current.Navigation.PopModalAsync();
					await Shell.Current.GoToAsync($"consulta-medica?idCita={Cita.IdCita}&modo=ver");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error viendo consulta: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task RefrescarDatos()
		{
			try
			{
				// Aquí podrías agregar lógica para refrescar los datos desde la API
				System.Diagnostics.Debug.WriteLine("🔄 Refrescando datos de la cita...");

				// Por ahora, solo notificamos que los datos han cambiado
				OnPropertyChanged(string.Empty); // Notifica cambio de todas las propiedades
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error refrescando datos: {ex.Message}");
			}
		}

		// Método helper para debugging
		public void DebugTriaje()
		{
			if (Cita?.Triaje != null)
			{
				var triajeJson = JsonSerializer.Serialize(Cita.Triaje, new JsonSerializerOptions { WriteIndented = true });
				System.Diagnostics.Debug.WriteLine($"🔍 TRIAJE DEBUG:\n{triajeJson}");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("🔍 TRIAJE DEBUG: Triaje es null");
			}
		}
	}
}