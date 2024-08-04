
namespace Yaref92.MAUISTT.Abstractions;

public interface ISpeechToTextConverter
{
    //void InitializeConverter();
    Task StartListenAsync();
    Task<string> PauseListenAsync();
    Task ResumeListenAsync();
    Task<string> StopListenAsync();
    void ResetConverter();
}
