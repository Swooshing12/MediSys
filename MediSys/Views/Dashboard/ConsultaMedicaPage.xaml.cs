// Views/Dashboard/ConsultaMedicaPage.xaml.cs - ACTUALIZADO
using MediSys.Services;

namespace MediSys.Views.Dashboard;

[QueryProperty(nameof(IdCita), "idCita")]
[QueryProperty(nameof(Modo), "modo")]
public partial class ConsultaMedicaPage : ContentPage
{
	private readonly MediSysApiService _apiService;
	private int _idCita;
	private string _modo = "crear";

	public string IdCita
	{
		set
		{
			if (int.TryParse(value, out int id))
			{
				_idCita = id;
				System.Diagnostics.Debug.WriteLine($"ID Cita: {_idCita}");
			}
		}
	}

	public string Modo { set => _modo = value ?? "crear"; }

	public ConsultaMedicaPage()
	{
		InitializeComponent();
		_apiService = new MediSysApiService();

		// Configurar fecha por defecto
		FechaSeguimientoPicker.Date = DateTime.Today.AddDays(7);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await CargarInformacionPaciente();
	}

	private async Task CargarInformacionPaciente()
	{
		try
		{
			if (_idCita <= 0)
			{
				PacienteInfoLabel.Text = "⚠️ No se recibió un ID de cita válido.";
				return;
			}

			var response = await _apiService.ObtenerDetalleConsultaAsync(_idCita);

			if (response == null)
			{
				PacienteInfoLabel.Text = "❌ No se pudo conectar al servidor.";
				return;
			}

			if (!response.Success)
			{
				PacienteInfoLabel.Text = $"❌ Error del servidor: {response.Message ?? "Respuesta inválida"}";
				return;
			}

			if (response.Data == null || response.Data.Paciente == null)
			{
				PacienteInfoLabel.Text = "⚠️ No se encontró información del paciente en esta cita.";
				return;
			}

			var paciente = response.Data.Paciente;

			var info = $"👤 {paciente.NombreCompleto}\n" +
					   $"📄 Cédula: {paciente.Cedula}\n" +
					   $"📞 Teléfono: {paciente.Telefono ?? "No disponible"}\n" +
					   $"📧 Email: {paciente.Correo ?? "No disponible"}\n" +
					   $"🎂 Edad: {paciente.Edad?.ToString() ?? "No disponible"} años\n" +
					   $"🩸 Tipo Sangre: {paciente.TipoSangre ?? "No disponible"}";

			if (!string.IsNullOrWhiteSpace(paciente.Alergias))
			{
				info += $"\n⚠️ Alergias: {paciente.Alergias}";
			}

			PacienteInfoLabel.Text = info;
		}
		catch (Exception ex)
		{
			PacienteInfoLabel.Text = $"⚠️ Error inesperado: {ex.Message}";
		}
	}


	private void OnTratamientoCheckChanged(object sender, CheckedChangedEventArgs e)
	{
		TratamientoSection.IsVisible = e.Value;

		if (!e.Value)
		{
			// Limpiar campos cuando se desactiva
			TratamientoEditor.Text = "";
			RecetaEditor.Text = "";
		}
	}

	private void OnSeguimientoCheckChanged(object sender, CheckedChangedEventArgs e)
	{
		SeguimientoSection.IsVisible = e.Value;

		if (!e.Value)
		{
			RecomendacionesEditor.Text = "";
		}
	}

	private void OnMedicamentoSelected(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			var medicamento = button.Text;
			var textoActual = RecetaEditor.Text ?? "";

			if (!string.IsNullOrEmpty(textoActual))
			{
				RecetaEditor.Text = textoActual + "\n" + medicamento + " - ";
			}
			else
			{
				RecetaEditor.Text = medicamento + " - ";
			}
		}
	}

	private async void OnGuardarClicked(object sender, EventArgs e)
	{
		try
		{
			if (!ValidarCampos())
				return;

			var consultaData = new Models.ConsultaMedicaRequest
			{
				MotivoConsulta = MotivoEntry.Text?.Trim() ?? "",
				Sintomatologia = SintomasEditor.Text?.Trim(),
				Diagnostico = DiagnosticoEditor.Text?.Trim() ?? "",
				Tratamiento = TratamientoCheckBox.IsChecked ? TratamientoEditor.Text?.Trim() : null,
				Observaciones = TratamientoCheckBox.IsChecked ? RecetaEditor.Text?.Trim() : null,
				FechaSeguimiento = SeguimientoCheckBox.IsChecked ?
					FechaSeguimientoPicker.Date.ToString("yyyy-MM-dd") : null
			};

			var response = await _apiService.CrearActualizarConsultaMedicaAsync(_idCita, consultaData);

			if (response?.Success == true)
			{
				await DisplayAlert("Éxito",
					"Consulta médica guardada correctamente.\n\n" +
					"📧 Se ha enviado un correo al paciente con el reporte médico en PDF.",
					"OK");
				await Shell.Current.GoToAsync("..");
			}
			else
			{
				await DisplayAlert("Error", response?.Message ?? "Error guardando la consulta", "OK");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error guardando: {ex.Message}");
			await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
		}
	}

	private bool ValidarCampos()
	{
		if (string.IsNullOrWhiteSpace(MotivoEntry.Text))
		{
			DisplayAlert("Validación", "El motivo de consulta es obligatorio", "OK");
			return false;
		}

		if (string.IsNullOrWhiteSpace(DiagnosticoEditor.Text))
		{
			DisplayAlert("Validación", "El diagnóstico es obligatorio", "OK");
			return false;
		}

		return true;
	}

	private async void OnCancelarClicked(object sender, EventArgs e)
	{
		var confirmar = await DisplayAlert("Confirmar",
			"¿Desea salir sin guardar los cambios?",
			"Sí", "No");

		if (confirmar)
		{
			await Shell.Current.GoToAsync("..");
		}
	}
}