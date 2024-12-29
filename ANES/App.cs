using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal readonly struct MainDisplay
{
	private const int _scale = 3;

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
			Ppu.ScreenWidth * _scale,
			Ppu.ScreenHeight * _scale,
			SdlWindowFlags.Resizable
		);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, Ppu.ScreenWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, Ppu.ScreenHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_texture = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		_texture.SetScaleMode(SdlScaleMode.Nearest);
		tilesTexProps.Destroy();
	}

	public void Iterate()
	{
		var surface = _texture.LockToSurface();

		if (surface.Format != SdlPixelFormat.Argb8888)
			throw new UnreachableException();

		var pixels = surface.GetPixels<int>();
		for (var y = 0; y < Ppu.ScreenHeight; y++)
		for (var x = 0; x < Ppu.ScreenWidth; x++)
			pixels[x + y * surface.Pitch / 4] = _nes.Ppu.Pixels[x + y * Ppu.ScreenWidth].ToArgb();

		_renderer.Clear();
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
			case SdlEventType.KeyDown:
				switch (sdlEvent.Key.Scancode)
				{
					case SdlScancode.A:
						_nes.Controllers.Controller1.ButtonA = true;
						break;
					case SdlScancode.S:
						_nes.Controllers.Controller1.ButtonB = true;
						break;
					case SdlScancode.RightShift:
						_nes.Controllers.Controller1.ButtonSelect = true;
						break;
					case SdlScancode.Return:
						_nes.Controllers.Controller1.ButtonStart = true;
						break;
					case SdlScancode.Up:
						_nes.Controllers.Controller1.ButtonUp = true;
						break;
					case SdlScancode.Down:
						_nes.Controllers.Controller1.ButtonDown = true;
						break;
					case SdlScancode.Left:
						_nes.Controllers.Controller1.ButtonLeft = true;
						break;
					case SdlScancode.Right:
						_nes.Controllers.Controller1.ButtonRight = true;
						break;
				}
				break;
			case SdlEventType.KeyUp:
				switch (sdlEvent.Key.Scancode)
				{
					case SdlScancode.A:
						_nes.Controllers.Controller1.ButtonA = false;
						break;
					case SdlScancode.S:
						_nes.Controllers.Controller1.ButtonB = false;
						break;
					case SdlScancode.RightShift:
						_nes.Controllers.Controller1.ButtonSelect = false;
						break;
					case SdlScancode.Return:
						_nes.Controllers.Controller1.ButtonStart = false;
						break;
					case SdlScancode.Up:
						_nes.Controllers.Controller1.ButtonUp = false;
						break;
					case SdlScancode.Down:
						_nes.Controllers.Controller1.ButtonDown = false;
						break;
					case SdlScancode.Left:
						_nes.Controllers.Controller1.ButtonLeft = false;
						break;
					case SdlScancode.Right:
						_nes.Controllers.Controller1.ButtonRight = false;
						break;
				}
				break;
		}
		return SdlAppResult.Continue;
	}

	protected override void Quit(SdlAppResult result)
	{
		_nes.Stop();

		_mainDisplay.Destroy();
	}
}
