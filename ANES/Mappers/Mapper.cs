namespace ANES.Mappers;

internal abstract class Mapper
{
	public abstract string Name { get; }

	public abstract byte CpuReadByte(ushort address, bool suppressSideEffects = false);
	public abstract void CpuWriteByte(ushort address, byte value);
	
	public abstract byte PpuReadByte(ushort address, bool suppressSideEffects = false);
	public abstract void PpuWriteByte(ushort address, byte value);
}
