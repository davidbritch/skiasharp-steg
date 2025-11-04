using SkiaSharp;

namespace ImageCompression.F5.Services;

public interface IF5Service
{
        public void Embed(SKBitmap image, string password, string message, BinaryWriter bw);
        public string Extract(string password, BinaryReader br);
}