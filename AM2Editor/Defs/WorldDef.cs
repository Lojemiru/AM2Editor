using System.Numerics;
using ImGuiNET;

namespace AM2Editor;

public class WorldDef
{
    public LevelDef[][][] Levels = [];
    
    public WorldSettings Settings = new();
    
    private Vector2 pan = Vector2.Zero;

    public void Draw()
    {
        DrawGrid();
    }
    
    private void DrawGrid()
    {
        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus;
        
        ImGui.SetNextWindowPos(new Vector2(0, 40));
        ImGui.SetNextWindowSize(new Vector2(1280, 700));
        
        ImGui.Begin("Levels", flags);
        
        var io = ImGui.GetIO();
        
        // Positions/dimensions
        var canvasPos = ImGui.GetCursorScreenPos();
        var canvasSize = ImGui.GetContentRegionAvail();
        var canvasEnd = new Vector2(canvasPos.X + canvasSize.X, canvasPos.Y + canvasSize.Y);
        
        // Zoom
        if (io.MouseWheel != 0.0f)
        {
            var mousePosInCanvas = io.MousePos - canvasPos;
            
            // Mouse world position
            var worldPosBeforeZoom = (mousePosInCanvas - pan) / EntryPoint.Zoom;

            // Zoom!
            var zoomFactor = 1.1f;
            EntryPoint.Zoom = io.MouseWheel > 0 ? EntryPoint.Zoom * zoomFactor : EntryPoint.Zoom / zoomFactor;
            EntryPoint.Zoom = Math.Clamp(EntryPoint.Zoom, 0.1f, 2.0f);

            // Adjust pan to compensate for shift
            pan = mousePosInCanvas - (worldPosBeforeZoom * EntryPoint.Zoom);
        }
        
        // Panning
        // TODO: Centralized "gesture" handling.
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) || ImGui.IsMouseDragging(ImGuiMouseButton.Right))
        {
            pan += io.MouseDelta;
        }

        var drawList = ImGui.GetWindowDrawList();

        // Background fill
        drawList.AddRectFilled(canvasPos, canvasEnd, ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.11f, 1.0f)));

        // Clip everything to canvas
        drawList.PushClipRect(canvasPos, canvasEnd, true);

        // TODO: Configurable somewhere? Or just centralized set of colors. Something.
        var gridColor = ImGui.GetColorU32(new Vector4(0.0f, 0.8f, 1.0f, 0.4f));

        // Vertical
        var gridStepX = Settings.GridX * EntryPoint.Zoom;
        var startX = canvasPos.X + pan.X % gridStepX;
        for (var x = startX; x < canvasEnd.X; x += gridStepX)
            drawList.AddLine(canvasPos with { X = x }, canvasEnd with { X = x }, gridColor);
        
        // Horizontal
        var gridStepY = Settings.GridY * EntryPoint.Zoom;
        var startY = canvasPos.Y + pan.Y % gridStepY;
        for (var y = startY; y < canvasEnd.Y; y += gridStepY)
            drawList.AddLine(canvasPos with { Y = y }, canvasEnd with { Y = y }, gridColor);

        // Don't want to clip things that come after
        drawList.PopClipRect();

        ImGui.End();
    }
}