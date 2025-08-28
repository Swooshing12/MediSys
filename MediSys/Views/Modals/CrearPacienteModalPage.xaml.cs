using MediSys.ViewModels;
using MediSys.Models;

namespace MediSys.Views.Modals;

public partial class CrearPacienteModalPage : ContentPage
{
	public event EventHandler<PacienteBusqueda>? PacienteCreado;

	private CrearPacienteModalViewModel _viewModel;

	public CrearPacienteModalPage(string cedula)
	{
		InitializeComponent();
		_viewModel = new CrearPacienteModalViewModel(cedula);
		_viewModel.PacienteCreado += OnPacienteCreado;
		_viewModel.CerrarModal += OnCerrarModal;
		BindingContext = _viewModel;
	}

	private void OnPacienteCreado(object? sender, PacienteBusqueda paciente)
	{
		PacienteCreado?.Invoke(this, paciente);
	}

	private async void OnCerrarModal(object? sender, EventArgs e)
	{
		await Shell.Current.Navigation.PopModalAsync();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PacienteCreado -= OnPacienteCreado;
		_viewModel.CerrarModal -= OnCerrarModal;
	}
}