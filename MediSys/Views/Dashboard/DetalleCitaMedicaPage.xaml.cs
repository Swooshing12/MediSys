using MediSys.Models;
using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class DetalleCitaMedicaPage : ContentPage
{
	private readonly DetalleCitaMedicaViewModel _viewModel;

	// ✅ CONSTRUCTOR QUE RECIBE LA CITA COMO PARÁMETRO
	public DetalleCitaMedicaPage(CitaConsultaMedica cita)
	{

				InitializeComponent();

		// Crear el ViewModel manualmente ya que necesitamos pasar la cita
		_viewModel = new DetalleCitaMedicaViewModel(cita);
		BindingContext = _viewModel;

		System.Diagnostics.Debug.WriteLine($"👁️ DetalleCitaMedicaPage initialized for cita: {cita.IdCita}");
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		System.Diagnostics.Debug.WriteLine("📱 DetalleCitaMedicaPage appearing");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		System.Diagnostics.Debug.WriteLine("📱 DetalleCitaMedicaPage disappearing");
	}

	// ✅ MANEJADOR PARA EL BOTÓN DE CERRAR (si prefieres manejarlo aquí en lugar del ViewModel)
	private async void OnCerrarClicked(object sender, EventArgs e)
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

	// ✅ MANEJADOR PARA INICIAR CONSULTA DESDE EL DETALLE
	private async void OnIniciarConsultaClicked(object sender, EventArgs e)
	{
		try
		{
			if (_viewModel.Cita?.PuedeConsultar == true)
			{
				// Cerrar este modal primero
				await Shell.Current.Navigation.PopModalAsync();

				// Luego navegar a la consulta
				await Shell.Current.GoToAsync($"consulta-medica?idCita={_viewModel.Cita.IdCita}");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"❌ Error iniciando consulta: {ex.Message}");
			await DisplayAlert("Error", $"No se pudo iniciar la consulta: {ex.Message}", "OK");
		}
	}
}