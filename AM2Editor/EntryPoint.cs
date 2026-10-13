using AM2E;
using ImGuiNET;

namespace AM2Editor;

public static class EntryPoint
{
    public static bool Exiting = false;
    
    public static void OnEntry()
    {
        Console.WriteLine("BAZINGA");

        _ = new MainActor();

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
        EngineCore.OnImGuiRender += () =>
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open", "Ctrl+O"))
                    {
                         // TODO: implement
                    }
                    
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Exit"))
                    {
                        // TODO: make sure stuff is saved, warn, etc.
                        Exiting = true;
                    }
                    
                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
            }
            
            ImGui.Begin("Load Project", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);
            
            ImGui.Text("TEST");
            
            ImGui.Text("BAZINGA");
            
            ImGui.End();
        };

        EngineCore.ImGuiActive = true;
    }
}