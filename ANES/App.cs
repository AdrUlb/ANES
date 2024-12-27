using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal readonly struct DebugDisplay
{
	private const int _scale = 2;

	private const int _patternTableWidth = 8 * 16;
	private const int _patternTableHeight = 8 * 16;
	private const int _patternTablePadding = 5;

	private readonly Nes _nes;

	public readonly SdlWindow Window;
	private readonly SdlRenderer _renderer;
	public readonly SdlWindowId WindowId;
	private readonly SdlTexture _texture;

	public DebugDisplay() => throw new InvalidOperationException();

	public DebugDisplay(Nes nes)
	{
		_nes = nes;
		/*(Window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES - Debug Display",
			(_patternTableWidth * 2 + _patternTablePadding * 3) * _scale,
			(_patternTableHeight + _patternTablePadding * 2) * _scale
		);*/
		(Window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES - Debug Display",
			256 * _scale,
			240 * _scale
		);
		WindowId = Window.GetId();

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		//tilesTexProps.Set(SdlProperties.TextureCreateWidth, _patternTableWidth);
		//tilesTexProps.Set(SdlProperties.TextureCreateHeight, _patternTableHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, 256);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, 240);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_texture = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		_texture.SetScaleMode(SdlScaleMode.Nearest);
		tilesTexProps.Destroy();
	}

	public void Iterate()
	{
		_renderer.SetDrawColor(Color.White);
		_renderer.Clear();

		var patternTable0Target = new RectangleF(_patternTablePadding, _patternTablePadding, _patternTableWidth, _patternTableHeight);
		var patternTable1Target = new RectangleF(_patternTablePadding * 2 + _patternTableWidth, _patternTablePadding, _patternTableWidth, _patternTableHeight);

		var surface = _texture.LockToSurface();

		if (surface.Format != SdlPixelFormat.Argb8888)
			throw new UnreachableException();

		var pixels = surface.GetPixels<int>();
		for (var y = 0; y < 240; y++)
		{
			for (var x = 0; x < 256; x++)
			{
				pixels[x + y * surface.Pitch / 4] = _nes.Ppu.Pixels[x + y * 256].ToArgb();
			}
		}
		
		_texture.Unlock();
		
		_texture.Render();
		
		_renderer.Present();
	}

	public void Destroy()
	{
		_texture.Destroy();
		_renderer.Destroy();
		Window.Destroy();
	}
}

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private DebugDisplay _debugDisplay;

	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	private int _windowCount = 0;

	protected override SdlAppResult Init()
	{
		_debugDisplay = new(_nes);
		_windowCount++;

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_debugDisplay.Iterate();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Event(SdlEvent sdlEvent)
	{
		switch (sdlEvent.Type)
		{
			case SdlEventType.WindowCloseRequested:
				{
					if (sdlEvent.Window.WindowID == _debugDisplay.WindowId)
					{
						_debugDisplay.Window.Hide();
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

		_debugDisplay.Destroy();
	}
}
