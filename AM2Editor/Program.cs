
using AM2Editor;

using var game = new AM2E.EngineCore("AM2Editor", ", AM2Editor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", EngineConfigurator.GetConfig(), EntryPoint.OnEntry);
game.Run();