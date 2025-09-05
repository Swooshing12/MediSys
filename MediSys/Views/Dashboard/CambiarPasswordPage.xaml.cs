using MediSys.ViewModels;

namespace MediSys.Views.Dashboard;

public partial class CambiarPasswordPage : ContentPage
{
	private CambiarPasswordViewModel _viewModel;

	public CambiarPasswordPage()
	{
		InitializeComponent();
		_viewModel = new CambiarPasswordViewModel();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Activar el ViewModel para cargar datos
		if (_viewModel is IActivatable activatable)
		{
			activatable.OnActivated();
		}
	}

	private void OnPasswordNuevaChanged(object sender, TextChangedEventArgs e)
	{
		_viewModel?.ValidarPasswordEnTiempoReal();
	}

	private void OnConfirmarPasswordChanged(object sender, TextChangedEventArgs e)
	{
		_viewModel?.ValidarCoincidenciaPassword();
	}
}

// Interface para activación del ViewModel
public interface IActivatable
{
	void OnActivated();
}