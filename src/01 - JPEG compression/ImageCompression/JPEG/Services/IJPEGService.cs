using SkiaSharp;

namespace ImageCompression.JPEG.Services;

public interface IJPEGService
{
    public void Encode(SKBitmap image, BinaryWriter bw);
}