
using System.ComponentModel;
using System.Numerics;
using AM2E;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Vector2 = System.Numerics.Vector2;

namespace AM2Editor;

public static class EntryPoint
{
    public static bool Exiting = false;
    
    private static string CurrentPath = AppContext.BaseDirectory[..^1];
    private static string? CurrentProject;

    private static bool IsNight = false;
    private static bool? PreviousIsNight = null;

    private static List<string> Objects = [];
    
    private static Mode CurrentMode = Mode.Open;
    
    private enum Mode
    {
        Open,
        Settings,
        Levels,
    }
    
    private static readonly string[] Splashes = 
    [
        "(Not Responding)",
        "Let me level with you",
        "TOOL! THIRTY! SCARAAAAAAAABS!",
        "Not a Hammer. But we like using a lot of those.",
        "it's like we're using some king of... level design tool",
        "it's like you're some king of... game maker",
        "Releasing Summer 2025!",
        "Quoth the Wombat; nevermall.",
        "SHIRTY!!! YOU'RE ON THE WRONG LAYER SHIRTY!!!",
        "Everyone! Into the level editor, NOW!",
        "Finally, a weapon to surpass JSONmancy!",
        "The fall began with an Apple...",
        "and brother",
        "It's always been like this.",
        "The level of unity this brings to the team of game makers is unreal...",
        "gm chat",
    ];

    private static readonly string[] SplashesButInsomnia =
    [
        "You are probably going to regret this in the morning.",
        "Causing sleep loss since 2026.",
        "All your sleep are belong to us.",
        "Good Morning",
        "It's awlays been like this.",
        "20% of all my best ideas are M3D's fault.",
        "Statistics are fictional, just like Canada.",
        "gn chat",
    ];
    
    public static void OnEntry()
    {
        SetTitle();
        
        _ = new MainActor();

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
        EngineCore.OnImGuiRender += () =>
        {
            #region MENU BAR
            
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open", "Ctrl+O"))
                    {
                        // TODO: make sure stuff is saved, warn, etc.
                        CurrentMode = Mode.Open;
                    }
                    
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Exit"))
                    {
                        // TODO: make sure stuff is saved, warn, etc.
                        Exiting = true;
                    }
                    
                    ImGui.EndMenu();
                }
                
                /*
                if (ImGui.Button("Let's go gambling!!!"))
                    MainActor.SlotsTime = 120;
                */
                
                ImGui.EndMainMenuBar();
            }
            
            #endregion

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove |
                        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.MenuBar;
            
            //ImGui.SetNextWindowPos(new Vector2(0, 20));
            //ImGui.SetNextWindowSize(new Vector2(1280, 700));

            if (CurrentMode != Mode.Open)
            {

                ImGui.Begin("##TabsMenu", flags);

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.Button("Settings"))
                        CurrentMode = Mode.Settings;

                    if (ImGui.Button("Levels"))
                        CurrentMode = Mode.Levels;

                    ImGui.EndMenuBar();
                }

                ImGui.End();
            }

            switch (CurrentMode)
            {
                case Mode.Open:
                    FileSelector();
                    break;
                case Mode.Settings:
                    break;
                case Mode.Levels:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        };

        EngineCore.ImGuiActive = true;
    }

    private static void FileSelector()
    {
        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize;
        
        ImGui.SetNextWindowPos(new Vector2(0, 20));
        ImGui.SetNextWindowSize(new Vector2(1280, 700));
        
        ImGui.Begin("Load Project", flags);
        
        var contents = Directory.GetDirectories(CurrentPath);
        Array.Sort(contents);
        
        ImGui.Text(Path.GetFullPath(CurrentPath));

        ImGui.Indent();
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();
        
        ImGui.Text("Folders:");
        
        ImGui.Indent();
        
        if (ImGui.Button(".."))
        {
            CurrentPath = Path.GetDirectoryName(CurrentPath) ?? CurrentPath;
        }
        
        foreach (var dir in contents)
        {
            var name = Path.GetFileName(dir);
            if (ImGui.Button(name.Truncate()))
            {
                CurrentPath += $"/{name}";
            }
        }
        
        ImGui.Unindent();
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();
        
        ImGui.Text("Files:");
        
        ImGui.Indent();

        contents = Directory.GetFiles(CurrentPath);
        Array.Sort(contents);

        foreach (var file in contents)
        {
            var name = Path.GetFileName(file);
            var isSln = name.EndsWith(".sln"); // TODO: actual regex or something, this isn't universal

            ImGui.PushStyleColor(ImGuiCol.Button, isSln ? new Vector4(0, 0.5f, 0.2f, 1.0f) : new Vector4());
            
            if (ImGui.Button(name.Truncate()) && isSln)
            {
                LoadProject(Path.GetDirectoryName(Path.GetFullPath(file)));
                
                CurrentMode = Mode.Settings;
            }
            
            ImGui.PopStyleColor();
        }
        
        ImGui.End();
    }

    // https://stackoverflow.com/questions/2776673/how-do-i-truncate-a-net-string
    private static string Truncate(this string value, int maxLength = 32, string truncationSuffix = "..")
    {
        return (value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value) ?? string.Empty;
    }

    private static void LoadProject(string path)
    {
        var excludedDirectories = new HashSet<string>(
            [ "bin", "obj", ".git", ".vs", ".idea"],
            StringComparer.OrdinalIgnoreCase);
        
        var files = Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);
        
        Objects.Clear();
        
        foreach (var file in files)
        {
            var excluded = false;
            var relativePath = Path.GetRelativePath(path, file);
            var split = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            foreach (var dir in split)
            {
                if (!excludedDirectories.Contains(dir)) 
                    continue;
                
                excluded = true;
                break;
            }

            if (excluded)
                continue;

            var source = File.ReadAllText(file);

            var tree = CSharpSyntaxTree.ParseText(source);
            var root = tree.GetCompilationUnitRoot();

            foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var placeable = false;
                
                var list = classDeclaration.AttributeLists
                    .SelectMany(a => a.Attributes);
                
                if (!list.Select(a => a.Name.ToString()).Contains("AM2EPlaceable"))
                    continue;
                
                foreach (var attribute in list)
                {
                    if (attribute.Name.ToString() == "AM2EPlaceable")
                    {
                        placeable = true;
                        continue;
                    }
                    
                    if (!attribute.Name.ToString().StartsWith("AM2E"))
                        return;
                }
                
                if (!placeable)
                    continue;
            
                Objects.Add(classDeclaration.Identifier.ToString());
            }
        }
        
        foreach (var obj in Objects)
            Console.WriteLine(obj);
    }
    
    public static void SetTitle(bool slotsMode = false)
    {
        IsNight = DateTime.Now.Hour is >= 0 and < 6;

        if (IsNight != PreviousIsNight || slotsMode)
        {
            var target = IsNight ? SplashesButInsomnia : Splashes;
            EngineCore.SetTitle("AM2Editor: " + target[RNG.Random(target.Length - 1)]);
        }
        
        PreviousIsNight = IsNight;
    }
}