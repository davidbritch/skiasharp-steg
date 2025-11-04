using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IEncodingService
{
    public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw);
    public DCTData DecodeData(ImageInfo jpeg, BinaryReader bw);
}