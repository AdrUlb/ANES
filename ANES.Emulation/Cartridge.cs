using ANES.Emulation.Mappers;

namespace ANES.Emulation;

public sealed class Cartridge
{
	public readonly IMapper _mapper;

	public Cartridge(Nes nes, string romFilePath)
	{
		Console.WriteLine($"Loading ROM file: {romFilePath}");
		ReadOnlySpan<byte> headerMagic = [(byte)'N', (byte)'E', (byte)'S', 0x1A];
		using var fs = File.OpenRead(romFilePath);

		Span<byte> magic = stackalloc byte[headerMagic.Length];
		fs.ReadExactly(magic);

		if (!magic.SequenceEqual(headerMagic))
			throw new FormatException("ROM file is not in iNES or NES 2.0 format.");

		var b4 = fs.ReadByte();
		var b5 = fs.ReadByte();
		var b6 = fs.ReadByte();
		var b7 = fs.ReadByte();
		var b8 = fs.ReadByte();
		var b9 = fs.ReadByte();
		var b10 = fs.ReadByte();
		var b11 = fs.ReadByte();
		var b12 = fs.ReadByte();
		var b13 = fs.ReadByte();
		var b14 = fs.ReadByte();
		var b15 = fs.ReadByte();

		var isNes20 = ((b7 >> 2) & 0b11) == 0b10;

		int prgRomBytes;
		int prgRamBytes;
		int prgNvramBytes;

		// NES 2.0: The PRG-ROM Area follows the 16-byte Header and the Trainer Area and precedes the CHR-ROM Area.
		//  Header byte 4 (LSB) and bits 0-3 of Header byte 9 (MSB) together specify its size.
		if (isNes20)
		{
			var msb = b9 & 0b1111;

			//  If the MSB nibble is $0-E, LSB and MSB together simply specify the PRG-ROM size in 16 KiB units:
			if (msb != 0xF)
			{
				prgRomBytes = 16 * 1024 * (b4 | (msb << 8));
			}
			else // If the MSB nibble is $F, an exponent-multiplier notation is used:
			{
				var mm = b4 & 0b11;
				var e = (b4 >> 2) & 0b1111_11;

				prgRomBytes = (1 << e) * (mm * 2 + 1);
			}

			var ramShiftCount = b10 & 0b1111;
			var nvramShiftCount = (b10 >> 4) & 0b1111;

			prgRamBytes = ramShiftCount != 0 ? 64 << ramShiftCount : 0;
			prgNvramBytes = nvramShiftCount != 0 ? 64 << nvramShiftCount : 0;
		}
		else // iNES: b4 = Size of PRG ROM in 16 KB units
		{
			prgRomBytes = 16 * 1024 * b4;

			// No PRG RAM is assumed for legacy iNES!!!
			prgRamBytes = 0;
			prgNvramBytes = 0;
		}

		if (prgRamBytes != 0)
			throw new NotSupportedException();

		if (prgNvramBytes != 0)
			throw new NotSupportedException();

		int chrRomBytes;
		int chrRamBytes = 0;
		int chrNvramBytes = 0;

		// NES 2.0: The CHR-ROM Area, if present, follows the Trainer and PRG-ROM Areas and precedes the Miscellaneous ROM Area.
		// Header byte 5 (LSB) and bits 4-7 of Header byte 9 (MSB) specify its size.
		if (isNes20)
		{
			var msb = (b9 >> 4) & 0b1111;

			//  If the MSB nibble is $0-E, LSB and MSB together simply specify the CHR-ROM size in 8 KiB units:
			if (msb != 0xF)
			{
				chrRomBytes = 8 * 1024 * (b5 | (msb << 8));
			}
			else // If the MSB nibble is $F, an exponent-multiplier notation is used:
			{
				var mm = b5 & 0b11;
				var e = (b5 >> 2) & 0b1111_11;

				chrRomBytes = (1 << e) * (mm * 2 + 1);
			}

			var ramShiftCount = b11 & 0b1111;
			var nvramShiftCount = (b11 >> 4) & 0b1111;

			chrRamBytes = ramShiftCount != 0 ? 64 << ramShiftCount : 0;
			chrNvramBytes = nvramShiftCount != 0 ? 64 << nvramShiftCount : 0;
		}
		else // iNES: b5 = Size of CHR ROM in 8 KB units (value 0 means the board uses CHR RAM)
		{
			chrRomBytes = 8 * 1024 * b5;

			if (b5 == 0)
			{
				chrRamBytes = 8 * 1024;
				chrRomBytes = 0;
			}
		}

		if (chrNvramBytes != 0)
			throw new NotSupportedException();

		var nametableLayout = (b6 & 1) == 0 ? NametableLayout.MirrorHorizonally : NametableLayout.MirrorVertically;

		var hasBattery = ((b6 >> 1) & 1) != 0;
		if (hasBattery)
			throw new NotSupportedException();

		var hasTrainer = ((b6 >> 2) & 1) != 0;
		if (hasTrainer)
			throw new NotSupportedException();

		var alternativeNametables = ((b6 >> 3) & 1) != 0;
		if (alternativeNametables)
			throw new NotSupportedException();

		var consoleType = (ConsoleType)(b7 & 0b11);
		if (consoleType != ConsoleType.Famicom)
			throw new NotSupportedException();

		var mapperNumber = ((b6 >> 4) & 0xF) | (b7 & 0xF0);
		var submapperNumber = -1;

		if (isNes20)
		{
			mapperNumber |= b8 & 0b1111;
			submapperNumber = (b8 >> 4) & 0b1111;
		}

		var prgRom = new byte[prgRomBytes];
		var chrRom = new byte[chrRomBytes];
		fs.ReadExactly(prgRom);
		fs.ReadExactly(chrRom);

		Console.WriteLine($"ROM Format: {(isNes20 ? "NES 2.0" : "iNES")}");
		Console.WriteLine($"PRG ROM: {prgRomBytes} bytes");
		Console.WriteLine($"CHR ROM: {chrRomBytes} bytes");
		Console.WriteLine($"CHR RAM: {chrRamBytes} bytes");
		Console.WriteLine($"Nametable layout: {nametableLayout}");

		_mapper = mapperNumber switch
		{
			0 => new Mapper0(nes, prgRom, chrRom, chrRamBytes, nametableLayout),
			_ => throw new NotSupportedException($"Mapper number {mapperNumber} is not supported.")
		};

		Console.WriteLine($"Mapper: {_mapper.Name}");
	}

	public byte CpuReadByte(ushort address, bool suppressSideEffects = false) => _mapper.CpuReadByte(address, suppressSideEffects);
	public void CpuWriteByte(ushort address, byte value) => _mapper.CpuWriteByte(address, value);
	public byte PpuReadByte(ushort address, bool suppressSideEffects = false) => _mapper.PpuReadByte(address, suppressSideEffects);
	public void PpuWriteByte(ushort address, byte value) => _mapper.PpuWriteByte(address, value);
}
