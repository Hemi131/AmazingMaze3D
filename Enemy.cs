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
    public class Enemy : IDisposable
    {

        public Shader Shader { get; set; }

        public Material Material { get; set; }

        public Vector3 Position { get; set; }

        public Texture[] Textures { get; set; }

        int vbo;
        int ibo;
        int vao;

        int triangles;

        Vector3 facing = new Vector3(1f, 0f, 1f);

        private EnemyAI enemyAI = new EnemyAI();

        private float[] possibleRotations = new float[]
        {
            0f,
            (float)Math.PI / 4f,
            -(float)Math.PI / 4f,
            (float)Math.PI / 2f,
            -(float)Math.PI / 2f,
            (float)Math.PI / 4f * 3f,
            -(float)Math.PI / 4f * 3f,
            (float)Math.PI,
            -(float)Math.PI
        };


        public Enemy(Vertex[] vertices, int[] indices, Vector3 position, Shader shader, Material material, Texture[] textures)
        {
            Position = position;
            Shader = shader;
            Material = material;
            Textures = textures;
            Create(vertices, indices);
        }

        public Enemy(string objFilename, Vector3 position, Shader shader, Material material, Texture[] textures)
        {
            Position = position;
            Shader = shader;
            Material = material;
            Textures = textures;
            (Vertex[] vertices, int[] indices) = BetterObj.LoadObj(objFilename);
            Create(vertices, indices);
        }

        protected void Create(Vertex[] vertices, int[] indices)
        {
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
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeOf(), (IntPtr)3 * sizeof(float));
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeOf(), (IntPtr)6 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Vykreslení modelu
        /// </summary>
        public void Draw(Camera camera, Flashlight flashlight, Light light, bool wireModel)
        {
            Vector3 lookDir = camera.pos - Position;
            lookDir.Y = 0;
            lookDir.Normalize();

            float angleToFacePlayer = MathF.Atan2(lookDir.X, lookDir.Z);

            for (int i = 0; i < possibleRotations.Length; i++)
            {
                if (Math.Abs(angleToFacePlayer - possibleRotations[i]) <= (Math.PI / 8f))
                {
                    angleToFacePlayer = possibleRotations[i];
                    break;
                }
            }

            Matrix4 translate = Matrix4.Identity;
            translate *= Matrix4.CreateRotationY(angleToFacePlayer);
            translate *= Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, (float)Position.Z);

            var direction = new Vector3(translate[0, 2], translate[1, 2], translate[2, 2]);

            direction.Y = 0;
            direction.Normalize();
            facing.Y = 0;
            facing.Normalize();
            float angleToRotateTexture = MathF.Acos(MathHelper.Clamp(Vector3.Dot(facing, direction), -1.00f, 1.00f));


            Vector3 cross = Vector3.Cross(facing, direction);
            int sign = MathF.Sign(cross.Y);
                
            if (sign == 0) sign = 1;

            angleToRotateTexture *= (float)sign;

            int segment = 0;

            for (int i = 0; i < possibleRotations.Length; i++)
            {
                if (Math.Abs(angleToRotateTexture - possibleRotations[i]) <= (Math.PI / 8f))
                {
                    segment = i;
                    break;
                }
            }

            if (segment > 7) segment = 7;

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

            Shader.SetUniform("lightOverdose", light.lightOverdose);

            Material.SetUniforms(Shader);

            Textures[segment].Bind(Shader.GetUniformLocation("objTex"));

            // Připojení bufferu
            GL.BindVertexArray(vao);

            //GL.Enable(EnableCap.CullFace);
            //GL.LineWidth(5);
            GL.PolygonMode(TriangleFace.FrontAndBack, wireModel ? PolygonMode.Line : PolygonMode.Fill);

            // Vykreslení pole vrcholů
            GL.DrawElements(PrimitiveType.Triangles, 3 * triangles, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        public void Animate(bool[,] wallHere, float deltaTime, Camera camera) {

            Matrix4 view = camera.View;
            Vector3 playerDir = new Vector3(view[0, 2], view[1, 2], view[2, 2]);
            Vector3 nextMove = enemyAI.GetNextMove(Position, camera.pos, playerDir, deltaTime, wallHere);

            Position += nextMove;
            if (nextMove != Vector3.Zero)
            {
                facing = nextMove;
                facing.X *= -1.00f; //nevim proč, ale jinak se to otáčí na ****
            }

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

        ~Enemy() => Dispose(false);
        #endregion
    }
}
