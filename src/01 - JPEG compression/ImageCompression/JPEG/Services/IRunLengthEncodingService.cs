using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IRunLengthEncodingService
{
    public List<Tuple<int, int>> Encode(Block8x8 block);
    public Block8x8 Decode(List<Tuple<int, int>> pairs);
}