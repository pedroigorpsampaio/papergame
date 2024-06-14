using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Gamerator
{
    public class Message
    {
        string text;
        Vector2 position;
        Color color;
        SpriteFont font;
        float size;
        GameController gameController;
        float duration;
        bool fixed_position;
        public bool FixedPosition { get { return fixed_position; } }
        public string Text { get { return text; } }
        public float Size { get { return size; } }

        private float timer = 0f;

        public Message(string text, Vector2 position, Color color, SpriteFont font, float size, float duration, GameController gameController)
        {
            this.text = text;
            this.position = position;
            this.color = color;
            this.font = font;
            this.size = size;
            this.duration = duration;
            this.gameController = gameController;
            fixed_position = true;

            timer = 0f;
        }

        public Message(string text, SpriteFont font, float size, float duration, GameController gameController)
        {
            this.text = text;
            color = Color.White;
            this.font = font;
            this.size = size;
            this.duration = duration;
            this.gameController = gameController;
            fixed_position = false;

            position.X = gameController.GraphicsDevice.Viewport.Width - (font.MeasureString(text).X * size);
            position.Y = gameController.GraphicsDevice.Viewport.Height - (font.MeasureString(text).Y * size);

            timer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(timer > duration)
                gameController.UnsubscribeMessage(this);

            timer+=delta;
        }

        public void Draw(SpriteBatch spriteBatch, float y_offset)
        {
            if (fixed_position)
                y_offset = 0f;

            spriteBatch.DrawString(font, text, new Vector2(position.X, position.Y + y_offset), color, 0f, Vector2.Zero, size, SpriteEffects.None, 0.999924f);
            spriteBatch.DrawString(font, text, new Vector2(position.X + 1, position.Y + y_offset + 1), Color.Black, 0f, Vector2.Zero, size, SpriteEffects.None, 0.999923f);
        }

        public float GetMessageHeight()
        {
            return font.MeasureString(text).Y * size;
        }
    }
}