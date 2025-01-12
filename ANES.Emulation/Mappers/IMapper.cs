namespace ANES.Emulation.Mappers;

public interface IMapper
{
	public string Name { get; }

	public byte CpuReadByte(ushort address, bool suppressSideEffects = false);
	public void CpuWriteByte(ushort address, byte value);
	
	public byte PpuReadByte(ushort address, bool suppressSideEffects = false);
	public void PpuWriteByte(ushort address, byte value);
}
