using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    /// <summary>
    /// Collider class (square colliders only)
    /// </summary>
    public class Collider : IDisposable
    {
        public float x, y;
        public float width, height;
        public Rectangle box;
        public bool solid;
        public float layer;
        public bool active;

        public float last_x, last_y;

        // for debug, collider visualization
        internal Texture2D visual_box;

        public Object parent;

        public Collider(float x, float y, float width, float height, bool solid, Object parent, GraphicsDevice graphicsDevice)
        {
            this.x = x;
            this.y = y;
            last_x = x;
            last_y = y;
            this.width = width;
            this.height = height;
            this.solid = solid;
            this.parent = parent;
            layer = 0.1f;
            box = new Rectangle((int)Math.Round(x), (int)Math.Round(y), 
                                (int)Math.Round(width), (int)Math.Round(height));
            active = true;

            visual_box = new Texture2D(graphicsDevice, (int)Math.Round(width), (int)Math.Round(height), false, SurfaceFormat.Color);
            Color[] colorData = new Color[(int)Math.Round(width) * (int)Math.Round(height)];

            for (int i = 0; i < Math.Round(width) * Math.Round(height); i++)
                colorData[i] = Color.White;
            visual_box.SetData<Color>(colorData);
        }

        public Collider(float x, float y, float width, float height, bool solid, Object parent, GraphicsDevice graphicsDevice, float layer)
        {
            this.x = x;
            this.y = y;
            last_x = x;
            last_y = y;
            this.width = width;
            this.height = height;
            this.solid = solid;
            this.parent = parent;
            this.layer = layer;
            box = new Rectangle((int)Math.Round(x), (int)Math.Round(y),
                                (int)Math.Round(width), (int)Math.Round(height));
            active = true;

            visual_box = new Texture2D(graphicsDevice, (int)Math.Round(width), (int)Math.Round(height), false, SurfaceFormat.Color);
            Color[] colorData = new Color[(int)Math.Round(width) * (int)Math.Round(height)];

            for (int i = 0; i < Math.Round(width) * Math.Round(height); i++)
                colorData[i] = Color.White;
            visual_box.SetData<Color>(colorData);
        }

        internal void UpdateCollider(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            box.X = (int)Math.Round(x);
            box.Y = (int)Math.Round(y);
            box.Width = (int)Math.Round(width);
            box.Height = (int)Math.Round(height);
        }

        public bool CheckCollision(float x_2, float y_2, float width_2, float height_2)
        {
            // If collider is out of screen, its not colliding with anyone
            if (x < -width || y < -height)
                return false;

            Rectangle box_2 = new Rectangle((int)Math.Round(x_2), (int)Math.Round(y_2),
                                            (int)Math.Round(width_2), (int)Math.Round(height_2));

            if (Rectangle.Intersect(box, box_2) != Rectangle.Empty)
                return true;
            else
                return false;
        }

        public bool CheckCollision(Rectangle box_2)
        {
            // If collider is out of screen, its not colliding with anyone
            if (x < -width || y < -height)
                return false;

            if (Rectangle.Intersect(box, box_2) != Rectangle.Empty)
                return true;
            else
                return false;
        }

        public bool CheckCollision(Collider collider_2)
        {
            // If collider is out of screen, its not colliding with anyone
            if (x < -width || y < -height)
                return false;

            Rectangle box_2 = new Rectangle((int)Math.Round(collider_2.x), (int)Math.Round(collider_2.y),
                                            (int)Math.Round(collider_2.width), (int)Math.Round(collider_2.height));

            if (Rectangle.Intersect(box, box_2) != Rectangle.Empty)
                return true;
            else
                return false;
        }

        public bool CheckCollision(Point point)
        {
            // If collider is out of screen, its not colliding with anyone
            if (x < -width || y < -height)
                return false;

            return box.Contains(point);
        }

        internal string GetParentClassName()
        {
            return (parent.ToString().Split('.')[1]);
        }

        internal Object GetParent()
        {
            return parent;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Collider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
