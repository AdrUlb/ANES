namespace Sdl3Sharp;

[Flags]
public enum SdlWindowFlags : uint
{
	Fullscreen = 0x0000000000000001, // window is in fullscreen mode
	Opengl = 0x0000000000000002, // window usable with OpenGL context
	Occluded = 0x0000000000000004, // window is occluded
	Hidden = 0x0000000000000008, // window is neither mapped onto the desktop nor shown in the taskbar/dock/window list; SDL_ShowWindow() is required for it to become visible
	Borderless = 0x0000000000000010, // no window decoration
	Resizable = 0x0000000000000020, // window can be resized
	Minimized = 0x0000000000000040, // window is minimized
	Maximized = 0x0000000000000080, // window is maximized
	MouseGrabbed = 0x0000000000000100, // window has grabbed mouse input
	InputFocus = 0x0000000000000200, // window has input focus
	MouseFocus = 0x0000000000000400, // window has mouse focus
	External = 0x0000000000000800, // window not created by SDL
	Modal = 0x0000000000001000, // window is modal
	HighPixelDensity = 0x0000000000002000, // window uses high pixel density back buffer if possible
	MouseCapture = 0x0000000000004000, // window has mouse captured (unrelated to MOUSE_GRABBED)
	MouseRelativeMode = 0x0000000000008000, // window has relative mode enabled
	AlwaysOnTop = 0x0000000000010000, // window should always be above others
	Utility = 0x0000000000020000, // window should be treated as a utility window, not showing in the task bar and window list
	Tooltip = 0x0000000000040000, // window should be treated as a tooltip and does not get mouse or keyboard focus, requires a parent window
	PopupMenu = 0x0000000000080000, // window should be treated as a popup menu, requires a parent window
	KeyboardGrabbed = 0x0000000000100000, // window has grabbed keyboard input
	Vulkan = 0x0000000010000000, // window usable for Vulkan surface
	Metal = 0x0000000020000000, // window usable for Metal view
	Transparent = 0x0000000040000000, // window with transparent buffer
	NotFocusable = 0x0000000080000000, // window should not be focusable
}
