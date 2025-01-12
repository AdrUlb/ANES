using ANES.Emulation;
using Sdl3Sharp;
using System.Diagnostics;
using System.Windows.Forms;

namespace ANES.Platform.WinForms;

public partial class PatternTablesWindow : Form
{
	private readonly Nes _nes;

	private readonly Thread _renderThread;

	private SdlRenderer _patternRenderer0 = null!;
	private SdlRenderer _patternRenderer1 = null!;
	private SdlTexture _patternTexture0 = null!;
	private SdlTexture _patternTexture1 = null!;

	private volatile bool _quit = false;

	private readonly Lock _lock = new();

	public PatternTablesWindow(Nes nes)
	{
		_nes = nes;

		SuspendLayout();

		InitializeComponent();

		// Add menu options for setting the scale to 1x-4x
		for (var i = 1; i <= 4; i++)
		{
			var scale = i;
			var item = new ToolStripMenuItem($"{i}x", null, (_, _) => SetScale(scale));
			mainMenuViewScale.DropDownItems.Add(item);
		}
		SetScale(3);

		ResumeLayout();

		_renderThread = new(Render);
	}

	private void CopyPatternTable(SdlTexture texture, int half)
	{

		var surface = texture.LockToSurface();

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

						var row = surface.GetPixels<int>(tileY * 8 + y);
						var pixel = (tileX * 8 + x);
						row[pixel] = color.ToArgb();
					}
				}
			}
		}
		texture.Unlock();

		//var address = (half << 12) | (tileIndex << 4) | (bitPlane << 3) | y;
	}

	internal void Render()
	{
		using (_lock.EnterScope())
		{
			if (_quit)
				return;

			if (_patternTexture0 == null || _patternTexture1 == null)
				return;

			CopyPatternTable(_patternTexture0, 0);
			CopyPatternTable(_patternTexture1, 1);

			_patternRenderer0.SetDrawColor(Color.Black);
			_patternRenderer0.Clear();
			_patternTexture0.Render();
			_patternRenderer0.Present();

			_patternRenderer1.SetDrawColor(Color.Black);
			_patternRenderer1.Clear();
			_patternTexture1.Render();
			_patternRenderer1.Present();
		}
	}

	private void SetScale(int scale)
	{
		SuspendLayout();
		var size = new Size(16 * 8 * scale, 16 * 8 * scale);
		patternGroup0.Size += size - patternSdl0.Size;
		patternGroup1.Size += size - patternSdl1.Size;
		patternGroup0.Location = new(8, myMenuStrip1.Height + 8);
		patternGroup1.Location = new(patternGroup0.Right + 8, myMenuStrip1.Height + 8);

		ClientSize = new(patternGroup1.Right + 8, patternGroup0.Bottom + 8);
		ResumeLayout();
	}

	protected override void OnLoad(EventArgs e)
	{
		_patternRenderer0 = SdlRenderer.Create(patternSdl0.SdlWindow ?? throw new UnreachableException());
		_patternRenderer1 = SdlRenderer.Create(patternSdl1.SdlWindow ?? throw new UnreachableException());

		var props = SdlProperties.Create();
		props.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		props.Set(SdlProperties.TextureCreateWidth, 16 * 8);
		props.Set(SdlProperties.TextureCreateHeight, 16 * 8);
		props.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_patternTexture0 = SdlTexture.CreateWithProperties(_patternRenderer0, props);
		_patternTexture0.SetScaleMode(SdlScaleMode.Nearest);
		_patternTexture1 = SdlTexture.CreateWithProperties(_patternRenderer1, props);
		_patternTexture1.SetScaleMode(SdlScaleMode.Nearest);

		_renderThread.Start();
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
