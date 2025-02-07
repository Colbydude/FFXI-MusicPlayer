namespace FFXIMusicPlayer;

public struct InstallPaths(string finalFantasyXI, string playOnlineViewer, string tetraMaster)
{
    public string FinalFantasyXI = finalFantasyXI;
    public string PlayOnlineViewer = playOnlineViewer;
    public string TetraMaster = tetraMaster;
}

public static class InstallPathTokens
{
    public const string FinalFantasyXI = "<installPath:ffxi>";
    public const string PlayOnlineViewer = "<installPath:pol>";
    public const string TetraMaster = "<installPath:tm>";
}
