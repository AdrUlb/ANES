using ANES.Emulation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Threading;

namespace ANES.Platform.Avalonia;

public class NesDisplayControl : Control
{
	private readonly int[] _data = new int[Ppu.PictureWidth * Ppu.PictureHeight];
	private readonly WriteableBitmap _bitmap = new(new(Ppu.PictureWidth, Ppu.PictureHeight), Vector.One * 96, PixelFormat.Rgba8888, AlphaFormat.Opaque);
	private readonly RenderOptions _bitmapRenderOptions = new()
	{
		EdgeMode = EdgeMode.Aliased,
		BitmapInterpolationMode = BitmapInterpolationMode.None,
		BitmapBlendingMode = BitmapBlendingMode.Source,
		RequiresFullOpacityHandling = false,
		TextRenderingMode = TextRenderingMode.Alias
	};
	private readonly Lock _lock = new();

	protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		Program.Nes.Ppu.Frame += OnFrame;
		base.OnAttachedToLogicalTree(e);
	}

	protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		Program.Nes.Ppu.Frame -= OnFrame;
		base.OnDetachedFromLogicalTree(e);
	}

	private void OnFrame(object? sender, EventArgs e)
	{
		using (_lock.EnterScope())
		{
			for (var i = 0; i < Ppu.PictureWidth * Ppu.PictureHeight; i++)
			{
				var color = Program.Nes.Ppu.Picture[i];
				_data[i] = (0xFF << 24) | (color.B << 16) | (color.G << 8) | color.R;
			}

			Dispatcher.UIThread.Post(InvalidateVisual);
		}
	}

	public override void Render(DrawingContext context)
	{
		using (_lock.EnterScope())
		{
			unsafe
			{
				using var buf = _bitmap.Lock();
				var pixels = new Span<int>((void*)buf.Address, Ppu.PictureWidth * Ppu.PictureHeight);

				for (var y = 0; y < Ppu.PictureHeight; y++)
					for (var x = 0; x < Ppu.PictureWidth; x++)
						pixels[x + y * (buf.RowBytes / 4)] = _data[x + y * Ppu.PictureWidth];
			}

			using (context.PushRenderOptions(_bitmapRenderOptions))
				context.DrawImage(_bitmap, new(0, 0, Bounds.Width, Bounds.Height));
		}
		
		base.Render(context);
	}
}
