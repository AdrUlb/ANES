using Sdl3Sharp;
using System.Diagnostics;
using System.Drawing;

namespace ANES;

public sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private const int _scale = 3;
	private const bool _palResolution = true;

	private const int _screenWidth = 256;
	private const int _screenHeight = _palResolution ? 240 : 224;
	private const int _screenOffsetTop = _palResolution ? 0 : 8;

	private readonly Nes _nes = new();
	private SdlWindow _window = null!;
	private SdlRenderer _renderer = null!;
	private SdlTexture _screen = null!;

	private volatile bool _quit = false;
	private volatile bool _waitingForVblank = true;

	public static int Test = 0;

	private void VblankHandler(object? sender, EventArgs e)
	{
		_waitingForVblank = false;

		while (!_waitingForVblank && !_quit) { }
	}

	protected override SdlAppResult Init()
	{
		(_window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES",
			_screenWidth * _scale,
			_screenHeight * _scale,
			SdlWindowFlags.Resizable
		);

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, Ppu.PictureWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, Ppu.PictureHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_screen = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		_screen.SetScaleMode(SdlScaleMode.Nearest);
		tilesTexProps.Destroy();

		_nes.Vblank += VblankHandler;

		_nes.Start();
		_nes.InsertCartridge(@"C:\Stuff\Roms\nes\donkeykong.nes");
		_nes.Reset();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		while (_waitingForVblank && !_quit) { }

		var surface = _screen.LockToSurface();
		for (var y = 0; y < Ppu.PictureHeight; y++)
		{
			var row = surface.GetPixels<int>(y);
			for (var x = 0; x < _screenWidth; x++)
				row[x] = _nes.Ppu.Picture[x + y * Ppu.PictureWidth].ToArgb();
		}
		_screen.Unlock();

		var srcRect = new RectangleF(0, _screenOffsetTop, _screenWidth, _screenHeight);

		_renderer.Clear();
		_screen.Render(srcRect, RectangleF.Empty);
		_renderer.Present();

		_waitingForVblank = true;
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
						_nes.Controllers.Controller1.ButtonB = true;
						break;
					case SdlScancode.S:
						_nes.Controllers.Controller1.ButtonA = true;
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
					case SdlScancode.Space:
						Test++;
						break;
				}
				break;
			case SdlEventType.KeyUp:
				switch (sdlEvent.Key.Scancode)
				{
					case SdlScancode.A:
						_nes.Controllers.Controller1.ButtonB = false;
						break;
					case SdlScancode.S:
						_nes.Controllers.Controller1.ButtonA = false;
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
		_quit = true;

		_nes.Stop();
		_nes.Vblank -= VblankHandler;

		_screen.Destroy();
		_renderer.Destroy();
		_window.Destroy();
	}
}
