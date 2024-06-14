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
    public class Item
    {
        // types of items
        public enum ItemType { Key, Food, Potion, Weapon, Shield, Armor, Treasure, None };
        // type of item
        public ItemType type;
        // item id
        public int item_id;
        // item spritesheet
        public Texture2D spritesheet;
        // item sprite in spritesheet
        public Rectangle sprite;
        // item position
        public Vector2 position;
        // item trigger collider
        public Collider trigger;
        // trigger x coord offset // dont bother with camera zoom
        private float trigger_offset_x = 3;
        // trigger y coord offset // dont bother with camera zoom
        private float trigger_offset_y = 10;
        // trigger width scale
        private float trigger_width_scale = 0.7f;
        // trigger height scale
        private float trigger_height_scale = 0.4f;
        // item x coord offset
        private float offset_x = 4;
        // item y coord offset
        private float offset_y = 7;
        // item scale
        private float scale = 0.80f;
        // reference to gameController
        public GameController gameController;
        // reference to the content manager
        ContentManager content;
        // tilesize
        int tilesize;
        // a camera reference
        Camera camera;
        // a bool that represents if items id on ground
        internal bool dropped;
        private bool active;

        public void Initialize(int item_id, ItemType type, bool dropped, Vector2 position, int tilesize, Camera camera, ContentManager content, GameController gameController)
        {
            this.item_id = item_id;
            this.type = type;
            this.position = position;
            this.tilesize = 34;
            this.camera = camera;
            this.content = content;
            this.gameController = gameController;

            this.dropped = dropped;

            if (type != ItemType.None)
            {
                spritesheet = content.Load<Texture2D>("items");

                // debug
                // Console.WriteLine(item_id + " : " + (item_id % 14 + " / " + item_id / 14 ) + " : " + type);

                sprite = new Rectangle(this.tilesize * (item_id % 14), this.tilesize * (item_id / 14), this.tilesize, this.tilesize);

                // item trigger collider
                float tilezoomed = camera.zoom * tilesize;
                
                trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + trigger_offset_y * camera.zoom,
                             tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (type != ItemType.None && dropped)
            {
                /*** camera framing ****/    
                float target_y = position.Y - camera.y;
                float target_x = position.X - camera.x;
                float tilezoomed = tilesize * camera.zoom;

                trigger.x = target_x + trigger_offset_x * camera.zoom + offset_x * camera.zoom;
                trigger.y = target_y + trigger_offset_y * camera.zoom + offset_y * camera.zoom;

                // check if item is visible on camera
                active = camera.IsOnCamera(tilesize, target_x, target_y);

                // update trigger collider subscription if visible on camera
                if (active)
                    gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale * scale, tilezoomed * trigger_height_scale * scale);
            }

        }

        public void Drop()
        {
            if (type != ItemType.None && !dropped)
            {
                dropped = true;
                // subscribe item
                gameController.SubscribeItem(this);
                // subscribe trigger collider (goes to trigger list instead of collider list)
                gameController.SubscribeCollider(trigger);
            }
        }

        public void Pick()
        {
            if (type != ItemType.None && dropped)
            {
                // pick up sound effect
                gameController.SubscribeSound(new Sound(content, Sound.SoundType.Item_1, 0.8f, gameController.volume * gameController.pickup_volume));
                // remove me from list of chests
                gameController.UnsubscribeItem(this);
                // remove my colliders from lists of colliders;
                gameController.UnsubscribeCollider(trigger);
            }

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // only draw item if it is active (visible on camera)
            if (type != ItemType.None && dropped && active)
            {
                /*** camera framing ****/
                float target_y = (float)Math.Round(position.Y) - camera.y + offset_y * camera.zoom;
                float target_x = (float)Math.Round(position.X) - camera.x + offset_x * camera.zoom;

                // draws item
                Vector2 itemPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                spriteBatch.Draw(spritesheet, itemPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom * scale, SpriteEffects.None, 0.1f);
            }
        }

    }
}
