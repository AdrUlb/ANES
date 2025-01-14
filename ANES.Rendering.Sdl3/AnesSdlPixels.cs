using Sdl3Sharp;
using System.Drawing;

namespace ANES.Rendering.Sdl3;
public sealed class AnesSdlPixels : IDisposable
{
	private readonly SdlTexture _texture;

	private readonly Lock _lock = new();
	private SdlSurface? _surface = null;

	public AnesSdlPixels(int width, int height, SdlRenderer _renderer)
	{
		var props = SdlProperties.Create();
		props.Set(SdlProperties.TextureCreateWidth, width);
		props.Set(SdlProperties.TextureCreateHeight, height);
		props.Set(SdlProperties.TextureCreateAccess, (long)SdlTextureAccess.Streaming);
		props.Set(SdlProperties.TextureCreateFormat, (long)SdlPixelFormat.Argb8888);
		_texture = SdlTexture.CreateWithProperties(_renderer, props);
		_texture.SetScaleMode(SdlScaleMode.Nearest);
	}

	private SdlSurface LockIfNecessary()
	{
		return _surface ??= _texture.LockToSurface();
	}

	private void UnlockIfNecessary()
	{
		if (_surface == null)
			return;

		_texture.Unlock();
		_surface = null;
	}

	public Span<T> GetRowSpan<T>(int y) where T : unmanaged
	{
		using (_lock.EnterScope())
		{
			return LockIfNecessary().GetPixelsRowSpan<T>(y);
		}
	}

	public void SetRow(int y, ReadOnlySpan<Color> row)
	{
		using (_lock.EnterScope())
		{
			var surfRow = LockIfNecessary().GetPixelsRowSpan<int>(y);
			for (var x = 0; x < row.Length && x < surfRow.Length; x++)
			{
				surfRow[x] = row[x].ToArgb();
			}
		}
	}

	public void Render(RectangleF sourceRect, RectangleF targetRect)
	{
		using (_lock.EnterScope())
		{
			UnlockIfNecessary();
			_texture.Render(sourceRect, targetRect);
		}
	}

	public void Render() => Render(RectangleF.Empty, RectangleF.Empty);

	~AnesSdlPixels() => Dispose();

	public void Dispose() => _texture.Destroy();
}
