//using Sandbox;
using StatimCodeGenerator;
using Sandbox.Adapters;
using StatimUI;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

//StatimUI.DebugTools.DebugSettings.ShowLayout = false;
//StatimUI.DebugTools.DebugSettings.ShowTextRect = false;

var compiler = new Compiler();
compiler.LoadEmbedded();
//FocusManager.FocusedComponent = statimWindow.Root;

StatimUI.Rendering.Renderer.Adapter = new OpenGLAdapter(compiler.CreatePanel("Window"), new Size(1600, 900));
StatimUI.Rendering.Renderer.Adapter.CreateSubWindow(new Dockspace(compiler.CreatePanel("Window")), new (400, 400));

Dockspace.Dockspaces[0].Split(SplitDirection.X, compiler.CreatePanel("MyPanel"));

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