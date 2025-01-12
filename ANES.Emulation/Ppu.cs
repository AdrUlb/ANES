using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ANES;

// https://www.nesdev.org/wiki/PPU_rendering
// https://www.nesdev.org/wiki/PPU_programmer_reference
// https://www.nesdev.org/wiki/PPU_registers
// https://www.nesdev.org/wiki/PPU_scrolling
// https://www.nesdev.org/wiki/PPU_sprite_evaluation
// https://www.nesdev.org/w/images/default/4/4f/Ppu.svg

public sealed class PpuVblankEventArgs(bool frame, bool nmi) : EventArgs
{
	public readonly bool Frame = frame;
	public readonly bool Nmi = nmi;
}

public sealed class Ppu(Nes nes)
{
	public const int PictureWidth = 256;
	public const int PictureHeight = 240;

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

	private byte _dataReadBuffer = 0;

	private bool _oddFrame = false;
	private int _scanline = 261;
	private int _cycle = 0;

	private ushort _regV;
	private ushort _regT;
	private byte _regX;
	private bool _regW;

	private byte _bgTileFetch;
	private byte _bgAttributeFetch;
	private byte _bgPatternLowFetch;
	private byte _bgPatternHighFetch;

	private ushort _bgPaletteShifter;
	private ushort _bgPatternLowShifter;
	private ushort _bgPatternHighShifter;

	private readonly byte[] _palette = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Composite_wiki.pal"));

	internal readonly byte[] Oam = new byte[256];
	public readonly Color[] Picture = new Color[PictureWidth * PictureHeight];
	private int _pictureIndex = 0;

	private bool IsRenderingEnabled => _maskEnableBackground || _maskEnableSprites;

	public event EventHandler<PpuVblankEventArgs>? Vblank;

	public byte ReadReg(int index, bool suppressSideEffects = false)
	{
		byte ret = 0;

		switch (index)
		{
			case 2: // PPUSTATUS
					// TODO: PPU OPEN BUS
				if (!suppressSideEffects)
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
				ret = _dataReadBuffer;
				_dataReadBuffer = nes.PpuBus.ReadByte((ushort)(_regV & 0b11_1111_1111_1111));
				if (!suppressSideEffects)
				{
					if (_ctrlVramIncrement32)
						_regV += 32;
					else
						_regV++;
				}
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
					Vblank.Invoke(this, new(false, true));

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
				nes.PpuBus.WriteByte((ushort)(_regV & 0b11_1111_1111_1111), value);
				if (_ctrlVramIncrement32)
					_regV += 32;
				else
					_regV++;
				break;
		}
	}

	public void Tick()
	{
		DoCycle();

		_cycle++;
		// Skip one dot on odd frames
		if (_cycle > 340 || (_scanline == 261 && _oddFrame && _cycle >= 340))
		{
			_cycle = 0;
			_scanline++;

			if (_scanline > 261)
			{
				_scanline = 0;
				_pictureIndex = 0;
				_oddFrame = !_oddFrame;
			}
		}
	}

	private void DoCycle()
	{
		// Vblank flag is set on scanline 241, cycle 1
		if (_scanline == 241 && _cycle == 1)
		{
			// TODO: THIS IS A HACK
			DrawSprites();

			// Set Vblank flag
			_statusVblank = true;

			Vblank?.Invoke(this, new(true, _ctrlVblankNmiEnable));

			return;
		}

		// No work is done between scanlines 240 and 260
		if (_scanline is >= 240 and <= 260)
			return;

		if (_cycle is (>= 1 and <= 256) or >= 321 && IsRenderingEnabled)
		{
			if (_scanline != 261 && _cycle is >= 1 and <= 256)
				OutputDot();

			if (_cycle <= 336)
			{
				_bgPatternLowShifter <<= 1;
				_bgPatternHighShifter <<= 1;
			}

			// https://www.nesdev.org/wiki/PPU_scrolling#Tile_and_attribute_fetching
			// The data for each tile is fetched during this phase. Each memory access takes 2 PPU cycles to complete, and 4 must be performed per tile
			// The data fetched from these accesses is placed into internal latches, and then fed to the appropriate shift registers when it's time to do so (every 8 cycles).
			// Because the PPU can only fetch an attribute byte every 8 cycles, each sequential string of 8 pixels is forced to have the same palette attribute.
			var fetchStep = (_cycle - 1) % 8;
			switch (fetchStep)
			{
				// TODO: actually do this "properly"
				case 0: // Nametable byte
					{
						var addr = (ushort)(0x2000 | (_regV & 0x0FFF));
						_bgTileFetch = nes.PpuBus.ReadByte(addr);
						break;
					}
				case 2: // Attribute table byte
					{
						var nametable = (_regV >> 10 & 0b11);
						var coarseX = _regV & 0b11111;
						var coarseY = (_regV >> 5) & 0b11111;
						var coarseXHigh3 = coarseX >> 2;
						var coarseYHigh3 = coarseY >> 2;
						var addr = (ushort)(0x23C0 | (nametable << 10) | (coarseYHigh3 << 3) | coarseXHigh3);
						//var addr = (ushort)(0x23C0 | (_regV & 0x0C00) | ((_regV >> 4) & 0x38) | ((_regV >> 2) & 0x07));
						_bgAttributeFetch = nes.PpuBus.ReadByte(addr);
						break;
					}
				case 4: // Pattern table tile low
					{
						var half = _ctrlBackgroundPatternTable ? 1 : 0;
						var fineY = (_regV >> 12) & 0b111;
						var addr = (ushort)((half << 12) | (_bgTileFetch << 4) | fineY);
						_bgPatternLowFetch = nes.PpuBus.ReadByte(addr);
						break;
					}
				case 6: // Pattern table tile high
					{
						var half = _ctrlBackgroundPatternTable ? 1 : 0;
						var fineY = (_regV >> 12) & 0b111;
						var addr = (ushort)((half << 12) | (_bgTileFetch << 4) | fineY);
						_bgPatternHighFetch = nes.PpuBus.ReadByte((ushort)(addr + 8));
						break;
					}
				case 7:
					{
						// On every 8th dot in these background fetch regions (the same dot on which the coarse x component of v is incremented),
						// the pattern and attributes data are transferred into registers used for producing pixel data.
						// For the pattern data, these transfers are into the high 8 bits of two 16-bit shift registers.
						// For the attributes data, only 2 bits are transferred and into two 1-bit latches that feed 8-bit shift registers.
						// The concept for both is the same, differing merely because the attributes data is the same for all 8 pixels, negating the need to store 8 copies of it.
						var coarseX = _regV & 0b11111;
						var coarseY = (_regV >> 5) & 0b11111;

						var attribute = ((coarseX >> 1) & 1, (coarseY >> 1) & 1) switch
						{
							(0, 0) => (_bgAttributeFetch >> 0) & 0b11,
							(1, 0) => (_bgAttributeFetch >> 2) & 0b11,
							(0, 1) => (_bgAttributeFetch >> 4) & 0b11,
							(1, 1) => (_bgAttributeFetch >> 6) & 0b11,
							_ => throw new UnreachableException()
						};

						_bgPaletteShifter <<= 2;
						_bgPaletteShifter |= (ushort)attribute;

						_bgPatternLowShifter |= _bgPatternLowFetch;
						_bgPatternHighShifter |= _bgPatternHighFetch;

						IncrementCoarseX();
					}
					break;
			}
		}

		// At dot 256 of each scanline
		// If rendering is enabled, the PPU increments the vertical position in v.
		// The effective Y scroll coordinate is incremented, which is a complex operation that will correctly skip the attribute table memory regions,
		// and wrap to the next nametable appropriately.
		if (_cycle == 256 && IsRenderingEnabled)
			IncrementY();

		// At dot 257 of each scanline
		// If rendering is enabled, the PPU copies all bits related to horizontal position from t to v
		if (_cycle == 257 && IsRenderingEnabled)
		{
			// v: ....A.. ...BCDEF <- t: ....A.. ...BCDEF
			_regV &= 0b111_1011_1110_0000;
			_regV |= (ushort)(_regT & 0b000_0100_0001_1111);
		}

		if (_scanline == 261)
		{
			if (_cycle == 1)
			{
				_statusVblank = false;
				_statusSpriteOverflow = false;
				_statusSprite0 = false;
			}

			// If rendering is enabled, at the end of vblank, shortly after the horizontal bits are copied from t to v at dot 257,
			// the PPU will repeatedly copy the vertical bits from t to v from dots 280 to 304, completing the full initialization of v from t
			if (_cycle is >= 280 and <= 304 && IsRenderingEnabled)
			{
				// v: GHIA.BC DEF..... <- t: GHIA.BC DEF.....
				_regV &= 0b000_0100_0001_1111;
				_regV |= (ushort)(_regT & 0b111_1011_1110_0000);
			}
		}
	}
	
	private void OutputDot()
	{
		var patternLow = (_bgPatternLowShifter >> (16 - 1 - _regX)) & 1;
		var patternHigh = (_bgPatternHighShifter >> (16 - 1 - _regX)) & 1;

		var pattern = (patternHigh << 1) | patternLow;

		var palette = (_bgPaletteShifter >> 2) & 0b11;

		int paletteIndex = pattern switch
		{
			0 => nes.PpuBus.ReadByte(0x3F00),
			1 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 1))),
			2 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 2))),
			3 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 3))),
			_ => throw new UnreachableException()
		};

		paletteIndex %= _palette.Length / 3;

		Picture[_pictureIndex++] = Color.FromArgb(_palette[paletteIndex * 3 + 0], _palette[paletteIndex * 3 + 1], _palette[paletteIndex * 3 + 2]);
	}

	private void DrawSprites()
	{
		for (var i = 0; i < 64; i++)
		{
			var x = Oam[i * 4 + 3];
			var y = Oam[i * 4 + 0] + 1;

			var index = Oam[i * 4 + 1];

			var attribute = Oam[i * 4 + 2];
			var palette = attribute & 0b11;
			var behindBackground = ((attribute >> 5) & 1) != 0;
			var flipX = ((attribute >> 6) & 1) != 0;
			var flipY = ((attribute >> 7) & 1) != 0;

			var patternHalf = _ctrlSpritePatternTable ? 1 : 0;

			for (var tileY = 0; tileY < 8; tileY++)
			{
				var yy = y + tileY;

				if (yy >= PictureHeight)
					break;

				var fineY = (flipY ? (7 - tileY) : tileY) % 8;
				var patternAddr = (ushort)((patternHalf << 12) | (index << 4) | fineY);
				var patternLow = nes.PpuBus.ReadByte(patternAddr);
				var patternHigh = nes.PpuBus.ReadByte((ushort)(patternAddr + 8));


				for (var tileX = 0; tileX < 8; tileX++)
				{
					var xx = x + tileX;

					if (xx >= PictureWidth)
						break;

					var pictureIndex = xx + yy * PictureWidth;

					var shift = flipX ? tileX : (7 - tileX);
					var pattern = (((patternHigh >> shift) & 1) << 1) | ((patternLow >> shift) & 1);

					if (pattern == 0)
						continue;

					int paletteIndex = pattern switch
					{
						0 => nes.PpuBus.ReadByte(0x3F10),
						1 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 1))),
						2 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 2))),
						3 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 3))),
						_ => throw new UnreachableException()
					};

					paletteIndex %= _palette.Length / 3;

					Picture[pictureIndex] = Color.FromArgb(_palette[paletteIndex * 3 + 0], _palette[paletteIndex * 3 + 1], _palette[paletteIndex * 3 + 2]);
				}
			}
		}
	}

	private void IncrementCoarseX()
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

	private void IncrementY()
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
