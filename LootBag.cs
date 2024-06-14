using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Gamerator
{
    public class LootBag
    {
        /// loot bag's items
        // number of keys that opens all chests with one click
        public int n_keys = 0;
        // number of food that will regain player life by a little
        public int n_foods = 0;
        // number of treasures that will increase players level reward
        public int n_treasures = 0;
        // number of shields that will defend player for one hit
        public int n_shields = 0;
        // number of armors that will increase players overall defense
        public int n_armors = 0;
        // number of potions that will increase players life
        public int n_potions = 0;
        // number of weapons that will increase players overall attack 
        public int n_weapons = 0;

        // Lootbag drop rates (will be modified by world level and reward multiplier)
        internal int food_chance = 14;
        internal int key_chance = 15;
        internal int potion_chance = 13;
        internal int weapon_chance = 11;
        internal int shield_chance = 9;
        internal int armor_chance = 6;
        internal int treasure_chance = 5;
        // Maximum drops per item type
        internal int max_foods = 3;
        internal int max_keys = 4;
        internal int max_potions = 3;
        internal int max_weapons = 2;
        internal int max_armors = 2;
        internal int max_shields = 3;
        internal int max_treasures = 2;
        // reward drop rates mutiplier
        internal int reward_multiplier = 4;

        internal Vector2 position;
        internal int tilesize;
        internal Camera camera;
        internal ContentManager content;
        internal GameController gameController;
        internal bool dropped;
        internal bool active;
        internal Texture2D spritesheet;
        internal Rectangle sprite;

        // lootbag trigger collider
        public Collider trigger;
        // trigger x coord offset // dont bother with camera zoom
        private float trigger_offset_x = 2.5f;
        // trigger y coord offset // dont bother with camera zoom
        private float trigger_offset_y = 8;
        // trigger width scale
        private float trigger_width_scale = 0.8f;
        // trigger height scale
        private float trigger_height_scale = 0.55f;

        public void Initialize(Vector2 position, int tilesize, Camera camera, ContentManager content, GameController gameController)
        {
            this.position = position;
            this.tilesize = tilesize;
            this.camera = camera;
            this.content = content;
            this.gameController = gameController;
            dropped = false;

            // load spritesheet and create sprite frame
            spritesheet = content.Load<Texture2D>("bag");
            sprite = new Rectangle(0, 0, tilesize, tilesize);

            // initialize trigger
            var tilezoomed = tilesize * camera.zoom;
            trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + trigger_offset_y * camera.zoom,
                    tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            if (dropped)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                /*** camera framing ****/
                float target_y = position.Y - camera.y;
                float target_x = position.X - camera.x;
                float tilezoomed = tilesize * camera.zoom;

                trigger.x = target_x + trigger_offset_x * camera.zoom * camera.zoom;
                trigger.y = target_y + trigger_offset_y * camera.zoom * camera.zoom;

                // updates gamecontroller time to be picked
                gameController.pick_timer += delta;

                // check if item is visible on camera
                active = camera.IsOnCamera(tilesize, target_x, target_y);

                // update trigger collider subscription if visible on camera
                if (active)
                    gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale );
            }

        }

        // generates the number of drops of a certain type of item
        public int CalculateDrop(Item.ItemType type, bool reward)
        {
            int multiplier = 1;
            Random rand = gameController.random_noseed;
            int lucky;

            if (reward)
                multiplier = reward_multiplier;

            int item_chance, max_items;

            switch(type)
            {
                case Item.ItemType.Armor:
                    item_chance = armor_chance;
                    max_items = max_armors;
                    break;
                case Item.ItemType.Food:
                    item_chance = food_chance;
                    max_items = max_foods;
                    break;
                case Item.ItemType.Key:
                    item_chance = key_chance;
                    max_items = max_keys;
                    break;
                case Item.ItemType.Potion:
                    item_chance = potion_chance;
                    max_items = max_potions;
                    break;
                case Item.ItemType.Shield:
                    item_chance = shield_chance;
                    max_items = max_shields;
                    break;
                case Item.ItemType.Treasure:
                    item_chance = treasure_chance;
                    max_items = max_treasures;
                    break;
                case Item.ItemType.Weapon:
                    item_chance = weapon_chance;
                    max_items = max_weapons;
                    break;
                default:
                    item_chance = 0;
                    max_items = 0;
                    break;
            }

            // calculate the drop
            lucky = rand.Next(0, 100);

            // this type of item was dropped
            if (lucky < (item_chance + ((gameController.level * 0.5) * multiplier)))
            {
                // check how many of this type was dropped
                lucky = rand.Next(max_items, max_items * 100);
                // gives more chance for lower n of items
                for(int i = max_items; i > 0; i--)
                    if (lucky % i == 0)
                        return i;
            } 
            // no drops of that type of item
            else
                return 0;

            return 0;
        }

        public void Drop(bool reward, Vector2 position)
        {
            if (!dropped)
            {
                this.position = position;

                // calculate how many items were dropped of each type
                n_armors = CalculateDrop(Item.ItemType.Armor, reward);
                n_weapons = CalculateDrop(Item.ItemType.Weapon, reward); ;
                n_foods = CalculateDrop(Item.ItemType.Food, reward); ;
                n_shields = CalculateDrop(Item.ItemType.Shield, reward); ;
                n_potions = CalculateDrop(Item.ItemType.Potion, reward); ;
                n_keys = CalculateDrop(Item.ItemType.Key, reward); ;
                n_treasures = CalculateDrop(Item.ItemType.Treasure, reward); ;

                dropped = true;
                // subscribe item
                gameController.SubscribeLootBag(this);
                // subscribe trigger collider (goes to trigger list instead of collider list)
                gameController.SubscribeCollider(trigger);
            }
        }

        public void Pick()
        {
            if (dropped)
            {
                dropped = false;
                // pick up sound effect
                gameController.SubscribeSound(new Sound(content, Sound.SoundType.Item_1, 0.8f, gameController.volume * gameController.pickup_volume));
                // remove me from list of chests
                gameController.UnsubscribeLootBag(this);
                // remove my colliders from lists of colliders;
                gameController.UnsubscribeCollider(trigger);
            }

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // only draw item if it is active (visible on camera)
            if (dropped && active)
            {
                /*** camera framing ****/
                float target_y = position.Y - camera.y;
                float target_x = position.X - camera.x;
                float tilezoomed = tilesize * camera.zoom;

                // draws lootBag
                Vector2 lootBagPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                float loot_bag_layer = 0.1f + (0.0001f * (target_y - 5f *camera.zoom)) + (0.00005f * target_x);
                spriteBatch.Draw(spritesheet, lootBagPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, loot_bag_layer);
            }
        }
    }
}