using AM2E;
using AM2E.Actors;
using AM2E.Input;

namespace AM2Editor;

/// <summary>
/// Handles misc. tasks that require updates outside of ImGui threads
/// </summary>
public class MainActor : Actor
{
    public static int SlotsTime = 0;
    
    public MainActor() 
        : base(0, 0, null)
    {
        SetPersistent(true);
        
    }

    protected override void OnPostStep()
    {
        if (EntryPoint.Exiting)
            EngineCore.GracefulExit();

        SlotsTime--;
        
        EntryPoint.SetTitle(SlotsTime % 3 == 1);

        var i =  InputManager.GetHeld(Input.ZoomIn) ? 1 : 0;
        var o = InputManager.GetHeld(Input.ZoomOut) ? 1 : 0;
        
        //EntryPoint.Zoom = Math.Clamp(EntryPoint.Zoom + (i - o) * 0.01f, 0.1f, 1f);
    }
}