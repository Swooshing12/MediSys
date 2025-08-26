using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
	private DashboardViewModel _viewModel;

	public DashboardPage()
	{
		InitializeComponent();
		_viewModel = new DashboardViewModel();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		System.Diagnostics.Debug.WriteLine("Dashboard page appearing - refreshing data");

		// FORZAR RECARGA DE DATOS CADA VEZ QUE APARECE LA PÁGINA
		await _viewModel.RefreshDashboardAsync();
	}
}