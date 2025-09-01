using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class ConsultarMedicoPage : ContentPage
{
	private ConsultarMedicoViewModel _viewModel;

	public ConsultarMedicoPage()
	{
		InitializeComponent();
		_viewModel = new ConsultarMedicoViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// ✅ SOLO limpiar búsqueda si NO hay médico encontrado
		if (_viewModel.MedicoEncontrado == null)
		{
			_viewModel.LimpiarBusquedaCommand.Execute(null);
		}
	}
}