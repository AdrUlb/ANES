namespace ANES;

internal sealed class Nes : IComputer
{
	private readonly Thread _thread;
	private readonly object _startStopLock = new();
	private bool _keepRunning = false;

	internal readonly byte[] Ram = new byte[0x800];
	internal readonly byte[] Vram = new byte[0x800];

	public IMemoryBus CpuMemoryBus { get; }

	internal readonly Cartridge Cartridge;
	private readonly Cpu _cpu;

	public Nes()
	{
		_thread = new(ThreadProc);
		CpuMemoryBus = new CpuMemoryBus(this);
		Cartridge = new(this, "Tests/nestest/nestest.nes");
		_cpu = new(this);
		_cpu.Reset();
	}

	private void ThreadProc()
	{
		while (_keepRunning)
		{
			_cpu.Tick();
			// NOTE: signal IRQs AFTER CPU tick
			// maybe NMIs before?
		}
	}

	/// <summary>
	/// Starts the emulation.
	/// </summary>
	public void Start()
	{
		lock (_startStopLock)
		{
			_keepRunning = true;
			_thread.Start();
		}
	}

	/// <summary>
	/// Stops the emulation.
	/// </summary>
	public void Stop()
	{
		lock (_startStopLock)
		{
			_keepRunning = false;
			SpinWait.SpinUntil(() => !_thread.IsAlive);
		}
	}

	public void Reset()
	{
		_cpu.Reset();
	}
}
