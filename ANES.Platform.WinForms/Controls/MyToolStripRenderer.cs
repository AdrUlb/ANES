using System.Drawing.Imaging.Effects;

namespace ANES.Platform.WinForms.Controls;

#pragma warning disable WFO5001

internal sealed class MyToolStripRenderer : ToolStripRenderer
{
	public static readonly MyToolStripRenderer Instance = new();

	private MyToolStripRenderer() { }

	private Color HighlightColor => Application.IsDarkModeEnabled ? Color.FromArgb(40, 40, 40) : SystemColors.Highlight;
	private Color HighlightTextColor => Application.IsDarkModeEnabled ? SystemColors.ControlText : SystemColors.HighlightText;

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		e.Graphics.FillRectangle(SystemBrushes.MenuBar, e.AffectedBounds);
		base.OnRenderToolStripBackground(e);
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		var item = e.Item;

		if (item != null)
			e.ArrowColor = item.Pressed || item.Selected ? HighlightTextColor : SystemColors.ControlText;

		base.OnRenderArrow(e);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		if (e.ToolStrip is ToolStripDropDown toolStripDropDown)
		{
			var bounds = e.ToolStrip.ClientRectangle;
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
			e.TextColor = HighlightTextColor;
		else
			e.TextColor = SystemColors.ControlText;

		if (e.ToolStrip?.Focused ?? false)
			e.TextFormat &= ~TextFormatFlags.HidePrefix;
		base.OnRenderItemText(e);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		base.OnRenderMenuItemBackground(e);

		var fillRect = new Rectangle(Point.Empty, e.Item.Size - new Size(1, 1));

		if (e.Item.IsOnDropDown)
		{
			fillRect.X += 2;
			fillRect.Width -= 3;
		}

		if (!e.Item.Pressed && !e.Item.Selected)
		{
			e.Graphics.FillRectangle(SystemBrushes.MenuBar, fillRect);
			return;
		}

		using (var br = new SolidBrush(HighlightColor))
			e.Graphics.FillRectangle(br, fillRect);

		if (e.Item.IsOnDropDown || !Application.IsDarkModeEnabled)
			return;

		using var pen = new Pen(Color.FromArgb(80, 80, 80));
		e.Graphics.DrawRectangle(pen, fillRect);

	}
}
#pragma warning restore WFO5001
