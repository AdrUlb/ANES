using System.Diagnostics;

namespace ANES;

internal readonly struct CpuOperation(CpuInstruction instruction, CpuAddressingMode addressingMode, bool isIllegal = false)
{
	public static readonly CpuOperation None = new(CpuInstruction.None, 0);

	public readonly CpuInstruction Instruction = instruction;
	public readonly CpuAddressingMode AddressingMode = addressingMode;

	public readonly bool IsIllegal = isIllegal;

	public readonly CpuOperationType Type = instruction switch
	{
		CpuInstruction.NotImplemented => CpuOperationType.Implied,
		CpuInstruction.None => CpuOperationType.Implied,
		CpuInstruction.Adc => CpuOperationType.Read,
		CpuInstruction.And => CpuOperationType.Read,
		CpuInstruction.Asl => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Bcc => CpuOperationType.Implied,
		CpuInstruction.Bcs => CpuOperationType.Implied,
		CpuInstruction.Beq => CpuOperationType.Implied,
		CpuInstruction.Bit => CpuOperationType.Read,
		CpuInstruction.Bmi => CpuOperationType.Implied,
		CpuInstruction.Bne => CpuOperationType.Implied,
		CpuInstruction.Bpl => CpuOperationType.Implied,
		CpuInstruction.Brk => CpuOperationType.Implied,
		CpuInstruction.Bvc => CpuOperationType.Implied,
		CpuInstruction.Bvs => CpuOperationType.Implied,
		CpuInstruction.Clc => CpuOperationType.Implied,
		CpuInstruction.Cld => CpuOperationType.Implied,
		CpuInstruction.Cli => CpuOperationType.Implied,
		CpuInstruction.Clv => CpuOperationType.Implied,
		CpuInstruction.Cmp => CpuOperationType.Read,
		CpuInstruction.Cpx => CpuOperationType.Read,
		CpuInstruction.Cpy => CpuOperationType.Read,
		CpuInstruction.Dec => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Dex => CpuOperationType.Implied,
		CpuInstruction.Dey => CpuOperationType.Implied,
		CpuInstruction.Eor => CpuOperationType.Read,
		CpuInstruction.Inc => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Inx => CpuOperationType.Implied,
		CpuInstruction.Iny => CpuOperationType.Implied,
		CpuInstruction.Jmp => CpuOperationType.Implied,
		CpuInstruction.Jsr => CpuOperationType.Implied,
		CpuInstruction.Lda => CpuOperationType.Read,
		CpuInstruction.Ldx => CpuOperationType.Read,
		CpuInstruction.Ldy => CpuOperationType.Read,
		CpuInstruction.Lsr => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Nop => CpuOperationType.Read,
		CpuInstruction.Ora => CpuOperationType.Read,
		CpuInstruction.Pha => CpuOperationType.Implied,
		CpuInstruction.Php => CpuOperationType.Implied,
		CpuInstruction.Pla => CpuOperationType.Implied,
		CpuInstruction.Plp => CpuOperationType.Implied,
		CpuInstruction.Rol => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Ror => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Rti => CpuOperationType.Implied,
		CpuInstruction.Rts => CpuOperationType.Implied,
		CpuInstruction.Sbc => CpuOperationType.Read,
		CpuInstruction.Sec => CpuOperationType.Implied,
		CpuInstruction.Sed => CpuOperationType.Implied,
		CpuInstruction.Sei => CpuOperationType.Implied,
		CpuInstruction.Sta => CpuOperationType.Write,
		CpuInstruction.Stx => CpuOperationType.Write,
		CpuInstruction.Sty => CpuOperationType.Write,
		CpuInstruction.Tax => CpuOperationType.Implied,
		CpuInstruction.Tay => CpuOperationType.Implied,
		CpuInstruction.Tsx => CpuOperationType.Implied,
		CpuInstruction.Txa => CpuOperationType.Implied,
		CpuInstruction.Txs => CpuOperationType.Implied,
		CpuInstruction.Tya => CpuOperationType.Implied,

		CpuInstruction.Alr => CpuOperationType.Implied,
		CpuInstruction.Anc => CpuOperationType.Implied,
		CpuInstruction.Arr => CpuOperationType.Implied,
		CpuInstruction.Dcp => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Isc => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Lax => CpuOperationType.Read,
		CpuInstruction.Lxa => CpuOperationType.Implied,
		CpuInstruction.Rla => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Rra => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Sax => CpuOperationType.Write,
		CpuInstruction.Sbx => CpuOperationType.Implied,
		CpuInstruction.Shx => CpuOperationType.Write,
		CpuInstruction.Shy => CpuOperationType.Write,
		CpuInstruction.Slo => CpuOperationType.ReadModifyWrite,
		CpuInstruction.Sre => CpuOperationType.ReadModifyWrite,
		_ => throw new UnreachableException($"Operation type for {instruction} not specified.")
	};
}
