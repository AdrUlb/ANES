using ANES.Emulation;
using ANES.Platform.WinForms.Controls;
using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.ComponentModel;
using System.Diagnostics;

namespace ANES.Platform.WinForms;

[DesignerCategory("")]
internal sealed class MainWindow : Form
{
	private readonly Nes _nes = new();

	private SdlControl _sdlControl = new();
	private SdlRenderer _sdlRenderer = null!;
	private AnesSdlRenderer? _anesRenderer;
	private PatternTablesWindow? _patternTablesWindow = null;

	public MainWindow()
	{
		_nes.Ppu.Frame += OnFrameReady;

		AnesSdlRenderer.SetRuntimeImportResolver();
		SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

		SuspendLayout();

		_sdlControl.Dock = DockStyle.Fill;

		Text = "ANES";

		var mainMenu = CreateMainMenu();

		Controls.AddRange(_sdlControl, mainMenu);

		MainMenuStrip = mainMenu;

		ResumeLayout();
	}

	private MyMenuStrip CreateMainMenu()
	{
		var mainMenu = new MyMenuStrip();
		mainMenu.SuspendLayout();
		mainMenu.Items.AddRange(
			CreateMainMenuFile(),
			CreateMainMenuView(),
			CreateMainMenuDebug(),
			CreateMainMenuHelp()
		);
		mainMenu.ResumeLayout(false);
		return mainMenu;
	}

	private ToolStripMenuItem CreateMainMenuFile()
	{
		var itemOpen = new ToolStripMenuItem
		{
			ShortcutKeys = Keys.Control | Keys.O,
			Text = "&Open"
		};
		itemOpen.Click += (_, _) => LoadRomDialog();

		var itemExit = new ToolStripMenuItem
		{
			ShortcutKeys = Keys.Control | Keys.Q,
			Text = "E&xit"
		};
		itemExit.Click += (_, _) => Close();

		var item = new ToolStripMenuItem();
		item.DropDownItems.AddRange(itemOpen, new ToolStripSeparator(), itemExit);
		item.Text = "&File";
		return item;
	}

	private ToolStripMenuItem CreateMainMenuView()
	{
		var itemScale = new ToolStripMenuItem
		{
			Text = "&Scale"
		};

		for (var i = 1; i <= 8; i++)
		{
			var scale = i;
			var itemScaleX = new ToolStripMenuItem($"{i}x", null, (_, _) => SetScale(scale));
			itemScale.DropDownItems.Add(itemScaleX);
		}

		var item = new ToolStripMenuItem
		{
			Text = "&View"
		};
		item.DropDownItems.Add(itemScale);
		return item;
	}

	private ToolStripMenuItem CreateMainMenuDebug()
	{
		var itemPatternTables = new ToolStripMenuItem
		{
			Text = "&Pattern Tables"
		};
		itemPatternTables.Click += (_, _) => ShowPatternTable();

		var item = new ToolStripMenuItem
		{
			Text = "&Debug"
		};
		item.DropDownItems.Add(itemPatternTables);
		return item;
	}

	private static ToolStripMenuItem CreateMainMenuHelp()
	{
		var itemAbout = new ToolStripMenuItem
		{
			Text = "&About"
		};

		var item = new ToolStripMenuItem();
		item.DropDownItems.Add(itemAbout);
		item.Text = "&Help";
		return item;
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
		var newWidth = (AnesSdlRenderer.ScreenWidth * scale) - _sdlControl.Width + ClientSize.Width;
		var newHeight = (AnesSdlRenderer.ScreenHeight * scale) - _sdlControl.Height + ClientSize.Height;
		ClientSize = new Size(newWidth, newHeight);
	}

	private void OnFrameReady(object? sender, EventArgs e)
	{
		_patternTablesWindow?.Render();
	}

	protected override void OnLoad(EventArgs e)
	{
		SetScale(2);

		if (_sdlControl.SdlWindow == null)
			throw new UnreachableException();

		_sdlRenderer = SdlRenderer.Create(_sdlControl.SdlWindow);
		_anesRenderer = new(_nes, _sdlRenderer);
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

	protected override void OnResize(EventArgs e)
	{
		if (_anesRenderer != null)
			_anesRenderer.PauseRendering = WindowState == FormWindowState.Minimized;

		base.OnResize(e);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		Hide();

		_nes.Stop();
		_anesRenderer?.Dispose();
		_sdlRenderer.Destroy();

		base.OnFormClosing(e);
	}

	private void LoadRomDialog()
	{
		var dialog = new OpenFileDialog
		{
			DefaultExt = "nes",
			Filter = "NES files|*.nes"
		};

		var result = dialog.ShowDialog();

		if (result != DialogResult.OK)
			return;

		var romFile = dialog.FileName;
		_nes.InsertCartridge(romFile);
		_nes.Reset();
	}

	private void ShowPatternTable()
	{
		if (_patternTablesWindow != null && !_patternTablesWindow.IsDisposed)
		{
			_patternTablesWindow.Focus();
			return;
		}

		_patternTablesWindow = new PatternTablesWindow(_nes);
		_patternTablesWindow.Show();
	}
}
