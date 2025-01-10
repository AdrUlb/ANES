using System.Diagnostics;

namespace ANES;

internal sealed class Nes : Computer
{
	private readonly Thread _thread;
	private readonly object _startStopLock = new();
	private bool _keepRunning = false;

	internal readonly byte[] Ram = new byte[0x800];
	internal readonly byte[] Vram = new byte[0x800];
	internal readonly byte[] PaletteRam = new byte[0x20];

	public override CpuBus CpuBus { get; }

	public PpuBus PpuBus { get; }

	internal readonly Controllers Controllers = new();
	internal readonly Cartridge Cartridge;
	internal readonly Ppu Ppu;
	private readonly Cpu _cpu;

	public Nes()
	{
		_thread = new(ThreadProc);
		CpuBus = new CpuBus(this);
		PpuBus = new PpuBus(this);
		Cartridge = new(this, @"C:\Stuff\Roms\nes\donkeykong.nes");
		Ppu = new(this);
		_cpu = new(this);
		_cpu.Reset();
	}

	private void ThreadProc()
	{
		ulong tick = 0;
		var start = Stopwatch.StartNew();

		while (_keepRunning)
		{
			if (tick % 4 == 0)
			{
				Ppu.Tick();

				if (Ppu.HandleNmi())
					_cpu.RaiseNmi();
			}

			if (tick % 12 == 0)
			{
				_cpu.Tick();
				// NOTE: signal IRQs AFTER CPU tick
				// maybe NMIs before?
			}

			tick++;
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
