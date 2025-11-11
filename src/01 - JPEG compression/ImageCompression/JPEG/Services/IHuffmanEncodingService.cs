using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IHuffmanEncodingService
{
    void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw);
    void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw);
    void EncodeChrominanceAC(Block8x8 block, BinaryWriter bw);
    void EncodeLuminanceAC(Block8x8 block, BinaryWriter bw);
    void FlushBuffer(BinaryWriter bw);
}