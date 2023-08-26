using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using StatimUI.Rendering;
using OpenTK.Graphics.OpenGL;
using Sandbox.Adapters;
using static System.Net.Mime.MediaTypeNames;
using StatimUI;

namespace Sandbox
{
    public class Window : GameWindow
    {
        public Window() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3), NumberOfSamples = 8 })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            FontManager.DefaultFont = FontManager.GetFont("arial.ttf", 14);


            GL.Enable(EnableCap.Multisample);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            Renderer.Adapter!.WindowResized(new System.Numerics.Vector2(ClientSize.X, ClientSize.Y));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(1, 1, 1, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            if (IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab))
                FocusManager.ShiftFocus();
            var watch = Stopwatch.StartNew();
            Renderer.Adapter!.Render();
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalMilliseconds);

            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }
    }
}