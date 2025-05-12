using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class Teleport
    {
        public string id;

        protected bool tp1PosSet = false;
        protected bool tp2PosSet = false;
        public Vector3 tp1Pos { get; set; }
        public Vector3 tp2Pos { get; set; }

        public Light light;

        public Camera camera;

        public bool finished = true;
        protected bool teleported = false;
        protected float time = 0f;
        protected Vector3 nextLocation;

        public Teleport (string id, Light light, Camera camera)
        {
            this.id = id;
            this.light = light;
            this.camera = camera;
        }

        public void teleport()
        {
            if (!finished) { return; }
            if ((camera.pos - tp1Pos).Length < 0.4f) { nextLocation = tp2Pos; finished = false; }
            if ((camera.pos - tp2Pos).Length < 0.4f) { nextLocation = tp1Pos; finished = false; }
        }
        public void teleportAnimation(float time)
        {
            if (finished) { return; }
            this.time += time;
            if (this.time >= 3f)
            {
                light.lightOverdose = new Vector3(0f);
                finished = true;
                teleported = false;
                this.time = 0f;
            }
            else
            {
                if (this.time < 1.5f) { light.lightOverdose = Vector3.Lerp(new Vector3(0f), new Vector3(1f), this.time / 1.5f); }
                if (this.time >= 1.5f) { 
                    light.lightOverdose = Vector3.Lerp(new Vector3(1f), new Vector3(0f), (this.time - 1.5f) / 1.5f);
                    if (!teleported) { camera.pos = nextLocation; }
                }
            }

        }

        public void setTeleportPos(Vector3 pos)
        {
            if (!tp1PosSet) { tp1Pos = pos; tp1PosSet = true; }
            else if (!tp2PosSet) { tp2Pos = pos; tp2PosSet = true; }
        }



        }
}
