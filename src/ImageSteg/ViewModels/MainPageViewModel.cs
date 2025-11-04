using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Storage;
using ImageSteg.Services;
using SkiaSharp;
using ImageCompression.F5.Services;
using ImageCompression.F5.Exceptions;

namespace ImageSteg.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    readonly IBitmapRendererService _bitmapService;
    readonly IFileSaver _fileSaver;
    readonly IF5Service _f5Service;

    string? _fileName;

    #region Properties

    public IBitmapRendererService BitmapRenderer
    {
        get => _bitmapService;
    }

    [ObservableProperty]
    public string? saveStatusMessage;

    [ObservableProperty]
    public string message = "Et In Arcadia Ego...";

    [ObservableProperty]
    public string password = "passw0rd";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EncodeImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecodeImageCommand))]
    public bool isLoaded;

    #endregion

    public MainPageViewModel(IBitmapRendererService bitmapService, IFileSaver fileSaver, IF5Service f5Service)
    {
        _bitmapService = bitmapService;
        _fileSaver = fileSaver;
        _f5Service = f5Service;
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

        // Encode
        try
        {
            _f5Service.Embed(_bitmapService.Bitmap!, Password, Message, binaryWriter);
            Console.WriteLine("Message successfully embedded.");
        }
        catch (CapacityException ce)
        {
            Console.WriteLine(ce.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        // Save
        try
        {
            var fileLocationResult = await _fileSaver.SaveAsync(_fileName!, memStream);
            fileLocationResult.EnsureSuccess();
            _fileName = fileLocationResult.FilePath;
            SaveStatusMessage = $"File saved: {fileLocationResult.FilePath}";
        }
        catch (Exception ex)
        {
            SaveStatusMessage = $"File isn't saved: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    void DecodeImage()
    {
        Message = string.Empty;

        using FileStream fileStream = new FileStream(_fileName!, FileMode.Open, FileAccess.Read);
        using BinaryReader binaryReader = new BinaryReader(fileStream);

        try
        {
            Message = _f5Service.Extract(Password, binaryReader);
        }
        catch (MatrixEncodingException me)
        {
            Console.WriteLine(me.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}