namespace ANES;

internal sealed class Ppu(Nes nes)
{
	private bool _statusVblank = true;
	private bool _statusSprite0 = false;
	private bool _statusSpriteOverflow = false;

	private bool _raiseNmi = false;

	private ulong _dot = 0;

	private byte _latchValue = 0;

	private ushort _ppuAddress = 0;
	private ushort _oamAddr = 0;

	public void Tick()
	{
		_dot++;

		if (_dot == 340 * 260)
		{
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
				ret = nes.PpuMemoryBus.ReadByte(_ppuAddress);
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
				// TODO
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
				// FIXME: this is a hack
				_ppuAddress <<= 8;
				_ppuAddress |= value;
				break;
			case 7: // PPUDATA
				//Console.WriteLine($"PPUDATA: [0x{_ppuAddress:X4}] = 0x{value:X2}");
				nes.PpuMemoryBus.WriteByte(_ppuAddress, value);
				_ppuAddress++;
				break;
		}
	}
}
