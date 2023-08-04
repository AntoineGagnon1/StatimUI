//using Sandbox;
using StatimUI;

Statim.LoadEmbedded();
StatimUI.Debug.DebugSettings.ShowLayout = true;

Sandbox.Window window = new Sandbox.Window();

window.Run();

/*namespace Sandbox;

partial class Program
{
    static void Main(string[] args)
    {
        HelloFrom("Generated Code");
    }

    static partial void HelloFrom(string name);
}*/