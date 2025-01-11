using Sdl3Sharp;

namespace ANES.Platform.Windows;
internal class SdlControl : Panel
{
	public SdlWindow SdlWindow { get; private set; } = null!;

	protected override void CreateHandle()
	{
		base.CreateHandle();


		if (!DesignMode && !IsAncestorSiteInDesignMode)
		{
			Sdl.Init(SdlInitFlags.Video);
			var props = SdlProperties.Create();
			props.Set(SdlProperties.WindowCreateWin32Hwnd, Handle);
			SdlWindow = SdlWindow.CreateWithProperties(props);
		}
		else
			SdlWindow = null!;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		// Do nothing
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (DesignMode || IsAncestorSiteInDesignMode)
		{
			e.Graphics.Clear(Color.Black);
		}
	}
}
