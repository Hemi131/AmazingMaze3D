using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class Material
    {
        public Vector3 diffuse = new Vector3(0.5f, 0.01f, 0.01f);
        public Vector3 specular = new Vector3(0.8f);
        public float shininess = 32.0f;
        public Vector3 ambient;

        public Material(Vector3 diff, Vector3 spec, float shin)
        {
            diffuse = diff;
            specular = spec;
            shininess = shin;
            ambient = new Vector3(1f, diff.Y, diff.Z);
        }

        public void SetUniforms(Shader shader)
        {
            //shader.SetUniform("material.diffuse", diffuse);
            shader.SetUniform("material.specular", specular);
            shader.SetUniform("material.shininess", shininess);
            shader.SetUniform("material.ambient", ambient);
        }
    }
}
