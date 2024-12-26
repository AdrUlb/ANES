using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private SdlWindow _window = null!;
	private SdlRenderer _renderer = null!;
	private SdlTexture _tilesTexture = null!;

	private const int tileSize = 8;
	private const int tilesX = 16;
	private const int tilesY = 16;

	private const int tilesW = tileSize * tilesX;
	private const int tilesH = tileSize * tilesY;

	private const int scale = 3;

	protected override SdlAppResult Init()
	{
		(_window, _renderer) = SdlWindow.CreateWithRenderer("ANES - Pattern Table 0x0000", tilesW * scale, tilesH * scale);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, tilesW);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, tilesH);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Rgba8888);
		_tilesTexture = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		tilesTexProps.Destroy();
		_tilesTexture.SetScaleMode(SdlScaleMode.Nearest);

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_renderer.SetDrawColor(Color.Orange);
		_renderer.Clear();

		CopyTiles();

		_tilesTexture.Render();

		_renderer.Present();
		return SdlAppResult.Continue;
	}

	private unsafe void CopyTiles()
	{
		var surf = _tilesTexture.LockToSurface().GetData();
		var pixels = (uint*)surf->Pixels;

		if (surf->Format != SdlPixelFormat.Rgba8888)
			throw new UnreachableException();

		for (var tileY = 0; tileY < tilesY; tileY++)
		{
			for (var tileX = 0; tileX < tilesX; tileX++)
			{
				var texX = tileX * tileSize;
				var texY = tileY * tileSize;

				CopyTile(pixels, surf->Pitch, tileX, tileY, texX, texY);
			}
		}

		_tilesTexture.Unlock();
	}

	private unsafe void CopyTile(uint* pixels, int pitch, int tileX, int tileY, int texX, int texY)
	{
		var tileNum = tileX + tileY * tilesX;

		for (var y = 0; y < tileSize; y++)
		{
			var plane0Index = (tileNum << 4) | y;
			var plane1Index = (tileNum << 4) | y | (1 << 3);

			var plane0 = _nes.PpuMemoryBus.ReadByte((ushort)plane0Index, true);
			var plane1 = _nes.PpuMemoryBus.ReadByte((ushort)plane1Index, true);

			for (var x = 0; x < tileSize; x++)
			{
				var bit0 = (plane0 >> (tileSize - 1 - x)) & 1;
				var bit1 = (plane1 >> (tileSize - 1 - x)) & 1;
				var colorIndex = (bit0 << 1) | bit1;

				var color = colorIndex switch
				{
					0 => Color.Black,
					1 => Color.Red,
					2 => Color.Green,
					3 => Color.Blue,
					_ => throw new UnreachableException()
				};

				var xx = x + texX;
				var yy = y + texY;
				var index = xx + yy * pitch / 4;
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

		_tilesTexture.Destroy();

		_window.Destroy();
	}
}
