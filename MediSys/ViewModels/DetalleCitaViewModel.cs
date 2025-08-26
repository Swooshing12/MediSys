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
		}

		[RelayCommand]
		private async Task CerrarModal()
		{
			await Shell.Current.Navigation.PopModalAsync();
		}

		[RelayCommand]
		private async Task ImprimirCita()
		{
			await Shell.Current.DisplayAlert("Imprimir",
				"Funcionalidad de impresión en desarrollo", "OK");
		}
	}
}
