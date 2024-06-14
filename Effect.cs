using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Gamerator
{
    public class Effect
    {
        public Texture2D texture;
        public Rectangle frame;
        public int tilesize_x = 32;
        public int tilesize_y = 32;
        public EffectType effect_type;
        public ContentManager content;
        public bool active;
        public float effect_timer;
        private float shrink_factor;
        private Camera camera;
        private SpriteBatch spriteBatch;
        private float pos_x;
        private float pos_y;
        private float scale;
        private float duration;
        private float center_factor_x;
        private float center_factor_y;

        public enum EffectType { Crack_1, Smoke_1 };

        // gets width of texture
        public int Width
        {
            get { return texture.Width; }
        }
        // Get height of texure
        public int Height
        {
            get { return texture.Height; }
        }

        public Effect(ContentManager Content, EffectType effect_type)
        {
            this.effect_type = effect_type;
            content = Content;
            texture = Content.Load<Texture2D>("effects");
            center_factor_x = 1f;
            center_factor_y = 1f;
            effect_timer = 0f;
            active = false;
        }

        public void LoadContent(SpriteBatch spriteBatch, Camera camera)
        {
            this.spriteBatch = spriteBatch;
            this.camera = camera;

            int ind_x, ind_y;
            int offset_x = 0, offset_y = 0;

            switch (effect_type)
            {
                case EffectType.Crack_1:
                    ind_x = 15; ind_y = 15;
                    offset_x = -13; offset_y = -13;
                    center_factor_x = 1.5f; center_factor_y = 3f; break;
                case EffectType.Smoke_1:
                    ind_x = 12; ind_y = 15;
                    offset_x = -14; offset_y = -22;
                    center_factor_x = 1f; center_factor_y = 1f; break;
                default:
                    ind_x = 0; ind_y = 0; break;
            }

            // sets the frame from the effect source sheet
            frame = new Rectangle(ind_x * tilesize_x + offset_x, ind_y * tilesize_y + offset_y, tilesize_x, tilesize_y);
        }

        public void UnloadContent()
        {
            texture.Dispose();
        }

        public void CreateEffect(GameController gameController, SpriteBatch spriteBatch, Camera camera,
                                   float x, float y, float scale, float duration)
        {
            // Initialize effect
            LoadContent(spriteBatch, camera);
            Update(x, y, scale, duration);
            // sets active and add to list to be draw
            active = true;
            // subscribe itself to the list of effects
            gameController.SubscribeEffect(this);
           
            // sound effect
            switch (effect_type)
            {
                case Effect.EffectType.Crack_1:
                    gameController.SubscribeSound(new Sound(content, Sound.SoundType.Hit_Chest, 0.5f, gameController.volume * gameController.hit_volume));
                    break;
                case Effect.EffectType.Smoke_1:
                    gameController.SubscribeSound(new Sound(content, Sound.SoundType.Destruction_1, duration, gameController.volume * gameController.destruction_volume));
                    break;
                default:
                    break;
            }
        }

        public void Update(float pos_x, float pos_y, float scale, float duration)
        {
            this.pos_x = pos_x;
            this.pos_y = pos_y;
            this.scale = scale;
            this.duration = duration;

            float temp_scale;

            // offset by scale
            if (scale - 1 < 0)
                temp_scale = 0;
            else
                temp_scale = scale - 1;
            this.pos_x = pos_x - ((temp_scale) * (tilesize_x * camera.zoom / 2));
            this.pos_y = pos_y - ((temp_scale) * (tilesize_y * camera.zoom / 2));
        }

        public void Draw(GameController gameController, float deltatime, Player player, float global_light)
        {
            if (!active)
                return;

            // increases timer
            effect_timer += deltatime;

            if (effect_timer < duration)
            {
                shrink_factor = (camera.zoom * scale) / duration;
                float s_scale = (camera.zoom * scale) - (effect_timer * shrink_factor);

                // to shrink and keep sprite centered
                float previousSize = Width * ((camera.zoom * scale) - ((effect_timer - deltatime) * shrink_factor));
                float newSize = Width * s_scale;
                pos_x += (Math.Abs(previousSize - newSize) / (tilesize_x * center_factor_x));
                pos_y += (Math.Abs(previousSize - newSize) / (tilesize_y * center_factor_y));

                // to not follow camera 
                if (player.walking)
                {
                    // if camera is not stationary at x coord
                    if(camera.x > 0 && camera.x < camera.max_x)
                        pos_x -= player.direction.X * player.moveSpeed * deltatime;
                    // if camera is not stationary at y coord
                    if (camera.y > 0 && camera.y < camera.max_y)
                        pos_y -= player.direction.Y * player.moveSpeed * deltatime;
                }

                float effect_layer = 0.1f;
                effect_layer = gameController.chest_layer + Math.Abs((gameController.player_layer - gameController.chest_layer) / 2);

                //TODO CORRECT LAYER SYSTEM
                //Console.WriteLine(gameController.player_layer + " / " + gameController.chest_layer + " : " + effect_layer);
                
                spriteBatch.Draw(texture, // Texture
                    new Vector2((float)Math.Round(pos_x), (float)Math.Round(pos_y)),      // Position
                    frame,                // Source rectangle
                    Color.White * global_light,            // Color
                    0f,                      // Rotation
                    Vector2.Zero,                 // Origin
                    s_scale,            // Scale
                    SpriteEffects.None,     // Mirroring effect
                     0.1f + (0.0001f * pos_y) + (0.00005f *pos_x));                  // Depth
            }
            else
            {
                effect_timer = 0f;
                active = false;
            }

        }
    }
}
