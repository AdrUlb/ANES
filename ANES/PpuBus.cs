namespace ANES;

internal sealed class PpuBus(Nes nes) : IMemoryBus
{
	public byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		// The PPU addresses a 14-bit (16kB) address space, $0000-$3FFF, completely separate from the CPU's address bus.
		address %= 0x4000;

		// $3F00-3FFF is not configurable, always mapped to the internal palette control.
		if (address >= 0x3F00)
		{
			address -= 0x3F00;
			address %= 0x20;
			return nes.PaletteRam[address];
		}

		return nes.Cartridge.PpuReadByte(address, suppressSideEffects);
	}

	public void WriteByte(ushort address, byte value)
	{
		// The PPU addresses a 14-bit (16kB) address space, $0000-$3FFF, completely separate from the CPU's address bus.
		address %= 0x4000;

		// $3F00-3FFF is not configurable, always mapped to the internal palette control.
		if (address >= 0x3F00)
		{
			address -= 0x3F00;
			address %= 0x20;
			nes.PaletteRam[address] = value;
		}

		nes.Cartridge.PpuWriteByte(address, value);
	}
}
