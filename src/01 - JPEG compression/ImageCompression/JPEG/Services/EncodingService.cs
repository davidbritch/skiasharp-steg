using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;

namespace ImageCompression.JPEG.Services;

public class EncodingService : IEncodingService
{
    readonly IHuffmanEncodingService _huffmanEncodingService;
    readonly IHuffmanDecodingService _huffmanDecodingService;
    readonly IPaddingService _paddingService;

    public EncodingService(IHuffmanEncodingService hhuffmanEncodingService,
        IHuffmanDecodingService huffmanDecodingService,
        IPaddingService paddingService)
    {
        _huffmanEncodingService = hhuffmanEncodingService;
        _paddingService = paddingService;
        _huffmanDecodingService = huffmanDecodingService;
    }

    public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw)
    {
        if (quantizedDCTData == null)
            throw new ArgumentNullException(nameof(quantizedDCTData), nameof(quantizedDCTData).ToArgumentNullExceptionMessage());

        if (bw == null)
            throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

        int mcuCount = quantizedDCTData.Y.Length;
        int prevDc_Y = 0;
        int prevDc_Cr = 0;
        int prevDc_Cb = 0;

        for (int i = 0; i < mcuCount; i++)
        {
            var yMCU = quantizedDCTData.Y[i];
            var crMCU = quantizedDCTData.Cr[i];
            var cbMCU = quantizedDCTData.Cb[i];

            EncodeMCUComponent(prevDc_Y, yMCU, bw, true);
            EncodeMCUComponent(prevDc_Cb, cbMCU, bw, false);
            EncodeMCUComponent(prevDc_Cr, crMCU, bw, false);

            prevDc_Y = (int)yMCU[0];
            prevDc_Cr = (int)crMCU[0];
            prevDc_Cb = (int)cbMCU[0];
        }

        _huffmanEncodingService.FlushBuffer(bw);
    }

    public DCTData DecodeData(ImageInfo jpeg, BinaryReader br)
    {
        if (jpeg == null)
            throw new ArgumentNullException(nameof(jpeg), nameof(jpeg).ToArgumentNullExceptionMessage());

        if (br == null)
            throw new ArgumentNullException(nameof(br), nameof(br).ToArgumentNullExceptionMessage());

        int mcuCount = CalculateMCUCount(jpeg);
        var result = new DCTData(mcuCount);

        int prevDc_Y = 0;
        int prevDc_Cr = 0;
        int prevDc_Cb = 0;

        for (int i = 0; i < mcuCount; i++)
        {
            result.Y[i] = DecodeMCUComponent(br, prevDc_Y, true);
            result.Cb[i] = DecodeMCUComponent(br, prevDc_Cb, false);
            result.Cr[i] = DecodeMCUComponent(br, prevDc_Cr, false);

            prevDc_Y = (int)result.Y[i][0];
            prevDc_Cb = (int)result.Cb[i][0];
            prevDc_Cr = (int)result.Cr[i][0];
        }

        return result;
    }

   void EncodeMCUComponent(int prevDC, Block8x8 mcu, BinaryWriter bw, bool isLuminance)
    {
        if (isLuminance)
        {
            _huffmanEncodingService.EncodeLuminanceDC((int)mcu[0], prevDC, bw);
            _huffmanEncodingService.EncodeLuminanceAC(mcu, bw);
        }
        else
        {
            _huffmanEncodingService.EncodeChrominanceDC((int)mcu[0], prevDC, bw);
            _huffmanEncodingService.EncodeChrominanceAC(mcu, bw);
        }
    }

    int CalculateMCUCount(ImageInfo jpeg)
    {
        int paddedHeight = _paddingService.CalculatePaddedDimension(jpeg.Height);
        int paddedWidth = _paddingService.CalculatePaddedDimension(jpeg.Width);
        int mcuCount = paddedHeight * paddedWidth / 64;
        return mcuCount;
    }

    Block8x8 DecodeMCUComponent(BinaryReader br, int prevDc, bool isLuminance)
    {
        int dc;
        Block8x8 result;
        if (isLuminance)
        {
            dc = _huffmanDecodingService.DecodeLuminanceDC(prevDc, br);
            result = _huffmanDecodingService.DecodeLuminanceAC(br);
        }
        else
        {
            dc = _huffmanDecodingService.DecodeChrominanceDC(prevDc, br);
            result = _huffmanDecodingService.DecodeChrominanceAC(br);
        }

        result[0] = dc;
        return result;
    }
}