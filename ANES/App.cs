using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private SdlWindow _patternTableWindow = null!;
	private SdlRenderer _patternTableRenderer = null!;
	private SdlWindowId _patternTableWindowId;
	private SdlTexture _patternTableTex = null!;

	private const int _patternTableWidth = 8 * 16;
	private const int _patternTableHeight = 8 * 16;
	private const int _patternTablePadding = 8;
	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	private int _windowCount = 0;

	private const int _scale = 3;

	protected override SdlAppResult Init()
	{
		(_patternTableWindow, _patternTableRenderer) = SdlWindow.CreateWithRenderer(
			"ANES - Pattern Tables",
			(_patternTableWidth * 2 + _patternTablePadding * 3) * _scale,
			(_patternTableHeight + _patternTablePadding * 2) * _scale
		);
		_patternTableWindowId = _patternTableWindow.GetId();
		_windowCount++;

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, _patternTableWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, _patternTableHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Rgba8888);
		_patternTableTex = SdlTexture.CreateWithProperties(_patternTableRenderer, tilesTexProps);
		tilesTexProps.Destroy();

		_patternTableTex.SetScaleMode(SdlScaleMode.Nearest);

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_patternTableRenderer.SetDrawColor(Color.White);
		_patternTableRenderer.Clear();

		_patternTableRenderer.SetScale(_scale, _scale);

		var patternTable0Target = new RectangleF(_patternTablePadding, _patternTablePadding, _patternTableWidth, _patternTableHeight);
		var patternTable1Target = new RectangleF(_patternTablePadding * 2 + _patternTableWidth, _patternTablePadding, _patternTableWidth, _patternTableHeight);

		CopyTiles(_patternTableTex, 0);
		_patternTableTex.Render(RectangleF.Empty, patternTable0Target);

		CopyTiles(_patternTableTex, 1);
		_patternTableTex.Render(RectangleF.Empty, patternTable1Target);

		_patternTableRenderer.SetScale(2, 2);

		_patternTableRenderer.SetDrawColor(Color.Black);
		_patternTableRenderer.RenderDebugText(_patternTablePadding * 1.5f, _patternTablePadding / 4.0f, "Pattern Table 0x0000");
		_patternTableRenderer.RenderDebugText((_patternTablePadding * 2 + _patternTableWidth) * 1.5f, _patternTablePadding / 4.0f, "Pattern Table 0x1000");

		_patternTableRenderer.Present();

		return SdlAppResult.Continue;
	}

	private void CopyTiles(SdlTexture tilesTex, int patternTableHalf)
	{
		var surface = tilesTex.LockToSurface();

		if (surface.Format != SdlPixelFormat.Rgba8888)
			throw new UnreachableException();

		for (var y = 0; y < 16; y++)
		{
			for (var x = 0; x < 16; x++)
			{
				var surfX = x * 8;
				var surfY = y * 8;
				var tileIndex = x + y * 16;

				CopyTile(surface, surfX, surfY, patternTableHalf, tileIndex);
			}
		}

		tilesTex.Unlock();
	}

	private void CopyTile(SdlSurface surface, int surfX, int surfY, int patternTableHalf, int tileIndex)
	{
		var pixels = surface.GetPixels<uint>();

		for (var y = 0; y < 8; y++)
		{
			var plane0Index = (patternTableHalf << 12) | (tileIndex << 4) | y;
			var plane1Index = (patternTableHalf << 12) | (tileIndex << 4) | y | (1 << 3);

			var plane0 = _nes.PpuMemoryBus.ReadByte((ushort)plane0Index, true);
			var plane1 = _nes.PpuMemoryBus.ReadByte((ushort)plane1Index, true);

			for (var x = 0; x < 8; x++)
			{
				var bit0 = (plane0 >> (8 - 1 - x)) & 1;
				var bit1 = (plane1 >> (8 - 1 - x)) & 1;
				var colorIndex = (bit0 << 1) | bit1;

				var color = Color.FromArgb(255 * colorIndex / 3, 255 * colorIndex / 3, 255 * colorIndex / 3);

				var xx = x + surfX;
				var yy = y + surfY;
				var index = xx + yy * surface.Pitch / 4;
				pixels[index] = ((uint)color.R << 24) | ((uint)color.G << 16) | ((uint)color.B << 8) | color.A;
			}
		}
	}

	protected override SdlAppResult Event(SdlEvent sdlEvent)
	{
		switch (sdlEvent.Type)
		{
			case SdlEventType.WindowCloseRequested:
				{
					if (sdlEvent.Window.WindowID == _patternTableWindowId)
					{
						_patternTableWindow.Hide();
						_windowCount--;
					}

					return _windowCount == 0 ? SdlAppResult.Success : SdlAppResult.Continue;
				}
		}
		return SdlAppResult.Continue;
	}

	protected override void Quit(SdlAppResult result)
	{
		_nes.Stop();

		_patternTableTex.Destroy();
		_patternTableRenderer.Destroy();
		_patternTableWindow.Destroy();
	}
}
