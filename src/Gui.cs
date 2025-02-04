using System.Numerics;
using FosterImGUI;
using ImGuiNET;

namespace FFXIMusicPlayer;

public class Gui(MusicPlayer player)
{
    private readonly Renderer _imRenderer = new(player);
    private readonly MusicPlayer _player = player;

    private int _selectedFileIndex = 0;

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
                if (ImGui.MenuItem("Close", "Esc")) { _player.Exit(); }

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
        float menuBarHeight = ImGui.GetFrameHeight(); // Height of the main menu bar

        // Set window position and size (excluding the menu bar height)
        ImGui.SetNextWindowPos(new Vector2(0, 0 + menuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(viewport.WorkSize.X, viewport.WorkSize.Y - 30));

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                       ImGuiWindowFlags.NoMove |
                                       ImGuiWindowFlags.NoBackground |
                                       ImGuiWindowFlags.NoSavedSettings |
                                       ImGuiWindowFlags.NoBringToFrontOnFocus;

        ImGui.Begin("Fullscreen Window", ImGuiWindowFlags.NoTitleBar | windowFlags);

        if (AudioFileReader.MusicFiles.Count == 0 || _player.InstallPath == null)
        {
            ImGui.Text("No music files found.");
            return;
        }

        // Left
        {
            ImGui.BeginChild("left pane", new Vector2(150, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);

            for (int i = 0; i < AudioFileReader.MusicFiles.Count; i++)
            {
                if (ImGui.Selectable(AudioFileReader.MusicFiles[i].ToString().Replace(_player.InstallPath, ""), _selectedFileIndex == i))
                {
                    _selectedFileIndex = i;
                    _player.Music?.Instance.Stop();
                    _player.Music?.LoadBGW(AudioFileReader.MusicFiles[i].ToString());
                }
            }

            ImGui.EndChild();
        }

        ImGui.SameLine();

        // Right
        {
            ImGui.BeginGroup();
            ImGui.BeginChild("File Information", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us

            ImGui.Text($"Path: {AudioFileReader.MusicFiles[_selectedFileIndex]?.ToString().Replace(_player.InstallPath, "")}");
            ImGui.Separator();

            ImGui.EndChild();
            ImGui.EndGroup();
        }

        ImGui.End();
    }
}
