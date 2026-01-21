using ImageCompression.JPEG.Services;
using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using SkiaSharp;

namespace ImageCompression.F5.Services;

public class F5Service : IF5Service
{
    readonly IColorTransformationService _colorTransformationService;
    readonly IDCTService _dctService;
    readonly IEncodingService _encodingService;
    readonly IHeaderService _headerService;
    readonly IF5EmbeddingService _embeddingService;
    readonly IF5ExtractingService _extractingService;

    public F5Service(IColorTransformationService colorTransformationService,
        IDCTService dCTService,
        IEncodingService encodingService,
        IHeaderService headerService,
        IF5EmbeddingService embeddingService,
        IF5ExtractingService extractingService)
    {
        _colorTransformationService = colorTransformationService;
        _dctService = dCTService;
        _encodingService = encodingService;
        _headerService = headerService;
        _embeddingService = embeddingService;
        _extractingService = extractingService;
    }

    public void Embed(SKBitmap image, string password, string message, BinaryWriter bw)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image), nameof(image).ToArgumentNullExceptionMessage());

        if (bw == null)
            throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

        ImageInfo jpeg = CreateImageInfo(image);

        // Create JFIF headers
        _headerService.WriteHeaders(bw, jpeg);

        // Transform image to YCBCR color space
        jpeg.ColourData = _colorTransformationService.RGBToYCbCr(jpeg.Bitmap!);

        // Calculate DCT values
        jpeg.DCTData = _dctService.CalculateDCT(jpeg.ColourData, jpeg.Width, jpeg.Height);

        // Quantize DCT values
        jpeg.QuantizedDCTData = _dctService.QuantizeDCT(jpeg.DCTData, null, null);

        // Embed the message
        jpeg.EmbeddedData = _embeddingService.Embed(jpeg.QuantizedDCTData, password, message);

        // Run length encoding and huffman encoding
        _encodingService.EncodeData(jpeg.EmbeddedData, bw);

        // Write end of image header
        _headerService.WriteEOI(bw);
    }

    public string Extract(string password, BinaryReader br)
    {
        if (br == null)
            throw new ArgumentNullException(nameof(br), nameof(br).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

        var jpeg = new ImageInfo();

        // Parse jpeg markers
        _headerService.ParseJpegMarkers(br, jpeg);

        // Read entropy coded data and decode it
        var quantizedDctData = _encodingService.DecodeData(jpeg, br);

        // Extract the message from the decoded data
        var message = _extractingService.Extract(quantizedDctData, password);

        return message;
    }

    ImageInfo CreateImageInfo(SKBitmap image)
    {
        var jpeg = new ImageInfo();
        jpeg.Width = image.Width;
        jpeg.Height = image.Height;
        jpeg.Bitmap = (SKBitmap)image;
        return jpeg;
    }
}