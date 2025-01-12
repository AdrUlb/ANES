using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ANES.Emulation;

public sealed class Nes : Computer
{
	private const double _masterTicksPerSecond = 236250000.0 / 11.0;
	private const double _ppuTicksPerSecond = _masterTicksPerSecond / 4.0;
	private const double _framesPerSecond = 60.0;
	private const double _millisPerFrame = 1000.0 / _framesPerSecond;
	private const double _ppuTicksPerFrame = _ppuTicksPerSecond / _framesPerSecond;

	private Thread? _thread;
	private readonly Lock _startStopLock = new();
	private bool _keepRunning = false;

	public readonly byte[] Palette = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Composite_wiki.pal"));

	internal readonly byte[] Ram = new byte[0x800];
	internal readonly byte[] Vram = new byte[0x800];
	public readonly byte[] PaletteRam = new byte[0x20];

	public override CpuBus CpuBus { get; }

	public PpuBus PpuBus { get; }

	public Cartridge? Cartridge = null;
	public readonly Controllers Controllers = new();
	public readonly Ppu Ppu;
	private readonly Cpu _cpu;

	public event EventHandler? FrameReady;

	private int _dmaLeft = 0;

	public Nes()
	{
		CpuBus = new CpuBus(this);
		PpuBus = new PpuBus(this);
		Ppu = new(this);
		_cpu = new(this);

		Ppu.Vblank += VblankHandler;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void WaitUntil(Func<bool> predicate)
	{
		while (!predicate()) { }
		//SpinWait.SpinUntil(predicate);
	}

	private void VblankHandler(object? sender, PpuVblankEventArgs e)
	{
		if (e.Nmi)
			_cpu.RaiseNmi();

		if (e.Frame)
			FrameReady?.Invoke(this, EventArgs.Empty);
	}

	public void InsertCartridge(string romFilePath)
	{
		Cartridge = new(this, romFilePath);
	}

	internal void BeginOamDma(byte page)
	{
		// TODO: implement this properly
		var address = (ushort)(page << 8);
		for (var i = 0; i < Ppu.Oam.Length; i++)
		{
			var value = CpuBus.ReadByte(address);
			Ppu.WriteReg(4, value);
			address++;
		}

		_dmaLeft = 512;
	}

	private void ThreadProc()
	{
		ulong tick = 0;
		double ticks = 0;
		var start = Stopwatch.StartNew();

		double timestamp = Stopwatch.GetTimestamp();

		// TODO: skip frames if necessary

		while (_keepRunning)
		{
			timestamp += Stopwatch.Frequency / _framesPerSecond;

			while (ticks < _ppuTicksPerFrame)
			{
				Ppu.Tick();

				if (tick % 3 == 0)
				{
					if (_dmaLeft != 0)
					{
						_dmaLeft--;
					}
					else
						_cpu.Tick();
					// NOTE: signal IRQs AFTER CPU tick
					// maybe NMIs before?
				}

				tick++;
				ticks++;
			}

			ticks -= _ppuTicksPerFrame;

			var millisecondsToSpare = (timestamp - Stopwatch.GetTimestamp()) * 1000.0 / Stopwatch.Frequency;
			var millisecondsTaken = _millisPerFrame - millisecondsToSpare;
			//Console.WriteLine($"Emulated frame took {millisecondsTaken:0.00}ms ({millisecondsToSpare:0.00}ms to spare)");

			WaitUntil(NextFrame);
		}

		bool NextFrame() => Stopwatch.GetTimestamp() >= timestamp;
	}

	/// <summary>
	/// Starts the emulation.
	/// </summary>
	public void Start()
	{
		using (_startStopLock.EnterScope())
		{
			_thread = new(ThreadProc);
			_keepRunning = true;
			_thread.Start();
		}
	}

	/// <summary>
	/// Stops the emulation.
	/// </summary>
	public void Stop()
	{
		using (_startStopLock.EnterScope())
		{
			_keepRunning = false;
			WaitUntil(() => !_thread?.IsAlive ?? true);
		}
	}

	public void Reset()
	{
		_cpu.Reset();
		Ppu.Reset();
	}
}
