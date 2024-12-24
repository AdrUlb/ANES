namespace ANES;

internal sealed class CpuMemoryBus(Nes nes)
{
	// TODO: Memory bus does not just consist of RAM


	public byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		var value = nes.Cartridge.CpuReadByte(address, suppressSideEffects);

		value &= address switch
		{
			< 0x0800 => nes.Ram[address],
			< 0x1000 => nes.Ram[address - 0x0800],
			< 0x1800 => nes.Ram[address - 0x1000],
			< 0x2000 => nes.Ram[address - 0x1800],
			_ => 0xFF
		};

		return value;
	}

	public void WriteByte(ushort address, byte value)
	{
		nes.Cartridge.CpuWriteByte(address, value);

		switch (address)
		{
			case < 0x0800: nes.Ram[address] = value; break;
			case < 0x1000: nes.Ram[address - 0x0800] = value; break;
			case < 0x1800: nes.Ram[address - 0x1000] = value; break;
			case < 0x2000: nes.Ram[address - 0x1800] = value; break;
		}
	}
}
