using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ANES.Platform.Avalonia;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	static MainWindow()
	{
		KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, handledEventsToo: false);
		KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, handledEventsToo: false);
	}

	private static void OnKeyDown(TopLevel sender, KeyEventArgs e)
	{
		switch (e.Key)
		{
			case Key.A:
				Program.Nes.Controllers.Controller1.ButtonB = true;
				break;
			case Key.S:
				Program.Nes.Controllers.Controller1.ButtonA = true;
				break;
			case Key.LeftShift:
			case Key.RightShift:
				Program.Nes.Controllers.Controller1.ButtonSelect = true;
				break;
			case Key.Enter:
				Program.Nes.Controllers.Controller1.ButtonStart = true;
				break;
			case Key.Up:
				Program.Nes.Controllers.Controller1.ButtonUp = true;
				break;
			case Key.Down:
				Program.Nes.Controllers.Controller1.ButtonDown = true;
				break;
			case Key.Left:
				Program.Nes.Controllers.Controller1.ButtonLeft = true;
				break;
			case Key.Right:
				Program.Nes.Controllers.Controller1.ButtonRight = true;
				break;
		}

		e.Handled = true;
	}

	private static void OnKeyUp(TopLevel sender, KeyEventArgs e)
	{
		switch (e.Key)
		{
			case Key.A:
				Program.Nes.Controllers.Controller1.ButtonB = false;
				break;
			case Key.S:
				Program.Nes.Controllers.Controller1.ButtonA = false;
				break;
			case Key.LeftShift:
			case Key.RightShift:
				Program.Nes.Controllers.Controller1.ButtonSelect = false;
				break;
			case Key.Enter:
				Program.Nes.Controllers.Controller1.ButtonStart = false;
				break;
			case Key.Up:
				Program.Nes.Controllers.Controller1.ButtonUp = false;
				break;
			case Key.Down:
				Program.Nes.Controllers.Controller1.ButtonDown = false;
				break;
			case Key.Left:
				Program.Nes.Controllers.Controller1.ButtonLeft = false;
				break;
			case Key.Right:
				Program.Nes.Controllers.Controller1.ButtonRight = false;
				break;
		}

		e.Handled = true;
	}

	private async Task OpenRomFileAsync()
	{
		// Get top level from the current control. Alternatively, you can use Window reference instead.
		var topLevel = TopLevel.GetTopLevel(this);

		if (topLevel == null)
			return;

		// Start async operation to open the dialog.
		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
		{
			Title = "Open ROM file",
			AllowMultiple = false,
			FileTypeFilter =
			[
				new("NES files")
				{
					Patterns = ["*.nes"],
					MimeTypes = ["application/octet-stream"]
				}
			],
		});

		if (files.Count < 1)
			return;

		Program.Nes.Stop();
		Program.Nes.InsertCartridge(files[0].Path.AbsolutePath);
		Program.Nes.Reset();
		Program.Nes.Start();
	}

	public async void MenuOpen(object? sender, RoutedEventArgs routedEventArgs)
	{
		try
		{
			await OpenRomFileAsync();
		}
		catch
		{
			// TODO
		}
	}

	public void MenuExit(object? sender, RoutedEventArgs routedEventArgs)
	{
		Close();
	}
}
