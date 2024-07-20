#if WINDOWS
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Storage;

using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.Utils;

using WinMedia = Windows.Media;

namespace Yaref92.MAUISTT.Services.Windows;

public sealed class AudioRecorder : IAudioRecorder, IDisposable
{
    private AudioGraph? _audioGraph;
    private AudioDeviceInputNode? _deviceInputNode;
    private AudioFileOutputNode? _fileOutputNode;
    private string _storagePath = "";
    private MediaRecorderState _state;
    private static readonly MediaEncodingProfile _profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

    public AudioRecorder()
    {
        _state = MediaRecorderState.Initial;
    }

    public async Task InitAudioRecorder()
    {
        _audioGraph = await InitAudioGraph();
        _deviceInputNode = await CreateAudioDeviceInputNode(_audioGraph);
        _state = MediaRecorderState.Reset;
    }

    private static async Task<AudioGraph> InitAudioGraph()
    {
        DeviceInformationCollection devices =
         await DeviceInformation.FindAllAsync(WinMedia.Devices.MediaDevice.GetAudioRenderSelector());

        AudioGraphSettings audioGraphSettings = new(WinMedia.Render.AudioRenderCategory.Media)
        {
            EncodingProperties = _profile.Audio,
            PrimaryRenderDevice = devices[0],
        };

        CreateAudioGraphResult result = await AudioGraph.CreateAsync(audioGraphSettings);
        if(result.Status != AudioGraphCreationStatus.Success)
        {
            throw new Exception("Failed to create AudioGraph", result.ExtendedError);
        }
        return result.Graph;
    }

    private static async Task<AudioDeviceInputNode> CreateAudioDeviceInputNode(AudioGraph audioGraph)
    {
        DeviceInformationCollection devices =
            await DeviceInformation.FindAllAsync(WinMedia.Devices.MediaDevice.GetAudioCaptureSelector());

        CreateAudioDeviceInputNodeResult result =
            await audioGraph.CreateDeviceInputNodeAsync(WinMedia.Capture.MediaCategory.Media,
                audioGraph.EncodingProperties, devices[^1]);

        if (result.Status != AudioDeviceNodeCreationStatus.Success)
        {
            throw new Exception("Failed to create AudioDeviceInputNode");
        }

        return result.DeviceInputNode;
    }

    public async void StartRecord(string projectName, string className)
    {
        if (_state is MediaRecorderState.Initial)
        {
            throw new InvalidOperationException("Audio recorder was not initialized");
        }
        if (_state is MediaRecorderState.Reset or MediaRecorderState.Stopped)
        {
            await PrepareForRecordingAsync(projectName, className);
            _audioGraph?.Start();
            _state = MediaRecorderState.Recording;
        }
        else
        {
            //TODO: Handle or not
        }
    }

    private async Task PrepareForRecordingAsync(string projectName, string className)
    {
        _storagePath = PathUtils.SetAudioFilePath(projectName, className);
        StorageFile file = await StorageFile.GetFileFromPathAsync(_storagePath);

        CreateAudioFileOutputNodeResult result = await _audioGraph?.CreateFileOutputNodeAsync(file, _profile);
        if (result.Status != AudioFileNodeCreationStatus.Success)
        {
            throw new Exception("Failed to create AudioFileOutputNode", result.ExtendedError);
        }

        _fileOutputNode = result.FileOutputNode;

        _deviceInputNode?.AddOutgoingConnection(_fileOutputNode);
    }

    public void PauseRecord()
    {
        if (_state is not MediaRecorderState.Recording)
        {
            return;
        }
        _audioGraph?.Stop();
        _state = MediaRecorderState.Paused;
    }

    public void ResumeRecord()
    {
        if (_state is MediaRecorderState.Paused)
        {
            _audioGraph?.Start();
            _state = MediaRecorderState.Recording;
        }
        else
        {
            // TODO: Handle or not
        }
    }

    public string StopRecord()
    {
        if (_state is not (MediaRecorderState.Recording or MediaRecorderState.Paused))
        {
            return string.Empty;
        }
        _audioGraph?.Stop();
        _fileOutputNode?.FinalizeAsync();
        _state = MediaRecorderState.Stopped;
        string savedRecordingPath = _storagePath;
        _storagePath = "";
        return savedRecordingPath;
    }

    public void ResetRecord()
    {
        _audioGraph?.ResetAllNodes();
        _state = MediaRecorderState.Reset;
    }

    public void Dispose()
    {
        _deviceInputNode?.Dispose();
        _fileOutputNode?.Dispose();
        _audioGraph?.Dispose();
        _state = MediaRecorderState.Released;
    }
}
#endif
