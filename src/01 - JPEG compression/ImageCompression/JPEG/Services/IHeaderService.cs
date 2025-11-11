using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IHeaderService
{
    void WriteHeaders(BinaryWriter bw, ImageInfo jpeg);
    void WriteEOI(BinaryWriter bw);
    void ParseJpegMarkers(BinaryReader br, ImageInfo jpeg);
}