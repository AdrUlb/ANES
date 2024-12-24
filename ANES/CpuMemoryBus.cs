namespace ANES;

internal sealed class CpuMemoryBus
{
	// TODO: Memory bus does not just consist of RAM
	private readonly byte[] _ram = new byte[0xFFFF + 1];

	public CpuMemoryBus()
	{
		// TODO: Do not copy test ROM
		var rom = File.ReadAllBytes("Tests/nestest/nestest.nes");
		rom[16..(16 + 16 * 1024)].CopyTo(_ram, 0x8000);
		rom[16..(16 + 16 * 1024)].CopyTo(_ram, 0xC000);
		_ram[0xFFFC] = 0;
	}

	public byte ReadByte(ushort address, bool suppressSideEffects = false)
	{
		return _ram[address];
	}

	public void WriteByte(ushort address, byte value)
	{
		_ram[address] = value;
	}

	public ushort ReadWord(ushort address)
	{
		var lo = ReadByte(address++);
		var hi = ReadByte(address);
		return (ushort)(lo | (hi << 8));
	}
}
