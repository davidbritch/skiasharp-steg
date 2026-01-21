using ImageCompression.F5.Models;

namespace ImageCompression.F5;

public static class EmbeddingRateTable
{
    public static readonly EmbeddingRateRecord[] Table =
    {
        // n, k, changeDensity, embeddingRate, embeddingEfficiency
        new EmbeddingRateRecord(1, 1, 0.50, 1, 2),
        new EmbeddingRateRecord(2, 3, 0.2500, 0.6667, 2.67),
        new EmbeddingRateRecord(3, 7, 0.1250, 0.4286, 3.43),
        new EmbeddingRateRecord(4, 15, 0.0625, 0.2667, 4.27),
        new EmbeddingRateRecord(5, 31, 0.0312, 0.1613, 5.16),
        new EmbeddingRateRecord(6, 63, 0.0156, 0.0952, 6.09),
        new EmbeddingRateRecord(7, 127, 0.0078, 0.0551, 7.06),
        new EmbeddingRateRecord(8, 255, 0.0039, 0.0314, 8.03),
        new EmbeddingRateRecord(9, 511, 0.0020, 0.0176, 9.02)
    };
}