using ImageCompression.JPEG.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImageCompression;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImageServices(this IServiceCollection services)
    {
        services.AddScoped<IJPEGService, JPEGService>();
        services.AddScoped<IColorTransformationService, ColorTransformationService>();
        services.AddScoped<IDCTService, DCTService>();
        services.AddScoped<IPaddingService, PaddingService>();
        services.AddScoped<IRunLengthEncodingService, RunLengthEncodingService>();
        services.AddScoped<IHuffmanEncodingService, HuffmanEncodingService>();
        services.AddScoped<IEncodingService, EncodingService>();
        services.AddScoped<IHeaderService, HeaderService>();
        services.AddScoped<IHuffmanDecodingService, HuffmanDecodingService>();
        services.AddScoped<IBitReaderService, BitReaderService>();
        return services;
    }
}
