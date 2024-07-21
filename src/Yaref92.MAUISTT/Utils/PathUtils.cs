#if ANDROID
using AndroidOS = Android.OS;
#elif IOS
using Foundation;
#endif

namespace Yaref92.MAUISTT.Utils;

public static class PathUtils
{
    public static string SetAudioFilePath(string projectName, string className)
    {
#if WINDOWS
        string fileName = $"{projectName}_{className}_{DateTime.UtcNow:ddMMM_hhmmss}.wav";
#else
        string fileName = $"{projectName}_{className}_{DateTime.UtcNow:ddMMM_hhmmss}.mp3";
#endif
        string directoryPath = "";
#if ANDROID
        directoryPath = Path.Combine(AndroidOS.Environment.ExternalStorageDirectory.AbsolutePath, AndroidOS.Environment.DirectoryRecordings);
#elif IOS
        directoryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.ApplicationDirectory, NSSearchPathDomain.All)[0];
#elif WINDOWS
        directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Recordings");
#endif
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string filePath = Path.Combine(directoryPath, fileName);
#if WINDOWS
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }
#endif
        return filePath;
    }
}
