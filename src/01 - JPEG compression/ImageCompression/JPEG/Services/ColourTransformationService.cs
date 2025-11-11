using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using SkiaSharp;

namespace ImageCompression.JPEG.Services;

public class ColorTransformationService : IColorTransformationService
{
    public ColourData RGBToYCbCr(SKBitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap), nameof(bitmap).ToArgumentNullExceptionMessage());

        var result = new ColourData(bitmap.Width, bitmap.Height);
        ApplyColorTransform(bitmap, result);

        return result;
    }

    unsafe void ApplyColorTransform(SKBitmap bitmap, ColourData result)
    {
        SKPixmap pixmap = bitmap.PeekPixels();
        byte* bmpPtr = (byte*)pixmap.GetPixels().ToPointer();
        int width = bitmap.Width;
        int height = bitmap.Height;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                byte red = *bmpPtr++;
                byte green = *bmpPtr++;
                byte blue = *bmpPtr++;
                byte alpha = *bmpPtr++; // Ignored

                float y = RGB.ToY(red, green, blue);
                float cb = RGB.ToCb(red, green, blue);
                float cr = RGB.ToCr(red, green, blue);

                float yColorShifted = (float)(y - 128);
                float cbColorShifted = (float)(cb - 128);
                float crColorShifted = (float)(cr - 128);

                result.Y[i, j] = yColorShifted;
                result.Cb[i, j] = cbColorShifted;
                result.Cr[i, j] = crColorShifted;
            }
        }
    }
}