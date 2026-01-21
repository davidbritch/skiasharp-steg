namespace ImageCompression.F5.Models;

public class EmbeddingRateRecord
{
    public int K { get; set; }
    public int N { get; set; }
    public double ChangeDensity { get; set; }
    public double EmbeddingRate { get; set; }
    public double EmbeddingEfficiency { get; set; }

    public EmbeddingRateRecord(int k, int n, double changeDensity, double rate, double efficiency) 
    {
        K = k;
        N = n;
        ChangeDensity = changeDensity;
        EmbeddingRate = rate;
        EmbeddingEfficiency = efficiency;
    }
}