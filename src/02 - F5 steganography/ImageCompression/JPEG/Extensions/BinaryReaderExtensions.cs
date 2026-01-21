namespace ImageCompression.JPEG.Extensions;

internal static class BinaryReaderExtensions
{
    internal static int Read2Bytes(this BinaryReader reader)
    {
        byte upper = reader.ReadByte();
        byte lower = reader.ReadByte();

        int result = (upper << 8) | lower;
        return result;
    }
}