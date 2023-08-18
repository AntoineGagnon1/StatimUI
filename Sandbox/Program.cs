//using Sandbox;
using StatimCodeGenerator;
using Sandbox.Adapters;
using StatimUI;

//StatimUI.DebugTools.DebugSettings.ShowLayout = false;
//StatimUI.DebugTools.DebugSettings.ShowTextRect = false;

var statimWindow = new StatimUI.Window();

var compiler = new Compiler();
compiler.LoadEmbedded();
statimWindow.Root = compiler.CreateComponent("Window");

Sandbox.Window window = new Sandbox.Window();

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