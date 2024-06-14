using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    public class Shatter
    {
        private GraphicsDevice graphicsDevice;
        BasicEffect basicEffect;
        VertexPositionTexture[] vert;
        short[] ind;

        public Shatter(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Destroy(Texture2D texture, float duration)
        {
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            vert = new VertexPositionTexture[4];
            vert[0].Position = new Vector3(0, 0, 0);
            vert[1].Position = new Vector3(100, 0, 0);
            vert[2].Position = new Vector3(0, 100, 0);
            vert[3].Position = new Vector3(100, 100, 0);

            vert[0].TextureCoordinate = new Vector2(0, 0);
            vert[1].TextureCoordinate = new Vector2(1, 0);
            vert[2].TextureCoordinate = new Vector2(0, 1);
            vert[3].TextureCoordinate = new Vector2(1, 1);

            ind = new short[6];
            ind[0] = 0;
            ind[1] = 2;
            ind[2] = 1;
            ind[3] = 1;
            ind[4] = 2;
            ind[5] = 3;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vert, 0, vert.Length, ind, 0, ind.Length / 3);
            }
        }
    }
}
