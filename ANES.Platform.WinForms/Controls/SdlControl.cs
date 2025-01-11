using Sdl3Sharp;
using System.ComponentModel;

namespace ANES.Platform.WinForms.Controls;
internal class SdlControl : Panel
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SdlWindow SdlWindow { get; private set; } = null!;

	public SdlControl()
	{
		BackColor = Color.Black;
	}

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
}
