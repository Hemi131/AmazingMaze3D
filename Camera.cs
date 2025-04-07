using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class Camera
    {
        private float zoom = 1;
        public Vector3 pos = new Vector3(0f,1.7f,5f);
        public float rx = 0;
        public float ry = 0;
        public float speed = 1.4f;
        public float sensitivity = 1;

        float peek = 0.2f;

        Vector2 vec1;
        Vector2 vec2;
        Vector2 vec3;
        Vector2 vec4;

        public Viewport viewport;
        public Camera(Viewport viewport)
        {
            this.viewport = viewport;

            vec1 = new Vector2(-1f, -1f) * peek;
            vec2 = new Vector2(1f, -1f) * peek;
            vec3 = new Vector2(-1f, 1f) * peek;
            vec4 = new Vector2(1f, 1f) * peek;
        }

        public virtual Matrix4 Projection
        {
            get
            {
                float ratio = (float)((viewport.Width * viewport.window.ClientSize.X) / (viewport.Height * viewport.window.ClientSize.Y));
                return Matrix4.CreatePerspectiveFieldOfView(zoom, ratio, 0.1f, 50);
            }
        }

        public void Zoom(float coef)
        {
            //zoom *= coef;
            zoom += coef / 10.0f;
            zoom = Math.Max(0.2f, Math.Min(2, zoom));
            Console.WriteLine(zoom);
        }

        public void Move(float xdt, float ydt, bool[,] wallHere)
        {
            float deltaX = speed * (float)(xdt * Math.Cos(ry) + ydt * Math.Sin(ry));
            float deltaZ = speed * (float)(xdt * Math.Sin(ry) - ydt * Math.Cos(ry));

            float potX = pos.X + deltaX;
            float potZ = pos.Z + deltaZ;

            int x1 = (int)((potX + 1f + vec1.X) / 2f);
            int z1 = (int)((potZ + 1f + vec1.Y) / 2f);
            int x2 = (int)((potX + 1f + vec2.X) / 2f);
            int z2 = (int)((potZ + 1f + vec2.Y) / 2f);
            int x3 = (int)((potX + 1f + vec3.X) / 2f);
            int z3 = (int)((potZ + 1f + vec3.Y) / 2f);
            int x4 = (int)((potX + 1f + vec4.X) / 2f);
            int z4 = (int)((potZ + 1f + vec4.Y) / 2f);

            int xn1 = (int)((pos.X + 1f + vec1.X) / 2f);
            int zn1 = (int)((pos.Z + 1f + vec1.Y) / 2f);
            int xn2 = (int)((pos.X + 1f + vec2.X) / 2f);
            int zn2 = (int)((pos.Z + 1f + vec2.Y) / 2f);
            int xn3 = (int)((pos.X + 1f + vec3.X) / 2f);
            int zn3 = (int)((pos.Z + 1f + vec3.Y) / 2f);
            int xn4 = (int)((pos.X + 1f + vec4.X) / 2f);
            int zn4 = (int)((pos.Z + 1f + vec4.Y) / 2f);

            if (!wallHere[x1, zn1] && !wallHere[x2, zn2] && !wallHere[x3, zn3] && !wallHere[x4, zn4]) pos.X = potX;
            if (!wallHere[xn1, z1] && !wallHere[xn2, z2] && !wallHere[xn3, z3] && !wallHere[xn4, z4]) pos.Z = potZ;

            //Console.WriteLine($"pos: {pos.X}, {pos.Y}, {pos.Z}");
            //for (int i = 0; i < 15 + 2; i++)
            //{
            //    for (int j = 0; j < 15 + 2; j++)
            //    {
            //        if (((int)((pos.X + 1f) / 2f) == j) && ((int)((pos.Z + 1f) / 2f) == i)) { Console.Write("O"); }
            //        else if (wallHere[j, i]) { Console.Write("X"); }
            //        else { Console.Write(" "); }
            //    }
            //    Console.WriteLine();
            //}
        }

        public void RotateX(float a)
        {
            rx += a * sensitivity;
            rx = (float)Math.Max(-Math.PI / 2, Math.Min(Math.PI / 2, rx));
        }

        public void RotateY(float a)
        {
            ry += a * sensitivity;
        }


        public virtual Matrix4 View
        {
            get
            {
                Matrix4 view;
                view = Matrix4.Identity;
                view *= Matrix4.CreateTranslation(-pos);
                view *= Matrix4.CreateRotationY(ry);
                view *= Matrix4.CreateRotationX(rx);
                return view;
            }

        }
    }
}
