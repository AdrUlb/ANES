using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private SdlWindow _patternWindow = null!;
	private SdlRenderer _patternRenderer = null!;
	private SdlTexture _tilesTex0 = null!;
	private SdlTexture _tilesTex1 = null!;

	private const int _tileSize = 8;
	private const int _tilesX = 16;
	private const int _tilesY = 16;

	private const int _tilesW = _tileSize * _tilesX;
	private const int _tilesH = _tileSize * _tilesY;

	private const int _scale = 4;

	protected override SdlAppResult Init()
	{
		(_patternWindow, _patternRenderer) = SdlWindow.CreateWithRenderer("ANES - Pattern Tables", _tilesW * _scale * 2, _tilesH * _scale);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, _tilesW);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, _tilesH);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Rgba8888);
		_tilesTex0 = SdlTexture.CreateWithProperties(_patternRenderer, tilesTexProps);
		_tilesTex1 = SdlTexture.CreateWithProperties(_patternRenderer, tilesTexProps);
		tilesTexProps.Destroy();
		
		_tilesTex0.SetScaleMode(SdlScaleMode.Nearest);
		_tilesTex1.SetScaleMode(SdlScaleMode.Nearest);

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_patternRenderer.SetDrawColor(Color.Black);
		_patternRenderer.Clear();
		
		CopyTiles(_tilesTex0, 0);
		CopyTiles(_tilesTex1, 1);
		
		_tilesTex0.Render(RectangleF.Empty, new(0, 0, _tilesW * _scale, _tilesH * _scale));
		_tilesTex1.Render(RectangleF.Empty, new(_tilesW * _scale, 0, _tilesW * _scale, _tilesH * _scale));
		
		_patternRenderer.Present();
		
		return SdlAppResult.Continue;
	}

	private unsafe void CopyTiles(SdlTexture tilesTex, int half)
	{
		var surf = tilesTex.LockToSurface().GetData();
		var pixels = (uint*)surf->Pixels;

		if (surf->Format != SdlPixelFormat.Rgba8888)
			throw new UnreachableException();

		for (var tileY = 0; tileY < _tilesY; tileY++)
		{
			for (var tileX = 0; tileX < _tilesX; tileX++)
			{
				var texX = tileX * _tileSize;
				var texY = tileY * _tileSize;

				CopyTile(pixels, half, surf->Pitch, tileX, tileY, texX, texY);
			}
		}

		tilesTex.Unlock();
	}

	private unsafe void CopyTile(uint* pixels, int half, int pitch, int tileX, int tileY, int texX, int texY)
	{
		var tileNum = tileX + tileY * _tilesX;

		for (var y = 0; y < _tileSize; y++)
		{
			var plane0Index = (half << 12) | (tileNum << 4) | y;
			var plane1Index = (half << 12) | (tileNum << 4) | y | (1 << 3);

			var plane0 = _nes.PpuMemoryBus.ReadByte((ushort)plane0Index, true);
			var plane1 = _nes.PpuMemoryBus.ReadByte((ushort)plane1Index, true);

			for (var x = 0; x < _tileSize; x++)
			{
				var bit0 = (plane0 >> (_tileSize - 1 - x)) & 1;
				var bit1 = (plane1 >> (_tileSize - 1 - x)) & 1;
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

		_tilesTex0.Destroy();
		_patternRenderer.Destroy();
		_patternWindow.Destroy();
	}
}
