#if ANDROID
using Yaref92.MAUISTT.Abstractions;

using Android.Media;
using Yaref92.MAUISTT.Utils;

namespace Yaref92.MAUISTT.Services.Android;

public sealed class AudioRecorder : IAudioRecorder, IDisposable
{
    private readonly MediaRecorder _mediaRecorder;
    private string _storagePath = "";
    private MediaRecorderState _state;

    public AudioRecorder()
    {
        _mediaRecorder = new MediaRecorder(Application.Context);
        _state = MediaRecorderState.Initial;
    }

    public void StartRecord(string projectName, string className)
    {
        if (_state is MediaRecorderState.Initial or MediaRecorderState.Reset or MediaRecorderState.Stopped)
        {
            PrepareForRecording(projectName, className);
            _mediaRecorder.Start();
            _state = MediaRecorderState.Recording;
        }
        else
        {
            //TODO: Handle or not
        }
    }

    private void PrepareForRecording(string projectName, string className)
    {
        _storagePath = PathUtils.SetAudioFilePath(projectName, className);
        ConfigureMediaRecorderFromInitialState();
        _mediaRecorder.SetOutputFile(_storagePath);
        _mediaRecorder.Prepare();
    }

    private void ConfigureMediaRecorderFromInitialState()
    {
        _mediaRecorder.SetAudioSource(AudioSource.Mic);
        _mediaRecorder.SetOutputFormat(OutputFormat.AacAdts);
        _mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
    }

    public void PauseRecord()
    {
        if (_state is not MediaRecorderState.Recording)
        {
            return;
        }
        _mediaRecorder.Pause();
        _state = MediaRecorderState.Paused;
    }

    public void ResumeRecord()
    {
        if (_state is MediaRecorderState.Paused)
        {
            _mediaRecorder.Resume();
            _state = MediaRecorderState.Recording;
        }
        else
        {
            // TODO: Handle or not
        }
    }

    public string StopRecord()
    {
        if ( _state is not (MediaRecorderState.Recording or MediaRecorderState.Paused))
        {
            return string.Empty;
        }
        if (_state is MediaRecorderState.Paused)
        {
            _mediaRecorder.Resume();
        }
        _mediaRecorder.Stop();
        _state = MediaRecorderState.Stopped;
        string savedRecordingPath = _storagePath;
        _storagePath = "";
        return savedRecordingPath;
    }

    public void ResetRecord()
    {
        if (_state is MediaRecorderState.Paused)
        {
            _mediaRecorder.Resume();
        }
        _mediaRecorder.Reset();
        _state = MediaRecorderState.Reset;
    }

    public void Dispose()
    {
        _mediaRecorder.Release();
        _mediaRecorder.Dispose();
        _state = MediaRecorderState.Released;
    }
}
#endif
