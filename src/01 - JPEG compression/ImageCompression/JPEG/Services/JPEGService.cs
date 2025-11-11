using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using SkiaSharp;

namespace ImageCompression.JPEG.Services;

public class JPEGService: IJPEGService
{
    readonly IColourTransformationService _colourTransformationService;
    readonly IDCTService _dctService;
    readonly IEncodingService _encodingService;
    readonly IHeaderService _headerService;

    public JPEGService(IColourTransformationService colourTransformationService,
        IDCTService dCTService,
        IEncodingService encodingService,
        IHeaderService headerService)
    {
        _colourTransformationService = colourTransformationService;
        _dctService = dCTService;
        _encodingService = encodingService;
        _headerService = headerService;
    }

    public void Encode(SKBitmap image, BinaryWriter bw)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image), nameof(image).ToArgumentNullExceptionMessage());

        if (bw == null)
            throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

        ImageInfo jpeg = CreateImageInfo(image);

        // Create JFIF headers
        _headerService.WriteHeaders(bw, jpeg);

        // Transform image to YCBCR colour space
        jpeg.ColourData = _colourTransformationService.RGBToYCbCr(jpeg.Bitmap!);

        // Calculate DCT values
        jpeg.DCTData = _dctService.CalculateDCT(jpeg.ColourData, jpeg.Width, jpeg.Height);

        // Quantize DCT values
        jpeg.QuantizedDCTData = _dctService.QuantizeDCT(jpeg.DCTData, null, null);

        // Run length encoding and huffman encoding
        _encodingService.EncodeData(jpeg.QuantizedDCTData, bw);

        // Write end of image header
        _headerService.WriteEOI(bw);
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