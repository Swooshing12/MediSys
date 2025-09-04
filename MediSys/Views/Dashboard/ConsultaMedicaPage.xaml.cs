// Views/Dashboard/ConsultaMedicaPage.xaml.cs - VERSIÓN FINAL
using MediSys.Models;
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
				System.Diagnostics.Debug.WriteLine($"🆔 ID Cita recibido: {_idCita}");
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

		System.Diagnostics.Debug.WriteLine("✅ ConsultaMedicaPage inicializada");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		System.Diagnostics.Debug.WriteLine($"📱 OnAppearing - Modo: {_modo}, ID Cita: {_idCita}");
		await CargarInformacionPaciente();
	}

	private async Task CargarInformacionPaciente()
	{
		try
		{
			// Limpiar contenedor
			PacienteInfoContainer.Children.Clear();

			// Mostrar loading
			PacienteInfoContainer.Children.Add(new ActivityIndicator
			{
				IsRunning = true,
				Color = Color.FromHex("#3B82F6")
			});

			if (_idCita <= 0)
			{
				MostrarError("No se recibió un ID de cita válido.");
				return;
			}

			var response = await _apiService.ObtenerInformacionCitaAsync(_idCita);

			// Limpiar loading
			PacienteInfoContainer.Children.Clear();

			if (response?.Success != true || response.Data?.Paciente == null)
			{
				MostrarError(response?.Message ?? "Error al cargar la información");
				return;
			}

			var paciente = response.Data.Paciente;
			var cita = response.Data.Cita;

			// 1. INFORMACIÓN PERSONAL
			var seccionPersonal = CrearSeccionInfo("Datos Personales", "👤", Color.FromHex("#F0F9FF"));
			var personalContainer = (seccionPersonal.Content as StackLayout);

			personalContainer.Children.Add(CrearFilaDato("Nombre:", paciente.NombreCompleto));
			personalContainer.Children.Add(CrearFilaDato("Cédula:", paciente.Cedula?.ToString(), "🪪"));
			personalContainer.Children.Add(CrearFilaDato("Teléfono:", paciente.Telefono ?? "No disponible", "📞"));
			personalContainer.Children.Add(CrearFilaDato("Email:", paciente.Correo ?? "No disponible", "📧"));
			personalContainer.Children.Add(CrearFilaDato("Edad:", $"{paciente.Edad?.ToString() ?? "No disponible"} años", "🎂"));
			personalContainer.Children.Add(CrearFilaDato("Sexo:", paciente.Sexo ?? "No especificado", "⚥"));
			personalContainer.Children.Add(CrearFilaDato("Tipo Sangre:", paciente.TipoSangre ?? "No disponible", "🩸"));

			PacienteInfoContainer.Children.Add(seccionPersonal);

			// 2. ALERGIAS (si existen)
			if (!string.IsNullOrWhiteSpace(paciente.Alergias))
			{
				var seccionAlergias = CrearSeccionInfo("Alergias", "⚠️", Color.FromHex("#FEF3C7"));
				var alergiasContainer = (seccionAlergias.Content as StackLayout);
				alergiasContainer.Children.Add(new Label
				{
					Text = paciente.Alergias,
					FontSize = 12,
					TextColor = Color.FromHex("#D97706"),
					FontAttributes = FontAttributes.Bold
				});
				PacienteInfoContainer.Children.Add(seccionAlergias);
			}

			// 3. INFORMACIÓN DE LA CITA
			var seccionCita = CrearSeccionInfo("Información de Cita", "📅", Color.FromHex("#F0FDF4"));
			var citaContainer = (seccionCita.Content as StackLayout);
			citaContainer.Children.Add(CrearFilaDato("Estado:", cita?.Estado ?? "No disponible", "📋"));
			citaContainer.Children.Add(CrearFilaDato("Modalidad:", cita?.Tipo ?? "No especificada", "🏥"));
			PacienteInfoContainer.Children.Add(seccionCita);

			// 4. ESTADO DE CONSULTA
			var consultaMedica = response.Data.ConsultaMedica ?? response.Data.Consulta;
			var seccionConsulta = consultaMedica?.Existe == true
				? CrearSeccionInfo("Consulta Registrada", "✅", Color.FromHex("#ECFDF5"))
				: CrearSeccionInfo("Nueva Consulta", "📝", Color.FromHex("#FFFBEB"));

			PacienteInfoContainer.Children.Add(seccionConsulta);

			if (consultaMedica?.Existe == true && _modo == "editar")
			{
				await CargarDatosConsultaExistente(consultaMedica);
			}

			// 5. TRIAJE
			if (response.Data.Triaje?.Completado == true)
			{
				var seccionTriaje = CrearSeccionInfo("Triaje Completado", "🏥", Color.FromHex("#F0FDF4"));
				var triajeContainer = (seccionTriaje.Content as StackLayout);

				var triaje = response.Data.Triaje;
				if (triaje.SignosVitales != null)
				{
					var sv = triaje.SignosVitales;
					triajeContainer.Children.Add(CrearFilaDato("Temperatura:", sv.TemperaturaDisplay ?? "N/A", "🌡️"));
					triajeContainer.Children.Add(CrearFilaDato("Presión:", sv.PresionArterial ?? "N/A", "💓"));
					triajeContainer.Children.Add(CrearFilaDato("FC:", sv.FrecuenciaCardiacaDisplay ?? "N/A", "❤️"));
					triajeContainer.Children.Add(CrearFilaDato("SatO₂:", sv.SaturacionOxigenoDisplay ?? "N/A", "🫁"));
				}
				PacienteInfoContainer.Children.Add(seccionTriaje);
			}
			else
			{
				var seccionTriajePendiente = CrearSeccionInfo("Triaje Pendiente", "⏳", Color.FromHex("#FEF3C7"));
				PacienteInfoContainer.Children.Add(seccionTriajePendiente);
			}

		}
		catch (Exception ex)
		{
			PacienteInfoContainer.Children.Clear();
			MostrarError($"Error inesperado: {ex.Message}");
		}
	}

	private Frame CrearSeccionInfo(string titulo, string icono, Color backgroundColor)
	{
		return new Frame
		{
			BackgroundColor = backgroundColor,
			CornerRadius = 8,
			Padding = new Thickness(12),
			HasShadow = false,
			Margin = new Thickness(0, 4),
			Content = new StackLayout
			{
				Spacing = 8,
				Children = {
				new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Spacing = 6,
					Children = {
						new Label { Text = icono, FontSize = 16 },
						new Label
						{
							Text = titulo,
							FontSize = 13,
							FontAttributes = FontAttributes.Bold,
							TextColor = Color.FromHex("#374151")
						}
					}
				}
			}
			}
		};
	}

	private StackLayout CrearFilaDato(string etiqueta, string valor, string icono = null)
	{
		return new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			Spacing = 6,
			Margin = new Thickness(0, 2),
			Children = {
			new Label
			{
				Text = icono ?? "•",
				FontSize = 12,
				TextColor = Color.FromHex("#6B7280"),
				WidthRequest = 20,
				VerticalOptions = LayoutOptions.Start
			},
			new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 4,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label
					{
						Text = etiqueta,
						FontSize = 12,
						FontAttributes = FontAttributes.Bold,
						TextColor = Color.FromHex("#6B7280"),
						WidthRequest = 70
					},
					new Label
					{
						Text = valor,
						FontSize = 12,
						TextColor = Color.FromHex("#111827"),
						LineBreakMode = LineBreakMode.WordWrap,
						HorizontalOptions = LayoutOptions.FillAndExpand
					}
				}
			}
		}
		};
	}

	private void MostrarError(string mensaje)
	{
		PacienteInfoContainer.Children.Clear();
		PacienteInfoContainer.Children.Add(new Frame
		{
			BackgroundColor = Color.FromHex("#FEF2F2"),
			CornerRadius = 8,
			Padding = new Thickness(12),
			HasShadow = false,
			Content = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 8,
				Children = {
				new Label { Text = "❌", FontSize = 16 },
				new Label
				{
					Text = mensaje,
					FontSize = 12,
					TextColor = Color.FromHex("#DC2626"),
					HorizontalOptions = LayoutOptions.FillAndExpand
				}
			}
			}
		});
	}

	// ✅ NUEVO MÉTODO para cargar datos de consulta existente (modo editar)
	private async Task CargarDatosConsultaExistente(ConsultaMedicaInfo3 consultaMedica)
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("🔄 Cargando datos de consulta médica existente...");
			// Cargar datos en los campos del formulario
			if (!string.IsNullOrEmpty(consultaMedica.MotivoConsulta))
			{
				MotivoConsultaEditor.Text = consultaMedica.MotivoConsulta;
			}

			if (!string.IsNullOrEmpty(consultaMedica.Sintomatologia))
			{
				SintomasEditor.Text = consultaMedica.Sintomatologia;
			}

			if (!string.IsNullOrEmpty(consultaMedica.Diagnostico))
			{
				DiagnosticoEditor.Text = consultaMedica.Diagnostico;
			}

			if (!string.IsNullOrEmpty(consultaMedica.Tratamiento))
			{
				TratamientoEditor.Text = consultaMedica.Tratamiento;
				TratamientoCheckBox.IsChecked = true;
				TratamientoSection.IsVisible = true;
			}

			if (!string.IsNullOrEmpty(consultaMedica.Observaciones))
			{
				ObservacionesEditor.Text = consultaMedica.Observaciones;
			}

			if (!string.IsNullOrEmpty(consultaMedica.Observaciones))
			{
				RecetaEditor.Text = consultaMedica.Observaciones;
			}

			if (!string.IsNullOrEmpty(consultaMedica.FechaSeguimiento) &&
				DateTime.TryParse(consultaMedica.FechaSeguimiento, out DateTime fechaSeguimiento))
			{
				FechaSeguimientoPicker.Date = fechaSeguimiento;
				SeguimientoCheckBox.IsChecked = true;
				SeguimientoSection.IsVisible = true;
			}

			System.Diagnostics.Debug.WriteLine("✅ Datos de consulta existente cargados");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"❌ Error cargando datos de consulta existente: {ex.Message}");
		}
	}

	// ✅ EVENTOS DE LA INTERFAZ
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
			ObservacionesEditor.Text = "";
		}
	}

	private void OnMedicamentoSelected(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			var medicamento = button.Text;
			var textoActual = RecetaEditor.Text ?? "";

			if (!textoActual.Contains(medicamento))
			{
				var nuevoTexto = string.IsNullOrEmpty(textoActual)
					? $"• {medicamento}"
					: textoActual + $"\n• {medicamento}";

				RecetaEditor.Text = nuevoTexto;
			}
		}
	}

	private async void OnGuardarClicked(object sender, EventArgs e)
	{
		try
		{
			if (!ValidarCampos())
				return;

			// ✅ AJUSTAR NOMBRES DE CONTROLES PARA QUE COINCIDAN CON TU XAML
			var consultaData = new Models.ConsultaMedicaRequest
			{
				MotivoConsulta = MotivoConsultaEditor.Text?.Trim() ?? "", // ✅ Cambiado de MotivoEntry
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

	// ✅ AGREGAR EL MÉTODO ValidarCampos QUE FALTA
	private bool ValidarCampos()
	{
		if (string.IsNullOrWhiteSpace(MotivoConsultaEditor.Text))
		{
			DisplayAlert("Error", "El motivo de consulta es obligatorio", "OK");
			return false;
		}

		if (string.IsNullOrWhiteSpace(DiagnosticoEditor.Text))
		{
			DisplayAlert("Error", "El diagnóstico es obligatorio", "OK");
			return false;
		}

		return true;
	}

	private async void OnCancelarClicked(object sender, EventArgs e)
	{
		try
		{
			var confirmar = await DisplayAlert("Confirmar",
				"¿Estás seguro de que deseas cancelar? Se perderán los cambios no guardados.",
				"Sí", "No");

			if (confirmar)
			{
				await Shell.Current.GoToAsync("..");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"❌ Error en OnCancelarClicked: {ex}");
		}
	}
}

