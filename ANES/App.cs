using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal readonly struct MainDisplay
{
	private const int _scale = 2;

	private readonly Nes _nes;

	private readonly SdlWindow _window;
	private readonly SdlRenderer _renderer;
	private readonly SdlTexture _texture;

	public MainDisplay() => throw new InvalidOperationException();

	public MainDisplay(Nes nes)
	{
		_nes = nes;
		(_window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES: Adrian's NES Emulator",
			256 * _scale,
			240 * _scale
		);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
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
		_window.Destroy();
	}
}

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private readonly Nes _nes = new();

	private MainDisplay _mainDisplay;

	private int _windowCount = 0;

	protected override SdlAppResult Init()
	{
		_mainDisplay = new(_nes);
		_windowCount++;

		_nes.Start();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_mainDisplay.Iterate();

		return SdlAppResult.Continue;
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

		_mainDisplay.Destroy();
	}
}
