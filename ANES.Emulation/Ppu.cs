using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ANES.Emulation;

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
	private int _scanline;
	private int _cycle;

	private ushort _regV;
	private ushort _regT;
	private byte _regX;
	private bool _regW;
	private byte _regOamAddr;

	private byte _bgTileFetch;
	private byte _bgAttributeFetch;
	private byte _bgPatternLowFetch;
	private byte _bgPatternHighFetch;

	private ushort _bgPaletteShifter;
	private ushort _bgPatternLowShifter;
	private ushort _bgPatternHighShifter;

	private int _spriteEvalStep;
	private int _spriteN;
	private int _spriteM;
	private int _spriteCount;
	private byte _spriteByte;
	private bool _sprite0Copied;
	private readonly Color[] _spritePixels = new Color[PictureWidth];
	private readonly bool[] _spritePixelsBehindBackground = new bool[PictureWidth];
	private readonly bool[] _spritePixelsSprite0 = new bool[PictureWidth];

	internal readonly byte[] Oam = new byte[256];
	internal readonly byte[] _secondaryOam = new byte[32];
	public readonly Color[] Picture = new Color[PictureWidth * PictureHeight];
	private int _pictureIndex = 0;

	private bool IsRenderingEnabled => _maskEnableBackground || _maskEnableSprites;

	public event EventHandler<PpuVblankEventArgs>? Vblank;

	internal byte ReadReg(int index, bool suppressSideEffects = false)
	{
		byte ret = 0;

		switch (index)
		{
			case 2: // PPUSTATUS
				{
					if (!suppressSideEffects)
						_regW = false;

					ret = (byte)(
						(_statusVblank ? 1 << 7 : 0) |
						(_statusSprite0 ? 1 << 6 : 0) |
						(_statusSpriteOverflow ? 1 << 5 : 0)
					);
					break;
				}
			case 4: // OAMDATA
				{
					// Cycles 1-64: Secondary OAM (32-byte buffer for current sprites on scanline) is initialized to $FF - attempting to read $2004 will return $FF
					if (_scanline is < 240 or 261 && _cycle is >= 1 and <= 64)
					{
						ret = 0xFF;
						break;
					}
					ret = Oam[_regOamAddr];
					break;
				}
			case 7: // PPUDATA
				{
					// Note that while the v register has 15 bits, the PPU memory space is only 14 bits wide. The highest bit is unused for access through $2007.
					ret = _dataReadBuffer;
					_dataReadBuffer = nes.PpuBus.ReadByte((ushort)(_regV & 0b111111_11111111));
					if (!suppressSideEffects)
					{

						if (_scanline is < 240 or 26 && IsRenderingEnabled)
						{
							IncrementCoarseX();
							IncrementY();
						}
						else if (_ctrlVramIncrement32)
							_regV += 32;
						else
							_regV++;

						_regV &= 0b1111111_11111111;
					}
					break;
				}
		}

		return ret;
	}

	internal void WriteReg(int index, byte value)
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
					Vblank?.Invoke(this, new(false, true));

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
				_regOamAddr = value;
				break;
			case 4: // OAMDATA
				Oam[_regOamAddr++] = value;
				break;
			case 5: // PPUSCROLL
				if (!_regW)
				{
					// t: ....... ...ABCDE <- d: ABCDE...
					_regT &= 0b111_1111_1110_0000;
					_regT |= (ushort)((value >> 3) & 0b11111);

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
				nes.PpuBus.WriteByte((ushort)(_regV & 0b11_1111_1111_1111), value);

				if (_scanline is < 240 or 26 && IsRenderingEnabled)
				{
					IncrementCoarseX();
					IncrementY();
				}
				else
				{

					if (_ctrlVramIncrement32)
						_regV += 32;
					else
						_regV++;

					_regV &= 0b1111111_11111111;
				}
				break;
		}
	}

	internal void Tick()
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

	internal void Reset()
	{
		Oam.AsSpan().Fill(0xFF);
		_scanline = 261;
		_cycle = 0;
	}

	private void DoCycle()
	{
		// Vblank flag is set on scanline 241, cycle 1
		if (_scanline == 241 && _cycle == 1)
		{
			// Set Vblank flag
			_statusVblank = true;

			Vblank?.Invoke(this, new(true, _ctrlVblankNmiEnable));

			return;
		}

		// No work is done between scanlines 240 and 260
		if (_scanline is > 239 and < 261)
			return;

		// This should be accurate enough?
		if (_cycle == 64)
		{
			// Clear secondary OAM
			_secondaryOam.AsSpan().Fill(0xFF);
			_spriteN = 0;
			_spriteM = 0;
			_spriteCount = 0;
			_spriteEvalStep = 1;
			_sprite0Copied = false;
		}

		if (_scanline != 261)
		{
			// Cycles 65-256: Sprite evaluation
			if (_cycle is >= 65 and <= 256 && IsRenderingEnabled)
			{

				if (_cycle % 2 == 1) // On odd cycles, data is read from (primary) OAM
				{
					_spriteByte = Oam[_spriteN * 4 + _spriteM];
				}
				else // On even cycles, data is written to secondary OAM (unless secondary OAM is full, in which case it will read the value in secondary OAM instead)
				{
					switch (_spriteEvalStep)
					{
						case 1:
							{
								// 1. Starting at n = 0, read a sprite's Y-coordinate (OAM[n][0], copying it to the next open slot in secondary OAM
								// 1a. If Y-coordinate is in range, copy remaining bytes of sprite data (OAM[n][1] thru OAM[n][3]) into secondary OAM.

								// Always copy the read byte into secondary OAM
								_secondaryOam[_spriteCount * 4 + _spriteM] = _spriteByte;

								var inRange = _spriteByte <= _scanline && _spriteByte + 8 > _scanline;

								// If this was the last byte of the sprite
								if (_spriteM == 3)
								{
									if (_spriteN == 0)
										_sprite0Copied = true;

									_spriteM = 0;
									_spriteCount++;
								}
								else if (_spriteM != 0 || inRange)
								{
									_spriteM++;
									break;
								}

								goto case 2;
							}
						case 2:
							// 2. Increment n
							_spriteN++;

							// 2a. If n has overflowed back to zero (all 64 sprites evaluated), go to 4
							if (_spriteN >= 64)
							{
								_spriteN %= 64;
								_spriteEvalStep = 4;
								break;
							}

							// 2b. If less than 8 sprites have been found, go to 1
							if (_spriteCount < 8)
							{
								_spriteEvalStep = 1;
								break;
							}

							// 2c. If exactly 8 sprites have been found, disable writes to secondary OAM because it is full. This causes sprites in back to drop out.
							_spriteEvalStep = 3;
							break;
						case 3: // TODO
								// 3. Starting at m = 0, evaluate OAM[n][m] as a Y-coordinate.
								// 3a. If the value is in range, set the sprite overflow flag in $2002 and read the next 3 entries of OAM (incrementing 'm' after each byte and incrementing 'n' when 'm' overflows); if m = 3, increment n
								// 3b. If the value is not in range, increment n and m (without carry). If n overflows to 0, go to 4; otherwise go to 3
								//	The m increment is a hardware bug - if only n was incremented, the overflow flag would be set whenever more than 8 sprites were present on the same scanline, as expected.
							break;
						case 4:
							// 4. Attempt (and fail) to copy OAM[n][0] into the next free slot in secondary OAM, and increment n (repeat until HBLANK is reached)
							break;
					}
				}
			}

			// Cycles 257-320: Sprite fetches (8 sprites total, 8 cycles per sprite)
			// TODO: this is a hack!
			if (_cycle == 257)
			{
				FetchSprites();
			}
		}

		// OAMADDR is set to 0 during each of ticks 257–320 (the sprite tile loading interval) of the pre-render and visible scanlines.
		if (_cycle is >= 257 and <= 320)
			_regOamAddr = 0;

		if (_scanline != 261 && _cycle is >= 1 and <= 256)
			OutputDot();

		if (_cycle is >= 1 and <= 336)
		{
			_bgPatternLowShifter <<= 1;
			_bgPatternHighShifter <<= 1;
		}

		if (_cycle is (>= 1 and <= 256) or >= 321 && IsRenderingEnabled)
		{
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
			_regV &= 0b1111011_11100000;
			_regV |= (ushort)(_regT & 0b0000100_00011111);
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
				_regV &= 0b0000100_00011111;
				_regV |= (ushort)(_regT & 0b1111011_11100000);
			}
		}
	}

	private Color GetPaletteColor(int paletteIndex)
	{
		return Color.FromArgb(nes.Palette[paletteIndex * 3 + 0], nes.Palette[paletteIndex * 3 + 1], nes.Palette[paletteIndex * 3 + 2]); ;
	}

	private void OutputDot()
	{
		// TODO: Sprite 0 hit acts as if the image starts at cycle 2 (which is the same cycle that the shifters shift for the first time), so the sprite 0 flag will be raised at this point at the earliest.
		// Actual pixel output is delayed further due to internal render pipelining, and the first pixel is output during cycle 4.
		var patternLow = (_bgPatternLowShifter >> (16 - 1 - _regX)) & 1;
		var patternHigh = (_bgPatternHighShifter >> (16 - 1 - _regX)) & 1;

		var pattern = (patternHigh << 1) | patternLow;

		var x = _cycle - 1;
		var shift = _regX > 7 - (x % 8) ? 0 : 2;
		var palette = (_bgPaletteShifter >> shift) & 0b11;

		var spritePixel = _spritePixels[_cycle - 1];
		var spriteBehindBackground = _spritePixelsBehindBackground[_cycle - 1];
		var sprite0 = _spritePixelsSprite0[_cycle - 1];

		if (spritePixel != Color.Transparent && pattern != 0 && sprite0)
		{
			_statusSprite0 = true;
		}

		Color pixel;
		if (spritePixel != Color.Transparent && (pattern == 0 || !spriteBehindBackground))
		{
			pixel = spritePixel;
		}
		else
		{
			int paletteIndex = pattern switch
			{
				0 => nes.PpuBus.ReadByte(0x3F00),
				1 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 1))),
				2 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 2))),
				3 => nes.PpuBus.ReadByte((ushort)(0x3F00 | (4 * palette + 3))),
				_ => throw new UnreachableException()
			};
			paletteIndex %= nes.Palette.Length / 3;
			pixel = GetPaletteColor(paletteIndex);
		}

		Picture[_pictureIndex] = pixel;

		_pictureIndex++;
	}

	private void FetchSprites()
	{
		_spritePixels.AsSpan().Fill(Color.Transparent);

		var patternHalf = _ctrlSpritePatternTable ? 1 : 0;

		for (var i = 0; i < 8; i++)
		{
			var y = _secondaryOam[i * 4 + 0];
			if (y == 0xFF)
				continue;

			var attribute = _secondaryOam[i * 4 + 2];
			if (attribute == 0xFF)
				continue;

			var x = _secondaryOam[i * 4 + 3];
			var index = _secondaryOam[i * 4 + 1];

			var palette = attribute & 0b11;
			var behindBackground = ((attribute >> 5) & 1) != 0;
			var flipX = ((attribute >> 6) & 1) != 0;
			var flipY = ((attribute >> 7) & 1) != 0;

			var tileY = _scanline - y;

			var fineY = (flipY ? (7 - tileY) : tileY) % 8;
			var patternAddr = (ushort)((patternHalf << 12) | (index << 4) | fineY);
			var patternLow = nes.PpuBus.ReadByte(patternAddr);
			var patternHigh = nes.PpuBus.ReadByte((ushort)(patternAddr + 8));

			for (var tileX = 0; tileX < 8; tileX++)
			{
				var xx = x + tileX;

				if (xx >= PictureWidth)
					break;

				if (_spritePixels[xx] != Color.Transparent)
					continue;

				var shift = flipX ? tileX : (7 - tileX);
				var pattern = (((patternHigh >> shift) & 1) << 1) | ((patternLow >> shift) & 1);

				if (pattern == 0)
					continue;

				if (pattern == 0)
					continue;

				int paletteIndex = pattern switch
				{
					1 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 1))),
					2 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 2))),
					3 => nes.PpuBus.ReadByte((ushort)(0x3F10 | (4 * palette + 3))),
					_ => throw new UnreachableException()
				};

				paletteIndex %= nes.Palette.Length / 3;

				_spritePixels[xx] = Color.FromArgb(nes.Palette[paletteIndex * 3 + 0], nes.Palette[paletteIndex * 3 + 1], nes.Palette[paletteIndex * 3 + 2]);
				_spritePixelsBehindBackground[xx] = behindBackground;
				_spritePixelsSprite0[xx] = _sprite0Copied && i == 0;
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
