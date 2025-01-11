using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.ComponentModel;

namespace ANES.Platform.Windows;

public partial class MainWindow : Form
{
	private readonly Thread _emuThread;

	private readonly Nes _nes = new();
	private SdlRenderer _sdlRenderer = null!;
	private AnesSdlRenderer _anesRenderer = null!;

	private volatile bool _keepRunning = true;

	private volatile bool _waitingForFrame = true;
	public MainWindow()
	{
		AnesSdlRenderer.SetRuntimeImportResolver();

		InitializeComponent();

		_nes.FrameReady += OnFrameReady;

		_emuThread = new(ThreadProc);

		SetScale(3);
	}

	private void SetScale(float scale)
	{
		// Adjust the window size to fit the NES screen
		var diffX = (int)((AnesSdlRenderer.ScreenWidth * scale) - sdlControl.Width);
		var diffY = (int)((AnesSdlRenderer.ScreenHeight * scale) - sdlControl.Height);
		ClientSize += new Size(diffX, diffY);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
	}

	protected override void OnLoad(EventArgs e)
	{
		_sdlRenderer = SdlRenderer.Create(sdlControl.SdlWindow);
		_anesRenderer = new(_nes, _sdlRenderer);
		_emuThread.Start();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
			case Keys.A:
				_nes.Controllers.Controller1.ButtonB = true;
				break;
			case Keys.S:
				_nes.Controllers.Controller1.ButtonA = true;
				break;
			case Keys.ShiftKey:
				_nes.Controllers.Controller1.ButtonSelect = true;
				break;
			case Keys.Enter:
				_nes.Controllers.Controller1.ButtonStart = true;
				break;
			case Keys.Up:
				_nes.Controllers.Controller1.ButtonUp = true;
				break;
			case Keys.Down:
				_nes.Controllers.Controller1.ButtonDown = true;
				break;
			case Keys.Left:
				_nes.Controllers.Controller1.ButtonLeft = true;
				break;
			case Keys.Right:
				_nes.Controllers.Controller1.ButtonRight = true;
				break;
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
			case Keys.A:
				_nes.Controllers.Controller1.ButtonB = false;
				break;
			case Keys.S:
				_nes.Controllers.Controller1.ButtonA = false;
				break;
			case Keys.ShiftKey:
				_nes.Controllers.Controller1.ButtonSelect = false;
				break;
			case Keys.Enter:
				_nes.Controllers.Controller1.ButtonStart = false;
				break;
			case Keys.Up:
				_nes.Controllers.Controller1.ButtonUp = false;
				break;
			case Keys.Down:
				_nes.Controllers.Controller1.ButtonDown = false;
				break;
			case Keys.Left:
				_nes.Controllers.Controller1.ButtonLeft = false;
				break;
			case Keys.Right:
				_nes.Controllers.Controller1.ButtonRight = false;
				break;
		}
	}

	private void OnFrameReady(object? sender, EventArgs e)
	{
		_waitingForFrame = false;

		while (!_waitingForFrame && _keepRunning) { }
	}

	private void ThreadProc()
	{
		_nes.Start();
		_nes.InsertCartridge(@"C:\Stuff\Roms\nes\smb1.nes");
		_nes.Reset();

		while (_keepRunning)
		{
			while (_waitingForFrame && _keepRunning) { }
			_sdlRenderer.SetDrawColor(Color.Black);
			_sdlRenderer.Clear();
			_anesRenderer.Render();
			_sdlRenderer.Present();
			_waitingForFrame = true;
		}

		_nes.Stop();
	}

	protected override void OnClosed(EventArgs e)
	{
		_keepRunning = false;
		SpinWait.SpinUntil(() => !_emuThread.IsAlive);

		_sdlRenderer.Destroy();

		base.OnClosed(e);
	}
}
