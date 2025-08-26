using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediSys.Models;
using MediSys.Services;

namespace MediSys.ViewModels
{
	public partial class DashboardViewModel : ObservableObject
	{
		private readonly AuthService _authService;
		private User? currentUser;

		[ObservableProperty]
		private string welcomeMessage = "Bienvenido";

		[ObservableProperty]
		private string currentDateTime = "";

		[ObservableProperty]
		private string userRole = "";

		[ObservableProperty]
		private string userInitials = "U";

		[ObservableProperty]
		private bool isMedico = false;

		[ObservableProperty]
		private bool canViewHistorial = false;

		[ObservableProperty]
		private int totalCitas = 0;

		[ObservableProperty]
		private int citasPendientes = 0;

		[ObservableProperty]
		private int historialesConsultados = 0;

		[ObservableProperty]
		private int misCitasHoy = 0;

		[ObservableProperty]
		private string lastSyncTime = "";

		public DashboardViewModel()
		{
			_authService = new AuthService();
			StartDateTimeTimer();
		}

		public async Task RefreshDashboardAsync()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Refreshing dashboard data...");

				currentUser = await _authService.GetCurrentUserAsync();

				if (currentUser != null)
				{
					System.Diagnostics.Debug.WriteLine($"Dashboard user loaded: {currentUser.Nombres} {currentUser.Apellidos} ({currentUser.Rol})");

					// Configurar saludo personalizado
					var greeting = GetGreetingByTime();
					WelcomeMessage = $"{greeting}, {currentUser.Nombres}";

					UserRole = currentUser.Rol ?? "Usuario";

					// Crear iniciales
					try
					{
						var nombres = currentUser.Nombres?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Usuario" };
						var apellidos = currentUser.Apellidos?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Sistema" };

						var inicial1 = nombres[0].Length > 0 ? nombres[0][0].ToString() : "U";
						var inicial2 = apellidos[0].Length > 0 ? apellidos[0][0].ToString() : "S";

						UserInitials = $"{inicial1}{inicial2}".ToUpper();
					}
					catch
					{
						UserInitials = "US";
					}

					// Configurar permisos
					IsMedico = currentUser.Rol?.ToLower() == "medico";
					CanViewHistorial = currentUser.Rol?.ToLower() != "paciente";

					// Cargar estadísticas
					await LoadStatsAsync();

					System.Diagnostics.Debug.WriteLine($"Dashboard refreshed for: {WelcomeMessage}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("No user found for dashboard");
					ResetDashboard();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error refreshing dashboard: {ex.Message}");
				ResetDashboard();
			}
		}

		private void ResetDashboard()
		{
			WelcomeMessage = "Bienvenido";
			UserRole = "Usuario";
			UserInitials = "U";
			IsMedico = false;
			CanViewHistorial = false;
			TotalCitas = 0;
			CitasPendientes = 0;
			MisCitasHoy = 0;
			HistorialesConsultados = 0;
		}

		private async Task LoadStatsAsync()
		{
			// Simular carga de estadísticas (aquí irían las llamadas reales a la API)
			await Task.Delay(500);

			if (IsMedico)
			{
				TotalCitas = 45;
				CitasPendientes = 8;
				MisCitasHoy = 5;
				HistorialesConsultados = 23;
			}
			else
			{
				TotalCitas = 12;
				CitasPendientes = 2;
				HistorialesConsultados = 8;
				MisCitasHoy = 0;
			}

			LastSyncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
		}

		private string GetGreetingByTime()
		{
			var hour = DateTime.Now.Hour;
			return hour switch
			{
				< 12 => "Buenos días",
				< 18 => "Buenas tardes",
				_ => "Buenas noches"
			};
		}

		private void StartDateTimeTimer()
		{
			var timer = new Timer((_) => {
				CurrentDateTime = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm");
			}, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
		}

		[RelayCommand]
		private async Task NavigateToHistorial()
		{
			await Shell.Current.GoToAsync("//historial");
		}

		[RelayCommand]
		private async Task NavigateToCitas()
		{
			await Shell.Current.GoToAsync("//consulta-citas");
		}

		[RelayCommand]
		private async Task NavigateToPerfil()
		{
			await Shell.Current.GoToAsync("//perfil");
		}

		[RelayCommand]
		private async Task NavigateToMisCitas()
		{
			await Shell.Current.GoToAsync("//mis-citas");
		}
	}
}