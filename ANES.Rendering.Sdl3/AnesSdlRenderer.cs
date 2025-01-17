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
	private readonly AnesSdlPixels _screen;
	private volatile bool _disposed = false;
	public bool PauseRendering = false;

	public AnesSdlRenderer(Nes nes, SdlRenderer renderer)
	{
		_nes = nes;
		_renderer = renderer;

		_screen = new(Ppu.PictureWidth, Ppu.PictureHeight, _renderer);
	}

	public void Render()
	{
		if (_disposed)
			return;

		if (PauseRendering)
			return;

		for (var y = 0; y < Ppu.PictureHeight; y++)
		{
			var row = _screen.GetRowSpan<int>(y);
			for (var x = 0; x < ScreenWidth; x++)
				row[x] = _nes.Ppu.Picture[x + y * Ppu.PictureWidth].ToArgb();
		}

		var srcRect = new RectangleF(0, _screenOffsetTop, ScreenWidth, ScreenHeight);

		_screen.Render(srcRect, RectangleF.Empty);
	}

	public static void SetRuntimeImportResolver() => NativeLibrary.SetDllImportResolver(typeof(Sdl).Assembly, SdlImportResolver);

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
		_screen.Dispose();
	}
}
