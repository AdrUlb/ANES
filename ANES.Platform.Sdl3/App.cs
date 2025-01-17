using ANES.Emulation;
using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.Drawing;

namespace ANES.Platform.Sdl3;

internal sealed class App
{
	private const int _scale = 2;

	private readonly Nes _nes = new();

	private readonly SdlWindow _window;
	private readonly SdlRenderer _renderer;
	private readonly AnesSdlRenderer _anesRenderer;

	private volatile bool _render = false;
	private volatile bool _quit = false;

	internal App()
	{
		var width = AnesSdlRenderer.ScreenWidth * _scale;
		var height = AnesSdlRenderer.ScreenHeight * _scale;
		(_window, _renderer) = SdlWindow.CreateWithRenderer(
			"ANES",
			width,
			height,
			SdlWindowFlags.Resizable
		);
		_anesRenderer = new(_nes, _renderer);

		_nes.Start();
		_nes.InsertCartridge("/mnt/ssd_1tb/Roms/NES/smb1.nes");
		_nes.Reset();

		_nes.Ppu.Frame += OnFrameReady;
	}

	private void OnFrameReady(object? sender, EventArgs e)
	{
		_render = true;

		while (_render && !_quit) { }
	}

	private void Render()
	{
		if (!_render)
			return;

		_renderer.SetDrawColor(Color.Black);
		_renderer.Clear();
		_anesRenderer.Render();
		_render = false;
		_renderer.Present();
	}

	private void HandleEvent(SdlEvent sdlEvent)
	{
		switch (sdlEvent.Type)
		{
			case SdlEventType.WindowCloseRequested:
				_quit = true;
				break;
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
	}

	public void Run()
	{
		while (!_quit)
		{
			Render();

			while (Sdl.PollEvent(out var sdlEvent))
				HandleEvent(sdlEvent);
		}

		_nes.Stop();
		_anesRenderer.Dispose();
		_renderer.Destroy();
		_window.Destroy();
	}
}
