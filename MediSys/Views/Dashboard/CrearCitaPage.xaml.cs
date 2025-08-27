using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class CrearCitaPage : ContentPage
{
	public CrearCitaPage()
	{
		InitializeComponent();
		BindingContext = new CrearCitaViewModel();
	}
}