using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal sealed class Ppu(Nes nes)
{
	internal const int ScreenWidth = 256;
	internal const int ScreenHeight = 240;

	// TODO: set flag correctly
	private bool _statusVblank = true;
	private bool _statusSprite0 = false;
	private bool _statusSpriteOverflow = false;

	private int _ctrlBaseNametable = 0;
	private bool _ctrlVramIncrement32 = false;
	private bool _ctrlSpritePatternTable = false;
	private bool _ctrlBackgroundPatternTable = false;
	private bool _ctrlSpriteSize = false;
	private bool _ctrlMasterSlaveSelect = false;
	private bool _ctrlVblankNmiEnable = false;

	private ulong _dot = 0;

	private bool _raiseNmi = false;
	private byte _latchValue = 0;

	// TODO: implement these properly
	private ushort _ppuAddress = 0;
	private ushort _oamAddr = 0;
	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	internal Color[] Pixels = new Color[ScreenWidth * ScreenHeight];

	public void Tick()
	{
		_dot++;

		// TODO: Implement this properly, this is obviously a BAD hack
		if (_dot == 340 * 260)
		{
			for (var tileY = 0; tileY < 240 / 8; tileY++)
			{
				for (var tileX = 0; tileX < 256 / 8; tileX++)
				{
					var nametableBase = _ctrlBaseNametable switch
					{
						0 => 0x2000,
						1 => 0x2400,
						2 => 0x2800,
						3 => 0x2C00,
						_ => throw new UnreachableException()
					};
					var addr = nametableBase + tileX + tileY * (256 / 8);
					var tileIndex = nes.PpuMemoryBus.ReadByte((ushort)addr, true);
					var attributeAddress = 0x23C0 | (_ctrlBaseNametable << 10) | (tileY / 4 * 8 + tileX / 4);
					var attributeByte = nes.PpuMemoryBus.ReadByte((ushort)attributeAddress);
					var attribute = (tileX % 4 / 2, tileY % 4 / 2) switch
					{
						(0, 0) => attributeByte & 0b11,
						(1, 0) => (attributeByte >> 2) & 0b11,
						(0, 1) => (attributeByte >> 4) & 0b11,
						(1, 1) => (attributeByte >> 6) & 0b11,
						_ => throw new UnreachableException()
					};


					var surfX = tileX * 8;
					var surfY = tileY * 8;

					for (var y = 0; y < 8; y++)
					{
						var plane0Index = (_ctrlBackgroundPatternTable ? 1 << 12 : 0) | (tileIndex << 4) | y;
						var plane1Index = (_ctrlBackgroundPatternTable ? 1 << 12 : 0) | (tileIndex << 4) | y | (1 << 3);

						var plane0 = nes.PpuMemoryBus.ReadByte((ushort)plane0Index, true);
						var plane1 = nes.PpuMemoryBus.ReadByte((ushort)plane1Index, true);

						for (var x = 0; x < 8; x++)
						{
							var bit0 = (plane0 >> (8 - 1 - x)) & 1;
							var bit1 = (plane1 >> (8 - 1 - x)) & 1;
							var colorIndex = (bit0 << 1) | bit1;

							var color = Color.FromArgb(255 * colorIndex / 3, 255 * colorIndex / 3, 255 * colorIndex / 3);

							//var r = _palette[attribute * 3 + 0];
							//var g = _palette[attribute * 3 + 1];
							//var b = _palette[attribute * 3 + 2];

							var xx = x + surfX;
							var yy = y + surfY;
							var index = xx + yy * ScreenWidth;

							var paletteOffset = colorIndex switch
							{
								0 => nes.PpuMemoryBus.ReadByte(0x3F00),
								1 => nes.PpuMemoryBus.ReadByte((ushort)(0x3F00 | (4 * attribute + 1))),
								2 => nes.PpuMemoryBus.ReadByte((ushort)(0x3F00 | (4 * attribute + 2))),
								3 => nes.PpuMemoryBus.ReadByte((ushort)(0x3F00 | (4 * attribute + 3))),
								_ => throw new UnreachableException()
							};

							Pixels[index] = Color.FromArgb(_palette[paletteOffset * 3 + 0], _palette[paletteOffset * 3 + 1], _palette[paletteOffset * 3 + 2]);
						}
					}
				}
			}

			if (_ctrlVblankNmiEnable)
				_raiseNmi = true;

			_dot = 0;
		}
	}

	public bool HandleNmi()
	{
		if (!_raiseNmi)
			return false;

		_raiseNmi = false;
		return true;
	}

	public byte ReadReg(int index, bool suppressSideEffects = false)
	{
		var ret = _latchValue;

		switch (index)
		{
			case 2: // PPUSTATUS
				// TODO: Reading this register has the side effect of clearing the PPU's internal w register.
				// TODO: PPU OPEN BUS
				ret = (byte)(
					(_statusVblank ? 1 << 7 : 0) |
					(_statusSprite0 ? 1 << 6 : 0) |
					(_statusSpriteOverflow ? 1 << 5 : 0)
				);
				break;
			case 4: // OAMDATA
				// TODO
				break;
			case 7: // PPUDATA
				// TODO: After access, the video memory address will increment by an amount determined by bit 2 of $2000.
				// TODO: VRAM reading and writing shares the same internal address register that rendering uses.
				ret = nes.PpuMemoryBus.ReadByte(_ppuAddress);
				if (_ctrlVramIncrement32)
					_ppuAddress += 32;
				else
					_ppuAddress++;
				break;
		}

		return ret;
	}

	public void WriteReg(int index, byte value)
	{
		_latchValue = value;

		switch (index)
		{
			case 0: // PPUCTRL
				/* TODO: Changing NMI enable from 0 to 1 while the vblank flag in PPUSTATUS is 1 will immediately trigger an NMI.
					This happens during vblank if the PPUSTATUS register has not yet been read.
					It can result in graphical glitches by making the NMI routine execute too late in vblank to finish on time, or cause the game to handle more frames than have actually occurred.
					To avoid this problem, it is prudent to read PPUSTATUS first to clear the vblank flag before enabling NMI in PPUCTRL. */
				_ctrlBaseNametable = value & 0b11;
				_ctrlVramIncrement32 = ((value >> 2) & 1) != 0;
				_ctrlSpritePatternTable = ((value >> 3) & 1) != 0;
				_ctrlBackgroundPatternTable = ((value >> 4) & 1) != 0;
				_ctrlSpriteSize = ((value >> 5) & 1) != 0;
				_ctrlMasterSlaveSelect = ((value >> 6) & 1) != 0;
				_ctrlVblankNmiEnable = ((value >> 7) & 1) != 0;
				break;
			case 1: // PPUMASK
				// TODO
				break;
			case 3: // OAMADDR
				// TODO
				break;
			case 4: // OAMDATA
				// TODO
				break;
			case 5: // PPUSCROLL
				// TODO
				break;
			case 6: // PPUADDR
				/* TODO: Whether this is the first or second write is tracked by the PPU's internal w register, which is shared with PPUSCROLL.
					If w is not 0 or its state is not known, it must be cleared by reading PPUSTATUS before writing the address. */
				_ppuAddress <<= 8;
				_ppuAddress |= value;
				break;
			case 7: // PPUDATA
				// TODO: After access, the video memory address will increment by an amount determined by bit 2 of $2000.
				// TODO: VRAM reading and writing shares the same internal address register that rendering uses.
				nes.PpuMemoryBus.WriteByte(_ppuAddress, value);
				if (_ctrlVramIncrement32)
					_ppuAddress += 32;
				else
					_ppuAddress++;
				break;
		}
	}
}
