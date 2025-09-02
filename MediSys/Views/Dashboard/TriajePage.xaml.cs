using MediSys.ViewModels;
using MediSys.Services;
using MediSys.Models;

namespace MediSys.Views.Dashboard;

// TriajePage.xaml.cs - Corregir el constructor
public partial class TriajePage : ContentPage
{
	public TriajePage()
	{
		InitializeComponent();
		var apiService = new MediSysApiService();
		var authService = new AuthService();
		BindingContext = new TriajeViewModel(apiService, authService);
	}

	private void OnCitaSeleccionada(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is CitaDetallada citaSeleccionada)
		{
			if (BindingContext is TriajeViewModel viewModel)
			{
				viewModel.SeleccionarCitaCommand.Execute(citaSeleccionada);
			}
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is TriajeViewModel viewModel)
		{
			viewModel.LimpiarFormularioCommand.Execute(null);
		}
	}
}