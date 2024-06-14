using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    public class Token
    {
        public enum TokenType { None, Bronze, Silver, Gold };

        // token values;
        public static int bronze = 2;
        public static int silver = 5;
        public static int gold = 10;

        public int value;
        public TokenType token_type;
        private Camera camera;
        private Texture2D tileset;
        private float trigger_offset_x = 1f;
        private float trigger_offset_y = 1f;
        private float trigger_width_scale = 0.90f;
        private float trigger_height_scale = 0.86f;
        private Rectangle sprite;
        private int tilesize;
        private GameController gameController;
        private Vector2 position;
        private bool dropped;
        private Collider trigger;
        private ContentManager content;
        private bool active;

        public void Initialize(TokenType token_type, int token_ind_x, int token_ind_y, int off_x, int off_y, Vector2 position,
                        int tilesize, Camera camera, ContentManager content, GameController gameController)
        {
            this.token_type = token_type;
            this.tilesize = tilesize;
            this.camera = camera;
            this.gameController = gameController;
            this.position = position;
            this.content = content;

            dropped = true;

            tileset = content.Load<Texture2D>("tiles");

            // one size trigger for all types of tokens (as of now)
            float tilezoomed = camera.zoom * tilesize;
            trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + trigger_offset_y * camera.zoom,
                     tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            // subscribe trigger
            gameController.SubscribeCollider(trigger);

            // token sprite
            sprite = new Rectangle(tilesize * token_ind_x + off_x, tilesize * token_ind_y + off_y, tilesize, tilesize);

            // sets token values
            if (token_type == TokenType.Bronze)
                value = bronze;
            else if (token_type == TokenType.Silver)
                value = silver;
            else if (token_type == TokenType.Gold)
                value = gold;
        }

        public void Update(GameTime gameTime)
        {
            if (token_type != TokenType.None && dropped)
            {
                /*** camera framing ****/
                float target_y = position.Y - camera.y;
                float target_x = position.X - camera.x;
                float tilezoomed = tilesize * camera.zoom;

                trigger.x = target_x + trigger_offset_x * camera.zoom + 0f * camera.zoom;
                trigger.y = target_y + trigger_offset_y * camera.zoom + 0f * camera.zoom;

                // check if token is visible on camera
                active = camera.IsOnCamera(tilesize, target_x, target_y);

                // update trigger collider subscription if visible on camera
                if (active)
                    gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
            }

        }

        public void Drop(Vector2 position)
        {
            if (token_type != TokenType.None && !dropped)
            {
                dropped = true;
                this.position = position;
                // subscribe token
                gameController.SubscribeToken(this);
                // subscribe trigger collider (goes to trigger list instead of collider list)
                gameController.SubscribeCollider(trigger);
            }
        }

        public void Pick()
        {
            if (token_type != TokenType.None && dropped)
            {
                dropped = false;

                // pick up sound effect
                gameController.SubscribeSound(new Sound(content, Sound.SoundType.Item_1, 0.8f, gameController.volume * gameController.pickup_volume));
                // remove me from list of token
                gameController.UnsubscribeToken(this);
                // remove my colliders from lists of colliders;
                gameController.UnsubscribeCollider(trigger);
            }

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // only draw item if it is active (visible on camera)
            if (token_type != TokenType.None && dropped && active)
            {
                /*** camera framing ****/
                float target_y = (float)Math.Round(position.Y) - camera.y + 0f * camera.zoom;
                float target_x = (float)Math.Round(position.X) - camera.x + 0f * camera.zoom;

                // draws token
                Vector2 tokenPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                spriteBatch.Draw(tileset, tokenPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, 0.1f);
            }
        }
    }
}
