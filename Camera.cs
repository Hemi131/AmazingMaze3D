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

        public Viewport viewport;
        public Camera(Viewport viewport)
        {
            this.viewport = viewport;
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

            //Vector2 dia1 = new Vector2(deltaX, deltaZ);
            //Vector2 dia2 = new Vector2(deltaX, deltaZ);

            //double angle = Math.PI / 4;
            //Matrix2 rot1 = new Matrix2((float)Math.Cos(angle), (float)Math.Sin(angle), -(float)Math.Sin(angle), (float)Math.Cos(angle));
            //Matrix2 rot2 = new Matrix2((float)Math.Cos(-angle), (float)Math.Sin(-angle), -(float)Math.Sin(-angle), (float)Math.Cos(-angle));

            //dia1 = rot1 * dia1;
            //dia2 = rot2 * dia2;


            float peek = 0.2f;

            int x1 = (int)((potX + 1f + peek) / 2f) + 1;
            int z1 = (int)((potZ + 1f + peek) / 2f);
            int x2 = (int)((potX + 1f - peek) / 2f) + 1;
            int z2 = (int)((potZ + 1f - peek) / 2f);
            //int x3 = (int)((pos.X + dia1.X + 1f + peek * 4) / 2f) + 1;
            //int z3 = (int)((pos.Z + dia1.Y + 1f + peek * 4) / 2f);
            //int x4 = (int)((pos.X + dia2.X + 1f + peek * 4) / 2f) + 1;
            //int z4 = (int)((pos.Z + dia2.Y + 1f + peek * 4) / 2f);


            if (!wallHere[x1,z1] && !wallHere[x2, z2] && !wallHere[x1, z2] && !wallHere[x2, z1]/* && !wallHere[x3, z3] && !wallHere[x4, z3]*/)
            {
                pos.X = potX;
                pos.Z = potZ;
            }
            else
            {
                Vector2 vec1 = new Vector2(deltaX, 0f);
                Vector2 vec2 = new Vector2(0f, deltaZ);
                vec1.Normalize();
                vec2.Normalize();
                vec1 *= peek;
                vec2 *= peek;

                if (!wallHere[(int)((potX + 1f + vec1.X) / 2f) + 1, (int)((potZ + 1f) / 2f)]) pos.X = potX;
                if (!wallHere[(int)((potX + 1f) / 2f) + 1, (int)((potZ + 1f + vec2.Y) / 2f)]) pos.Z = potZ;

            }

            //for (int i = 0; i < 17 + 2; i++)
            //{
            //    for (int j = 0; j < 24 + 2; j++)
            //    {
            //        if ((x2 == j) && (z2 == i)) { Console.Write("O"); }
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
