using ImageCompression.F5.Services;
using ImageCompression.JPEG.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImageCompression;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImageServices(this IServiceCollection services)
    {
        services.AddScoped<IColorTransformationService, ColorTransformationService>();
        services.AddScoped<IF5Service, F5Service>();
        services.AddScoped<IDCTService, DCTService>();
        services.AddScoped<IPaddingService, PaddingService>();
        services.AddScoped<IRunLengthEncodingService, RunLengthEncodingService>();
        services.AddScoped<IHuffmanEncodingService, HuffmanEncodingService>();
        services.AddScoped<IEncodingService, EncodingService>();
        services.AddScoped<IHeaderService, HeaderService>();
        services.AddScoped<IHuffmanDecodingService, HuffmanDecodingService>();
        services.AddScoped<IBitReaderService, BitReaderService>();
        services.AddScoped<IMCUConverterService, MCUConverterService>();
        services.AddScoped<IF5EmbeddingService, F5EmbeddingService>();
        services.AddScoped<IPermutationService, PermutationService>();
        services.AddScoped<IF5ParameterCalculatorService, F5ParameterCalculatorService>();
        services.AddScoped<IF5ExtractingService, F5ExtractingService>();

        return services;
    }
}
