using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private SdlWindow _patternTableWindow = null!;
	private SdlRenderer _patternTableRenderer = null!;
	private SdlTexture _patternTable0Tex = null!;
	private SdlTexture _patternTable1Tex = null!;
	private SdlWindowId _patternTableWindowId;

	private const int _patternTableWidth = 8 * 16;
	private const int _patternTableHeight = 8 * 16;
	private const int _patternTablePadding = 8;
	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	private const int _scale = 3;

	protected override SdlAppResult Init()
	{
		(_patternTableWindow, _patternTableRenderer) = SdlWindow.CreateWithRenderer("ANES - Debug", (_patternTableWidth * 2 + _patternTablePadding * 3) * _scale,
			(_patternTableHeight + _patternTablePadding * 2) * _scale);

		_patternTableWindowId = _patternTableWindow.GetId();

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, _patternTableWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, _patternTableHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Rgba8888);
		_patternTable0Tex = SdlTexture.CreateWithProperties(_patternTableRenderer, tilesTexProps);
		_patternTable1Tex = SdlTexture.CreateWithProperties(_patternTableRenderer, tilesTexProps);
		tilesTexProps.Destroy();

		_patternTable0Tex.SetScaleMode(SdlScaleMode.Nearest);
		_patternTable1Tex.SetScaleMode(SdlScaleMode.Nearest);

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_patternTableRenderer.SetDrawColor(Color.White);
		_patternTableRenderer.Clear();

		CopyTiles(_patternTable0Tex, 0);
		CopyTiles(_patternTable1Tex, 1);

		var patternTable0Target = new RectangleF(_patternTablePadding, _patternTablePadding, _patternTableWidth, _patternTableHeight);
		var patternTable1Target = new RectangleF(_patternTablePadding * 2 + _patternTableWidth, _patternTablePadding, _patternTableWidth, _patternTableHeight);

		_patternTableRenderer.SetScale(_scale, _scale);

		_patternTable0Tex.Render(RectangleF.Empty, patternTable0Target);
		_patternTable1Tex.Render(RectangleF.Empty, patternTable1Target);

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

		for (var tileY = 0; tileY < 16; tileY++)
		{
			for (var tileX = 0; tileX < 16; tileX++)
			{
				var texX = tileX * 8;
				var texY = tileY * 8;

				CopyTile(surface, texX, texY, patternTableHalf, tileX, tileY);
			}
		}

		tilesTex.Unlock();
	}

	private void CopyTile(SdlSurface surface, int surfX, int surfY, int patternTableHalf, int tileX, int tileY)
	{
		var tileNum = tileX + tileY * 16;

		var pixels = surface.GetPixels<uint>();

		for (var y = 0; y < 8; y++)
		{
			var plane0Index = (patternTableHalf << 12) | (tileNum << 4) | y;
			var plane1Index = (patternTableHalf << 12) | (tileNum << 4) | y | (1 << 3);

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
				return SdlAppResult.Success;
		}
		return SdlAppResult.Continue;
	}

	protected override void Quit(SdlAppResult result)
	{
		_nes.Stop();

		_patternTable0Tex.Destroy();
		_patternTable1Tex.Destroy();
		_patternTableRenderer.Destroy();
		_patternTableWindow.Destroy();
	}
}
