using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class RegistrarMedicoPage : ContentPage
{
	private RegistrarMedicoViewModel _viewModel;

	public RegistrarMedicoPage()
	{
		InitializeComponent();
		_viewModel = new RegistrarMedicoViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}
}