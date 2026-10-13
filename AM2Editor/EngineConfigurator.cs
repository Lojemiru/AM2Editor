namespace AM2Editor;

using AM2E;

// TODO: This will get filled out with editor binds later.
public enum Input
{
    Dummy,
}

public static class EngineConfigurator
{
    public static AM2EConfig GetConfig()
    {
        return new AM2EConfig(typeof(Input), "AM2Editor")
        {
            IsMouseVisible = true,
            DefaultResolutionWidth = 1280,
            DefaultResolutionHeight = 720,
        };
    }
}