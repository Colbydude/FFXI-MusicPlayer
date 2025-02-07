using Microsoft.Win32;

namespace FFXIMusicPlayer;

public static class Registry
{
    public static InstallPaths? GetInstallDirectories()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        const string registryPath = @"SOFTWARE\WOW6432Node\PlayOnlineUS\InstallFolder";
        string[] registryKeys = ["0001", "1000", "0002"];
        string[] installPaths = new string[registryKeys.Length];

        try
        {
            using RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(registryPath);

            if (key == null)
            {
                Console.WriteLine($"Registry key '{registryPath}' not found.");
                return null;
            }

            bool allFound = true;

            for (int i = 0; i < registryKeys.Length; i++)
            {
                installPaths[i] = key.GetValue(registryKeys[i]) as string ?? string.Empty;
                if (string.IsNullOrEmpty(installPaths[i]))
                {
                    Console.WriteLine($"Warning: Registry value '{registryKeys[i]}' not found.");
                    allFound = false;
                }
            }

            return allFound ? new InstallPaths(installPaths[0], installPaths[1], installPaths[2]) : null;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Registry access denied: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error reading registry: {ex.Message}");
        }

        return null;
    }
}
