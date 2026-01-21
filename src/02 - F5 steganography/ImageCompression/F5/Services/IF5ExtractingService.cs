using ImageCompression.JPEG.Models;

namespace ImageCompression.F5.Services;

public interface IF5ExtractingService
{
    string Extract(DCTData dctData, string password);
}