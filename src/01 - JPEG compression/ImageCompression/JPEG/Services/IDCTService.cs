using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IDCTService
{
    public DCTData CalculateDCT(ColourData input, int width, int height);
    public DCTData QuantizeDCT(DCTData input, byte[]? chrominanceTable, byte[]? luminanceTable);
}