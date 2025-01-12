using ANES.Emulation;
using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.ComponentModel;
using System.Diagnostics;

namespace ANES.Platform.WinForms;

public partial class MainWindow : Form
{
	private readonly Thread _renderThread;

	private readonly Nes _nes = new();
	private SdlRenderer _sdlRenderer = null!;
	private AnesSdlRenderer _anesRenderer = null!;

	private volatile bool _quit = false;

	private PatternTablesWindow? _patternTablesWindow = null;

	public MainWindow()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

		AnesSdlRenderer.SetRuntimeImportResolver();

		InitializeComponent();

		SuspendLayout();
		// Add menu options for setting the scale to 1x-8x
		for (var i = 1; i <= 8; i++)
		{
			var scale = i;
			var item = new ToolStripMenuItem($"{i}x", null, (_, _) => SetScale(scale));
			mainMenuViewScale.DropDownItems.Add(item);
		}
		SetScale(2);
		ResumeLayout();

		_renderThread = new(RenderProc);
	}

	private void SetScale(int scale)
	{
		// Limit the scale to the maximum size of the working area
		var workingArea = Screen.FromControl(this).WorkingArea;
		var maxScaleX = workingArea.Width / AnesSdlRenderer.ScreenWidth;
		var maxScaleY = workingArea.Height / AnesSdlRenderer.ScreenHeight;
		var maxScale = Math.Min(maxScaleX, maxScaleY);

		if (scale > maxScale)
			MessageBox.Show(this, $"The requested scale ({scale}x) would exceed the screen size.\nScaling to the maximum possible size ({maxScale}x) instead.", "Scale", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

		scale = Math.Min(scale, maxScale);

		// Adjust the window size to fit the NES screen
		var newWidth = (AnesSdlRenderer.ScreenWidth * scale) - sdlControl.Width + ClientSize.Width;
		var newHeight = (AnesSdlRenderer.ScreenHeight * scale) - sdlControl.Height + ClientSize.Height;
		ClientSize = new Size(newWidth, newHeight);
	}

	private void RenderProc()
	{
		while (!_quit)
		{
			_sdlRenderer.SetDrawColor(Color.Black);
			_sdlRenderer.Clear();
			_anesRenderer.Render();
			_patternTablesWindow?.Render();
			_sdlRenderer.Present();
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		if (sdlControl.SdlWindow == null)
			throw new UnreachableException();
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

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		Hide();
		_quit = true;
		SpinWait.SpinUntil(() => !_renderThread.IsAlive);
		_anesRenderer.Dispose();
		_sdlRenderer.Destroy();

		base.OnFormClosing(e);

		_nes.Stop();
	}

	private void mainMenuFileExit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void mainMenuFileOpen_Click(object sender, EventArgs e)
	{
		var result = romOpenFIleDialog.ShowDialog();

		if (result != DialogResult.OK)
			return;

		var romFile = romOpenFIleDialog.FileName;
		_nes.InsertCartridge(romFile);
		_nes.Reset();
	}

	private void mainMenuDebugPatternTableViewer_Click(object sender, EventArgs e)
	{
		if (_patternTablesWindow == null || _patternTablesWindow.IsDisposed)
		{
			_patternTablesWindow = new PatternTablesWindow(_nes);
			_patternTablesWindow.Show();
		}
		else _patternTablesWindow.Focus();
	}
}
