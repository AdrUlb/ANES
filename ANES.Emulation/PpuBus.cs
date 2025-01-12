namespace ANES.Emulation;

// https://www.nesdev.org/wiki/PPU_memory_map

public sealed class PpuBus(Nes nes) : MemoryBus
{
	public override byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		// The PPU addresses a 14-bit (16kB) address space, $0000-$3FFF, completely separate from the CPU's address bus.
		address %= 0x4000;

		if (address >= 0x3F00)
		{
			// $3F00-3FFF is not configurable, always mapped to the internal palette control.
			address -= 0x3F00;
			// Palette RAM as a whole is also mirrored through the entire $3F00-$3FFF region.
			address %= 0x20;
			// The backdrop color can be written through both $3F00 and $3F10.
			if (address == 0x10)
				address = 0;

			return nes.PaletteRam[address];
		}

		return nes.Cartridge?.PpuReadByte(address, suppressSideEffects) ?? 0xFF;
	}

	public override void WriteByte(ushort address, byte value)
	{
		// The PPU addresses a 14-bit (16kB) address space, $0000-$3FFF, completely separate from the CPU's address bus.
		if (address >= 0x4000)
		{
			Console.WriteLine($"{address:X4} = {value:X2}");
		}
		
		address %= 0x4000;

		// $3F00-3FFF is not configurable, always mapped to the internal palette control.
		if (address >= 0x3F00)
		{
			address -= 0x3F00;
			// Palette RAM as a whole is also mirrored through the entire $3F00-$3FFF region.
			address %= 0x20;
			// The backdrop color can be written through both $3F00 and $3F10.
			if (address == 0x10)
				address = 0;

			nes.PaletteRam[address] = value;
		}

		nes.Cartridge?.PpuWriteByte(address, value);

	}
}
