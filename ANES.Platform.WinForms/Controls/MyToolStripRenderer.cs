namespace ANES.Platform.WinForms.Controls;

internal class MyToolStripRenderer : ToolStripRenderer
{
	public static readonly MyToolStripRenderer Instance = new();

	private MyToolStripRenderer() { }

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		Rectangle bounds = e.AffectedBounds;

		var backBrush = SystemBrushes.MenuBar;
		e.Graphics.FillRectangle(backBrush, bounds);
		base.OnRenderToolStripBackground(e);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		Rectangle bounds = e.ToolStrip.ClientRectangle;

		if (e.ToolStrip is ToolStripDropDown toolStripDropDown)
		{
			// Paint the border for the window depending on whether or not we have a drop shadow effect. 
			bounds.Width -= 1;
			bounds.Height -= 1;
			e.Graphics.DrawRectangle(SystemPens.ControlDark, bounds);
		}

		base.OnRenderToolStripBorder(e);
	}
	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		var bounds = new Rectangle(Point.Empty, e.Item.Size);

		if (bounds.Width >= 4)
			bounds.Inflate(-2, 0);

		var y = bounds.Height / 2;
		e.Graphics.DrawLine(SystemPens.ControlDark, bounds.Left, y, bounds.Right, y);
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		if (e.Item.Selected || e.Item.Pressed)
			e.TextColor = SystemColors.HighlightText;
		else
			e.TextColor = SystemColors.ControlText;

		if (e.ToolStrip?.Focused ?? false)
			e.TextFormat &= ~TextFormatFlags.HidePrefix;
		base.OnRenderItemText(e);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		base.OnRenderMenuItemBackground(e);

		Rectangle fillRect = new Rectangle(Point.Empty, e.Item.Size);
		if (e.Item.IsOnDropDown)
		{
			// VSWhidbey 518568: scoot in by 2 pixels when selected
			fillRect.X += 2;
			fillRect.Width -= 3; //its already 1 away from the right edge
		}

		Brush brush;

		if (SystemInformation.HighContrast)
		{
			brush = e.Item.Pressed || e.Item.Selected ? SystemBrushes.Highlight : SystemBrushes.MenuBar;
		}
		else
		{
			brush = (e.Item.Pressed || e.Item.Selected) ? SystemBrushes.MenuHighlight : SystemBrushes.MenuBar;
		}
		e.Graphics.FillRectangle(brush, fillRect);
	}
}
