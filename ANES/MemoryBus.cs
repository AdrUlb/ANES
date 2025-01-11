namespace ANES;

public abstract class MemoryBus
{
	public abstract byte ReadByte(ushort address, bool suppressSideEffects = false);
	public abstract void WriteByte(ushort address, byte value);
}
