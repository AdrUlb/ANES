namespace ANES;

internal enum CpuAddressingMode
{
	NotImplemented,
	Accumulator,
	Absolute,
	AbsoluteXIndexed,
	AbsoluteYIndexed,
	Immediate,
	Implied,
	Indirect,
	XIndexedIndirect,
	IndirectYIndexed,
	Relative,
	ZeroPage,
	ZeroPageXIndexed,
	ZeroPageYIndexed,
}
