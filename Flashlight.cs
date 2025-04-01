using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazingMaze3D
{
    public class Flashlight
    {
        public Vector4 position;
        public Vector3 color;
        public float intensity;
        public Vector3 direction;
        public float depression = -2f * (float)Math.PI / 180f;
        bool toggle = true;

        public Flashlight()
        {
            position = new Vector4(Vector3.Zero, 1f);
            color = Vector3.One;
            intensity = 1f;
            direction = Vector3.UnitZ;
        }

        public static Quaternion FromEulerAngles(float pitch, float yaw)
        {
            // Convert degrees to radians
            float pitchRad = MathHelper.DegreesToRadians(pitch);
            float yawRad = MathHelper.DegreesToRadians(yaw);

            // Calculate the quaternion for each axis rotation
            Quaternion qPitch = Quaternion.FromAxisAngle(Vector3.UnitX, pitchRad);
            Quaternion qYaw = Quaternion.FromAxisAngle(Vector3.UnitY, yawRad);

            // Combine the quaternions
            Quaternion result = qYaw * qPitch;
            return result;
        }

        public void UpdatePositionAndDirection(Camera camera)
        {
            Vector3 posVec3 = new Vector3(camera.pos.X, 2.05f, camera.pos.Z);

            Matrix4 view;
            view = Matrix4.Identity;
            view *= Matrix4.CreateTranslation(-posVec3);
            view *= Matrix4.CreateRotationY(camera.ry);
            view *= Matrix4.CreateRotationX(camera.rx - depression);

            position = new Vector4(posVec3, 1f);
            direction = new Vector3(view[0, 2], view[1, 2], view[2, 2]);
            direction.Normalize();
        }
        public void Toggle()
        {
            if (toggle)
            {
                intensity = 0f;
            }
            else
            {
                intensity = 1f;
            }
            toggle = !toggle;
        }
    }
}
