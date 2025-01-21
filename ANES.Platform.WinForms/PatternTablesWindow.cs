using ANES.Emulation;
using ANES.Platform.WinForms.Controls;
using System.ComponentModel;

namespace ANES.Platform.WinForms;

[DesignerCategory("")]
internal sealed class PatternTablesWindow : Form
{
	const int _padding = 12;

	private readonly Nes _nes;

	private MyMenuStrip _mainMenu;
	private GroupBox _patternGroup0 = new();
	private GroupBox _patternGroup1 = new();
	private readonly PixelRenderer _patternRenderer0 = new(16 * 8, 16 * 8);
	private readonly PixelRenderer _patternRenderer1 = new(16 * 8, 16 * 8);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Quit { get; private set; } = false;

	private readonly Lock _lock = new();

	public PatternTablesWindow(Nes nes)
	{
		_nes = nes;

		SuspendLayout();

		Text = "Pattern Tables";
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;

		_patternRenderer0.Dock = DockStyle.Fill;
		_patternRenderer0.Size = new Size(131, 125);

		_patternGroup0.SuspendLayout();
		_patternGroup0.Padding = new Padding(12);
		_patternGroup0.Size = new Size(155, 165);
		_patternGroup0.Text = "Pattern Table 0x0000";
		_patternGroup0.Controls.Add(_patternRenderer0);
		_patternGroup0.ResumeLayout(false);

		_patternRenderer1.Dock = DockStyle.Fill;
		_patternRenderer1.Size = new Size(131, 125);

		_patternGroup1.SuspendLayout();
		_patternGroup1.Padding = new Padding(12);
		_patternGroup1.Size = new Size(161, 166);
		_patternGroup1.Text = "Pattern Table 0x1000";
		_patternGroup1.Controls.Add(_patternRenderer1);
		_patternGroup1.ResumeLayout(false);

		_mainMenu = CreateMainMenu();

		Controls.AddRange(_mainMenu, _patternGroup0, _patternGroup1);

		MainMenuStrip = _mainMenu;

		ResumeLayout();

		SetScale(3);
	}

	private MyMenuStrip CreateMainMenu()
	{
		_mainMenu = new ANES.Platform.WinForms.Controls.MyMenuStrip();
		_mainMenu.SuspendLayout();
		_mainMenu.Items.AddRange([
			CreateMainMenuFile(),
			CreateMainMenuView(),
		]);
		_mainMenu.ResumeLayout(false);
		return _mainMenu;
	}

	private ToolStripMenuItem CreateMainMenuFile()
	{
		var itemExit = new ToolStripMenuItem
		{
			Name = "mainMenuFileExit",
			Size = new Size(93, 22),
			Text = "E&xit"
		};
		itemExit.Click += (_, _) => Close();

		var item = new ToolStripMenuItem();
		item.DropDownItems.Add(itemExit);
		item.Text = "&File";
		return item;
	}

	private ToolStripMenuItem CreateMainMenuView()
	{
		var itemScale = new ToolStripMenuItem
		{
			Text = "&Scale"
		};

		for (var i = 1; i <= 5; i++)
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

	private void SetScale(int scale)
	{
		var tableWidth = 16 * 8 * scale;
		var tableHeight = 16 * 8 * scale;
		var tableSize = new Size(tableWidth, tableHeight);

		_patternGroup0.Size += tableSize - _patternRenderer0.Size;
		_patternGroup1.Size += tableSize - _patternRenderer1.Size;


		_patternGroup0.Location = new(_padding, _mainMenu.Height + _padding);
		_patternGroup1.Location = new(_patternGroup0.Right + _padding, _mainMenu.Height + _padding);
		ClientSize = new((_patternGroup0.Width * 2) + (_padding * 3), _patternGroup0.Height + (_padding * 2) + _mainMenu.Height);
	}

	private void CopyPatternTable(PixelRenderer renderer, int half)
	{
		for (var tileY = 0; tileY < 16; tileY++)
		{
			for (var tileX = 0; tileX < 16; tileX++)
			{
				var tileIndex = tileX + tileY * 16;

				for (var y = 0; y < 8; y++)
				{
					var address = (ushort)((half << 12) | (tileIndex << 4) | (0 << 3) | y);
					var patternLow = _nes.PpuBus.ReadByte(address, true);
					address += 8;
					var patternHigh = _nes.PpuBus.ReadByte(address, true);
					for (var x = 0; x < 8; x++)
					{
						var pattern = ((patternHigh >> 7) & 1) << 1 | ((patternLow >> 7) & 1);
						patternLow <<= 1;
						patternHigh <<= 1;

						var color = Color.FromArgb(255 * pattern / 3, 255 * pattern / 3, 255 * pattern / 3);

						renderer.SetPixel((tileX * 8) + x, (tileY * 8) + y, color);
					}
				}
			}
		}
	}

	internal void Render()
	{
		if (Quit)
			return;

		CopyPatternTable(_patternRenderer0, 0);
		CopyPatternTable(_patternRenderer1, 1);

		_patternRenderer0.Invalidate();
		_patternRenderer1.Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		var groupWidth = (ClientSize.Width - (_padding * 3)) / 2;
		var groupHeight = ClientSize.Height - _mainMenu.Height - (2 * _padding);
		var groupSize = new Size(groupWidth, groupHeight);

		_patternGroup0.Size = groupSize;
		_patternGroup1.Size = groupSize;

		_patternGroup0.Location = new(_padding, _mainMenu.Height + _padding);
		_patternGroup1.Location = new(_patternGroup0.Right + _padding, _mainMenu.Height + _padding);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		Quit = true;
		Hide();
		e.Cancel = true;
		base.OnFormClosing(e);
	}
}
