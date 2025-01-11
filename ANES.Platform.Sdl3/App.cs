using ANES.Rendering.Sdl3;
using Sdl3Sharp;

namespace ANES.Platform.Sdl3;

internal sealed class App() : SdlApp(SdlInitFlags.Video)
{
	private const int _scale = 3;

	private readonly Nes _nes = new();

	private SdlWindow _window = null!;
	private SdlRenderer _renderer = null!;
	private AnesSdlRenderer _anesRenderer = null!;

	private volatile bool _waitingForFrame = true;

	protected override SdlAppResult Init()
	{
		(_window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES",
			AnesSdlRenderer.ScreenWidth * _scale,
			AnesSdlRenderer.ScreenHeight * _scale,
			SdlWindowFlags.Resizable
		);
		_anesRenderer = new(_nes, _renderer);

		_nes.Start();
		_nes.InsertCartridge(@"C:\Stuff\Roms\nes\pacman.nes");
		_nes.Reset();

		return SdlAppResult.Continue;
	}

	protected override SdlAppResult Iterate()
	{
		_renderer.Clear();
		_waitingForFrame = false;
		_anesRenderer.Render();
		_renderer.Present();

		_waitingForFrame = true;
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
		SpinWait.SpinUntil(() => _waitingForFrame);

		_anesRenderer.Dispose();
		_renderer.Destroy();
		_window.Destroy();

		_nes.Stop();
	}
}
