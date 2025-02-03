using FosterImGUI;
using ImGuiNET;

namespace FFXIMusicPlayer;

public static class Gui
{
    public static void UpdateGui(MusicPlayer player, Renderer renderer)
    {
        renderer.BeginLayout();

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Close", "Esc")) { player.Exit(); }

                ImGui.EndMenu();
            }
        }
        ImGui.EndMainMenuBar();

        renderer.EndLayout();
    }
}
