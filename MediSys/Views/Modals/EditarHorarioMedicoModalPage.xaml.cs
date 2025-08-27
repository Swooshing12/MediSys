using MediSys.Models;
using static MediSys.Models.CitaExtensions;

namespace MediSys.Views.Modals;

public partial class EditarHorarioMedicoModalPage : ContentPage
{
	public event EventHandler<HorarioCrear2>? HorarioGuardado;

	private List<Sucursal> _sucursales;
	private HorarioDoctor? _horarioExistente; // Para modo edición
	private bool _esEdicion = false;

	// Constructor para AGREGAR nuevo horario
	public EditarHorarioMedicoModalPage(List<Sucursal> sucursales)
	{
		InitializeComponent();
		_sucursales = sucursales;
		_esEdicion = false;
		InicializarComponentes();
		TituloModal.Text = "➕ Agregar Horario";
		BtnGuardar.Text = "Agregar";
	}

	// Constructor para EDITAR horario existente
	public EditarHorarioMedicoModalPage(List<Sucursal> sucursales, HorarioDoctor horarioExistente)
	{
		InitializeComponent();
		_sucursales = sucursales;
		_horarioExistente = horarioExistente;
		_esEdicion = true;
		InicializarComponentes();
		CargarDatosExistentes();
		TituloModal.Text = "✏️ Editar Horario";
		BtnGuardar.Text = "Actualizar";
	}

	private void InicializarComponentes()
	{
		// Configurar picker de sucursales
		SucursalPicker.ItemsSource = _sucursales;
		SucursalPicker.ItemDisplayBinding = new Binding("Nombre");

		// Configurar días de la semana
		var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
		DiaPicker.ItemsSource = dias;

		// Configurar duraciones
		var duraciones = new[] { "15 minutos", "20 minutos", "30 minutos", "45 minutos", "60 minutos" };
		DuracionPicker.ItemsSource = duraciones;
		DuracionPicker.SelectedIndex = 2; // 30 minutos por defecto

		// Configurar horas por defecto
		HoraInicioPicker.Time = new TimeSpan(8, 0, 0); // 8:00 AM
		HoraFinPicker.Time = new TimeSpan(17, 0, 0);   // 5:00 PM

		// Eventos para actualizar vista previa
		SucursalPicker.SelectedIndexChanged += OnCampoChanged;
		DiaPicker.SelectedIndexChanged += OnCampoChanged;
		HoraInicioPicker.PropertyChanged += OnCampoChanged;
		HoraFinPicker.PropertyChanged += OnCampoChanged;
		DuracionPicker.SelectedIndexChanged += OnCampoChanged;

		ActualizarVistaPrevia();
	}

	private void CargarDatosExistentes()
	{
		if (_horarioExistente == null) return;

		// Seleccionar sucursal por IdSucursal
		var sucursal = _sucursales.FirstOrDefault(s => s.IdSucursal == _horarioExistente.IdSucursal);
		if (sucursal != null)
		{
			SucursalPicker.SelectedItem = sucursal;
		}

		// Seleccionar día (DiaSemana es 1-7, picker es 0-6)
		DiaPicker.SelectedIndex = _horarioExistente.DiaSemana - 1;

		// Configurar horas
		if (TimeSpan.TryParse(_horarioExistente.HoraInicio, out TimeSpan horaInicio))
		{
			HoraInicioPicker.Time = horaInicio;
		}

		if (TimeSpan.TryParse(_horarioExistente.HoraFin, out TimeSpan horaFin))
		{
			HoraFinPicker.Time = horaFin;
		}

		// Seleccionar duración
		var duraciones = new[] { 15, 20, 30, 45, 60 };
		var duracionIndex = Array.IndexOf(duraciones, _horarioExistente.DuracionCita);
		if (duracionIndex >= 0)
		{
			DuracionPicker.SelectedIndex = duracionIndex;
		}

		ActualizarVistaPrevia();
	}

	private void OnCampoChanged(object? sender, EventArgs e)
	{
		ActualizarVistaPrevia();
	}

	private void ActualizarVistaPrevia()
	{
		try
		{
			if (SucursalPicker.SelectedItem is Sucursal sucursal &&
				DiaPicker.SelectedIndex >= 0 &&
				HoraInicioPicker.Time < HoraFinPicker.Time)
			{
				var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
				var dia = dias[DiaPicker.SelectedIndex];
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
			// Validaciones
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

			// Crear objeto horario
			var duraciones = new[] { 15, 20, 30, 45, 60 };
			var duracion = duraciones[DuracionPicker.SelectedIndex >= 0 ? DuracionPicker.SelectedIndex : 2];

			var horario = new HorarioCrear2
			{
				IdSucursal = sucursal.IdSucursal,
				NombreSucursal = sucursal.Nombre,
				DiaSemana = DiaPicker.SelectedIndex + 1,
				HoraInicio = HoraInicioPicker.Time.ToString(@"hh\:mm\:ss"),
				HoraFin = HoraFinPicker.Time.ToString(@"hh\:mm\:ss"),
				DuracionCita = duracion
			};

			// Si es edición, pasar también el ID del horario original
			if (_esEdicion && _horarioExistente != null)
			{
				// Agregar el ID del horario existente para la edición
				horario.IdHorarioExistente = _horarioExistente.IdHorario;
			}

			System.Diagnostics.Debug.WriteLine($"MODAL: Disparando evento - {(_esEdicion ? "Editar" : "Agregar")}");

			// Cerrar modal primero
			await Shell.Current.Navigation.PopModalAsync();

			// Disparar evento
			await Task.Delay(200);
			HorarioGuardado?.Invoke(this, horario);

		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
		}
	}
}