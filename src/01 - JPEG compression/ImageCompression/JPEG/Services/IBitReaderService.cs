namespace ImageCompression.JPEG.Services;

public interface IBitReaderService
{
    int Read(BinaryReader br, bool reset);
}