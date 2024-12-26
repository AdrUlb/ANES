using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp;

public readonly record struct SdlWindowId
{
	private readonly uint _value;

	internal SdlWindowId(uint value) => _value = value;
}
