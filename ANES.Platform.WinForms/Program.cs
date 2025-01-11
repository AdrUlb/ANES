namespace ANES.Platform.WinForms;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();

#pragma warning disable WFO5001
		Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001

		Application.Run(new MainWindow());
	}
}