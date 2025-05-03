using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace AmazingMaze3D
{
    public class Texture
    {
        private int texID;

        public Texture(string filename)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult image;

            using (var stream = File.OpenRead(filename))
            {
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            }
            Create(image.Width, image.Height, image.Data);
        }

        protected void Create(int width, int height, byte[] data)
        {
            texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            //nahrne bitmapu do textury
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, data);


            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            if (Program.IsExtensionSupported("GL_EXT_texture_filter_anisotropic"))
            {
                GL.GetFloat((GetPName)All.MaxTextureMaxAnisotropyExt, out float maxAnisotropy);
                maxAnisotropy = Math.Min(maxAnisotropy, 16.0f);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureMaxAnisotropyExt, maxAnisotropy);
            }

        }



        public void Bind(int shaderloc)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.Uniform1(shaderloc, 0);
        }

    }
}
