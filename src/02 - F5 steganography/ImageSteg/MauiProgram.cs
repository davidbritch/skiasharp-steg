using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ImageSteg.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using ImageSteg.Services;
using ImageCompression;

namespace ImageSteg;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.UseMauiCommunityToolkit()
			.RegisterServices()
			.RegisterViewModels()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
	{
		mauiAppBuilder.Services.AddImageServices();
		mauiAppBuilder.Services.AddTransient<IBitmapRendererService, BitmapRendererService>();
		mauiAppBuilder.Services.AddSingleton<IFileSaver>(FileSaver.Default);			

		return mauiAppBuilder;
	}

	public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
	{
		mauiAppBuilder.Services.AddSingleton<MainPageViewModel>();

		return mauiAppBuilder;
	}
}
