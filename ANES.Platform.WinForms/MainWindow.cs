using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.ComponentModel;

namespace ANES.Platform.WinForms;

public partial class MainWindow : Form
{
	private readonly Thread _renderThread;

	private readonly Nes _nes = new();
	private SdlRenderer _sdlRenderer = null!;
	private AnesSdlRenderer _anesRenderer = null!;

	private volatile bool _quit = false;

	public MainWindow()
	{
		DoubleBuffered = true;

		AnesSdlRenderer.SetRuntimeImportResolver();

		InitializeComponent();

		_renderThread = new(RenderProc);

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
		_renderThread.Start();
		_nes.Start();
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

	private void RenderProc()
	{
		while (!_quit)
		{
			_sdlRenderer.SetDrawColor(Color.Black);
			_sdlRenderer.Clear();
			_anesRenderer.Render();
			_sdlRenderer.Present();
		}
	}

	protected override void OnClosed(EventArgs e)
	{
		_quit = true;
		_anesRenderer.Dispose();
		SpinWait.SpinUntil(() => !_renderThread.IsAlive);

		_sdlRenderer.Destroy();

		base.OnClosed(e);

		_nes.Stop();
	}

	private void mainMenuExit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void mainMenuOpen_Click(object sender, EventArgs e)
	{
		var result = romOpenFIleDialog.ShowDialog();

		if (result != DialogResult.OK)
			return;

		var romFile = romOpenFIleDialog.FileName;
		_nes.InsertCartridge(romFile);
		_nes.Reset();
	}
}
