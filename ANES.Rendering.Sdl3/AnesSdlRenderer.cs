using ANES.Emulation;
using Sdl3Sharp;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ANES.Rendering.Sdl3;

public sealed class AnesSdlRenderer : IDisposable
{
	private readonly Nes _nes;
	private const bool _palResolution = true;
	private const int _screenOffsetTop = _palResolution ? 0 : 8;
	public const int ScreenWidth = 256;
	public const int ScreenHeight = _palResolution ? 240 : 224;

	private readonly SdlRenderer _renderer;
	private SdlTexture _screen = null!;
	private volatile bool _waitingForFrame = true;
	private volatile bool _disposed = false;


	public AnesSdlRenderer(Nes nes, SdlRenderer renderer)
	{
		_nes = nes;
		_renderer = renderer;

		var tilesTexProps = SdlProperties.Create();
		tilesTexProps.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		tilesTexProps.Set(SdlProperties.TextureCreateWidth, Ppu.PictureWidth);
		tilesTexProps.Set(SdlProperties.TextureCreateHeight, Ppu.PictureHeight);
		tilesTexProps.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_screen = SdlTexture.CreateWithProperties(_renderer, tilesTexProps);
		_screen.SetScaleMode(SdlScaleMode.Nearest);
		tilesTexProps.Destroy();

		_nes.FrameReady += OnFrameReady;
	}

	private void OnFrameReady(object? sender, EventArgs e)
	{
		_waitingForFrame = false;
		Nes.WaitUntil(FrameRendered);

		bool FrameRendered() => _waitingForFrame || _disposed;
	}

	public void Render()
	{
		Nes.WaitUntil(FrameReady);

		if (_disposed)
			return;

		var surface = _screen.LockToSurface();
		for (var y = 0; y < Ppu.PictureHeight; y++)
		{
			var row = surface.GetPixels<int>(y);
			for (var x = 0; x < ScreenWidth; x++)
				row[x] = _nes.Ppu.Picture[x + y * Ppu.PictureWidth].ToArgb();
		}
		_screen.Unlock();

		var srcRect = new RectangleF(0, _screenOffsetTop, ScreenWidth, ScreenHeight);

		_screen.Render(srcRect, RectangleF.Empty);

		_waitingForFrame = true;

		bool FrameReady() => !_waitingForFrame || _disposed;
	}

	public static void SetRuntimeImportResolver() => NativeLibrary.SetDllImportResolver(typeof(SdlApp).Assembly, SdlImportResolver);

	private static nint SdlImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName != "SDL3")
			return 0;

		var (ridOs, libPrefix, libSuffix) =
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ("win", "", ".dll") :
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ("linux", "lib", ".so") :
			throw new PlatformNotSupportedException("Operating system is not supported.");

		var ridArch = RuntimeInformation.ProcessArchitecture switch
		{
			Architecture.X64 => "x64",
			Architecture.X86 => "x86",
			_ => throw new PlatformNotSupportedException("Architecture is not supported.")
		};

		var rid = ridOs + '-' + ridArch;

		var libFilePath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", $"{libPrefix}SDL3{libSuffix}");

		return File.Exists(libFilePath) ? NativeLibrary.Load(libFilePath) : 0;
	}

	public void Dispose()
	{
		_disposed = true;
		_screen.Destroy();
	}
}
