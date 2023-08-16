//using Sandbox;
using Sandbox.Adapters;
using StatimUI;

Statim.LoadEmbedded();
StatimUI.Debug.DebugSettings.ShowLayout = true;

var statimWindow = new StatimUI.Window();
statimWindow.Root = Statim.CreateComponent("Window");

Sandbox.Window window = new Sandbox.Window(statimWindow);

StatimUI.Rendering.Renderer.Adapter = new OpenGLAdapter(statimWindow);

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