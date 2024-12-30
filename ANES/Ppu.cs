using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal sealed class Ppu(Nes nes)
{
	internal const int ScreenWidth = 256;
	internal const int ScreenHeight = 224;

	// TODO: set flag correctly
	private bool _statusVblank = true;
	private bool _statusSprite0 = false;
	private bool _statusSpriteOverflow = false;

	private bool _ctrlVramIncrement32 = false;
	private bool _ctrlSpritePatternTable = false;
	private bool _ctrlBackgroundPatternTable = false;
	private bool _ctrlSpriteSize = false;
	private bool _ctrlMasterSlaveSelect = false;
	private bool _ctrlVblankNmiEnable = false;

	private ulong _dot = 0;

	private bool _raiseNmi = false;
	private byte _latchValue = 0;

	private ushort _regV;
	private ushort _regT;
	private byte _regX;
	private bool _regW;

	// TODO: implement these properly
	private ushort _oamAddr = 0;
	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	internal readonly Color[] Picture = new Color[ScreenWidth * ScreenHeight];

	public void Tick()
	{
		_dot++;

		// TODO: Implement this properly, this is obviously a BAD hack
		if (_dot == 340 * 260)
		{
			var ctrlBaseNametable = (_regT >> 10) & 0b11;
			var nametableBase = ctrlBaseNametable switch
			{
				0 => 0x2000,
				1 => 0x2400,
				2 => 0x2800,
				3 => 0x2C00,
				_ => throw new UnreachableException()
			};

			for (var tileY = 0; tileY < ScreenHeight / 8; tileY++)
			{
				for (var tileX = 0; tileX < ScreenWidth / 8; tileX++)
				{
					var nametablePatternAddress = nametableBase + tileX + tileY * (ScreenWidth / 8);
					var attributeAddress = 0x23C0 | (ctrlBaseNametable << 10) | (tileY / 4 * 8 + tileX / 4);
					var nametablePattern = nes.PpuMemoryBus.ReadByte((ushort)nametablePatternAddress, true);

					var attribute = nes.PpuMemoryBus.ReadByte((ushort)attributeAddress);
					if (tileX % 4 != 0)
						attribute >>= 2;
					if (tileY % 4 != 0)
						attribute >>= 4;
					attribute &= 0b11;

					var surfX = tileX * 8;
					var surfY = tileY * 8;

					for (var y = 0; y < 8; y++)
					{
						var patternPlane0Index = (_ctrlBackgroundPatternTable ? 1 << 12 : 0) | (nametablePattern << 4) | y;
						var patternPlane1Index = patternPlane0Index | (1 << 3);

						var patternPlane0 = nes.PpuMemoryBus.ReadByte((ushort)patternPlane0Index, true);
						var patternPlane1 = nes.PpuMemoryBus.ReadByte((ushort)patternPlane1Index, true);

						for (var x = 0; x < 8; x++)
						{
							var patternBit0 = (patternPlane0 >> (8 - 1 - x)) & 1;
							var patternBit1 = (patternPlane1 >> (8 - 1 - x)) & 1;

							var colorIndex = (patternBit1 << 1) | patternBit0;

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

							Picture[index] = Color.FromArgb(_palette[paletteOffset * 3 + 0], _palette[paletteOffset * 3 + 1], _palette[paletteOffset * 3 + 2]);
						}
					}
				}
			}

			if (_ctrlVblankNmiEnable)
				_raiseNmi = true;

			_dot = 0;
		}

		return;

		void IncrementCoarseX()
		{
			// The coarse X component of v needs to be incremented when the next tile is reached. Bits 0-4 are incremented, with overflow toggling bit 10.
			// This means that bits 0-4 count from 0 to 31 across a single nametable, and bit 10 selects the current nametable horizontally.
			if ((_regV & 0x001F) == 31) // if coarse X == 31
			{
				_regV &= unchecked((ushort)~0x001F); // coarse X = 0
				_regV ^= 0x0400; // switch horizontal nametable
			}
			else
				_regV++; // increment coarse X
		}

		void IncrementY()
		{
			if ((_regV & 0x7000) != 0x7000) // If fine Y < 7
			{
				_regV += 0x1000; // Increment fine Y
			}
			else
			{
				_regV &= unchecked((ushort)~0x7000); // Fine Y = 0

				var coarseY = (_regV & 0x03E0) >> 5;

				switch (coarseY)
				{
					case 29:
						coarseY = 0; // Coarse Y = 0
						_regV ^= 0x0800; // Switch vertical nametable
						break;
					case 31:
						coarseY = 0; // Coarse Y = 0, nametable not switched
						break;
					default:
						coarseY += 1; // Increment coarse Y
						break;
				}
				// Put coarse Y back into v
				_regV &= unchecked((ushort)~0x03E0);
				_regV |= (ushort)(coarseY << 5);
			}
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
				// TODO: PPU OPEN BUS
				ret = (byte)(
					(_statusVblank ? 1 << 7 : 0) |
					(_statusSprite0 ? 1 << 6 : 0) |
					(_statusSpriteOverflow ? 1 << 5 : 0)
				);
				_regW = false;
				break;
			case 4: // OAMDATA
				// TODO
				break;
			case 7: // PPUDATA
				// Note that while the v register has 15 bits, the PPU memory space is only 14 bits wide. The highest bit is unused for access through $2007.
				// TODO: During rendering (on the pre-render line and the visible lines 0-239, provided either background or sprite rendering is enabled),
				//	it will update v in an odd way, triggering a coarse X increment and a Y increment simultaneously (with normal wrapping behavior).
				ret = nes.PpuMemoryBus.ReadByte((ushort)(_regV & 0b11_1111_1111_1111));
				if (_ctrlVramIncrement32)
					_regV += 32;
				else
					_regV++;
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
				_regT = (ushort)((_regT & 0b111_0011_1111_1111) | ((value & 0b11) << 10));
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
				if (!_regW)
				{
					// t: ....... ...ABCDE <- d: ABCDE...
					_regT &= 0b111_1111_1110_0000;
					_regT |= (ushort)((value >> 3) & 0b111);

					// x:              FGH <- d: .....FGH
					_regX = (byte)(value & 0b111);
				}
				else
				{
					// t: FGH..AB CDE..... <- d: ABCDEFGH
					_regT &= 0b000_1100_0001_1111;
					_regT |= (ushort)(((value >> 3) & 0b11111) << 5);
					_regT |= (ushort)((value & 0b111) << 12);
				}
				_regW = !_regW;
				break;
			case 6: // PPUADDR
				if (!_regW) // first write (w is 0)
				{
					// t: .CDEFGH ........ <- d: ..CDEFGH
					// t: Z...... ........ <- 0 (bit Z is cleared)
					_regT &= 0b000_0000_1111_1111;
					_regT |= (ushort)((value & 0b11_1111) << 8);
				}
				else // second write (w is 1)
				{
					// t: ....... ABCDEFGH <- d: ABCDEFGH
					_regT &= 0b111_1111_0000_0000;
					_regT |= value;

					// v: <...all bits...> <- t: <...all bits...>
					_regV = _regT;
				}
				_regW = !_regW;
				break;
			case 7: // PPUDATA
				// Note that while the v register has 15 bits, the PPU memory space is only 14 bits wide. The highest bit is unused for access through $2007.
				// TODO: During rendering (on the pre-render line and the visible lines 0-239, provided either background or sprite rendering is enabled),
				//	it will update v in an odd way, triggering a coarse X increment and a Y increment simultaneously (with normal wrapping behavior).
				//	see: https://www.nesdev.org/wiki/PPU_scrolling#$2007_(PPUDATA)_reads_and_writes
				nes.PpuMemoryBus.WriteByte((ushort)(_regV & 0b11_1111_1111_1111), value);
				if (_ctrlVramIncrement32)
					_regV += 32;
				else
					_regV++;
				break;
		}
	}
}
