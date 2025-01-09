using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal readonly struct MainDisplay
{
	private const int _scale = 3;

	private const int _screenWidth = 256;
	private const int _screenHeight = 224;

	private readonly Nes _nes;

	private readonly SdlWindow _window;
	private readonly SdlRenderer _renderer;
	private readonly SdlTexture _texture;

	public MainDisplay() => throw new InvalidOperationException();

	public MainDisplay(Nes nes)
	{
		_nes = nes;
		(_window, _renderer) = SdlWindow.CreateWithRenderer(
			"Adrian's NES Emulator",
			_screenWidth * _scale,
			_screenHeight * _scale,
			SdlWindowFlags.Resizable
		);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, _screenWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, _screenHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_texture = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		_texture.SetScaleMode(SdlScaleMode.Nearest);
		tilesTexProps.Destroy();
	}

	public void Iterate()
	{
		var surface = _texture.LockToSurface();

		for (var y = 0; y < _screenHeight; y++)
		{
			var row = surface.GetPixels<int>(y);
			for (var x = 0; x < _screenWidth; x++)
				row[x] = _nes.Ppu.Picture[x + (y + 8) * Ppu.PictureWidth].ToArgb();
		}

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
