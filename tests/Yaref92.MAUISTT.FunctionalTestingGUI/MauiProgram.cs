using Microsoft.Extensions.Logging;

using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

namespace Yaref92.MAUISTT.FunctionalTestingGUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("AudioIconFonts.ttf", "AudioIconFonts");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
#if ANDROID
        builder.Services.AddSingleton<IAudioRecorder, Services.Android.AudioRecorder>();
#elif IOS
        builder.Services.AddSingleton<IAudioRecorder, Services.iOS.AudioRecorder>();
#elif WINDOWS
        builder.Services.AddSingleton<IAudioRecorder, Services.Windows.AudioRecorder>();
#endif
        builder.Services.AddSingleton<MainViewModel>()
            .AddSingleton<MainPage>();
        return builder.Build();
    }
}
