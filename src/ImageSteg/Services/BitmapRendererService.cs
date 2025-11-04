using SkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageSteg.Services;

public class BitmapRendererService : ObservableObject, IBitmapRendererService
{
    #region Properties

    SKBitmap _bitmap = null;
    public SKBitmap Bitmap
    {
        get => _bitmap;
        set
        {
            SetProperty(ref _bitmap, value);
            InvalidateSurfaceRequest?.Invoke(this, EventArgs.Empty);
        }
    }
    
    #endregion

    public void PaintSurface(SKSurface surface, SKImageInfo info)
    {
        SKCanvas canvas = surface.Canvas;
        canvas.Clear();

        if (_bitmap != null)
        {
            canvas.DrawBitmap(_bitmap, info.Rect, ImageStretch.Uniform);
        }
    }

    public void InvalidateSurface()
    {
        InvalidateSurfaceRequest?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler InvalidateSurfaceRequest;    
}