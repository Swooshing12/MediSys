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

		// ===== EDICIÓN =====
		[ObservableProperty]
		private bool isEditing = false;

		[ObservableProperty]
		private ObservableCollection<HorarioCrear> nuevosHorarios = new();

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
		private void IniciarEdicion()
		{
			if (MedicoEncontrado == null) return;

			IsEditing = true;
			NuevosHorarios.Clear();

			// Copiar horarios existentes para edición
			foreach (var horario in Horarios)
			{
				var sucursal = MedicoEncontrado.Sucursales.FirstOrDefault(s => s.IdSucursal == horario.IdSucursal);
				if (sucursal != null)
				{
					NuevosHorarios.Add(new HorarioCrear
					{
						IdSucursal = horario.IdSucursal,
						NombreSucursal = sucursal.NombreSucursal,
						DiaSemana = horario.DiaSemana,
						HoraInicio = horario.HoraInicio,
						HoraFin = horario.HoraFin,
						DuracionCita = horario.DuracionCita
					});
				}
			}
		}

		[RelayCommand]
		private void CancelarEdicion()
		{
			IsEditing = false;
			NuevosHorarios.Clear();
		}

		[RelayCommand]
		private async Task AgregarNuevoHorarioAsync()
		{
			if (MedicoEncontrado?.Sucursales == null || MedicoEncontrado.Sucursales.Count == 0)
			{
				await Shell.Current.DisplayAlert("Error", "El médico no tiene sucursales asignadas", "OK");
				return;
			}

			var modalPage = new Views.Modals.AgregarHorarioModalPage(
				MedicoEncontrado.Sucursales.Select(s => new Sucursal
				{
					IdSucursal = s.IdSucursal,
					Nombre = s.NombreSucursal
				}).ToList());

			modalPage.HorarioGuardado += (sender, horario) =>
			{
				NuevosHorarios.Add(horario);
			};

			await Shell.Current.Navigation.PushModalAsync(modalPage);
		}

		[RelayCommand]
		private void RemoverHorario(HorarioCrear horario)
		{
			if (horario != null && NuevosHorarios.Contains(horario))
			{
				NuevosHorarios.Remove(horario);
			}
		}

		[RelayCommand]
		private async Task GuardarHorariosAsync()
		{
			if (MedicoEncontrado == null) return;

			IsLoading = true;

			try
			{
				var request = new GuardarHorariosRequest
				{
					IdDoctor = MedicoEncontrado.IdDoctor,
					Horarios = NuevosHorarios.Select(h => new CrearHorarioRequest
					{
						IdSucursal = h.IdSucursal,
						DiaSemana = h.DiaSemana,
						HoraInicio = h.HoraInicio,
						HoraFin = h.HoraFin,
						DuracionCita = h.DuracionCita
					}).ToList()
				};

				var result = await ApiService.GuardarHorariosAsync(request);

				if (result.Success)
				{
					await Shell.Current.DisplayAlert("✅ Horarios Actualizados",
						$"Los horarios del Dr. {MedicoEncontrado.NombreCompleto} han sido actualizados exitosamente",
						"OK");

					IsEditing = false;
					NuevosHorarios.Clear();

					// Recargar horarios
					await CargarHorariosAsync();
				}
				else
				{
					await Shell.Current.DisplayAlert("Error",
						result.Message ?? "Error actualizando horarios",
						"OK");
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
		private async Task VerDetalleSucursalAsync(HorariosPorSucursal sucursal)
		{
			if (sucursal == null) return;

			var mensaje = $"🏢 {sucursal.NombreSucursal}\n\n";
			mensaje += $"📊 Estadísticas:\n";
			mensaje += $"• Total horarios: {sucursal.Estadisticas.TotalHorarios}\n";
			mensaje += $"• Horas semanales: {sucursal.Estadisticas.HorasSemanales:F1}h\n";
			mensaje += $"• Citas estimadas/semana: ~{sucursal.Estadisticas.CitasEstimadasSemana}\n\n";
			mensaje += $"⏰ Horarios:\n";

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
			IsEditing = false;
			Horarios.Clear();
			HorariosPorSucursal.Clear();
			NuevosHorarios.Clear();
		}
	}
}