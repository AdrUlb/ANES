using ANES;

//var nes = new Nes();
//nes.Start();

var computer = new TestComputer();
var cpu = new Cpu(computer)
{
	RegPc = 0x400
};

while (true)
{
	cpu.Tick();
}

internal sealed class TestBus : IMemoryBus
{
	private readonly byte[] _ram = new byte[0x1_0000];
	public TestBus() => File.ReadAllBytes("Tests/6502_65C02_functional_tests/6502_functional_test.bin").CopyTo(_ram, 0);

	public byte ReadByte(ushort address, bool suppressSideEffects = false) => _ram[address];

	public void WriteByte(ushort address, byte value) => _ram[address] = value;
}

internal sealed class TestComputer : IComputer
{
	public IMemoryBus CpuMemoryBus { get; } = new TestBus();
}
