using MediSys.Models;
using MediSys.ViewModels;

namespace MediSys.Views.Modals;

public partial class AgregarHorarioModalPage : ContentPage
{
	public event EventHandler<HorarioCrear>? HorarioGuardado;

	private List<Sucursal> _sucursalesDisponibles;

	public AgregarHorarioModalPage(List<Sucursal> sucursalesDisponibles)
	{
		InitializeComponent();

		_sucursalesDisponibles = sucursalesDisponibles;

		// Configurar picker de sucursales
		SucursalPicker.ItemsSource = sucursalesDisponibles;
		SucursalPicker.ItemDisplayBinding = new Binding("Nombre");

		// Configurar eventos para vista previa
		SucursalPicker.SelectedIndexChanged += OnSelectionChanged;
		DiaPicker.SelectedIndexChanged += OnSelectionChanged;
		HoraInicioPicker.PropertyChanged += OnTimeChanged;
		HoraFinPicker.PropertyChanged += OnTimeChanged;
		DuracionPicker.SelectedIndexChanged += OnSelectionChanged;
	}

	private void OnSelectionChanged(object? sender, EventArgs e)
	{
		ActualizarVistaPrevia();
	}

	private void OnTimeChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(TimePicker.Time))
		{
			ActualizarVistaPrevia();
		}
	}

	private void ActualizarVistaPrevia()
	{
		try
		{
			if (SucursalPicker.SelectedItem is Sucursal sucursal &&
				DiaPicker.SelectedIndex >= 0 &&
				HoraInicioPicker.Time < HoraFinPicker.Time)
			{
				var dia = DiaPicker.Items[DiaPicker.SelectedIndex];
				var horaInicio = HoraInicioPicker.Time.ToString(@"hh\:mm");
				var horaFin = HoraFinPicker.Time.ToString(@"hh\:mm");
				var duraciones = new[] { 15, 20, 30, 45, 60 };
				var duracion = duraciones[DuracionPicker.SelectedIndex >= 0 ? DuracionPicker.SelectedIndex : 2];

				VistaPrevia.Text = $"🏢 {sucursal.Nombre}\n📅 {dia} de {horaInicio} a {horaFin}\n⏱️ {duracion} minutos por cita";

				// Calcular citas estimadas
				var minutosTotales = (HoraFinPicker.Time - HoraInicioPicker.Time).TotalMinutes;
				var citasEstimadas = (int)(minutosTotales / duracion);

				CitasEstimadas.Text = $"Citas estimadas: ~{citasEstimadas} por día";
				CitasEstimadas.TextColor = citasEstimadas > 0 ? Colors.Green : Colors.Red;
			}
			else
			{
				VistaPrevia.Text = "⚠️ Complete todos los campos o verifique que la hora de inicio sea menor que la hora de fin";
				CitasEstimadas.Text = "Citas estimadas: --";
				CitasEstimadas.TextColor = Colors.Gray;
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error actualizando vista previa: {ex.Message}");
		}
	}

	private async void OnCancelarClicked(object sender, EventArgs e)
	{
		await Shell.Current.Navigation.PopModalAsync();
	}

	private async void OnGuardarClicked(object sender, EventArgs e)
	{
		try
		{
			// Validar campos
			if (SucursalPicker.SelectedItem is not Sucursal sucursal)
			{
				await DisplayAlert("Error", "Seleccione una sucursal", "OK");
				return;
			}

			if (DiaPicker.SelectedIndex < 0)
			{
				await DisplayAlert("Error", "Seleccione un día de la semana", "OK");
				return;
			}

			if (HoraInicioPicker.Time >= HoraFinPicker.Time)
			{
				await DisplayAlert("Error", "La hora de inicio debe ser menor que la hora de fin", "OK");
				return;
			}

			// Crear horario
			var duraciones = new[] { 15, 20, 30, 45, 60 };
			var duracion = duraciones[DuracionPicker.SelectedIndex >= 0 ? DuracionPicker.SelectedIndex : 2];

			var horario = new HorarioCrear
			{
				IdSucursal = sucursal.IdSucursal,
				NombreSucursal = sucursal.Nombre,
				DiaSemana = DiaPicker.SelectedIndex + 1,
				HoraInicio = HoraInicioPicker.Time.ToString(@"hh\:mm\:ss"),
				HoraFin = HoraFinPicker.Time.ToString(@"hh\:mm\:ss"),
				DuracionCita = duracion
			};

			System.Diagnostics.Debug.WriteLine($"🔥 MODAL: Disparando evento HorarioGuardado con: {horario.HorarioDisplay}");

			// 🔥 DISPARAR EVENTO ANTES DE CERRAR MODAL
			HorarioGuardado?.Invoke(this, horario);

			// 🔥 PEQUEÑA PAUSA PARA ASEGURAR QUE EL EVENTO SE PROCESE
			await Task.Delay(100);

			await Shell.Current.Navigation.PopModalAsync();

			await DisplayAlert("✅ Horario Agregado",
				$"Horario agregado: {horario.DiaSemanaTexto} de {horario.HoraInicio} a {horario.HoraFin}",
				"OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error guardando horario: {ex.Message}", "OK");
		}
	}
}