namespace ImageCompression.JPEG.Models;

public class ColourData
{
    public float[,] Y { get; set; }
    public float[,] Cr { get; set; }
    public float[,] Cb { get; set; }

    public ColourData(int width, int height)
    {
        Y = new float[height, width];
        Cr = new float[height, width];
        Cb = new float[height, width];
    }
}