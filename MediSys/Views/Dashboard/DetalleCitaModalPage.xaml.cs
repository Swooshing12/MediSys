using MediSys.ViewModels;
using MediSys.Models;

namespace MediSys.Views.Dashboard;

public partial class DetalleCitaModalPage : ContentPage
{
	public DetalleCitaModalPage(CitaMedica cita)
	{
		InitializeComponent();
		BindingContext = new DetalleCitaViewModel(cita);
	}
}