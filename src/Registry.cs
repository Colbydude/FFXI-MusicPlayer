using Microsoft.Win32;

namespace FFXIMusicPlayer;

public static class Registry
{
    public static string? GetInstallDirectory()
    {
        string registryPath = @"SOFTWARE\WOW6432Node\PlayOnlineUS\InstallFolder";
        string valueName = "0001";

        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            using RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(registryPath);

            if (key != null)
            {
                object? value = key.GetValue(valueName);

                if (value != null)
                    return value.ToString();
                else
                    throw new Exception($"Value '{valueName}' not found.");
            }
            else
                throw new Exception($"Registry key '{registryPath}' not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading registry: {ex.Message}");
            return null;
        }
    }
}
