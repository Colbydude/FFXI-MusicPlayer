using System.Numerics;
using FosterImGUI;
using ImGuiNET;

namespace FFXIMusicPlayer;

public class Gui(MusicPlayer player)
{
    private readonly Renderer _imRenderer = new(player);
    private readonly MusicPlayer _player = player;

    private string _selectedFilePath = string.Empty;

    public void Dispose()
    {
        _imRenderer.Dispose();
    }

    public void Update()
    {
        _imRenderer.BeginLayout();

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Close", "Esc"))
                    _player.Exit();

                ImGui.EndMenu();
            }
        }
        ImGui.EndMainMenuBar();

        ShowFileWindow();

        _imRenderer.EndLayout();
    }

    public void Render()
    {
        _imRenderer.Render();
    }

    private void ShowFileWindow()
    {
        var viewport = ImGui.GetMainViewport();
        float menuBarHeight = ImGui.GetFrameHeight();
        float playbackBarHeight = 30;
        Vector2 windowPos = new(0, menuBarHeight);
        Vector2 windowSize = new(viewport.WorkSize.X, viewport.WorkSize.Y - menuBarHeight - playbackBarHeight);

        ImGui.SetNextWindowPos(windowPos);
        ImGui.SetNextWindowSize(windowSize);

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                       ImGuiWindowFlags.NoMove |
                                       ImGuiWindowFlags.NoBackground |
                                       ImGuiWindowFlags.NoSavedSettings |
                                       ImGuiWindowFlags.NoBringToFrontOnFocus;

        if (!ImGui.Begin("Fullscreen Window", ImGuiWindowFlags.NoTitleBar | windowFlags))
        {
            ImGui.End();
            return;
        }

        if (AudioFileReader.PathData == null || string.IsNullOrEmpty(AudioFileReader.InstallPaths.FinalFantasyXI))
        {
            ImGui.Text("No music files found.");
            ImGui.End();
            return;
        }

        // Left
        {
            ImGui.BeginChild("left pane", new Vector2(150, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);

            foreach (var entry in AudioFileReader.PathData)
            {
                var subdirectories = entry.Value.Subdirectories;

                if (subdirectories.Count == 0)
                    continue;

                string displayName = entry.Key == InstallPathTokens.FinalFantasyXI ? "Final Fantasy XI" : entry.Key;

                if (subdirectories.Count == 1)
                    RenderSubdirectory(subdirectories[0]);
                else if (ImGui.TreeNode(displayName)) // Root Node
                {
                    foreach (var subdir in subdirectories)
                        RenderSubdirectory(subdir);

                    ImGui.TreePop();
                }
            }

            ImGui.EndChild();
        }

        ImGui.SameLine();

        // Right
        {
            if (_selectedFilePath != string.Empty)
            {
                ImGui.BeginGroup();
                ImGui.BeginChild("File Information", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us

                ImGui.Text($"Path: {_selectedFilePath}");
                ImGui.Separator();

                ImGui.EndChild();
                ImGui.EndGroup();
            }
        }

        ImGui.End();
    }

    private void RenderSubdirectory(MusicPathData.Subdirectory subdir)
    {
        if (ImGui.TreeNode(subdir.Name)) // Subfolder Node
        {
            foreach (var file in subdir.Files)
            {
                if (ImGui.Selectable(file.DisplayName, _selectedFilePath == file.FullPath))
                {
                    _selectedFilePath = file.FullPath!;

                    if (_player.Music == null)
                        _player.Music = new();
                    else
                        _player.Music.Instance.Stop();

                    _player.Music.LoadBGW(file.FullPath!);
                }
            }

            ImGui.TreePop();
        }
    }
}
