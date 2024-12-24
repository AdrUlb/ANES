using System.Diagnostics;

namespace ANES;

/*
Delayed IRQ response after CLI, SEI, and PLP
	The RTI instruction affects IRQ inhibition immediately. If an IRQ is pending and an RTI is executed that clears the I flag, the CPU will invoke the IRQ handler immediately after RTI finishes executing. This is due to RTI restoring the I flag from the stack before polling for interrupts.

	The CLI, SEI, and PLP instructions on the other hand change the I flag after polling for interrupts (like all two-cycle instructions they poll the interrupt lines at the end of the first cycle), meaning they can effectively delay an interrupt until after the next instruction.
	For example, if an interrupt is pending and the I flag is currently set, executing CLI will execute the next instruction before the CPU invokes the IRQ handler.
*/

internal sealed class Cpu(CpuMemoryBus bus)
{
	private static readonly IReadOnlyList<CpuOperation> _operationsByOpcode =
	[
		// 0x00 - 0x0F
		new(CpuInstruction.Brk, CpuAddressingMode.Implied),
		new(CpuInstruction.Ora, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Slo, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Ora, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Asl, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Slo, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Php, CpuAddressingMode.Implied),
		new(CpuInstruction.Ora, CpuAddressingMode.Immediate),
		new(CpuInstruction.Asl, CpuAddressingMode.Accumulator),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Nop, CpuAddressingMode.Absolute, true),
		new(CpuInstruction.Ora, CpuAddressingMode.Absolute),
		new(CpuInstruction.Asl, CpuAddressingMode.Absolute),
		new(CpuInstruction.Slo, CpuAddressingMode.Absolute, true),
		// 0x10 - 0x1F
		new(CpuInstruction.Bpl, CpuAddressingMode.Relative),
		new(CpuInstruction.Ora, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Slo, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Ora, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Asl, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Slo, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Clc, CpuAddressingMode.Implied),
		new(CpuInstruction.Ora, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Slo, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.Ora, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Asl, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Slo, CpuAddressingMode.AbsoluteXIndexed, true),
		// 0x20 - 0x2F
		new(CpuInstruction.Jsr, CpuAddressingMode.Absolute),
		new(CpuInstruction.And, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Rla, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Bit, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.And, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Rol, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Rla, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Plp, CpuAddressingMode.Implied),
		new(CpuInstruction.And, CpuAddressingMode.Immediate),
		new(CpuInstruction.Rol, CpuAddressingMode.Accumulator),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Bit, CpuAddressingMode.Absolute),
		new(CpuInstruction.And, CpuAddressingMode.Absolute),
		new(CpuInstruction.Rol, CpuAddressingMode.Absolute),
		new(CpuInstruction.Rla, CpuAddressingMode.Absolute, true),
		// 0x30 - 0x3F
		new(CpuInstruction.Bmi, CpuAddressingMode.Relative),
		new(CpuInstruction.And, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Rla, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.And, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Rol, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Rla, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Sec, CpuAddressingMode.Implied),
		new(CpuInstruction.And, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Rla, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.And, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Rol, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Rla, CpuAddressingMode.AbsoluteXIndexed, true),
		// 0x40 - 0x4F
		new(CpuInstruction.Rti, CpuAddressingMode.Implied),
		new(CpuInstruction.Eor, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Sre, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Eor, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Lsr, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Sre, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Pha, CpuAddressingMode.Implied),
		new(CpuInstruction.Eor, CpuAddressingMode.Immediate),
		new(CpuInstruction.Lsr, CpuAddressingMode.Accumulator),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Jmp, CpuAddressingMode.Absolute),
		new(CpuInstruction.Eor, CpuAddressingMode.Absolute),
		new(CpuInstruction.Lsr, CpuAddressingMode.Absolute),
		new(CpuInstruction.Sre, CpuAddressingMode.Absolute, true),
		// 0x50 - 0x5F
		new(CpuInstruction.Bvc, CpuAddressingMode.Relative),
		new(CpuInstruction.Eor, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Sre, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Eor, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Lsr, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Sre, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Cli, CpuAddressingMode.Implied),
		new(CpuInstruction.Eor, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Sre, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.Eor, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Lsr, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Sre, CpuAddressingMode.AbsoluteXIndexed, true),
		// 0x60 - 0x6F
		new(CpuInstruction.Rts, CpuAddressingMode.Implied),
		new(CpuInstruction.Adc, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Rra, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Adc, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Ror, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Rra, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Pla, CpuAddressingMode.Implied),
		new(CpuInstruction.Adc, CpuAddressingMode.Immediate),
		new(CpuInstruction.Ror, CpuAddressingMode.Accumulator),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Jmp, CpuAddressingMode.Indirect),
		new(CpuInstruction.Adc, CpuAddressingMode.Absolute),
		new(CpuInstruction.Ror, CpuAddressingMode.Absolute),
		new(CpuInstruction.Rra, CpuAddressingMode.Absolute, true),
		// 0x70 - 0x7F
		new(CpuInstruction.Bvs, CpuAddressingMode.Relative),
		new(CpuInstruction.Adc, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Rra, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Adc, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Ror, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Rra, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Sei, CpuAddressingMode.Implied),
		new(CpuInstruction.Adc, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Rra, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.Adc, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Ror, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Rra, CpuAddressingMode.AbsoluteXIndexed, true),
		// 0x80 - 0x8F
		new(CpuInstruction.Nop, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Sta, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.Nop, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Sax, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Sty, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Sta, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Stx, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Sax, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Dey, CpuAddressingMode.Implied),
		new(CpuInstruction.Nop, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Txa, CpuAddressingMode.Implied),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Sty, CpuAddressingMode.Absolute),
		new(CpuInstruction.Sta, CpuAddressingMode.Absolute),
		new(CpuInstruction.Stx, CpuAddressingMode.Absolute),
		new(CpuInstruction.Sax, CpuAddressingMode.Absolute, true),
		// 0x90 - 0x9F
		new(CpuInstruction.Bcc, CpuAddressingMode.Relative),
		new(CpuInstruction.Sta, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Sty, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Sta, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Stx, CpuAddressingMode.ZeroPageYIndexed),
		new(CpuInstruction.Sax, CpuAddressingMode.ZeroPageYIndexed),
		new(CpuInstruction.Tya, CpuAddressingMode.Implied),
		new(CpuInstruction.Sta, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Txs, CpuAddressingMode.Implied),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Sta, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		// 0xA0 - 0xAF
		new(CpuInstruction.Ldy, CpuAddressingMode.Immediate),
		new(CpuInstruction.Lda, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.Ldx, CpuAddressingMode.Immediate),
		new(CpuInstruction.Lax, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Ldy, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Lda, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Ldx, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Lax, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Tay, CpuAddressingMode.Implied),
		new(CpuInstruction.Lda, CpuAddressingMode.Immediate),
		new(CpuInstruction.Tax, CpuAddressingMode.Implied),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Ldy, CpuAddressingMode.Absolute),
		new(CpuInstruction.Lda, CpuAddressingMode.Absolute),
		new(CpuInstruction.Ldx, CpuAddressingMode.Absolute),
		new(CpuInstruction.Lax, CpuAddressingMode.Absolute, true),
		// 0xB0 - 0xBF
		new(CpuInstruction.Bcs, CpuAddressingMode.Relative),
		new(CpuInstruction.Lda, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Lax, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Ldy, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Lda, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Ldx, CpuAddressingMode.ZeroPageYIndexed),
		new(CpuInstruction.Lax, CpuAddressingMode.ZeroPageYIndexed, true),
		new(CpuInstruction.Clv, CpuAddressingMode.Implied),
		new(CpuInstruction.Lda, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Tsx, CpuAddressingMode.Implied),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Ldy, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Lda, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Ldx, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Lax, CpuAddressingMode.AbsoluteYIndexed, true),
		// 0xC0 - 0xCF
		new(CpuInstruction.Cpy, CpuAddressingMode.Immediate),
		new(CpuInstruction.Cmp, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.Nop, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Dcp, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Cpy, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Cmp, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Dec, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Dcp, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Iny, CpuAddressingMode.Implied),
		new(CpuInstruction.Cmp, CpuAddressingMode.Immediate),
		new(CpuInstruction.Dex, CpuAddressingMode.Implied),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Cpy, CpuAddressingMode.Absolute),
		new(CpuInstruction.Cmp, CpuAddressingMode.Absolute),
		new(CpuInstruction.Dec, CpuAddressingMode.Absolute),
		new(CpuInstruction.Dcp, CpuAddressingMode.Absolute, true),
		// 0xD0 - 0xDF
		new(CpuInstruction.Bne, CpuAddressingMode.Relative),
		new(CpuInstruction.Cmp, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Dcp, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Cmp, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Dec, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Dcp, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Cld, CpuAddressingMode.Implied),
		new(CpuInstruction.Cmp, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Dcp, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.Cmp, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Dec, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Dcp, CpuAddressingMode.AbsoluteXIndexed, true),
		// 0xE0 - 0xEF
		new(CpuInstruction.Cpx, CpuAddressingMode.Immediate),
		new(CpuInstruction.Sbc, CpuAddressingMode.XIndexedIndirect),
		new(CpuInstruction.Nop, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Isc, CpuAddressingMode.XIndexedIndirect, true),
		new(CpuInstruction.Cpx, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Sbc, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Inc, CpuAddressingMode.ZeroPage),
		new(CpuInstruction.Isc, CpuAddressingMode.ZeroPage, true),
		new(CpuInstruction.Inx, CpuAddressingMode.Implied),
		new(CpuInstruction.Sbc, CpuAddressingMode.Immediate),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Sbc, CpuAddressingMode.Immediate, true),
		new(CpuInstruction.Cpx, CpuAddressingMode.Absolute),
		new(CpuInstruction.Sbc, CpuAddressingMode.Absolute),
		new(CpuInstruction.Inc, CpuAddressingMode.Absolute),
		new(CpuInstruction.Isc, CpuAddressingMode.Absolute, true),
		// 0xF0 - 0xFF
		new(CpuInstruction.Beq, CpuAddressingMode.Relative),
		new(CpuInstruction.Sbc, CpuAddressingMode.IndirectYIndexed),
		new(CpuInstruction.NotImplemented, CpuAddressingMode.NotImplemented),
		new(CpuInstruction.Isc, CpuAddressingMode.IndirectYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Sbc, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Inc, CpuAddressingMode.ZeroPageXIndexed),
		new(CpuInstruction.Isc, CpuAddressingMode.ZeroPageXIndexed, true),
		new(CpuInstruction.Sed, CpuAddressingMode.Implied),
		new(CpuInstruction.Sbc, CpuAddressingMode.AbsoluteYIndexed),
		new(CpuInstruction.Nop, CpuAddressingMode.Implied, true),
		new(CpuInstruction.Isc, CpuAddressingMode.AbsoluteYIndexed, true),
		new(CpuInstruction.Nop, CpuAddressingMode.AbsoluteXIndexed, true),
		new(CpuInstruction.Sbc, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Inc, CpuAddressingMode.AbsoluteXIndexed),
		new(CpuInstruction.Isc, CpuAddressingMode.AbsoluteXIndexed, true)
	];


	private const ushort _interruptVectorReset = 0xFFFC;
	private const ushort _interruptVectorNmi = 0xFFFA;
	private const ushort _interruptVectorIrq = 0xFFFE;

	private ushort _regPc;
	private byte _regSpLow;

	private bool _flagCarry = false;
	private bool _flagZero = false;
	private bool _flagInterruptDisable = true;
	private bool _flagDecimal = false;
	private bool _flagOverflow = false;
	private bool _flagNegative = false;

	private byte _regA;
	private byte _regX;
	private byte _regY;

	private ushort _internalAddress;
	private byte _internalOperand;
	private CpuOperation _op = CpuOperation.None;
	private int _opCycle;

	private bool _signalInterruptReset = false;
	private bool _signalInterruptNmi = false;
	private bool _signalInterruptIrq = false;

	private bool _servicingInterrupt = false;
	private bool _delayIrq = false;
	private bool _delayAllInterrupts = false;

	private ushort RegSp
	{
		get => (ushort)(0x100 | _regSpLow);
		set => _regSpLow = (byte)(value & 0xFF);
	}

	private byte GetStatusByte(bool setBFlag)
	{
		return (byte)(
			(_flagCarry ? 1 : 0) |
			(_flagZero ? 1 << 1 : 0) |
			(_flagInterruptDisable ? 1 << 2 : 0) |
			(_flagDecimal ? 1 << 3 : 0) |
			(setBFlag ? 1 << 4 : 0) |
			(1 << 5) |
			(_flagOverflow ? 1 << 6 : 0) |
			(_flagNegative ? 1 << 7 : 0)
		);
	}

	private void SetStatusByte(byte value)
	{
		_flagCarry = (value & 1) != 0;
		_flagZero = ((value >> 1) & 1) != 0;
		_flagInterruptDisable = ((value >> 2) & 1) != 0;
		_flagDecimal = ((value >> 3) & 1) != 0;
		_flagOverflow = ((value >> 6) & 1) != 0;
		_flagNegative = ((value >> 7) & 1) != 0;
	}

	private byte FetchOperationByte()
	{
		return bus.ReadByte(_regPc++);
	}

	private void FetchNextOperation()
	{
		var opcode = FetchOperationByte();
		_op = _operationsByOpcode[opcode];
		if (_op.Instruction == CpuInstruction.NotImplemented)
			throw new NotImplementedException($"Opcode 0x{opcode:X2} not implemented.");
	}

	private ulong _traceCycles = 0;

	private string GenerateTraceLine()
	{
		var b0 = bus.ReadByte(_regPc, true);
		var b1 = bus.ReadByte((ushort)(_regPc + 1), true);
		var b2 = bus.ReadByte((ushort)(_regPc + 2), true);

		var operand16 = b1 | (b2 << 8);

		var operation = _operationsByOpcode[b0];
		var instructionString = (operation.IsIllegal ? "*" : " ") + operation.Instruction.ToString().ToUpper();

		var disassembly = $"{_regPc:X4}  {b0:X2} ";

		disassembly += operation.AddressingMode switch
		{
			CpuAddressingMode.Accumulator => $"      {$"{instructionString} A",-33}",
			CpuAddressingMode.Absolute => $"{b1:X2} {b2:X2} {$"{instructionString} ${operand16:X4}",-33}",
			CpuAddressingMode.AbsoluteXIndexed => $"{b1:X2} {b2:X2} {$"{instructionString} ${operand16:X4},X",-33}",
			CpuAddressingMode.AbsoluteYIndexed => $"{b1:X2} {b2:X2} {$"{instructionString} ${operand16:X4},Y",-33}",
			CpuAddressingMode.Immediate => $"{b1:X2}    {$"{instructionString} #${b1:X2}",-33}",
			CpuAddressingMode.Implied => $"      {instructionString,-33}",
			CpuAddressingMode.Indirect => $"{b1:X2} {b2:X2} {$"{instructionString} (${operand16:X4})",-33}",
			CpuAddressingMode.XIndexedIndirect => $"{b1:X2}    {$"{instructionString} (${b1:X2},X)",-33}",
			CpuAddressingMode.IndirectYIndexed => $"{b1:X2}    {$"{instructionString} (${b1:X2}),Y",-33}",
			CpuAddressingMode.Relative => $"{b1:X2}    {$"{instructionString} *{((sbyte)b1 >= -2 ? "+" : "")}{(sbyte)b1 + 2}",-33}",
			CpuAddressingMode.ZeroPage => $"{b1:X2}    {$"{instructionString} ${b1:X2}",-33}",
			CpuAddressingMode.ZeroPageXIndexed => $"{b1:X2}    {$"{instructionString} ${b1:X2},X",-33}",
			CpuAddressingMode.ZeroPageYIndexed => $"{b1:X2}    {$"{instructionString} ${b1:X2},Y",-33}",
			CpuAddressingMode.NotImplemented => $"{b1:X2} {b2:X2} {$"{instructionString} ???",-33}",
			_ => throw new UnreachableException()
		};

		return $"{disassembly} A:{_regA:X2} X:{_regX:X2} Y:{_regY:X2} P:{GetStatusByte(false):X2} SP:{_regSpLow:X2} CYC:{_traceCycles}";
	}

	/// <summary>
	/// Immediately triggers a RESET interrupt the reset vector located at 0xFFFC into the PC register.
	/// </summary>
	public void Reset()
	{
		_op = CpuOperation.None; // Do not wait until the current instruction has finished executing
		_traceCycles = 0; // Reset trace cycle counting
		_signalInterruptReset = true;
	}

	public void Tick()
	{
		_opCycle++;

		if (_op.Instruction == CpuInstruction.None && !_servicingInterrupt)
		{
			Console.WriteLine(GenerateTraceLine());

			FetchNextOperation();

			// Always process a reset, process NMIs if not delaying all interrupts, process IRQs if neither delaying all interrupts nor delaing IRQs specifically
			if (_signalInterruptReset || (_signalInterruptNmi && !_delayAllInterrupts) || (_signalInterruptIrq && !_delayAllInterrupts && !_delayIrq))
			{
				_op = _operationsByOpcode[0]; // Force BRK into the internal instruction register
				_servicingInterrupt = true; // Set a flag indicating that the "B flag" is not to be set
				_regPc--; // Undo PC increment from instruction fetch
			}

			_opCycle = 1;
			_delayAllInterrupts = false;
			_delayIrq = false;
		}

		switch (_op.AddressingMode)
		{
			case CpuAddressingMode.Accumulator: ExecAddressingAccumulatorOrImplied(); break;
			case CpuAddressingMode.Absolute: ExecAddressingAbsolute(); break;
			case CpuAddressingMode.AbsoluteXIndexed: ExecAddressingAbsoluteIndexed(_regX); break;
			case CpuAddressingMode.AbsoluteYIndexed: ExecAddressingAbsoluteIndexed(_regY); break;
			case CpuAddressingMode.Immediate: ExecAddressingImmediate(); break;
			case CpuAddressingMode.Implied: ExecAddressingAccumulatorOrImplied(); break;
			case CpuAddressingMode.Indirect: ExecAddressingIndirect(); break;
			case CpuAddressingMode.XIndexedIndirect: ExecAddressingXIndexedIndirect(); break;
			case CpuAddressingMode.IndirectYIndexed: ExecAddressingIndirectYIndexed(); break;
			case CpuAddressingMode.Relative: ExecAddressingRelative(); break;
			case CpuAddressingMode.ZeroPage: ExecAddressingZeroPage(); break;
			case CpuAddressingMode.ZeroPageXIndexed: ExecAddressingZeroPageIndexed(_regX); break;
			case CpuAddressingMode.ZeroPageYIndexed: ExecAddressingZeroPageIndexed(_regY); break;
			default: throw new UnreachableException();
		}

		_traceCycles++;
	}

	private void ExecAddressingAbsolute()
	{
		switch (_op.Instruction)
		{
			case CpuInstruction.Jmp: SpecialJmp(); break;
			case CpuInstruction.Jsr: SpecialJsr(); break;
			default:
				switch (_op.Type)
				{
					case CpuOperationType.Read:
						switch (_opCycle)
						{
							case 1: // fetch opcode, increment PC
								break;
							case 2: // fetch low byte of address, increment PC
								_internalAddress = FetchOperationByte();
								break;
							case 3: // fetch high byte of address, increment PC
								_internalAddress |= (ushort)(FetchOperationByte() << 8);
								break;
							case 4: // read from effective address
								_internalOperand = bus.ReadByte(_internalAddress);
								ExecInst();
								_op = CpuOperation.None;
								break;
						}
						break;
					case CpuOperationType.ReadModifyWrite:
						switch (_opCycle)
						{
							case 1: // fetch opcode, increment PC
								break;
							case 2: // fetch low byte of address, increment PC
								_internalAddress = FetchOperationByte();
								break;
							case 3: // fetch high byte of address, increment PC
								_internalAddress |= (ushort)(FetchOperationByte() << 8);
								break;
							case 4: // read from effective address
								_internalOperand = bus.ReadByte(_internalAddress);
								break;
							case 5: // write the value back to effective address, and do the operation on it
								bus.WriteByte(_internalAddress, _internalOperand);
								ExecInst();
								break;
							case 6: // write the new value to effective address
								bus.WriteByte(_internalAddress, _internalOperand);
								_op = CpuOperation.None;
								break;
						}
						break;
					case CpuOperationType.Write:
						switch (_opCycle)
						{
							case 1: // fetch opcode, increment PC
								break;
							case 2: // fetch low byte of address, increment PC
								_internalAddress = FetchOperationByte();
								break;
							case 3: // fetch high byte of address, increment PC
								_internalAddress |= (ushort)(FetchOperationByte() << 8);
								break;
							case 4: // write register to effective address
								ExecInst();
								bus.WriteByte(_internalAddress, _internalOperand);
								_op = CpuOperation.None;
								break;
						}
						break;
				}
				break;
		}
		return;

		void SpecialJmp()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // fetch low address byte, increment PC
					_internalAddress = FetchOperationByte();
					break;
				case 3: // copy low address byte to PCL, fetch high address byte to PCH
					_internalAddress |= (ushort)(FetchOperationByte() << 8);
					_regPc = _internalAddress;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialJsr()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // fetch low address byte, increment PC
					_internalAddress = FetchOperationByte();
					break;
				case 3: // internal operation (predecrement S?)
					break;
				case 4: // push PCH on stack, decrement S
					bus.WriteByte(RegSp, (byte)(_regPc >> 8));
					_regSpLow--;
					break;
				case 5: // push PCL on stack, decrement S
					bus.WriteByte(RegSp, (byte)(_regPc & 0xFF));
					_regSpLow--;
					break;
				case 6: // copy low address byte to PCL, fetch high address byte to PCH
					_internalAddress |= (ushort)(FetchOperationByte() << 8);
					_regPc = _internalAddress;
					_op = CpuOperation.None;
					break;
			}
		}
	}

	private void ExecAddressingAccumulatorOrImplied()
	{
		switch (_op.Instruction)
		{
			case CpuInstruction.Brk: SpecialBrk(); break;
			case CpuInstruction.Rti: SpecialRti(); break;
			case CpuInstruction.Rts: SpecialRts(); break;
			case CpuInstruction.Pha: SpecialPha(); break;
			case CpuInstruction.Php: SpecialPhp(); break;
			case CpuInstruction.Pla: SpecialPla(); break;
			case CpuInstruction.Plp: SpecialPlp(); break;
			default:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // read next instruction byte (and throw it away)
						bus.ReadByte(_regPc);

						// Accumulator becomes the operand
						_internalOperand = _regA;
						var prevOperand = _internalOperand;
						ExecInst();
						// If the operand changed, make acc reflect this
						if (_internalOperand != prevOperand)
							_regA = _internalOperand;

						_op = CpuOperation.None;
						break;
				}
				break;
		}
		return;


		void SpecialBrk()
		{
			// Reset reuses interrupt logic
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
				case 2: // read next instruction byte (and throw it away), increment PC if not serving an interrupt
					bus.ReadByte(_regPc);
					if (!_servicingInterrupt)
						_regPc++;

					break;
				case 3: // push PCH on stack, decrement S
					bus.WriteByte(RegSp, (byte)(_regPc >> 8));
					_regSpLow--;
					break;
				case 4: // push PCL on stack, decrement S
					bus.WriteByte(RegSp, (byte)(_regPc & 0xFF));
					_regSpLow--;

					// Facilitate "interrupt hijacking"
					if (_signalInterruptReset)
					{
						_signalInterruptReset = false;
						_signalInterruptNmi = false;
						_signalInterruptIrq = false;
						_internalAddress = _interruptVectorReset;
					}
					else if (_signalInterruptNmi)
					{
						_signalInterruptNmi = false;
						_internalAddress = _interruptVectorNmi;
					}
					else // IRQ and BRK
					{
						_signalInterruptIrq = false;
						_internalAddress = _interruptVectorIrq;
					}

					break;
				case 5: // push P on stack (with B flag set if not servicing an interrupt), decrement S
					bus.WriteByte(RegSp, GetStatusByte(!_servicingInterrupt));
					_regSpLow--;
					break;
				case 6: // Fetch PCL
					_regPc = bus.ReadByte(_internalAddress++);
					_flagInterruptDisable = true;
					break;
				case 7: // Fetch PCH
					_regPc |= (ushort)(bus.ReadByte(_internalAddress++) << 8);
					_servicingInterrupt = false; // No longer servicing an interrupt
					// Interrupts are always ignored for the first instruction of an interrupt handler
					//	(obviously true for IRQ and BRK as the interrupt disable flag is set, but even NMIs are delayed!)
					_delayAllInterrupts = true;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialRti()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // increment S
					_regSpLow++;
					break;
				case 4: // pull P from stack, increment S
					SetStatusByte(bus.ReadByte(RegSp));
					_regSpLow++;
					break;
				case 5: // pull PCL from stack, increment S
					_regPc = bus.ReadByte(RegSp);
					_regSpLow++;
					break;
				case 6: // pull PCH from stack
					_regPc |= (ushort)(bus.ReadByte(RegSp) << 8);
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialRts()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // increment S
					_regSpLow++;
					break;
				case 4: // pull PCL from stack, increment S
					_regPc = bus.ReadByte(RegSp);
					_regSpLow++;
					break;
				case 5: // pull PCH from stack
					_regPc |= (ushort)(bus.ReadByte(RegSp) << 8);
					break;
				case 6: // increment PC
					_regPc++;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialPha()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // push register on stack, decrement S
					bus.WriteByte(RegSp, _regA);
					_regSpLow--;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialPhp()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // push register on stack, decrement S
					bus.WriteByte(RegSp, GetStatusByte(true));
					_regSpLow--;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialPla()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // increment S
					_regSpLow++;
					break;
				case 4: // pull register from stack
					_regA = bus.ReadByte(RegSp);
					_flagNegative = (sbyte)_regA < 0;
					_flagZero = _regA == 0;
					_op = CpuOperation.None;
					break;
			}
		}

		void SpecialPlp()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // read next instruction byte (and throw it away)
					bus.ReadByte(_regPc);
					break;
				case 3: // increment S
					_regSpLow++;
					break;
				case 4: // pull register from stack
					SetStatusByte(bus.ReadByte(RegSp));
					_op = CpuOperation.None;
					break;
			}
		}
	}

	private void ExecAddressingIndirect()
	{
		switch (_op.Instruction)
		{
			case CpuInstruction.Jmp: SpecialJmp(); break;
			default: throw new UnreachableException();
		}
		return;

		void SpecialJmp()
		{
			switch (_opCycle)
			{
				case 1: // fetch opcode, increment PC
					break;
				case 2: // fetch pointer address low, increment PC
					_internalAddress = FetchOperationByte();
					break;
				case 3: // fetch pointer address high, increment PC
					_internalAddress |= (ushort)(FetchOperationByte() << 8);
					break;
				case 4: // fetch low address to latch
					_internalOperand = bus.ReadByte(_internalAddress);
					break;
				case 5: // fetch PCH, copy latch to PCL
					_internalAddress = (ushort)((_internalAddress & 0xFF00) | ((_internalAddress + 1) & 0x00FF));
					_regPc = (ushort)((_internalOperand) | (bus.ReadByte(_internalAddress) << 8));
					_op = CpuOperation.None;
					break;
			}
		}

	}

	private void ExecAddressingXIndexedIndirect()
	{
		switch (_op.Type)
		{
			case CpuOperationType.Read:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // read from the address, add X to it
						bus.ReadByte(_internalOperand);
						_internalOperand += _regX;
						break;
					case 4: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 5: // fetch effective address high
						_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
						break;
					case 6: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						ExecInst();
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.ReadModifyWrite:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // read from the address, add X to it
						bus.ReadByte(_internalOperand);
						_internalOperand += _regX;
						break;
					case 4: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 5: // fetch effective address high
						_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
						break;
					case 6: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						break;
					case 7: // write the value back to effective address, and do the operation on it
						bus.WriteByte(_internalAddress, _internalOperand);
						ExecInst();
						break;
					case 8: // write the new value to effective address;
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.Write:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // read from the address, add X to it
						bus.ReadByte(_internalOperand);
						_internalOperand += _regX;
						break;
					case 4: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 5: // fetch effective address high
						_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
						break;
					case 6: // write to effective address
						ExecInst();
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
		}
	}

	private void ExecAddressingIndirectYIndexed()
	{
		switch (_op.Type)
		{
			case CpuOperationType.Read:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 4: // fetch effective address high, add Y to low byte of effective address
						// Add Y to low byte, if high byte does not need to be fixed skip cycle 5
						_internalAddress += _regY;
						if (_internalAddress <= 0xFF)
							_opCycle++;
						_internalAddress &= 0x00FF;

						_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
						break;
					case 5: // read from effective address, fix high byte of effective address
						bus.ReadByte(_internalAddress);
						_internalAddress += 0x0100;
						break;
					case 6: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						ExecInst();
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.ReadModifyWrite:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 4: // fetch effective address high, add Y to low byte of effective address
						{
							// Add Y to low byte, if high byte does not need to be fixed skip cycle 5
							_internalAddress += _regY;
							var mustAdjust = _internalAddress > 0xFF;
							_internalAddress &= 0x00FF;

							_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
							_internalOperand = (byte)(mustAdjust ? 1 : 0);
							break;
						}
					case 5: // read from effective address, fix high byte of effective address
						bus.ReadByte(_internalAddress);
						if (_internalOperand != 0)
							_internalAddress += 0x100;
						break;
					case 6: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						break;
					case 7: // write the value back to effective address, and do the operation on it
						bus.WriteByte(_internalAddress, _internalOperand);
						ExecInst();
						break;
					case 8: // write the new value to effective address
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.Write:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch pointer address, increment PC
						_internalOperand = FetchOperationByte();
						break;
					case 3: // fetch effective address low
						_internalAddress = bus.ReadByte(_internalOperand++);
						break;
					case 4: // fetch effective address high, add Y to low byte of effective address
						{
							// Add Y to low byte, if high byte does not need to be fixed skip cycle 5
							_internalAddress += _regY;
							var mustAdjust = _internalAddress > 0xFF;
							_internalAddress &= 0x00FF;

							_internalAddress |= (ushort)(bus.ReadByte(_internalOperand) << 8);
							_internalOperand = (byte)(mustAdjust ? 1 : 0);
							break;
						}
					case 5: // read from effective address, fix high byte of effective address
						bus.ReadByte(_internalAddress);
						if (_internalOperand != 0)
							_internalAddress += 0x100;
						break;
					case 6: // write to effective address
						ExecInst();
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
		}
	}

	private void ExecAddressingRelative()
	{
		switch (_opCycle)
		{
			case 1: // fetch opcode, increment PC
				break;
			case 2: // fetch operand, increment PC
				{
					_internalOperand = FetchOperationByte();

					var branchCondition = _op.Instruction switch
					{
						CpuInstruction.Bcc => !_flagCarry,
						CpuInstruction.Bcs => _flagCarry,
						CpuInstruction.Beq => _flagZero,
						CpuInstruction.Bmi => _flagNegative,
						CpuInstruction.Bne => !_flagZero,
						CpuInstruction.Bpl => !_flagNegative,
						CpuInstruction.Bvc => !_flagOverflow,
						CpuInstruction.Bvs => _flagOverflow,
						_ => throw new UnreachableException()
					};

					if (!branchCondition)
						_op = CpuOperation.None;

					break;
				}
			case 3: // Fetch opcode of next instruction, add operand to PCL.
				{
					// Even if the branch is taken the next instruction opcode is fetched
					bus.ReadByte(_regPc);

					var expectedPc = (ushort)(_regPc + (sbyte)_internalOperand);

					_regPc = (ushort)(
						(_regPc & 0xFF00) | // Preserve high byte
						((_regPc + (sbyte)_internalOperand) & 0x00FF) // Add operand to low byte
					);

					// If expected PC and actual PC match no additional cycle is required to fix PCs high byte
					if (_regPc == expectedPc)
						_op = CpuOperation.None;

					break;
				}
			case 4: // Fetch opcode of next instruction. Fix PCH.
				bus.ReadByte(_regPc);
				_regPc += 0x100;
				_op = CpuOperation.None;
				break;
		}
	}

	private void ExecAddressingZeroPage()
	{
		switch (_op.Type)
		{
			case CpuOperationType.Read:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						ExecInst();
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.ReadModifyWrite:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						break;
					case 4: // write the value back to effective address, and do the operation on it
						bus.WriteByte(_internalAddress, _internalOperand);
						ExecInst();
						break;
					case 5: // write the new value to effective address
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.Write:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // write register to effective address
						ExecInst();
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
		}
	}

	private void ExecAddressingZeroPageIndexed(byte regIndex)
	{
		switch (_op.Type)
		{
			case CpuOperationType.Read:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // read from address, add index register to it
						bus.ReadByte(_internalAddress);
						_internalAddress += regIndex;
						_internalAddress &= 0x00FF;
						break;
					case 4: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						ExecInst();
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.ReadModifyWrite:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // read from address, add index register to it
						bus.ReadByte(_internalAddress);
						_internalAddress += regIndex;
						_internalAddress &= 0x00FF;
						break;
					case 4: // read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						break;
					case 5: // write the value back to effective address, and do the operation on it
						bus.WriteByte(_internalAddress, _internalOperand);
						ExecInst();
						break;
					case 6: // write the new value to effective address
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.Write:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // read from address, add index register to it
						bus.ReadByte(_internalAddress);
						_internalAddress += regIndex;
						_internalAddress &= 0x00FF;
						break;
					case 4: // write to effective address
						ExecInst();
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
		}
	}

	private void ExecAddressingAbsoluteIndexed(byte regIndex)
	{
		switch (_op.Type)
		{
			case CpuOperationType.Read:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch low byte of address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // fetch high byte of address, add index register to low address byte, increment PC
						_internalAddress += regIndex;
						if (_internalAddress <= 0xFF)
							_opCycle++;
						_internalAddress &= 0x00FF;

						_internalAddress |= (ushort)(FetchOperationByte() << 8);
						break;
					case 4: // read from effective address, fix the high byte of effective address
						bus.ReadByte(_internalAddress);
						_internalAddress += 0x0100;
						break;
					case 5: // re-read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						ExecInst();
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.ReadModifyWrite:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch low byte of address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // fetch high byte of address, add index register to low address byte, increment PC
						_internalAddress += regIndex;
						var mustAdjust = _internalAddress > 0xFF;
						_internalAddress &= 0x00FF;

						_internalAddress |= (ushort)(FetchOperationByte() << 8);
						_internalOperand = (byte)(mustAdjust ? 1 : 0);
						break;
					case 4: // read from effective address, fix the high byte of effective address
						bus.ReadByte(_internalAddress);
						if (_internalOperand != 0)
							_internalAddress += 0x100;
						break;
					case 5: // re-read from effective address
						_internalOperand = bus.ReadByte(_internalAddress);
						break;
					case 6: // write the value back to effective address, and do the operation on it
						bus.WriteByte(_internalAddress, _internalOperand);
						ExecInst();
						break;
					case 7: // write the new value to effective address
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
			case CpuOperationType.Write:
				switch (_opCycle)
				{
					case 1: // fetch opcode, increment PC
						break;
					case 2: // fetch low byte of address, increment PC
						_internalAddress = FetchOperationByte();
						break;
					case 3: // fetch high byte of address, add index register to low address byte, increment PC
						_internalAddress += regIndex;
						var mustAdjust = _internalAddress > 0xFF;
						_internalAddress &= 0x00FF;

						_internalAddress |= (ushort)(FetchOperationByte() << 8);
						_internalOperand = (byte)(mustAdjust ? 1 : 0);
						break;
					case 4: // read from effective address, fix the high byte of effective address
						bus.ReadByte(_internalAddress);
						if (_internalOperand != 0)
							_internalAddress += 0x100;
						break;
					case 5: // write to effective address
						ExecInst();
						bus.WriteByte(_internalAddress, _internalOperand);
						_op = CpuOperation.None;
						break;
				}
				break;
		}
	}

	private void ExecAddressingImmediate()
	{
		switch (_opCycle)
		{
			case 1: // fetch opcode, increment PC
				break;
			case 2: // fetch value, increment PC
				_internalOperand = FetchOperationByte();
				ExecInst();
				_op = CpuOperation.None;
				break;
		}
	}

	private void ExecInst()
	{
		switch (_op.Instruction)
		{
			case CpuInstruction.Adc: InstAdc(); break;
			case CpuInstruction.And: InstAnd(); break;
			case CpuInstruction.Asl: InstAsl(); break;
			case CpuInstruction.Bit: InstBit(); break;
			case CpuInstruction.Clc: InstClc(); break;
			case CpuInstruction.Cld: InstCld(); break;
			case CpuInstruction.Cli: InstCli(); break;
			case CpuInstruction.Clv: InstClv(); break;
			case CpuInstruction.Cmp: InstCmp(); break;
			case CpuInstruction.Cpx: InstCpx(); break;
			case CpuInstruction.Cpy: InstCpy(); break;
			case CpuInstruction.Dec: InstDec(); break;
			case CpuInstruction.Dex: InstDex(); break;
			case CpuInstruction.Dey: InstDey(); break;
			case CpuInstruction.Eor: InstEor(); break;
			case CpuInstruction.Inc: InstInc(); break;
			case CpuInstruction.Inx: InstInx(); break;
			case CpuInstruction.Iny: InstIny(); break;
			case CpuInstruction.Lda: InstLda(); break;
			case CpuInstruction.Ldx: InstLdx(); break;
			case CpuInstruction.Ldy: InstLdy(); break;
			case CpuInstruction.Lsr: InstLsr(); break;
			case CpuInstruction.Nop: InstNop(); break;
			case CpuInstruction.Ora: InstOra(); break;
			case CpuInstruction.Rol: InstRol(); break;
			case CpuInstruction.Ror: InstRor(); break;
			case CpuInstruction.Sbc: InstSbc(); break;
			case CpuInstruction.Sec: InstSec(); break;
			case CpuInstruction.Sed: InstSed(); break;
			case CpuInstruction.Sei: InstSei(); break;
			case CpuInstruction.Sta: InstSta(); break;
			case CpuInstruction.Stx: InstStx(); break;
			case CpuInstruction.Sty: InstSty(); break;
			case CpuInstruction.Tax: InstTax(); break;
			case CpuInstruction.Tay: InstTay(); break;
			case CpuInstruction.Tsx: InstTsx(); break;
			case CpuInstruction.Txa: InstTxa(); break;
			case CpuInstruction.Txs: InstTxs(); break;
			case CpuInstruction.Tya: InstTya(); break;

			case CpuInstruction.Dcp: InstDcp(); break;
			case CpuInstruction.Isc: InstIsc(); break;
			case CpuInstruction.Lax: InstLax(); break;
			case CpuInstruction.Rla: InstRla(); break;
			case CpuInstruction.Rra: InstRra(); break;
			case CpuInstruction.Sax: InstSax(); break;
			case CpuInstruction.Slo: InstSlo(); break;
			case CpuInstruction.Sre: InstSre(); break;
			default: throw new NotImplementedException($"Instruction {_op.Instruction} not implemented.");
		}
		return;

		void InstAdc()
		{
			var result = _regA + _internalOperand + (_flagCarry ? 1 : 0);
			var signedResult = (sbyte)_regA + (sbyte)_internalOperand + (_flagCarry ? 1 : 0);
			_regA = (byte)result;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
			_flagCarry = result > 0xFF;
			_flagOverflow = ((sbyte)_regA > 0 && signedResult < 0) || ((sbyte)_regA < 0 && signedResult > 0);
		}

		void InstAnd()
		{
			_regA &= _internalOperand;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstAsl()
		{
			_flagCarry = ((_internalOperand >> 7) & 1) != 0;
			_internalOperand <<= 1;
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstBit()
		{
			_flagNegative = ((_internalOperand >> 7) & 1) != 0;
			_flagOverflow = ((_internalOperand >> 6) & 1) != 0;
			_flagZero = (_internalOperand & _regA) == 0;
		}

		void InstClc() => _flagCarry = false;
		void InstCld() => _flagDecimal = false;
		void InstCli() => _flagInterruptDisable = false;
		void InstClv() => _flagOverflow = false;

		void InstCmp()
		{
			var result = _regA - _internalOperand;
			_flagNegative = (sbyte)result < 0;
			_flagZero = (sbyte)result == 0;
			_flagCarry = _regA >= _internalOperand;
		}

		void InstCpx()
		{
			var result = _regX - _internalOperand;
			_flagNegative = (sbyte)result < 0;
			_flagZero = (sbyte)result == 0;
			_flagCarry = _regX >= _internalOperand;
		}

		void InstCpy()
		{
			var result = _regY - _internalOperand;
			_flagNegative = (sbyte)result < 0;
			_flagZero = (sbyte)result == 0;
			_flagCarry = _regY >= _internalOperand;
		}

		void InstDec()
		{
			_internalOperand--;
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstDex()
		{
			_regX--;
			_flagNegative = (sbyte)_regX < 0;
			_flagZero = _regX == 0;
		}

		void InstDey()
		{
			_regY--;
			_flagNegative = (sbyte)_regY < 0;
			_flagZero = _regY == 0;
		}

		void InstEor()
		{
			_regA ^= _internalOperand;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstInc()
		{
			_internalOperand++;
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstInx()
		{
			_regX++;
			_flagNegative = (sbyte)_regX < 0;
			_flagZero = _regX == 0;
		}

		void InstIny()
		{
			_regY++;
			_flagNegative = (sbyte)_regY < 0;
			_flagZero = _regY == 0;
		}

		void InstLda()
		{
			_regA = _internalOperand;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstLdx()
		{
			_regX = _internalOperand;
			_flagNegative = (sbyte)_regX < 0;
			_flagZero = _regX == 0;
		}

		void InstLdy()
		{
			_regY = _internalOperand;
			_flagNegative = (sbyte)_regY < 0;
			_flagZero = _regY == 0;
		}

		void InstLsr()
		{
			_flagCarry = (_internalOperand & 1) != 0;
			_internalOperand >>= 1;
			_flagNegative = false;
			_flagZero = _internalOperand == 0;
		}

		static void InstNop() { }

		void InstOra()
		{
			_regA |= _internalOperand;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstRol()
		{
			var oldCarry = _flagCarry ? 1 : 0;
			_flagCarry = ((_internalOperand >> 7) & 1) != 0;
			_internalOperand <<= 1;
			_internalOperand |= (byte)oldCarry;
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstRor()
		{
			var oldCarry = _flagCarry ? 1 : 0;
			_flagCarry = (_internalOperand & 1) != 0;
			_internalOperand >>= 1;
			_internalOperand |= (byte)(oldCarry << 7);
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstSbc()
		{
			var result = _regA - _internalOperand - (_flagCarry ? 0 : 1);
			var signedResult = (sbyte)_regA - (sbyte)_internalOperand - (_flagCarry ? 0 : 1);
			var resultByte = (byte)result;
			_flagNegative = (sbyte)result < 0;
			_flagZero = (sbyte)result == 0;
			_flagCarry = _regA >= _internalOperand;
			_flagOverflow = ((sbyte)resultByte > 0 && signedResult < 0) || ((sbyte)resultByte < 0 && signedResult > 0);

			_regA = resultByte;
		}

		void InstSec() => _flagCarry = true;
		void InstSed() => _flagDecimal = true;
		void InstSei() => _flagInterruptDisable = true;
		void InstSta() => _internalOperand = _regA;
		void InstStx() => _internalOperand = _regX;
		void InstSty() => _internalOperand = _regY;

		void InstTax()
		{
			_regX = _regA;
			_flagNegative = (sbyte)_regX < 0;
			_flagZero = _regX == 0;
		}

		void InstTay()
		{
			_regY = _regA;
			_flagNegative = (sbyte)_regY < 0;
			_flagZero = _regY == 0;
		}

		void InstTsx()
		{
			_regX = _regSpLow;
			_flagNegative = (sbyte)_regX < 0;
			_flagZero = _regX == 0;
		}

		void InstTxa()
		{
			_regA = _regX;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstTxs() => _regSpLow = _regX;

		void InstTya()
		{
			_regA = _regY;
			_flagNegative = (sbyte)_regA < 0;
			_flagZero = _regA == 0;
		}

		void InstDcp()
		{
			InstDec();
			InstCmp();
		}

		void InstIsc()
		{
			InstInc();
			InstSbc();
		}

		void InstLax()
		{
			_regA = _internalOperand;
			_regX = _internalOperand;
			_flagNegative = (sbyte)_internalOperand < 0;
			_flagZero = _internalOperand == 0;
		}

		void InstRla()
		{
			InstRol();
			InstAnd();
		}

		void InstRra()
		{
			InstRor();
			InstAdc();
		}

		void InstSax() => _internalOperand = (byte)(_regA & _regX);

		void InstSlo()
		{
			InstAsl();
			InstOra();
		}

		void InstSre()
		{
			InstLsr();
			InstEor();
		}
	}
}
