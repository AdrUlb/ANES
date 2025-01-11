namespace ANES.Platform.WinForms.Controls;

internal class MyMenuStrip : MenuStrip
{
	public MyMenuStrip()
	{
		Renderer = MyToolStripRenderer.Instance;
		BackColor = SystemColors.MenuBar;
	}
}
