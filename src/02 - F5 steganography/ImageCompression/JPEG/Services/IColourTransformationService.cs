using ImageCompression.JPEG.Models;
using SkiaSharp;

namespace ImageCompression.JPEG.Services;

public interface IColorTransformationService
{
    public ColourData RGBToYCbCr(SKBitmap bitmap);
}