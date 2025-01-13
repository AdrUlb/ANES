using ANES.Emulation;
using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.Diagnostics;

namespace ANES.Platform.WinForms;

public partial class PatternTablesWindow : Form
{
	const int _padding = 12;

	private readonly Nes _nes;

	private readonly Thread _renderThread;

	private SdlRenderer _patternRenderer0 = null!;
	private SdlRenderer _patternRenderer1 = null!;
	private AnesSdlPixels _patternPixels0 = null!;
	private AnesSdlPixels _patternPixels1 = null!;

	private volatile bool _quit = false;

	private readonly Lock _lock = new();

	public PatternTablesWindow(Nes nes)
	{
		_nes = nes;

		SuspendLayout();

		InitializeComponent();

		// Add menu options for setting the scale to 1x-4x
		for (var i = 1; i <= 5; i++)
		{
			var scale = i;
			var item = new ToolStripMenuItem($"{i}x", null, (_, _) => SetScale(scale));
			mainMenuViewScale.DropDownItems.Add(item);
		}
		SetScale(3);

		ResumeLayout();

		_renderThread = new(Render);
	}

	private void SetScale(int scale)
	{
		var tableWidth = 16 * 8 * scale;
		var tableHeight = 16 * 8 * scale;
		var tableSize = new Size(tableWidth, tableHeight);
		SetTableSize(tableSize, true);
	}

	private void SetTableSize(Size size, bool changeWindowSize)
	{
		SuspendLayout();
		patternGroup0.Size = size;
		patternGroup1.Size = size;

		patternGroup0.Location = new(_padding, myMenuStrip1.Height + _padding);
		patternGroup1.Location = new(patternGroup0.Right + _padding, myMenuStrip1.Height + _padding);

		if (changeWindowSize)
			ClientSize = new(size.Width * 2 + 3 * _padding, size.Height + myMenuStrip1.Height + 2 * _padding);
		ResumeLayout();
	}

	private void CopyPatternTable(AnesSdlPixels texture, int half)
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

						var row = texture.GetRowSpan<int>(tileY * 8 + y);
						var pixel = (tileX * 8 + x);
						row[pixel] = color.ToArgb();
					}
				}
			}
		}
	}

	internal void Render()
	{
		using (_lock.EnterScope())
		{
			if (_quit)
				return;

			if (_patternPixels0 == null || _patternPixels1 == null)
				return;

			CopyPatternTable(_patternPixels0, 0);
			CopyPatternTable(_patternPixels1, 1);

			_patternRenderer0.SetDrawColor(Color.Black);
			_patternRenderer0.Clear();
			_patternPixels0.Render();
			_patternRenderer0.Present();

			_patternRenderer1.SetDrawColor(Color.Black);
			_patternRenderer1.Clear();
			_patternPixels1.Render();
			_patternRenderer1.Present();
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		_patternRenderer0 = SdlRenderer.Create(patternSdl0.SdlWindow ?? throw new UnreachableException());
		_patternRenderer1 = SdlRenderer.Create(patternSdl1.SdlWindow ?? throw new UnreachableException());

		_patternPixels0 = new(_patternRenderer0, 16 * 8, 16 * 8);
		_patternPixels1 = new(_patternRenderer1, 16 * 8, 16 * 8);

		_renderThread.Start();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		var tableWidth = (ClientSize.Width - 3 * _padding) / 2;
		var tableHeight = ClientSize.Height - myMenuStrip1.Height - 2 * _padding;
		var tableSize = new Size(tableWidth, tableHeight);
		SetTableSize(tableSize, false);
		
		base.OnSizeChanged(e);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		using (_lock.EnterScope())
		{
			Hide();
			_quit = true;
			SpinWait.SpinUntil(() => !_renderThread.IsAlive);

			_patternRenderer0.Destroy();
			_patternRenderer1.Destroy();
			base.OnFormClosing(e);
		}
	}

	private void mainMenuFileExit_Click(object sender, EventArgs e)
	{
		Close();
	}
}
