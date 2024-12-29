namespace Sdl3Sharp;

[Flags]
public enum SdlKeymod : ushort
{
	None = 0x0000, // no modifier is applicable.
	LeftShift = 0x0001, // the left Shift key is down.
	RightShift = 0x0002, // the right Shift key is down.
	LeftControl = 0x0040, // the left Ctrl (Control) key is down.
	RightControl = 0x0080, // the right Ctrl (Control) key is down.
	LeftAlt = 0x0100, // the left Alt key is down.
	RightAlt = 0x0200, // the right Alt key is down.
	LeftGui = 0x0400, // the left GUI key (often the Windows key) is down.
	RightGui = 0x0800, // the right GUI key (often the Windows key) is down.
	Num = 0x1000, // the Num Lock key (may be located on an extended keypad) is down.
	Caps = 0x2000, // the Caps Lock key is down.
	Mode = 0x4000, // the !AltGr key is down.
	Scroll = 0x8000, // the Scroll Lock key is down.
	Ctrl = (LeftControl | RightControl), // Any Ctrl key is down.
	Shift = (LeftShift | RightShift), // Any Shift key is down.
	Alt = (LeftAlt | RightAlt), // Any Alt key is down.
	Gui = (LeftGui | RightGui), // Any GUI key is down.
}
