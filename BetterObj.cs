using AmazingMaze3D;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class BetterObj
    {
        public static (Vertex[] verticecs, int[] indices) LoadObj(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            List<Vector3> verts = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();

            List<Vertex> verticesList = new();
            List<int> indicesList = new();

            Dictionary<string, int> idx = new Dictionary<string, int>();

            foreach (string line in lines)
            {
                if (line.StartsWith("v "))
                {
                    verts.Add(ProcessV3(line));
                }
                else if (line.StartsWith("vn "))
                {
                    normals.Add(ProcessV3(line));
                }
                else if (line.StartsWith("vt "))
                {
                    uvs.Add(ProcessV2(line));
                }
                else if (line.StartsWith("f "))
                {
                    var vertgroups = line.Split(' ');
                    for (int i = 1; i < 4; i++)
                    {
                        if (!idx.ContainsKey(vertgroups[i]))
                        {
                            var positions = vertgroups[i].Split('/');
                            int.TryParse(positions[0], out int v);
                            int.TryParse(positions[1], out int t);
                            int.TryParse(positions[2], out int n);
                            verticesList.Add(new Vertex(verts[v - 1], normals[n - 1], uvs[t - 1]));
                            idx.Add(vertgroups[i], verticesList.Count);
                        }

                        indicesList.Add(idx[vertgroups[i]] - 1);
                    }
                }
            }
            return (verticesList.ToArray(), indicesList.ToArray());
        }

        private static Vector3 ProcessV3(string line)
        {
            var tokens = line.Split(' ');
            float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
            float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
            float.TryParse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z);
            return new Vector3(x, y, z);
        }

        private static Vector2 ProcessV2(string line)
        {
            var tokens = line.Split(' ');
            float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
            float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
            return new Vector2(x, y);
        }
    }
}
