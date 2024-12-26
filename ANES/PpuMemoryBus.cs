namespace ANES;

internal sealed class PpuMemoryBus(Nes nes) : IMemoryBus
{
	public byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		var value = nes.Cartridge.PpuReadByte(address, suppressSideEffects);

		value &= address switch
		{
			_ => 0xFF
		};

		return value;
	}

	public void WriteByte(ushort address, byte value)
	{
		nes.Cartridge.PpuWriteByte(address, value);

		switch (address) { }
	}
}
