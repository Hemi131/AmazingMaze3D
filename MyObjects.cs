using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AmazingMaze3D
{
    public class MyObjects
    {
        public Shader Shader;

        public Dictionary<string, ((Vertex[] verticecs, int[] indices), Texture)> modelData;

        public MyObjects(Shader shader)
        {

            Shader = shader;
            modelData = new Dictionary<string, ((Vertex[] verticecs, int[] indices), Texture)>();

            modelData.Add("wall1", (BetterObj.LoadObj("objects/wall1.obj"), new Texture("textures/wall22E.jpg", 0)));
            modelData.Add("wall1Blood", (BetterObj.LoadObj("objects/wall1.obj"), new Texture("textures/wall22Blood.jpg", 0)));
            modelData.Add("freeSpaceTop1", (BetterObj.LoadObj("objects/freeSpaceTop1.obj"), new Texture("textures/top2E.jpg", 0)));
            modelData.Add("freeSpaceBottom1", (BetterObj.LoadObj("objects/freeSpaceBottom1.obj"), new Texture("textures/floor24E.jpg", 0)));
            modelData.Add("wallPic1L", (BetterObj.LoadObj("objects/wall1.obj"), new Texture("textures/wallPicL.jpg", 0)));
            modelData.Add("wallPic1R", (BetterObj.LoadObj("objects/wall1.obj"), new Texture("textures/wallPicR.jpg", 0)));
            modelData.Add("floorTeleport", (BetterObj.LoadObj("objects/freeSpaceBottom1.obj"), new Texture("textures/floorTeleport.jpg", 0)));

        }

        public Model getWall1(Vector3 position)
        {
            var data = modelData["wall1"];

            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;

            Material material = new Material(diff, spec, shin);

            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getFreeSpaceTop1(Vector3 position)
        {
            var data = modelData["freeSpaceTop1"];

            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;

            Material material = new Material(diff, spec, shin);

            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getFreeSpaceBottom1(Vector3 position)
        {
            var data = modelData["freeSpaceBottom1"];
            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;
            Material material = new Material(diff, spec, shin);
            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getWallPic1L(Vector3 position)
        {
            var data = modelData["wallPic1L"];
            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;
            Material material = new Material(diff, spec, shin);
            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getWallPic1R(Vector3 position)
        {
            var data = modelData["wallPic1R"];
            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;
            Material material = new Material(diff, spec, shin);
            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getWall1Blood(Vector3 position)
        {
            var data = modelData["wall1Blood"];
            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;
            Material material = new Material(diff, spec, shin);
            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

        public Model getFloorTeleport(Vector3 position)
        {
            var data = modelData["floorTeleport"];
            Vector3 diff = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 spec = new Vector3(0.2f, 0.2f, 0.2f);
            float shin = 32.0f;
            Material material = new Material(diff, spec, shin);
            return new Model(data.Item1.verticecs, data.Item1.indices, position, Shader, material, data.Item2);
        }

    }
}
