using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Storage;
using ImageSteg.Services;
using SkiaSharp;
using ImageCompression.JPEG.Services;

namespace ImageSteg.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    readonly IJPEGService _jpegService;
    readonly IBitmapRendererService _bitmapService;
    readonly IFileSaver _fileSaver;

    string? _fileName;

    #region Properties

    public IBitmapRendererService BitmapRenderer
    {
        get => _bitmapService;
    }

    [ObservableProperty]
    public string? saveStatusMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EncodeImageCommand))]
    public bool isLoaded;

    #endregion

    public MainPageViewModel(IJPEGService jpegService, IBitmapRendererService bitmapService, IFileSaver fileSaver)
    {
        _jpegService = jpegService;
        _bitmapService = bitmapService;
        _fileSaver = fileSaver;
    }

    [RelayCommand]
    async Task LoadImage()
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync();
            await CopyImageAsync(image!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadImage threw: {ex.Message}");
        }
    }

    [RelayCommand]
    async Task CopyImageAsync(FileResult imageFile)
    {
        if (imageFile == null)
            return;

        // Save the file into local storage
        _fileName = imageFile.FileName;
        string localFilePath = Path.Combine(FileSystem.CacheDirectory, imageFile.FileName);
        using Stream sourceStream = await imageFile.OpenReadAsync();
        using FileStream localFileStream = File.OpenWrite(localFilePath);
        await sourceStream.CopyToAsync(localFileStream);

        sourceStream.Position = 0;
        _bitmapService.Bitmap = SKBitmap.Decode(sourceStream);
        IsLoaded = true;
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    async Task EncodeImage()
    {
        using MemoryStream memStream = new MemoryStream();
        using BinaryWriter binaryWriter = new BinaryWriter(memStream);

        try
        {
            // Encode
            _jpegService.Encode(_bitmapService.Bitmap!, binaryWriter);
            Console.WriteLine("Image encoded to JPEG successfully.");

            // Save
            var fileLocationResult = await _fileSaver.SaveAsync(_fileName!, memStream);
            fileLocationResult!.EnsureSuccess();
            _fileName = fileLocationResult.FilePath;
            SaveStatusMessage = $"File saved: {fileLocationResult.FilePath}";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            SaveStatusMessage = $"File isn't saved: {ex.Message}";
        }
    }
}