using ANES;
using Sdl3Sharp;
using System.Runtime.InteropServices;

#if MY_SET_DLLIMPORTRESOLVER
NativeLibrary.SetDllImportResolver(typeof(Sdl).Assembly, (name, _, _) =>
{
	if (name != "SDL3")
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

	if (!File.Exists(libFilePath))
		return 0;

	return NativeLibrary.Load(libFilePath);
});
#endif

var nes = new Nes();
nes.Start();

if (!Sdl.Init(SdlInitFlags.Video))
	throw new Exception($"Failed to initialize SDL: {Sdl.GetError()}.");

if (!Sdl.CreateWindowAndRenderer("test", 800, 600, SdlWindowFlags.Resizable, out var window, out var renderer))
	throw new Exception($"Failed to create SDL window and renderer: {Sdl.GetError()}.");

while (true)
{
	
}

Sdl.Quit();
