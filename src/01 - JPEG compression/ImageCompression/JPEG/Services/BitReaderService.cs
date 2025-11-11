namespace ImageCompression.JPEG.Services;

public class BitReaderService : IBitReaderService
{
    byte BufferedByte = 0;
    byte PreviousBufferedByte = 0;
    int CurrentBitIndex = 0;
    int BufferedResult = 0;

    public int Read(BinaryReader br, bool reset)
    {
        // Read bit from the binary reader and buffer the value read. Each consecutive read will appends to the buffered value.
        if (CurrentBitIndex == 0)
        {
            PreviousBufferedByte = BufferedByte;
            BufferedByte = br.ReadByte();
        }
        if (CurrentBitIndex > 7)
        {
            PreviousBufferedByte = BufferedByte;
            BufferedByte = br.ReadByte();

            // Since FF is a special marker, if "FF" is encountered in the entropy coded segment, it will have "00" bytes after it.
            if (PreviousBufferedByte == 0xFF && BufferedByte == 0x00)
            {
                PreviousBufferedByte = BufferedByte;
                BufferedByte = br.ReadByte();
            }
            CurrentBitIndex = 0;
        }

        if (reset)
            BufferedResult = 0;

        bool bit = (BufferedByte & (1 << (7 - CurrentBitIndex))) != 0;
        BufferedResult = (BufferedResult << 1) | (bit ? 1 : 0);

        CurrentBitIndex++;

        return BufferedResult;
    }
}