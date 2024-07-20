using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Yaref92.MAUISTT.Abstractions;

namespace Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAudioRecorder _audioRecorder;

    [ObservableProperty]
    string lastRecordingPath = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRecording))]
    [NotifyPropertyChangedFor(nameof(IsNotPaused))]
    bool isRecording = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotPaused))]
    bool isPaused = false;
    [ObservableProperty]
    string currentAudioPostion = string.Format("{0:mm\\:ss}", TimeSpan.Zero);

    public bool IsNotRecording => !IsRecording;
    public bool IsNotPaused => !IsPaused && IsRecording;

    IDispatcherTimer timer;

    //DateTime recordingStart;
    TimeSpan recordingLength = TimeSpan.Zero;

    public MainViewModel(IAudioRecorder audioRecorder)
    {
        _audioRecorder = audioRecorder;
        timer = Dispatcher.GetForCurrentThread()?.CreateTimer()!;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            recordingLength +=  TimeSpan.FromSeconds(1);
            CurrentAudioPostion = string.Format("{0:mm\\:ss}", recordingLength);
        });
    }

    [RelayCommand]
    async Task RecordAudio()
    {
        PermissionStatus microphoneStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (microphoneStatus != PermissionStatus.Granted)
        {
            try
            {
                microphoneStatus = await Permissions.RequestAsync<Permissions.Microphone>();
                if (microphoneStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(title: "Microphone permission not granted",
                        message: "This app can't proceed without microphone permission and will therefore close",
                        accept: null, cancel: "Ok");
                    throw new UnauthorizedAccessException("Microphone access not granted");
                }
            }
            catch (PermissionException)
            {
                Debug.WriteLine("Microphone permission not supported");
                throw;
            }
        }
        _audioRecorder.StartRecord(typeof(MainViewModel).Namespace!, nameof(MainViewModel));
        //recordingStart = DateTime.Now;
        recordingLength = TimeSpan.Zero;
        timer.Start();
        IsRecording = true;
    }

    [RelayCommand]
    void PauseRecording()
    {
        _audioRecorder.PauseRecord();
        IsPaused = true;
        timer.Stop();
    }

    [RelayCommand]
    void ResumeRecording()
    {
        _audioRecorder.ResumeRecord();
        timer.Start();
        //recordingStart = DateTime.Now;
        IsPaused = false;
    }

    [RelayCommand]
    void StopRecording()
    {
        LastRecordingPath = _audioRecorder.StopRecord();
        IsPaused = false;
        IsRecording = false;
        timer.Stop();
        CurrentAudioPostion = string.Format("{0:mm\\:ss}", TimeSpan.Zero);
    }

#if WINDOWS
    internal async Task InitializeAudioRecorder()
    {
        await (_audioRecorder as Services.Windows.AudioRecorder)?.InitAudioRecorder()!;
    }
#endif
}
