#if IOS
using System;
using System.Text;

using AVFoundation;

using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.Utils;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Yaref92.MAUISTT.Services.iOS;

public sealed class AudioRecorder : IAudioRecorder
{
    private const float SampleRate = 44100.0f;
    private const int FormatID = (int) AudioToolbox.AudioFormatType.LinearPCM;
    private const int NumberOfChannels = 2;
    private const int LinearPCMBitDepth = 16;
    private const bool IsLinearPCMBigEndian = false;
    private const bool IsLinearPCMFloat = false;

    private AVAudioRecorder _mediaRecorder;
    private NSUrl? _url;
    private NSError? _error;
    private NSDictionary _settings;
    private string _storagePath = "";
    private double _pausedAt = 0;
    private MediaRecorderState _state;

    public AudioRecorder()
    {
        _mediaRecorder = new AVAudioRecorder();
        _state = MediaRecorderState.Initial;
        InitializeAudioSession();
    }
    private static bool InitializeAudioSession()
    {
        AVAudioSession audioSession = AVAudioSession.SharedInstance();
        NSError? err = audioSession.SetCategory(AVAudioSessionCategory.Record);
        if (err is not null)
        {
            Console.WriteLine("audioSession: {0}", err);
            return false;
        }
        err = audioSession.SetActive(true);
        if (err is not null)
        {
            Console.WriteLine("audioSession: {0}", err);
            return false;
        }
        return true;
    }

    public void StartRecord(string projectName, string className)
    {
        if (_state is MediaRecorderState.Initial or MediaRecorderState.Reset or MediaRecorderState.Stopped)
        {
            PrepareForRecording(projectName, className);
            _mediaRecorder.Record();
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
        _url = NSUrl.FromFilename(_storagePath);
        _settings = GenerateSettings();
        _mediaRecorder = AVAudioRecorder.Create(_url, new AudioSettings(_settings), out _error)!;
        _mediaRecorder!.PrepareToRecord();
    }

    private static NSDictionary GenerateSettings()
    {
        NSObject[] values =
        [
            NSNumber.FromFloat(SampleRate),
            NSNumber.FromInt32(FormatID),
            NSNumber.FromInt32(NumberOfChannels),
            NSNumber.FromInt32(LinearPCMBitDepth),
            NSNumber.FromBoolean(IsLinearPCMBigEndian),
            NSNumber.FromBoolean(IsLinearPCMFloat)
        ];
        NSObject[] keys =
        [
            AVAudioSettings.AVSampleRateKey,
            AVAudioSettings.AVFormatIDKey,
            AVAudioSettings.AVNumberOfChannelsKey,
            AVAudioSettings.AVLinearPCMBitDepthKey,
            AVAudioSettings.AVLinearPCMIsBigEndianKey,
            AVAudioSettings.AVLinearPCMIsFloatKey
        ];
        return NSDictionary.FromObjectsAndKeys(values, keys);
    }

    public void PauseRecord()
    {

        if (_state is not MediaRecorderState.Recording)
        {
            return;
        }
        _mediaRecorder.Pause();
        _pausedAt = _mediaRecorder.CurrentTime;
        _state = MediaRecorderState.Paused;
    }

    public void ResumeRecord()
    {
        if (_state is MediaRecorderState.Paused)
        {
            _mediaRecorder.RecordAt(_pausedAt);
            _state = MediaRecorderState.Recording;
        }
        else
        {
            // TODO: Handle or not
        }
    }

    public string StopRecord()
    {
        if (_state is not MediaRecorderState.Recording or MediaRecorderState.Paused)
        {
            return string.Empty;
        }
        _mediaRecorder.Stop();
        _state = MediaRecorderState.Stopped;
        string savedRecordingPath = _storagePath;
        _storagePath = "";
        return savedRecordingPath;
    }

    public void ResetRecord()
    {
        _mediaRecorder.Dispose();
        _mediaRecorder = new AVAudioRecorder();
        _state = MediaRecorderState.Initial;
    }
}
#endif
