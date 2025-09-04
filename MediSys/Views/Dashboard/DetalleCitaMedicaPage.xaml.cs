// Views/Dashboard/DetalleCitaMedicaPage.xaml.cs - VERSIÓN CORREGIDA
using MediSys.Models;
using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class DetalleCitaMedicaPage : ContentPage
{
	private DetalleCitaMedicaViewModel _viewModel;

	public DetalleCitaMedicaPage(CitaConsultaMedica cita)
	{
		InitializeComponent();

		// Crear y asignar el ViewModel
		_viewModel = new DetalleCitaMedicaViewModel(cita);
		BindingContext = _viewModel;

		System.Diagnostics.Debug.WriteLine($"✅ DetalleCitaMedicaPage inicializada para cita: {cita.IdCita}");
		System.Diagnostics.Debug.WriteLine($"📊 Paciente: {cita.Paciente?.NombreCompleto ?? "Sin nombre"}");
		System.Diagnostics.Debug.WriteLine($"🔬 Tiene triaje: {cita.TieneTriaje}");
		System.Diagnostics.Debug.WriteLine($"📋 Tiene consulta: {cita.TieneConsulta}");

		// Debug del triaje si existe
		if (cita.TieneTriaje && cita.Triaje != null)
		{
			System.Diagnostics.Debug.WriteLine($"🌡️ Temperatura: {cita.Triaje.SignosVitales?.Temperatura}");
			System.Diagnostics.Debug.WriteLine($"❤️ Presión: {cita.Triaje.SignosVitales?.PresionArterial}");
			System.Diagnostics.Debug.WriteLine($"⚠️ Nivel urgencia: {cita.Triaje.Evaluacion?.NivelUrgencia}");
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		System.Diagnostics.Debug.WriteLine("📱 DetalleCitaMedicaPage OnAppearing");

		// Opcional: refrescar datos o hacer algún debug adicional
		_viewModel?.DebugTriaje();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		System.Diagnostics.Debug.WriteLine("📱 DetalleCitaMedicaPage OnDisappearing");
	}

	// Método helper para depuración (puedes eliminarlo después)
	private void OnDebugButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel != null && _viewModel.Cita != null)
		{
			System.Diagnostics.Debug.WriteLine("=== DEBUG DETALLE CITA ===");
			System.Diagnostics.Debug.WriteLine($"ID Cita: {_viewModel.Cita.IdCita}");
			System.Diagnostics.Debug.WriteLine($"Estado: {_viewModel.Cita.Estado}");
			System.Diagnostics.Debug.WriteLine($"Paciente: {_viewModel.Cita.Paciente?.NombreCompleto}");
			System.Diagnostics.Debug.WriteLine($"Doctor: {_viewModel.Cita.Doctor?.NombreCompleto}");
			System.Diagnostics.Debug.WriteLine($"Tiene Triaje: {_viewModel.Cita.TieneTriaje}");
			System.Diagnostics.Debug.WriteLine($"Triaje NULL: {_viewModel.Cita.Triaje == null}");

			if (_viewModel.Cita.Triaje != null)
			{
				System.Diagnostics.Debug.WriteLine($"Signos Vitales NULL: {_viewModel.Cita.Triaje.SignosVitales == null}");
				System.Diagnostics.Debug.WriteLine($"Evaluacion NULL: {_viewModel.Cita.Triaje.Evaluacion == null}");
			}

			System.Diagnostics.Debug.WriteLine("========================");
		}
	}
}