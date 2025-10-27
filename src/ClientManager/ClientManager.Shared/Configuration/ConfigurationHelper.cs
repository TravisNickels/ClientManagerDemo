using dotenv.net;

namespace ClientManager.Shared.Configuration;

public class ConfigurationHelper
{
    public static void LoadDotEnvFile(string? basePath = null)
    {
        basePath ??= AppContext.BaseDirectory;
        var envPath = FindEnvFile(basePath);
        if (envPath is not null && File.Exists(envPath))
            DotEnv.Load(new DotEnvOptions(envFilePaths: [envPath]));
        else
            Console.WriteLine(
                $"Warning: .env file not found at {basePath} or in any parent folders. Using appsettings.json for environment variables or default configuration."
            );
    }

    public static string? FindEnvFile(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);
        while (currentDir != null)
        {
            var envFilePath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envFilePath))
                return envFilePath;

            currentDir = currentDir.Parent;
        }
        return null;
    }
}
