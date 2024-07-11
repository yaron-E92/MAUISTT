namespace Yaref92.MAUISTT.Abstractions;

public interface IAudioRecorder
{
    void StartRecord(string projectName, string className);
    void PauseRecord();
    void ResumeRecord();
    string StopRecord();
    void ResetRecord();
}
