using System.Diagnostics;
using System.Drawing;

namespace ANES;

internal sealed class Ppu(Nes nes)
{
	internal const int PictureWidth = 256;
	internal const int PictureHeight = 240;

	// TODO: set flag correctly
	private bool _statusVblank;
	private bool _statusSprite0;
	private bool _statusSpriteOverflow;

	private bool _ctrlVramIncrement32;
	private bool _ctrlSpritePatternTable;
	private bool _ctrlBackgroundPatternTable;
	private bool _ctrlSpriteSize;
	private bool _ctrlMasterSlaveSelect;
	private bool _ctrlVblankNmiEnable;

	private bool _maskGrayscale; // TODO
	private bool _maskShowBackgroundLeft; // TODO
	private bool _maskShowSpritesLeft; // TODO
	private bool _maskEnableBackground; // TODO
	private bool _maskEnableSprites; // TODO
	private bool _maskEmphasizeRed; // TODO // NOTE: Green on PAL/Dendy
	private bool _maskEmphasizeGreen; // TODO // NOTE: Red on PAL/Dendy
	private bool _maskEmphasizeBlue; // TODO

	private bool _oddFrame = false;
	private ulong _dotX = 0;
	private ulong _dotY = 261;

	private bool _raiseNmi = false;

	private ushort _regV;
	private ushort _regT;
	private byte _regX;
	private bool _regW;

	// TODO: implement these properly
	private ushort _oamAddr = 0;
	private readonly byte[] _palette = File.ReadAllBytes("Palette.pal");

	internal readonly Color[] Picture = new Color[PictureWidth * PictureHeight];

	public byte ReadReg(int index, bool suppressSideEffects = false)
	{
		byte ret = 0;

		switch (index)
		{
			case 2: // PPUSTATUS
				// TODO: PPU OPEN BUS
				_regW = false;
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
		switch (index)
		{
			case 0: // PPUCTRL
				var prevNmiEnable = _ctrlVblankNmiEnable;

				_regT = (ushort)((_regT & 0b111_0011_1111_1111) | ((value & 0b11) << 10));
				_ctrlVramIncrement32 = ((value >> 2) & 1) != 0;
				_ctrlSpritePatternTable = ((value >> 3) & 1) != 0;
				_ctrlBackgroundPatternTable = ((value >> 4) & 1) != 0;
				_ctrlSpriteSize = ((value >> 5) & 1) != 0;
				_ctrlMasterSlaveSelect = ((value >> 6) & 1) != 0;
				_ctrlVblankNmiEnable = ((value >> 7) & 1) != 0;

				// Changing NMI enable from 0 to 1 while the vblank flag in PPUSTATUS is 1 will immediately trigger an NMI.
				if (!prevNmiEnable && _ctrlVblankNmiEnable && _statusVblank)
					_raiseNmi = true;

				break;
			case 1: // PPUMASK
				_maskGrayscale = (value & 1) != 0;
				_maskShowBackgroundLeft = ((value >> 1) & 1) != 0;
				_maskShowSpritesLeft = ((value >> 2) & 1) != 0;
				_maskEnableBackground = ((value >> 3) & 1) != 0;
				_maskEnableSprites = ((value >> 4) & 1) != 0;
				_maskEmphasizeRed = ((value >> 5) & 1) != 0;
				_maskEmphasizeGreen = ((value >> 6) & 1) != 0;
				_maskEmphasizeBlue = ((value >> 7) & 1) != 0;
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

	public void Tick()
	{
		if (_dotY == 241 && _dotX == 1)
		{
			// Set Vblank flag
			_statusVblank = true;

			if (_ctrlVblankNmiEnable)
				_raiseNmi = true;
		}

		if (_dotY == 261 && _dotX == 1)
		{
			// TODO: Clear sprite 0
			// TODO: Clear overflow
			_statusVblank = false;
		}

		// TODO: This is an ugly hack
		if (_dotY <= 239 && _dotX == 340)
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

			var tileY = (int)(_dotY / 8);
			for (var tileX = 0; tileX < PictureWidth / 8; tileX++)
			{
				var nametablePatternAddress = nametableBase + tileX + tileY * (PictureWidth / 8);
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

				var y = (int)(_dotY % 8);
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
						var index = xx + yy * PictureWidth;

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

		_dotX++;
		// Skip one dot on odd frames
		if (_dotX > 340 || (_dotY == 261 && _oddFrame && _dotX >= 340))
		{
			_oddFrame = !_oddFrame;
			_dotX = 0;
			_dotY++;

			if (_dotY > 261)
				_dotY = 0;
		}
	}

	public bool HandleNmi()
	{
		if (!_raiseNmi)
			return false;

		_raiseNmi = false;
		return true;
	}

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
