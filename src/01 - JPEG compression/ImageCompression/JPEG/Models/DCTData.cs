namespace ImageCompression.JPEG.Models;

public class DCTData
{
    public Block8x8[] Y { get; set; }
    public Block8x8[] Cr { get; set; }
    public Block8x8[] Cb { get; set; }

    public DCTData()
    {
        Y = new Block8x8[] {};
        Cr = new Block8x8[] {};
        Cb = new Block8x8[] {};
    }

    public DCTData(int dctCount)
    {
        Y = new Block8x8[dctCount];
        Cr = new Block8x8[dctCount];
        Cb = new Block8x8[dctCount];
    }    
}