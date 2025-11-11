using SkiaSharp;

namespace ImageSteg.Services;

public interface IBitmapRendererService
{
    SKBitmap? Bitmap { get; set; }
    void PaintSurface(SKSurface surface, SKImageInfo info);
    void InvalidateSurface();
    event EventHandler InvalidateSurfaceRequest;
}