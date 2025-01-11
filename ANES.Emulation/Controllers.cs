namespace ANES;

public sealed class Controllers
{
	public struct ControllerState()
	{
		public bool ButtonA = false;
		public bool ButtonB = false;
		public bool ButtonSelect = false;
		public bool ButtonStart = false;
		public bool ButtonUp = false;
		public bool ButtonDown = false;
		public bool ButtonLeft = false;
		public bool ButtonRight = false;
	}
	
	/* TODO: In the NES and Famicom, the top three (or five) bits are not driven, and so retain the bits of the previous byte on the bus.
	 Usually this is the most significant byte of the address of the controller port—0x40.
	 Certain games (such as Paperboy) rely on this behavior and require that reads from the controller ports return exactly $40 or $41 as appropriate.
	 See: NESDEV WIKI: Controller reading: unconnected data lines. */
	private bool _strobe = false;

	private int _counter1 = 0;
	private int _counter2 = 0;

	public ControllerState Controller1;
	public ControllerState Controller2;

	public void WriteStrobe(byte value)
	{
		_strobe = (value & 1) != 0;
		if (_strobe)
		{
			_counter1 = 0;
			_counter2 = 0;
		}
	}

	public byte ReadData1()
	{
		// Unless not all buttons have been read		
		var buttonDown = _counter1 switch
		{
			0 => Controller1.ButtonA,
			1 => Controller1.ButtonB,
			2 => Controller1.ButtonSelect,
			3 => Controller1.ButtonStart,
			4 => Controller1.ButtonUp,
			5 => Controller1.ButtonDown,
			6 => Controller1.ButtonLeft,
			7 => Controller1.ButtonRight,
			_ => true
		};
		
		if (!_strobe)
			_counter1++;

		return (byte)(buttonDown ? 1 : 0);
	}

	public byte ReadData2()
	{
		// Unless not all buttons have been read		
		var buttonDown = _counter2 switch
		{
			0 => Controller1.ButtonA,
			1 => Controller1.ButtonB,
			2 => Controller1.ButtonSelect,
			3 => Controller1.ButtonStart,
			4 => Controller1.ButtonUp,
			5 => Controller1.ButtonDown,
			6 => Controller1.ButtonLeft,
			7 => Controller1.ButtonRight,
			_ => true
		};
		
		if (!_strobe)
			_counter2++;

		return (byte)(buttonDown ? 1 : 0);
	}
}
