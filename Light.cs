using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class Light
    {
        public Vector3 position;
        public Vector3 color;
        public float intensity;
        public Vector3 lightOverdose;

        public Light(Vector3 pos, float intens, Vector3 col, Vector3 lightOverdose) 
        {
            position = pos;
            color = col;
            intensity = intens;
            this.lightOverdose = lightOverdose;
        }
    }
}
