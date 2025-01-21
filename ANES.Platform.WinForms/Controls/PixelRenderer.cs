using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ANES.Platform.WinForms.Controls;

internal class PixelRenderer : Control
{
	private readonly int[] _data;
	private readonly GCHandle _dataHandle;
	private readonly Bitmap _bitmap;
	private readonly Lock _lock = new();

	public PixelRenderer(int width, int height)
	{
		SetStyle(ControlStyles.Selectable, false);

		BackColor = Color.Black;
		Size = new Size(400, 400);
		DoubleBuffered = true;

		_data = new int[width * height];
		_dataHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
		_bitmap = new(width, height, width * 4, PixelFormat.Format32bppArgb, _dataHandle.AddrOfPinnedObject());
	}

	public void SetPixel(int x, int y, Color color)
	{
		using (_lock.EnterScope())
			_data[x + (y * _bitmap.Width)] = color.ToArgb();
	}

	public void CopyPixels(ReadOnlySpan<Color> pixels)
	{
		using (_lock.EnterScope())
			for (var i = 0; i < pixels.Length; i++)
				_data[i] = pixels[i].ToArgb();
	}


	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		// No interpolation
		e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
		// No antialiasing
		e.Graphics.SmoothingMode = SmoothingMode.None;
		// No alpha blending
		e.Graphics.CompositingMode = CompositingMode.SourceCopy;
		e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

		e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

		using (_lock.EnterScope())
			e.Graphics.DrawImage(_bitmap, 0, 0, ClientSize.Width, ClientSize.Height);
	}

	protected override void Dispose(bool disposing)
	{
		_bitmap.Dispose();
		_dataHandle.Free();
		base.Dispose(disposing);
	}
}
