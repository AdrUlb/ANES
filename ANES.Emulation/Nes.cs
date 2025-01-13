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

	public event EventHandler? Frame;

	private int _dmaLeft = 0;
	private ulong _tick = 0;
	private bool _frameDone = false;

	public Nes()
	{
		CpuBus = new CpuBus(this);
		PpuBus = new PpuBus(this);
		Ppu = new(this);
		_cpu = new(this);

		Ppu.Nmi += PpuNmiHandler;
		Ppu.Frame += PpuFrameHandler;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void WaitUntil(Func<bool> predicate)
	{
		while (!predicate()) { }
		//SpinWait.SpinUntil(predicate);
	}

	private void PpuNmiHandler(object? sender, EventArgs e)
	{
		_cpu.RaiseNmi();
	}

	private void PpuFrameHandler(object? sender, EventArgs e)
	{
		Frame?.Invoke(this, e);
		_frameDone = true;
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
		double nextFrameStart = Stopwatch.GetTimestamp();

		while (_keepRunning)
		{
			nextFrameStart += Stopwatch.Frequency / _framesPerSecond;

			while (!_frameDone)
				Tick();

			var swTicksToSpare = nextFrameStart - Stopwatch.GetTimestamp();
			var millisecondsToSpare = swTicksToSpare * 1000.0 / Stopwatch.Frequency;
			var millisecondsTaken = _millisPerFrame - millisecondsToSpare;
			Console.WriteLine($"Emulated frame took {millisecondsTaken:0.00}ms ({millisecondsToSpare:0.00}ms to spare)");

			WaitUntil(NextFrame);
			_frameDone = false;
		}

		bool NextFrame() => Stopwatch.GetTimestamp() >= nextFrameStart;
	}

	private void Tick()
	{
		Ppu.Tick();

		if (_tick % 3 == 0)
		{
			if (_dmaLeft == 0)
			{
				_cpu.Tick();
			}
			else
				_dmaLeft--;
			// NOTE: signal IRQs AFTER CPU tick
			// maybe NMIs before?
		}

		_tick++;
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
		_tick = 0;
	}
}
