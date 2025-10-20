namespace ClientManagerAPI.Config;

public class ConfigHelper
{
    public static string? FindEnvFile(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);
        while (currentDir != null)
        {
            var envFilePath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envFilePath))
            {
                return envFilePath;
            }
            currentDir = currentDir.Parent;
        }
        return null;
    }
}

