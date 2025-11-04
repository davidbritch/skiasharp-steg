namespace ImageCompression.JPEG.Extensions;

internal static class BinaryReaderExtensions
{
    internal static int Read2Bytes(this BinaryReader reader)
    {
        var upper = reader.ReadByte();
        var lower = reader.ReadByte();

        var result = (upper << 8) | lower;
        return result;
    }
}