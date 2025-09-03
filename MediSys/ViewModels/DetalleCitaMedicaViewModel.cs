// ViewModels/DetalleCitaMedicaViewModel.cs - ACTUALIZADO
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;

namespace MediSys.ViewModels
{
	public partial class DetalleCitaMedicaViewModel : ObservableObject
	{
		[ObservableProperty]
		private CitaConsultaMedica cita;

		public DetalleCitaMedicaViewModel(CitaConsultaMedica cita)
		{
			this.Cita = cita;
			System.Diagnostics.Debug.WriteLine($"📋 DetalleCitaMedicaViewModel created for: {cita.Paciente.NombreCompleto}");
		}

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
				if (Cita?.PuedeConsultar == true)
				{
					// Cerrar modal y navegar a consulta
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
				if (Cita?.TieneConsulta == true)
				{
					// Cerrar modal y navegar a ver consulta
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
	}
}