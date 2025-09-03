// Views/Dashboard/DetalleCitaMedicaPage.xaml.cs - CORREGIDO
using MediSys.Models;
using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class DetalleCitaMedicaPage : ContentPage
{
	private DetalleCitaMedicaViewModel? _viewModel;

	// ✅ CONSTRUCTOR SIN PARÁMETROS (requerido por MAUI)
	public DetalleCitaMedicaPage()
	{
		InitializeComponent();
		System.Diagnostics.Debug.WriteLine("👁️ DetalleCitaMedicaPage initialized without parameters");
	}

	// ✅ CONSTRUCTOR CON CITA (para uso manual)
	public DetalleCitaMedicaPage(CitaConsultaMedica cita) : this()
	{
		_viewModel = new DetalleCitaMedicaViewModel(cita);
		BindingContext = _viewModel;

		System.Diagnostics.Debug.WriteLine($"👁️ DetalleCitaMedicaPage initialized for cita: {cita.IdCita}");
	}

	// ✅ MÉTODO PARA ESTABLECER LA CITA DESPUÉS DE LA CREACIÓN
	public void SetCita(CitaConsultaMedica cita)
	{
		_viewModel = new DetalleCitaMedicaViewModel(cita);
		BindingContext = _viewModel;
		System.Diagnostics.Debug.WriteLine($"👁️ Cita establecida: {cita.IdCita}");
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