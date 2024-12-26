namespace ANES.Mappers;

internal sealed class Mapper0 : IMapper
{
	private readonly Nes _nes;

	private readonly byte[] _prgRom;
	private const int _prgBank1Offset = 0;
	private readonly int _prgBank2Offset;

	private readonly byte[] _chrRom;

	private readonly byte[][] _nametables = [new byte[0x400], new byte[0x400]];
	private readonly int _nt1Offset;
	private readonly int _nt4Offset;
	private readonly int _nt2Offset;
	private readonly int _nt3Offset;

	public string Name { get; } = "NROM";

	public Mapper0(Nes nes, byte[] prgRom, byte[] chrRom, NametableLayout nametableLayout)
	{
		_nes = nes;
		_prgRom = prgRom;
		_chrRom = chrRom;

		_prgBank2Offset = prgRom.Length switch
		{
			16 * 1024 => 0,
			32 * 1024 => 16 * 1024,
			_ => throw new FormatException("NROM only supports 16K or 32K PRG ROM.")
		};

		if (chrRom.Length != 8 * 1024)
			throw new FormatException("NROM only supports 8K CHR ROM.");

		switch (nametableLayout)
		{
			case NametableLayout.MirrorHorizonally:
				_nt1Offset = 0x000;
				_nt2Offset = 0x000;
				_nt3Offset = 0x400;
				_nt4Offset = 0x400;
				break;
			case NametableLayout.MirrorVertically:
				_nt1Offset = 0x000;
				_nt2Offset = 0x400;
				_nt3Offset = 0x000;
				_nt4Offset = 0x400;
				break;
		}
	}

	public byte CpuReadByte(ushort address, bool suppressSideEffects = false) => address switch
	{
		>= 0x8000 and <= 0xBFFF => _prgRom[address - 0x8000 + _prgBank1Offset],
		>= 0xC000 and <= 0xFFFF => _prgRom[address - 0xC000 + _prgBank2Offset],
		_ => 0xFF
	};

	public void CpuWriteByte(ushort address, byte value) { }

	public byte PpuReadByte(ushort address, bool suppressSideEffects = false)
	{
		if (address is >= 0x3000 and <= 0x3EFF)
			address -= 0x1000;

		return address switch
		{
			< 0x2000 => _chrRom[address],
			< 0x2400 => _nes.Vram[address - 0x2000 + _nt1Offset],
			< 0x2800 => _nes.Vram[address - 0x2400 + _nt2Offset],
			< 0x2C00 => _nes.Vram[address - 0x2800 + _nt3Offset],
			< 0x3000 => _nes.Vram[address - 0x2C00 + _nt4Offset],
			_ => 0xFF
		};
	}

	public void PpuWriteByte(ushort address, byte value)
	{
		if (address is >= 0x3000 and <= 0x3EFF)
			address -= 0x1000;

		switch (address)
		{
			case < 0x2000: break;
			case < 0x2400: _nes.Vram[address - 0x2000 + _nt1Offset] = value; break;
			case < 0x2800: _nes.Vram[address - 0x2400 + _nt2Offset] = value; break;
			case < 0x2C00: _nes.Vram[address - 0x2800 + _nt3Offset] = value; break;
			case < 0x3000: _nes.Vram[address - 0x2C00 + _nt4Offset] = value; break;
		}
	}
}
