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
        string fileName = $"{projectName}_{className}_{DateTime.UtcNow:ddMMM_hhmmss}.mp3";
        string directoryPath = "";
#if ANDROID
        directoryPath = Path.Combine(AndroidOS.Environment.ExternalStorageDirectory.AbsolutePath, AndroidOS.Environment.DirectoryRecordings);
#elif IOS
        directoryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.ApplicationDirectory, NSSearchPathDomain.All)[0];
#endif
        string filePath = Path.Combine(directoryPath, fileName);
        return filePath;
    }
}
