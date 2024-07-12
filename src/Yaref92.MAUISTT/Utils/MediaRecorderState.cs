namespace Yaref92.MAUISTT.Utils;

public enum MediaRecorderState
{
    /// <summary>
    /// At this state, the MediaRecorder cannot be used again to record audio
    /// </summary>
    Released = -1,
    /// <summary>
    /// At this state, the MediaRecorder needs to go through configuration
    /// of audio source, format, encoding and output path
    /// </summary>
    Initial = 0,
    Reset = Initial,
    Recording,
    Paused,
    Stopped = Initial,
}
