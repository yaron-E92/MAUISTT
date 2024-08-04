using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;

using Microsoft.Extensions.Logging;

using Yaref92.Events.Abstractions;
using Yaref92.Events;
using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;
using Yaref92.MAUISTT.FunctionalTestingGUI.Pages;
using Yaref92.MAUISTT.SpeechToTextConversion;

namespace Yaref92.MAUISTT.FunctionalTestingGUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
        builder.Services.AddSingleton<IAudioRecorder, AudioRecording.Android.AudioRecorder>();
#elif IOS
        builder.Services.AddSingleton<IAudioRecorder, AudioRecording.iOS.AudioRecorder>();
#elif WINDOWS
        builder.Services.AddSingleton<IAudioRecorder, AudioRecording.Windows.AudioRecorder>();
#endif
        builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default)
            .AddSingleton<ISpeechToTextConverter, SpeechToTextConverter>()
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddScoped<ISubscription, Subscription>();

        builder.Services.AddSingleton<MainViewModel>()
            .AddSingleton<MainPage>();

        builder.Services.AddSingleton<STTViewModel>()
            .AddSingleton<STTPage>();

        return builder.Build();
    }
}
