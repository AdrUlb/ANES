using Sdl3Sharp;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ANES.Platform.WinForms.Controls;
internal partial class SdlControl : Control
{
	private const int SWP_NOSIZE = 0x0001;
	private const int SWP_NOZORDER = 0x0004;
	private const int SWP_FRAMECHANGED = 0x0020;

	const int GWL_STYLE = -16;
	const nint WS_CHILD = 0x40000000;
	const nint WS_VISIBLE = 0x10000000;

	[LibraryImport("user32")]
	private static partial nint SetWindowPos(nint hwnd, nint handleInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[LibraryImport("user32")]
	private static partial nint SetParent(nint hWndChild, nint hWndNewParent);

	[LibraryImport("user32")]
	private static partial nint SetWindowLongPtrA(nint hWnd, int nIndex, nint dwNewLong);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

	public SdlWindow? SdlWindow { get; private set; } = null;
	private nint _sdlWindowHandle;

	public SdlControl()
	{
		BackColor = Color.Black;
		SetStyle(ControlStyles.UserPaint, true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		SetStyle(ControlStyles.Selectable, false);
	}

	protected override void CreateHandle()
	{
		base.CreateHandle();

		if (!DesignMode && !IsAncestorSiteInDesignMode)
		{
			Sdl.Init(SdlInitFlags.Video);

			SdlWindow = SdlWindow.Create("", Width, Height, SdlWindowFlags.Resizable | SdlWindowFlags.Borderless);
			var props = SdlWindow.GetProperties();
			_sdlWindowHandle = props.Get(SdlProperties.WindowWin32Hwnd, 0);
			SetWindowLongPtrA(_sdlWindowHandle, GWL_STYLE, WS_CHILD | WS_VISIBLE);
			SetParent(_sdlWindowHandle, Handle);
			SetWindowPos(_sdlWindowHandle, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

			/*var props = SdlProperties.Create();
			props.Set(SdlProperties.WindowCreateWin32Hwnd, Handle);
			SdlWindow = SdlWindow.CreateWithProperties(props);*/
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (Width > 0 && Height > 0)
			SdlWindow?.SetSize(Width, Height);
		base.OnSizeChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (DesignMode)
			e.Graphics.Clear(Color.Black);
	}

	protected override void OnPaintBackground(PaintEventArgs e) { }

	protected override void Dispose(bool disposing)
	{
		SdlWindow?.Destroy();
		base.Dispose(disposing);
	}
}
