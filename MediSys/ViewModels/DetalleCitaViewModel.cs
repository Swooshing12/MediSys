using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;

namespace MediSys.ViewModels
{
	public partial class DetalleCitaViewModel : ObservableObject
	{
		[ObservableProperty]
		private CitaMedica cita;

		public DetalleCitaViewModel(CitaMedica cita)
		{
			this.cita = cita;
			System.Diagnostics.Debug.WriteLine($"DetalleCitaViewModel inicializado para cita ID: {cita?.IdCita}");
		}

		[RelayCommand]
		private async Task CerrarModal()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Cerrando modal de detalle de cita");
				await Shell.Current.Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error cerrando modal: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task ImprimirCita()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Iniciando impresión de cita");

				await Shell.Current.DisplayAlert(
					"🖨️ Generar PDF",
					$"Se generará un PDF completo del expediente médico de la cita del {Cita.FechaHora:dd/MM/yyyy}.\n\n" +
					"Incluirá:\n" +
					"• Información del paciente\n" +
					"• Datos del médico\n" +
					"• Resultados de triaje\n" +
					"• Consulta médica completa\n" +
					"• Diagnóstico y tratamiento\n\n" +
					"Funcionalidad en desarrollo.",
					"Entendido");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error en impresión: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "No se pudo generar el PDF", "OK");
			}
		}

		[RelayCommand]
		private async Task CompartirExpediente()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Iniciando compartir expediente");

				await Shell.Current.DisplayAlert(
					"📤 Compartir Expediente",
					$"Se preparará el expediente médico para compartir de forma segura.\n\n" +
					"Opciones disponibles:\n" +
					"• Email al paciente\n" +
					"• Envío a otro médico\n" +
					"• Exportar a sistema externo\n" +
					"• Generar enlace seguro\n\n" +
					"Funcionalidad en desarrollo.",
					"Entendido");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error compartiendo expediente: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", "No se pudo compartir el expediente", "OK");
			}
		}

		// Método para debugging
		public void LogCitaInfo()
		{
			System.Diagnostics.Debug.WriteLine("=== INFORMACIÓN DE LA CITA ===");
			System.Diagnostics.Debug.WriteLine($"ID: {Cita?.IdCita}");
			System.Diagnostics.Debug.WriteLine($"Estado: {Cita?.Estado}");
			System.Diagnostics.Debug.WriteLine($"Fecha: {Cita?.FechaHora}");
			System.Diagnostics.Debug.WriteLine($"Doctor: {Cita?.DoctorCompleto}");
			System.Diagnostics.Debug.WriteLine($"Tiene Triaje: {Cita?.TieneTriaje}");
			System.Diagnostics.Debug.WriteLine($"Tiene Consulta: {Cita?.TieneConsulta}");
			System.Diagnostics.Debug.WriteLine("==============================");
		}
	}
}