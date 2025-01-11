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
#if !PUBLISH
		AnesSdlRenderer.SetRuntimeImportResolver();
#endif

		InitializeComponent();

		_nes.FrameReady += OnFrameReady;

		_emuThread = new(ThreadProc);

		ClientSize = new(AnesSdlRenderer.ScreenWidth, AnesSdlRenderer.ScreenWidth);
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

	private void OnFrameReady(object? sender, EventArgs e)
	{
		_waitingForFrame = false;

		while (!_waitingForFrame && _keepRunning) { }
	}

	private void ThreadProc()
	{
		_nes.InsertCartridge(@"C:\Stuff\Roms\nes\pacman.nes");
		_nes.Start();
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
