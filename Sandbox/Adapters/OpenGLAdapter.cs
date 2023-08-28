using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StatimUI;
using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sandbox.Adapters
{

    public class OpenGLAdapter : StatimUI.Rendering.IRenderingAdapter, IDisposable
    {
        private struct SubWindow
        {
            public NativeWindow NativeWindow;
            public int VertexArrayObject; // Cannot be shared between contexts
        }

        // Shared for all windows
        private int vertexBuffer;
        private int vertexBufferSize;
        private int indexBuffer;
        private int indexBufferSize;
        private int shader;
        private int shaderProjectionMatrixLocation;

        private NativeWindow mainWindow;
        private Dictionary<Panel, SubWindow> subWindows = new ();

        private int msaaSamples;

        public static Version OpenglGLVersion = new Version(3, 3);

        public OpenGLAdapter(Panel panel, NativeWindow nativeWindow, int msaa = 4)
        {
            msaaSamples = msaa;
            mainWindow = nativeWindow;

            AnimationManager.Start(new AnimationDesc(panel.Children[0], new Test(), 2f, new OutBounce()));

            mainWindow.Resize += (e) => {
                panel.Size = new(e.Size.X, e.Size.Y);
            };

            mainWindow.MakeCurrent();

            vertexBufferSize = 10000;
            indexBufferSize = 2000;
            CreateSharedData();

            GL.Enable(EnableCap.Multisample);

            subWindows.Add(panel, new() { NativeWindow = mainWindow, VertexArrayObject = CreateVAO(mainWindow) }); // Add the main window to the list of subwindows, this way it will also be rendered
        }


        // Will create a main window
        public OpenGLAdapter(Panel window, int msaa = 4)
            : this(
                  window, 
                  new NativeWindow(new NativeWindowSettings()
                  {
                    Size = new Vector2i(window.Size.Width, window.Size.Height),
                    WindowBorder = WindowBorder.Resizable,
                    NumberOfSamples = msaa,
                    APIVersion = OpenglGLVersion,
                    Vsync = VSyncMode.On
                  }), 
                  msaa
              )
        {
        }

        public unsafe void Start()
        {
            while (!OpenTK.Windowing.GraphicsLibraryFramework.GLFW.WindowShouldClose(mainWindow.WindowPtr))
            {
                OpenTK.Windowing.GraphicsLibraryFramework.GLFW.PollEvents();

                Update(0.016f);

                foreach(var pair in subWindows)
                {
                    pair.Value.NativeWindow.ProcessInputEvents();
                }
            }
        }

        public void Update(float dt)
        {
            AnimationManager.Update(dt);

            foreach (var pair in subWindows)
            {
                if(!pair.Value.NativeWindow.Context.IsCurrent) // must check, otherwise will crash
                    pair.Value.NativeWindow.Context.MakeCurrent();

                if (pair.Value.NativeWindow.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab))
                    FocusManager.ShiftFocus();

                Renderer.ClearLayers();
                Renderer.CurrentLayer.PushClipRect(new RectangleF(0f, 0f, pair.Key.Size.Width, pair.Key.Size.Height));
                pair.Key.Update(); // TODO : remove ref to window when the window system changes
                pair.Key.Render();
                Renderer.CurrentLayer.PopClipRect();

                GL.Viewport(0, 0, pair.Key.Size.Width, pair.Key.Size.Height);
                GL.ClearColor(1, 1, 1, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

                var watch = Stopwatch.StartNew();
                RenderTriangles(pair.Key, pair.Value);
                watch.Stop();
                Console.WriteLine($"{pair.Value.NativeWindow.Title} : {watch.Elapsed.TotalMilliseconds}");

                pair.Value.NativeWindow.Context.SwapBuffers();
            }
        }

        private void RenderTriangles(Panel window, SubWindow subWindow)
        {
            GL.BindVertexArray(subWindow.VertexArrayObject);
            CheckGLError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.UseProgram(shader);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);

            CheckGLError();

            // Vertex buffer
            int vertexSize = Renderer.MaxVerticesCount() * Unsafe.SizeOf<Vertex>();
            if (vertexSize > vertexBufferSize)
            {
                int newSize = (int)Math.Max(vertexBufferSize * 1.5f, vertexSize);
                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                vertexBufferSize = newSize;
            }

            CheckGLError();

            // Index buffer
            int indexSize = Renderer.MaxIndicesCount() * sizeof(uint);
            if (indexSize > indexBufferSize)
            {
                int newSize = (int)Math.Max(indexBufferSize * 1.5f, indexSize);
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                indexBufferSize = newSize;
            }

            CheckGLError();

            var pMatrix = Matrix4x4.CreateOrthographicOffCenter(
                0.0f,
                window.Size.Width,
                window.Size.Height,
                0.0f,
                -1.0f,
                1.0f);

            CheckGLError();

            // Draw the layers
            foreach (var layer in Renderer.Layers)
            {
                foreach (var command in layer.Commands)
                {
                    if (command.VerticesCount == 0 || command.Indices.Count == 0)
                        continue;

                    if (command.Texture.Id != IntPtr.Zero)
                        BindTexture(command.Texture.Id);
                    unsafe
                    {
                        var matrix = command.Transform * pMatrix;
                        GL.UniformMatrix4(shaderProjectionMatrixLocation, 1, false, (float*)&matrix);
                    }

                    // TODO : dont convert to array
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, command.VerticesCount * Unsafe.SizeOf<Vertex>(), command.VerticesToArray());
                    GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, command.Indices.Count * sizeof(uint), command.Indices.ToArray());

                    // 0,0 is bottom-left in opengl
                    GL.Scissor((int)command.ClipRect.X, (int)(window.Size.Height - command.ClipRect.Height - command.ClipRect.Y), (int)command.ClipRect.Width, (int)command.ClipRect.Height);

                    GL.DrawElements(BeginMode.Triangles, command.Indices.Count, DrawElementsType.UnsignedInt, 0);
                }
            }

            CheckGLError();
        }

        public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
        {
            int program = GL.CreateProgram();

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            CheckGLError();
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);
            CheckGLError();

            GL.AttachShader(program, vertex);
            CheckGLError();
            GL.AttachShader(program, fragment);
            CheckGLError();

            GL.LinkProgram(program);
            CheckGLError();

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
            }
            CheckGLError();

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
            CheckGLError();

            return program;
        }

        private static int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            CheckGLError();

            GL.ShaderSource(shader, source);
            CheckGLError();
            GL.CompileShader(shader);
            CheckGLError();

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }
            CheckGLError();

            return shader;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vertexBuffer);
            GL.DeleteBuffer(indexBuffer);

            GL.DeleteProgram(shader);
            CheckGLError();
        }

        public static void CheckGLError()
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine($"OpenGL Error ({i++}): {error}");
            }
        }

        public void FreeTexture(IntPtr ptr)
        {
            GL.DeleteTexture((int)ptr);
            CheckGLError();
        }

        public nint MakeTexture()
        {
            var texture = GL.GenTexture();
            BindTexture(texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return texture;
        }

        public void SetTextureData(IntPtr texture, Size size, byte[] data)
        {
            BindTexture(texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.Width, size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            CheckGLError();
        }

        public void BindTexture(nint texture)
        {
            GL.BindTexture(TextureTarget.Texture2D, (int)texture);
            CheckGLError();
        }

        public void CreateSubWindow(StatimUI.Panel window)
        {
            var nativeWindow = new NativeWindow(new NativeWindowSettings()
            {
                SharedContext = mainWindow.Context,
                Size = new Vector2i(window.Size.Width, window.Size.Height),
                WindowBorder = WindowBorder.Resizable,
                NumberOfSamples = msaaSamples,
                APIVersion = OpenglGLVersion
            });

            nativeWindow.Resize += (e) => {
                window.Size = new (e.Size.X, e.Size.Y);
            };

            nativeWindow.Closing += (e) => {
                window.TryClose();
            };

            subWindows.Add(window, new() { NativeWindow = nativeWindow, VertexArrayObject = CreateVAO(nativeWindow) });
            nativeWindow.MakeCurrent();

            GL.Enable(EnableCap.Multisample);
        }

        public void DestroySubWindow(StatimUI.Panel window)
        {
            var subWindow = subWindows[window];

            subWindow.NativeWindow.MakeCurrent();
            GL.DeleteVertexArray(subWindow.VertexArrayObject);
            subWindow.NativeWindow.Dispose();
            subWindows.Remove(window);
        }

        // Creates a vao for the window
        private int CreateVAO(NativeWindow window)
        {
            window.MakeCurrent();

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            int stride = Unsafe.SizeOf<Vertex>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            CheckGLError();

            return vao;
        }

        private void CreateSharedData()
        {
            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            string VertexSource = @"
#version 330 core
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 UV;

uniform mat4 projection_matrix;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    UV = in_UV;
    color = in_color;
}";
            string FragmentSource = @"
#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 UV;

out vec4 output_color;

void main()
{
    output_color = color * texture(in_fontTexture, UV);
}";

            shader = CreateProgram("StatimUI", VertexSource, FragmentSource);
            CheckGLError();
            shaderProjectionMatrixLocation = GL.GetUniformLocation(shader, "projection_matrix");
            CheckGLError();
        }
    }
}
