namespace ANES.Platform.WinForms.Controls;

internal sealed class MyMenuStrip : MenuStrip
{
	public MyMenuStrip()
	{
		Renderer = MyToolStripRenderer.Instance;
		Padding = new(0);
	}
}
