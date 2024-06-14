using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    public class Camera
    {
        // camera position
        public float x;
        public float y;
        // camera viewport's size
        public int height;
        public int width;
        // camera limits
        // The lower limit will nearly always be (0,0), and in this case the upper limit 
        // is equal to the size of the world minus the size of the camera's viewport.
        public float max_x;
        public float max_y;
        // zoom of camera
        public float zoom = 1.5f;
        public float zoom_speed;
        public float zoom_min;
        public float zoom_max;
        // world size in lines/cols
        int w_size_x;
        int w_size_y;
        // tileset's tilesize
        int tilesize;

        public void Initialize(float x, float y, int height, int width, int w_size_x, int w_size_y, int tilesize)
        {
            zoom_speed = 1f;
            zoom_min = 1f;
            zoom_max = 2f;
            max_x = (w_size_x * tilesize * zoom) - width;
            max_y = (w_size_y * tilesize * zoom) - height;
            this.x = x;
            this.y = y;
            this.height = height;
            this.width = width;
            this.w_size_x = w_size_x;
            this.w_size_y = w_size_y;
            this.tilesize = tilesize;
        }

        public void Update()
        {
            max_x = (w_size_x * tilesize * zoom) - width;
            max_y = (w_size_y * tilesize * zoom) - height;
        }

        /// <summary>
        /// transform world position to camera position
        /// </summary>
        /// <param name="position">world position of an object</param>
        /// <returns></returns>
        public Vector2 TransformToCameraPosition(Vector2 position)
        {
            return new Vector2(position.X - x, position.Y - y);
        }

        /// <summary>
        /// update camera position of an object
        /// </summary>
        /// <param name="position">world position of an object</param>
        /// <param name="cameraPosition">reference to camera position of an object</param>
        /// <returns></returns>
        public void UpdateCameraPosition(Vector2 position, ref Vector2 cameraPosition)
        {
            cameraPosition.X = (float)Math.Round(position.X - x);
            cameraPosition.Y = (float)Math.Round(position.Y - y);
        }

        /// <summary>
        /// returns the orthogonal layer of an object
        /// </summary>
        /// <param name="cameraPosition">object`s camera position</param>
        /// <returns></returns>
        public float GetOrthogonalLayer(Vector2 cameraPosition)
        {
            return 0.1f + (0.0001f * cameraPosition.Y) + (0.00005f * cameraPosition.X);
        }

        // camera offset must be applied before (target_x = position.x - camera.x...)
        public bool IsOnCamera(float tilesize, float target_x, float target_y)
        {
            float tilezoomed = tilesize * zoom;
            bool onCamera;

            // check if item is visible on camera
            if (target_x < -tilezoomed || target_y < -tilezoomed || target_x > width + tilezoomed || target_y > height + tilezoomed)
                onCamera = false;
            else
                onCamera = true;

            return onCamera;
        }

        // returns point in screen considering [i,j] from initial grid
        public Vector2 GetPointInScreen(int i, int j)
        {
            Vector2 destination;
            float tilezoomed = zoom * tilesize;
            int startCol = (int)(Math.Floor(x / tilezoomed));
            int startRow = (int)(Math.Floor(y / tilezoomed));
            Double offsetX = -x + startCol * tilezoomed;
            Double offsetY = -y + startRow * tilezoomed;

            var target_x = (j - (Math.Floor(x / tilezoomed))) * tilezoomed + offsetX;
            var target_y = (i - (Math.Floor(y / tilezoomed))) * tilezoomed + offsetY;

            destination = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));

            return destination;
        }

        public void Draw()
        {

        }

    }
}
