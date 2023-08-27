//using Sandbox;
using StatimCodeGenerator;
using Sandbox.Adapters;
using StatimUI;
using OpenTK.Windowing.GraphicsLibraryFramework;

//StatimUI.DebugTools.DebugSettings.ShowLayout = false;
//StatimUI.DebugTools.DebugSettings.ShowTextRect = false;

var statimWindow = new StatimUI.Panel() { Size = new(1600, 900)};

var compiler = new Compiler();
compiler.LoadEmbedded();
statimWindow.Root = compiler.CreateComponent("Window");
FocusManager.FocusedComponent = statimWindow.Root;

StatimUI.Rendering.Renderer.Adapter = new OpenGLAdapter(statimWindow);
StatimUI.Rendering.Renderer.Adapter.CreateSubWindow(new StatimUI.Panel() { Size = new (400, 400), Root = compiler.CreateComponent("Window") });
(StatimUI.Rendering.Renderer.Adapter as OpenGLAdapter).Start();

/*namespace Sandbox;

partial class Program
{
    static void Main(string[] args)
    {
        HelloFrom("Generated Code");
    }

    static partial void HelloFrom(string name);
}*/