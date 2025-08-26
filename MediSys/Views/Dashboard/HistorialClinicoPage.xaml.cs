using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class HistorialClinicoPage : ContentPage
{
	private HistorialClinicoViewModel _viewModel;

	public HistorialClinicoPage()
	{
		InitializeComponent();
		_viewModel = new HistorialClinicoViewModel();
		BindingContext = _viewModel;
	}

	private async void OnEspecialidadChanged(object sender, EventArgs e)
	{
		if (_viewModel.EspecialidadSeleccionada != null)
		{
			await _viewModel.CargarDoctoresPorEspecialidadCommand.ExecuteAsync(null);
		}
	}
}