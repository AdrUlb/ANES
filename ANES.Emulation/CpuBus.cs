namespace ANES.Emulation;

public sealed class CpuBus(Nes nes) : MemoryBus
{
	public override byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		var value = nes.Cartridge?.CpuReadByte(address, suppressSideEffects) ?? 0xFF;

		value &= address switch
		{
			< 0x0800 => nes.Ram[address],
			< 0x1000 => nes.Ram[address - 0x0800],
			< 0x1800 => nes.Ram[address - 0x1000],
			< 0x2000 => nes.Ram[address - 0x1800],
			< 0x4000 => nes.Ppu.ReadReg((address - 0x2000) % 8, suppressSideEffects),
			0x4016 => nes.Controllers.ReadData1(),
			0x4017 => nes.Controllers.ReadData2(),
			_ => 0xFF
		};

		return value;
	}

	public override void WriteByte(ushort address, byte value)
	{
		nes.Cartridge?.CpuWriteByte(address, value);

		switch (address)
		{
			case < 0x0800: nes.Ram[address] = value; break;
			case < 0x1000: nes.Ram[address - 0x0800] = value; break;
			case < 0x1800: nes.Ram[address - 0x1000] = value; break;
			case < 0x2000: nes.Ram[address - 0x1800] = value; break;
			case < 0x4000: nes.Ppu.WriteReg((address - 0x2000) % 8, value); break;
			case 0x4014: nes.BeginOamDma(value); break;
			case 0x4016: nes.Controllers.WriteStrobe(value); break;
		}
	}
}
