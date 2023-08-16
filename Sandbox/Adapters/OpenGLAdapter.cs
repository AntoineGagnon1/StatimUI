using OpenTK.Graphics.OpenGL;
using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sandbox.Adapters
{
    public class OpenGLAdapter : StatimUI.Rendering.IRenderingAdapter, IDisposable
    {
        private int vertexArray;
        private int vertexBuffer;
        private int vertexBufferSize;
        private int indexBuffer;
        private int indexBufferSize;

        private int shader;
        private Vector2 windowSize = Vector2.Zero;
        private int shaderProjectionMatrixLocation;

        private StatimUI.Window window;

        public OpenGLAdapter(StatimUI.Window window)
        {
            vertexBufferSize = 10000;
            indexBufferSize = 2000;

            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

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

            int stride = Unsafe.SizeOf<Vertex>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            CheckGLError();

            // Go back to the previously bound state
            GL.BindVertexArray(prevVAO);
            CheckGLError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            this.window = window;
            CheckGLError();
        }

        public void Render()
        {
            Renderer.ClearLayers();

            window.Update(); // TODO : remove ref to window when the window system changes

            // Cache the current state
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);

            // Bind our stuff
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.UseProgram(shader);
            GL.BindVertexArray(vertexArray);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

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
                windowSize.X,
                windowSize.Y,
                0.0f,
                -1.0f,
                1.0f);

            CheckGLError();

            // Draw the layers
            foreach (var layer in Renderer.Layers)
            {
                foreach (var command in layer.Commands)
                {
                    if (command.Vertices.Count == 0 || command.Indices.Count == 0)
                        continue;

                    if (command.Texture != IntPtr.Zero)
                        BindTexture(command.Texture);

                    unsafe
                    {
                        var matrix = command.Transform * pMatrix;
                        GL.UniformMatrix4(shaderProjectionMatrixLocation, 1, false, (float*)&matrix);
                    }

                    // TODO : dont convert to array
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, command.Vertices.Count * Unsafe.SizeOf<Vertex>(), command.Vertices.ToArray());
                    GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, command.Indices.Count * sizeof(uint), command.Indices.ToArray());

                    GL.DrawElements(BeginMode.Triangles, command.Indices.Count, DrawElementsType.UnsignedInt, 0);
                }
            }

            CheckGLError();

            // Reset state
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate((BlendingFactorSrc)prevBlendFuncSrcRgb, (BlendingFactorDest)prevBlendFuncDstRgb, (BlendingFactorSrc)prevBlendFuncSrcAlpha, (BlendingFactorDest)prevBlendFuncDstAlpha);

            if (prevBlendEnabled) 
                GL.Enable(EnableCap.Blend); 
            else 
                GL.Disable(EnableCap.Blend);

            if (prevDepthTestEnabled) 
                GL.Enable(EnableCap.DepthTest);
            else 
                GL.Disable(EnableCap.DepthTest);

            if (prevCullFaceEnabled) 
                GL.Enable(EnableCap.CullFace); 
            else 
                GL.Disable(EnableCap.CullFace);

            CheckGLError();
        }

        public void WindowResized(Vector2 size)
        {
            windowSize = size;
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
            GL.DeleteVertexArray(vertexArray);
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
                Debug.Print($"OpenGL Error ({i++}): {error}");
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
    }
}
