using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Gamerator
{
    public class Cursor
    {
        public Texture2D texture;
        public Rectangle frame;
        public int tilesize_x = 28;
        public int tilesize_y = 30;

        public enum CursorType { Arrow_1, Arrow_2, Sword_1, Sword_2, Pointer_1, Pointer_2, Grab_1, Grab_2, Look_1, Look_2 };

        // gets width of player
        public int Width
        {
            get { return texture.Width; }
        }
        // Get the height of the player
        public int Height
        {
            get { return texture.Height; }
        }

        public Cursor(ContentManager Content, CursorType cursor_type)
        {
            SetCursorType(Content, cursor_type);
            this.texture = Content.Load<Texture2D>("cursors");
        }

        public void SetCursorType(ContentManager Content, CursorType cursor_type)
        {
            int ind_x, ind_y;
            int offset_x = 0;
            int offset_y = 0;

            switch(cursor_type)
            {
                case CursorType.Arrow_1:
                    ind_x = 0; ind_y = 0; break;
                case CursorType.Arrow_2:
                    ind_x = 3; ind_y = 3; offset_x = 3; offset_y = 6; break;
                case CursorType.Sword_1:
                    ind_x = 2; ind_y = 0; break;
                case CursorType.Sword_2:
                    ind_x = 7; ind_y = 0; break;
                case CursorType.Pointer_1:
                    ind_x = 3; ind_y = 0; offset_y = 2; break;
                case CursorType.Pointer_2:
                    ind_x = 3; ind_y = 1; offset_x = 0;  offset_y = 6; break;
                case CursorType.Grab_1:
                    ind_x = 5; ind_y = 0; offset_x = 7;  break;
                case CursorType.Grab_2:
                    ind_x = 3; ind_y = 4; offset_x = 3; offset_y = 7; break;
                case CursorType.Look_1:
                    ind_x = 4; ind_y = 0; offset_x = 5; offset_y = 8; break;
                case CursorType.Look_2:
                    ind_x = 4; ind_y = 1; offset_x = 5; offset_y = 6; break;
                default:
                    ind_x = 0; ind_y = 0; break;
            }
            
            // sets the frame from the cursor source sheet
            frame = new Rectangle(ind_x * tilesize_x + offset_x, ind_y * tilesize_y + offset_y, tilesize_x, tilesize_y);
        }
    }
}
