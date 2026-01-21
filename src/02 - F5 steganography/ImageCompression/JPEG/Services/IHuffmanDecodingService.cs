using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IHuffmanDecodingService
{
        int DecodeChrominanceDC(int prevDC, BinaryReader bw);
        int DecodeLuminanceDC(int prevDC, BinaryReader bw);
        Block8x8 DecodeChrominanceAC(BinaryReader bw);
        Block8x8 DecodeLuminanceAC(BinaryReader bw);
}