namespace MediSys
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			MainPage = new AppShell();
		}

		protected override void OnStart()
		{
			base.OnStart();
			System.Diagnostics.Debug.WriteLine("🚀 App iniciada");

			// Verificar inicialización de servicios críticos
			try
			{
				var apiService = new Services.MediSysApiService();
				System.Diagnostics.Debug.WriteLine("✅ ApiService inicializado correctamente");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ Error inicializando ApiService: {ex.Message}");
			}
		}

		protected override void OnSleep()
		{
			base.OnSleep();
			System.Diagnostics.Debug.WriteLine("😴 App en segundo plano");
		}

		protected override void OnResume()
		{
			base.OnResume();
			System.Diagnostics.Debug.WriteLine("🔄 App reanudada");
		}
	}
}