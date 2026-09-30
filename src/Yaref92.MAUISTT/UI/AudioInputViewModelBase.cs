using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Yaref92.MAUISTT.UI;

/// <summary>
/// A base class for view models which use MAUISTT's audio input
/// capabilities.
/// Inherits from <seealso cref="ObservableObject"/>
/// </summary>
public abstract partial class AudioInputViewModelBase : ObservableObject
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRecording))]
    [NotifyPropertyChangedFor(nameof(IsNotPaused))]
    protected bool isRecording = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotPaused))]
    protected bool isPaused = false;
    [ObservableProperty]
    protected string currentAudioPostion = string.Format("{0:mm\\:ss}", TimeSpan.Zero);

    protected IDispatcherTimer timer;

    protected TimeSpan recordingLength = TimeSpan.Zero;

    public bool IsNotRecording => !IsRecording;
    public bool IsNotPaused => !IsPaused && IsRecording;

    protected void InitTimer()
    {
        timer = Dispatcher.GetForCurrentThread()?.CreateTimer()!;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
    }

    protected void Timer_Tick(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            recordingLength +=  TimeSpan.FromSeconds(1);
            CurrentAudioPostion = string.Format("{0:mm\\:ss}", recordingLength);
        });
    }

    protected static async Task GrantMicrophonePermissionIfNecessary()
    {
        string permissionString = "Microphone";
        await GrantPermissionIfNecessary<Permissions.Microphone>(permissionString);
    }

    protected static async Task GrantSpeechRecognitionPermissionIfNecessary()
    {
        string permissionString = "Speech recognition";
        await GrantPermissionIfNecessary<Permissions.Speech>(permissionString);
    }

    private static async Task GrantPermissionIfNecessary<TPermission>(string permissionString) where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<TPermission>();
        if (permissionStatus != PermissionStatus.Granted)
        {
            try
            {
                permissionStatus = await Permissions.RequestAsync<TPermission>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(title: $"{permissionString} permission not granted",
                        message: $"This app can't proceed without {permissionString.ToLowerInvariant()} permission and will therefore close",
                        accept: null, cancel: "Ok");
                    throw new UnauthorizedAccessException($"{permissionString} access not granted");
                }
            }
            catch (PermissionException)
            {
                Debug.WriteLine($"{permissionString} permission not supported");
                throw;
            }
        }
    }
}
