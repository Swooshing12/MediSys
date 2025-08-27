using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;
using System.Collections.ObjectModel;

namespace MediSys.ViewModels
{
	public partial class ConsultarMedicoViewModel : ObservableObject
	{
		private static MediSysApiService? _sharedApiService;

		private MediSysApiService ApiService
		{
			get
			{
				if (_sharedApiService == null)
					_sharedApiService = new MediSysApiService();
				return _sharedApiService;
			}
		}

		// ===== BÚSQUEDA =====
		[ObservableProperty]
		private string cedulaBusqueda = "";

		[ObservableProperty]
		private bool isLoading = false;

		[ObservableProperty]
		private bool showResults = false;

		// ===== DATOS DEL MÉDICO =====
		[ObservableProperty]
		private MedicoCompleto? medicoEncontrado;

		// ===== HORARIOS =====
		[ObservableProperty]
		private ObservableCollection<HorarioDoctor> horarios = new();

		[ObservableProperty]
		private ObservableCollection<HorariosPorSucursal> horariosPorSucursal = new();

		[ObservableProperty]
		private EstadisticasGenerales? estadisticas;

		[ObservableProperty]
		private bool showHorarios = false;

		[ObservableProperty]
		private SucursalAsignada? sucursalSeleccionada;

		[RelayCommand]
		private async Task BuscarMedicoAsync()
		{
			if (string.IsNullOrWhiteSpace(CedulaBusqueda))
			{
				await Shell.Current.DisplayAlert("Error", "Ingrese la cédula del médico", "OK");
				return;
			}

			if (CedulaBusqueda.Length != 10)
			{
				await Shell.Current.DisplayAlert("Error", "La cédula debe tener 10 dígitos", "OK");
				return;
			}

			IsLoading = true;
			ShowResults = false;

			try
			{
				System.Diagnostics.Debug.WriteLine($"🔍 Buscando médico con cédula: {CedulaBusqueda}");

				var medicoResult = await ApiService.BuscarMedicoPorCedulaAsync(CedulaBusqueda.Trim());

				if (medicoResult.Success && medicoResult.Data != null)
				{
					MedicoEncontrado = medicoResult.Data;
					ShowResults = true;

					// Cargar horarios automáticamente
					await CargarHorariosAsync();
				}
				else
				{
					await Shell.Current.DisplayAlert("Médico no encontrado",
						medicoResult.Message ?? "No se encontró un médico con esa cédula", "OK");

					MedicoEncontrado = null;
					ShowResults = false;
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task CargarHorariosAsync()
		{
			if (MedicoEncontrado == null) return;

			try
			{
				System.Diagnostics.Debug.WriteLine($"📅 Cargando horarios del médico ID: {MedicoEncontrado.IdDoctor}");

				var horariosResult = await ApiService.ObtenerHorariosAsync(MedicoEncontrado.IdDoctor);

				if (horariosResult.Success && horariosResult.Data != null)
				{
					Horarios.Clear();
					HorariosPorSucursal.Clear();

					foreach (var horario in horariosResult.Data.HorariosRaw)
					{
						Horarios.Add(horario);
					}

					foreach (var sucursal in horariosResult.Data.HorariosPorSucursal)
					{
						HorariosPorSucursal.Add(sucursal);
					}

					Estadisticas = horariosResult.Data.Estadisticas;
					ShowHorarios = Horarios.Count > 0;

					System.Diagnostics.Debug.WriteLine($"✅ Cargados {Horarios.Count} horarios en {HorariosPorSucursal.Count} sucursales");
				}
				else
				{
					ShowHorarios = false;
					System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron horarios: {horariosResult.Message}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error cargando horarios: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error cargando horarios: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task AgregarNuevoHorarioAsync()
		{
			if (MedicoEncontrado?.Sucursales == null || MedicoEncontrado.Sucursales.Count == 0)
			{
				await Shell.Current.DisplayAlert("Error", "El médico no tiene sucursales asignadas", "OK");
				return;
			}

			// CAPTURAR EL ID ANTES DE ABRIR EL MODAL
			var idDoctor = MedicoEncontrado.IdDoctor;

			var modalPage = new Views.Modals.EditarHorarioMedicoModalPage(
				MedicoEncontrado.Sucursales.Select(s => new Sucursal
				{
					IdSucursal = s.IdSucursal,
					Nombre = s.NombreSucursal
				}).ToList());

			modalPage.HorarioGuardado += async (sender, horario) =>
			{
				System.Diagnostics.Debug.WriteLine($"EVENTO RECIBIDO: Agregar horario");

				MainThread.BeginInvokeOnMainThread(async () =>
				{
					IsLoading = true;
					try
					{
						var request = new GuardarHorariosRequest
						{
							IdDoctor = idDoctor, // USAR LA VARIABLE CAPTURADA
							Horarios = new List<CrearHorarioRequest>
					{
						new CrearHorarioRequest
						{
							IdSucursal = horario.IdSucursal,
							DiaSemana = horario.DiaSemana,
							HoraInicio = horario.HoraInicio,
							HoraFin = horario.HoraFin,
							DuracionCita = horario.DuracionCita
						}
					}
						};

						var result = await ApiService.GuardarHorariosAsync2(request);

						if (result.Success)
						{
							await Shell.Current.DisplayAlert("Horario Agregado",
								"El horario se agregó exitosamente", "OK");
							await CargarHorariosAsync();
						}
						else
						{
							await Shell.Current.DisplayAlert("Error",
								result.Message ?? "Error guardando horario", "OK");
						}
					}
					catch (Exception ex)
					{
						await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
					}
					finally
					{
						IsLoading = false;
					}
				});
			};

			await Shell.Current.Navigation.PushModalAsync(modalPage);
		}

		[RelayCommand]
		private async Task EditarHorarioAsync(HorarioDoctor horario)
		{
			// Abrir modal con datos existentes
			var modalPage = new Views.Modals.EditarHorarioMedicoModalPage(
				MedicoEncontrado.Sucursales.Select(s => new Sucursal
				{
					IdSucursal = s.IdSucursal,
					Nombre = s.NombreSucursal
				}).ToList(),
				horario); // Pasar el horario a editar

			modalPage.HorarioGuardado += async (sender, horarioEditado) =>
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					IsLoading = true;
					try
					{
						var request = new EditarHorarioRequest
						{
							IdHorario = horario.IdHorario,
							IdSucursal = horarioEditado.IdSucursal,
							DiaSemana = horarioEditado.DiaSemana,
							HoraInicio = horarioEditado.HoraInicio,
							HoraFin = horarioEditado.HoraFin,
							DuracionCita = horarioEditado.DuracionCita
						};

						var result = await ApiService.EditarHorarioAsync(request);

						if (result.Success)
						{
							await Shell.Current.DisplayAlert("Actualizado", "Horario actualizado exitosamente", "OK");
							await CargarHorariosAsync();
						}
						else
						{
							await Shell.Current.DisplayAlert("Error", result.Message ?? "Error actualizando", "OK");
						}
					}
					catch (Exception ex)
					{
						await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
					}
					finally
					{
						IsLoading = false;
					}
				});
			};

			await Shell.Current.Navigation.PushModalAsync(modalPage);
		}

		[RelayCommand]
		private async Task EliminarHorarioAsync(HorarioDoctor horario)
		{
			if (horario == null || MedicoEncontrado == null) return;

			System.Diagnostics.Debug.WriteLine($"Iniciando eliminación de horario ID: {horario.IdHorario}");

			var confirmar = await Shell.Current.DisplayAlert(
				"Confirmar Eliminación",
				$"¿Eliminar este horario?\n\n{horario.NombreSucursal}\n{horario.DiaSemanaTexto}: {horario.HoraInicio} - {horario.HoraFin}",
				"Sí, eliminar",
				"Cancelar");

			if (!confirmar) return;

			try
			{
				IsLoading = true;

				var result = await ApiService.EliminarHorarioAsync(horario.IdHorario);

				if (result.Success)
				{
					await Shell.Current.DisplayAlert("Eliminado",
						"Horario eliminado exitosamente", "OK");
					await CargarHorariosAsync();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error eliminando horario", "OK");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error eliminando: {ex.Message}");
				await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task VerDetalleSucursalAsync(HorariosPorSucursal sucursal)
		{
			if (sucursal == null) return;

			var mensaje = $"{sucursal.NombreSucursal}\n\n";
			mensaje += $"Estadísticas:\n";
			mensaje += $"• Total horarios: {sucursal.Estadisticas.TotalHorarios}\n";
			mensaje += $"• Horas semanales: {sucursal.Estadisticas.HorasSemanales:F1}h\n";
			mensaje += $"• Citas estimadas/semana: ~{sucursal.Estadisticas.CitasEstimadasSemana}\n\n";
			mensaje += $"Horarios:\n";

			foreach (var horario in sucursal.Horarios.OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio))
			{
				mensaje += $"• {horario.DiaSemanaTexto}: {horario.HoraInicio} - {horario.HoraFin} ({horario.DuracionCita}min)\n";
			}

			await Shell.Current.DisplayAlert($"Detalles - {sucursal.NombreSucursal}", mensaje, "OK");
		}

		[RelayCommand]
		private void LimpiarBusqueda()
		{
			CedulaBusqueda = "";
			MedicoEncontrado = null;
			ShowResults = false;
			ShowHorarios = false;
			Horarios.Clear();
			HorariosPorSucursal.Clear();
		}
	}
}