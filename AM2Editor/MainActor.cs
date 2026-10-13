using AM2E;
using AM2E.Actors;

namespace AM2Editor;

/// <summary>
/// Handles misc. tasks that require updates outside of ImGui threads
/// </summary>
public class MainActor : Actor
{
    public MainActor() 
        : base(0, 0, null)
    {
        SetPersistent(true);
        
    }

    protected override void OnPostStep()
    {
        if (EntryPoint.Exiting)
            EngineCore.GracefulExit();
    }
}