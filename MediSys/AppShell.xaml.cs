using MediSys.Views.Auth;

namespace MediSys
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			// 🔥 NAVEGAR DIRECTAMENTE AL LOGIN AL INICIAR
			Loaded += OnAppShellLoaded;
		}

		private async void OnAppShellLoaded(object? sender, EventArgs e)
		{
			// Asegurar que siempre vaya al login primero
			await Shell.Current.GoToAsync("//login");
		}
	}
}