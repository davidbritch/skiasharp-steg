using ImageCompression.JPEG.Models;

namespace ImageCompression.F5.Services;

public interface IF5EmbeddingService
{
    DCTData Embed(DCTData quantizedData, string password, string message);
}