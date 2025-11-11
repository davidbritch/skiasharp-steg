namespace ImageCompression.JPEG;

internal enum Marker : byte
{
        Padding = 0xFF,
        StartOfImage = 0xD8,
        App0 = 0xE0,
        DefineQuantizationTable = 0xDB,
        StartOfFrame0 = 0xC0,
        DefineHuffmanTable = 0xC4,
        StartOfScan = 0xDA,
        EndOfImage = 0xD9    
}