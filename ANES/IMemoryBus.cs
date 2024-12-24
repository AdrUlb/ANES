namespace ANES;

internal interface IMemoryBus
{
	public byte ReadByte(ushort address, bool suppressSideEffects = false);
	public void WriteByte(ushort address, byte value);
}
