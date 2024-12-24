namespace ANES;

internal sealed class Nes
{
	private readonly Thread _thread;
	private readonly object _startStopLock = new();
	private bool _keepRunning = false;

	private readonly CpuMemoryBus _cpuMemoryBus = new();
	private readonly Cpu _cpu;

	public Nes()
	{
		_thread = new(ThreadProc);
		_cpu = new(_cpuMemoryBus);
		_cpu.Reset();
	}

	private void ThreadProc()
	{
		while (_keepRunning)
		{
			_cpu.Tick();
			// NOTE: signal interrupts AFTER CPU tick
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
