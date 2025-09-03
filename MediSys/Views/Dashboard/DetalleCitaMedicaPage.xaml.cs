using MediSys.Models;
using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class DetalleCitaMedicaPage : ContentPage
{
	private DetalleCitaMedicaViewModel? _viewModel;

	

	// Constructor con cita
	 public DetalleCitaMedicaPage(CitaConsultaMedica cita)
    {
        InitializeComponent();

        BindingContext = new DetalleCitaMedicaViewModel(cita);

        System.Diagnostics.Debug.WriteLine($"👁️ DetalleCitaMedicaPage initialized for cita: {cita.IdCita}");
    }

	// Método para establecer la cita después de la creación
	public void SetCita(CitaConsultaMedica cita)
	{
		_viewModel = new DetalleCitaMedicaViewModel(cita);
		BindingContext = _viewModel;
		System.Diagnostics.Debug.WriteLine($"📌 Cita establecida: {cita.IdCita}");
		System.Diagnostics.Debug.WriteLine($"📌 Triaje recibido: {System.Text.Json.JsonSerializer.Serialize(cita.Triaje)}");
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
}
