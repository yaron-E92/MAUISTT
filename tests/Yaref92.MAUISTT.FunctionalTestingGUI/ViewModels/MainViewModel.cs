using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.FunctionalTestingGUI.Pages;
using Yaref92.MAUISTT.UI;

namespace Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

public partial class MainViewModel : AudioInputViewModelBase
{
    private readonly IAudioRecorder _audioRecorder;

    [ObservableProperty]
    string lastRecordingPath = "";

    public MainViewModel(IAudioRecorder audioRecorder)
    {
        _audioRecorder = audioRecorder;
        InitTimer();
    }

    [RelayCommand]
    async Task RecordAudio()
    {
        await GrantMicrophonePermissionIfNecessary();
        _audioRecorder.StartRecord(typeof(MainViewModel).Namespace!, nameof(MainViewModel));
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

    [RelayCommand]
    async Task GoToSTT()
    {
        await Shell.Current.GoToAsync(nameof(STTPage));
    }

#if WINDOWS
    internal async Task InitializeAudioRecorder()
    {
        await (_audioRecorder as AudioRecording.Windows.AudioRecorder)?.InitAudioRecorder()!;
    }
#endif
}
