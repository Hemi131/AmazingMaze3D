using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    /// <summary>
    /// Jednoduchý model obsahující seznam vrcholů
    /// </summary>
    public class Model : IDisposable
    {

        public Shader Shader { get; set; }

        public Material Material { get; set; }            

        public Vector3 position;

        int vbo;       
        int ibo;
        int vao;

        int triangles;

        public Model(Vector3 pos, SimpleObj objFile, Shader shader, Vector3 diff, Vector3 spec, float shin)
        {
            Material = new Material(diff, spec, shin);
            position = pos;
            Shader = shader;
            Vertex[] vertices = objFile.verticecs;
            int[] indices = objFile.indices;
            triangles = indices.Length / 3;

            // vytvoření a připojení VAO
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // Vytvoření a připojení VBO
            vbo = GL.GenBuffer();
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.SizeOf(), vertices, BufferUsageHint.StaticDraw);

            // vytvoření a připojení IBO
            ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // namapování pointerů na lokace v shaderu
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeOf(), IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeOf(), (IntPtr)3*sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }


        /// <summary>
        /// Vykreslení modelu
        /// </summary>
        public void Draw(Camera camera, Flashlight flashlight, Light light)
        {
            Matrix4 translate = Matrix4.CreateTranslation((float)position.X, (float)position.Y, (float)position.Z);

            Shader.Use();
            Shader.SetUniform("projection", camera.Projection);
            Shader.SetUniform("view", camera.View);
            Shader.SetUniform("model", translate);

            Shader.SetUniform("cameraPosWorld", camera.pos);

            Shader.SetUniform("lightPosWorld", light.position);
            Shader.SetUniform("lightColor", light.color);
            Shader.SetUniform("lightIntensity", light.intensity * 0.5f);

            Shader.SetUniform("flashlightPosWorld", flashlight.position);
            Shader.SetUniform("flashlightColor", flashlight.color);
            Shader.SetUniform("flashlightIntensity", flashlight.intensity);
            Shader.SetUniform("flashlightDirWorld", flashlight.direction);

            Material.SetUniforms(Shader);

            // Připojení bufferu
            GL.BindVertexArray(vao);

            //GL.Enable(EnableCap.CullFace);
            //GL.LineWidth(5);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

            // Vykreslení pole vrcholů
            GL.DrawElements(PrimitiveType.Triangles, 3*triangles, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        #region Dispose - uvolnění paměti
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                GL.DeleteBuffer(vbo);
            }
        }

        ~Model() => Dispose(false);
        #endregion
    }
}
