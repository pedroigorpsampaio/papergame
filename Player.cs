using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Gamerator
{
    public class Player
    {
        // Position of the Player relative to the upper left side of the screen
        public Vector2 Position;
        // stores last position to help in collisions
        public Vector2 lastPosition;
        // reference to camera
        private Camera camera;
        // player rotation
        public float angle = 0;
        // player origin for rotationg
        Vector2 origin;
        // State of the player
        public bool active;
        // Amount of hit points that player has
        public int health;
        // Player maximum health;
        public int max_health;
        // Player's attack;
        public float attack;
        // Player's defense;
        public float defense;
        // Player's attack_speed;
        public float attack_speed;
        // Player's maximum attack speed;
        public float max_attack_speed;
        // Player's critical hit chance
        public float critical_chance;
        // Player's critical hit multiplier
        public float critical_multiplier;
        // spritesheet
        public Texture2D spritesheet;
        // char id for spritesheet
        public int char_id;
        // char rectangle sprite in spritesheet
        public Rectangle sprite;
        // tilesize - same as spritesize
        public int tilesize;
        // player screen coords to guide camera
        public float screenX;
        public float screenY;
        // player movement direction
        public Vector2 direction;
        // initial animation frame;
        public int initial_frame = 1;
        // animation frame;
        public int frame = 1;
        // spritesheet number of frames
        public int n_frames = 3;
        // number of animation cycles;
        public int n_cycles = 1;
        // initial time between frames (without player speed)
        public float init_time_between_frames = 20f;
        // time between frames;
        public float time_between_frames;
        // frame time counter
        public float frame_timer = 0f;
        // player movement speed
        public float moveSpeed;
        // bool that represents if a player is walking
        public bool walking = false;

        /// player's items
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
        // atk speed will increase when finishing a level

        // player's list of tokens
        public List<Token> tokens;
        // a reference to the grid structure of the level
        public char[,] grid;
        // enemy collider
        public Collider collider;
        // reference to gameController
        public GameController gameController;
        // reference to the list of colliders
        public List<Collider> colliders;
        // coll x coord offset
        private float coll_offset_x = 8.7f;
        // coll y coord offset
        private float coll_offset_y = 14f;
        // coll width scale
        private float coll_width_scale = 0.5f;
        // coll height scale
        private float coll_height_scale = 0.4f;
        // player list of items
        public List<Item> items;
        // godmode
        internal bool godmode;
        // player trigger collider
        private Collider trigger;
        // trigger x coord offset
        private float trigger_offset_x = 8f;
        // trigger y coord offset
        private float trigger_offset_y = 13f;
        // trigger width scale
        private float trigger_width_scale = 0.55f;
        // trigger height scale
        private float trigger_height_scale = 0.45f;

        // player is alive
        internal bool alive;

        // player regen (will be affected by foods)
        float player_regen;
        float player_regen_tick;
        float player_regen_timer;
        float min_regen_tick;

        // death item punishement amount
        int death_item_loss = 10;

        // gets width of player
        public int Width
        {
            get { return sprite.Width; }
        }
        // Get the height of the player
        public int Height
        {
            get { return sprite.Height; }
        }

        public void Initialize(Texture2D spritesheet, int char_id, Vector2 position, int tilesize, Camera camera, char[,] grid, GameController gameController)
        {
            // calculates initial sprite by its char_id
            this.spritesheet = spritesheet;
            this.char_id = char_id;
            this.tilesize = tilesize;
            this.camera = camera;
            this.grid = grid;
            this.gameController = gameController;
            colliders = gameController.GetSubscribedColliders(true);

            // initial target = null
            gameController.player_target = null;

            godmode = false;

            sprite = new Rectangle(tilesize * (((char_id%4) * 3)+1), tilesize * 0, tilesize, tilesize);

            // player collider
            float tilezoomed = camera.zoom * tilesize;
            collider = new Collider(position.X - (tilezoomed / 2) + coll_offset_x * camera.zoom , position.Y - (tilezoomed / 1.5f) + coll_offset_y *camera.zoom, 
                                    tilezoomed * coll_width_scale, tilezoomed * coll_height_scale, true, this, gameController.GraphicsDevice);
            // subscribe collider
            gameController.SubscribeCollider(collider);

            // chest trigger collider
            trigger = new Collider(position.X - (tilezoomed / 2) + trigger_offset_x * camera.zoom, position.Y - (tilezoomed / 1.5f) + trigger_offset_y * camera.zoom,
                         tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            // subscribe trigger collider (goes to trigger list instead of collider list)
            gameController.SubscribeCollider(trigger);

            // Set the starting position of the player around the middle of the screen and to the back
            Position = position;
            lastPosition = position;

            screenX = (float)Math.Floor(position.X);
            screenY = (float)Math.Floor(position.Y);

            // Set the player to be active
            active = true;

            // Set the player's initial health
            health = 100;
            // Set player's initial max health
            max_health = 100;
            // Set player's initial attack
            attack = 15;
            // Set player's initial defense
            defense = 15;
            // Set player's initial attack speed
            attack_speed = 2f;
            // Set player's critical hit chance
            critical_chance = 10f;
            // Set player's critical hit multipler
            critical_multiplier = 3f;
            // Max attack speed
            max_attack_speed = 10f;

            // player regen initial values (will be affected by foods)
            player_regen = 0f;
            player_regen_tick = 2f;
            player_regen_timer = 0f;
            min_regen_tick = 0.25f;

            // Initialize list of tokens
            tokens = new List<Token>();

            // Initialize list of items
            items = new List<Item>();

            // initial direction (sprite looking down)
            direction = new Vector2(0,1);

            // sets origin for rotation
            origin = new Vector2(0, 0);

            // player is alive
            alive = true;
        }

        // updates player
        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update liveness of player
            if (health <= 0f)
            {
                health = 0;
                alive = false;
            }

            // check if player is alive
            if(!alive)
            {
                die_timer += delta;
                Die();
                return;
            }

            // update stats accordingly to items
            attack = 15 + (n_weapons * 0.6f);
            defense = 15 + (n_armors * 0.3f);
            max_health = 100 + (int)Math.Round((n_potions * 5.5f));
            attack_speed = 2f + (n_weapons * 0.01f);
            critical_chance = 10f + (n_weapons * 0.05f);
            critical_multiplier = 3f + (n_weapons * 0.005f);
            // update regen stats
            player_regen = 1f + (n_foods * 0.050f);
            player_regen_tick = 4f - (n_foods * 0.015f);
            if (player_regen_tick <= min_regen_tick)
                player_regen_tick = min_regen_tick;
            // regen life
            if(player_regen_timer >= player_regen_tick)
            {
                player_regen_timer = 0f;
                health += (int)Math.Floor(player_regen);
                if (health >= max_health)
                    health = max_health;
            }
            // increase regen timer
            player_regen_timer += delta;

            // update colliders
            /*** camera framing ****/
            float target_y = Position.Y - camera.y;
            float target_x = Position.X - camera.x;

            //check if walked into a trigger
            CheckTrigger();

            float tilezoomed = tilesize * camera.zoom;

            collider.x = target_x - (tilezoomed / 2) + coll_offset_x * camera.zoom;
            collider.y = target_y - (tilezoomed / 1.5f) + coll_offset_y * camera.zoom;

            trigger.x = target_x - (tilezoomed / 2)  + trigger_offset_x * camera.zoom;
            trigger.y = target_y - (tilezoomed / 1.5f) + trigger_offset_y * camera.zoom;

            // update colliders subscription 
            gameController.UpdateSubscription(collider, collider.x, collider.y, tilezoomed * coll_width_scale, tilezoomed * coll_height_scale);
            gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
        }

        private void RefreshCollider()
        {
            float target_y = Position.Y - camera.y;
            float target_x = Position.X - camera.x;
            float tilezoomed = tilesize * camera.zoom;

            collider.x = target_x - (tilezoomed / 2) + coll_offset_x * camera.zoom;
            collider.y = target_y - (tilezoomed / 1.5f) + coll_offset_y * camera.zoom;
        }

        float die_timer = 0f;
        float die_time = 5f;
        bool sound_played = false;
        int max_item_loss;

        private void Die()
        {
            int previous_level = gameController.level - 1;
            int previous_seed = gameController.seed - 1;

            if (previous_level < 1)
                previous_level = 1;
            if (previous_seed < 1)
                previous_seed = 1;
   
            // death sound / itme loss count  - first frame
            if (!sound_played)
            {
                string deathMessage = "YOU DIED";
                float scale = gameController.hud_scale * 2f;
                float deathMessageWidth = gameController.HUDFont.MeasureString(deathMessage).X * scale;
                float deathMessageHeight = gameController.HUDFont.MeasureString(deathMessage).Y * scale;
                Vector2 deathMessagePos = new Vector2(gameController.GraphicsDevice.Viewport.Width / 2 - deathMessageWidth / 2,
                                                        gameController.GraphicsDevice.Viewport.Height / 2 - deathMessageHeight * 1.5f);
                gameController.SubscribeMessage(new Message(deathMessage, deathMessagePos, Color.DarkRed, gameController.HUDFont, scale, 5f, gameController));


                gameController.SubscribeSound(new Sound(gameController.Content, Sound.SoundType.Monster_Growl_1, 3f, gameController.volume));
                sound_played = true;

                max_item_loss = death_item_loss;
                // count player n_items in case its less than death loss
                int player_item_count = 0;
                player_item_count += n_weapons;
                player_item_count += n_armors;
                player_item_count += n_keys;
                player_item_count += n_foods;
                player_item_count += n_potions;
                player_item_count += n_treasures;
                player_item_count += n_shields;

                if (player_item_count < death_item_loss)
                    max_item_loss = player_item_count;

                gameController.SubscribeMessage(new Message("Death Toll: " + max_item_loss + " items", gameController.HUDFont, gameController.hud_scale * 0.4f, 4f, gameController));
            }


            if (die_timer > die_time)
            {
                // applies player item punishment
                Punish(max_item_loss);
                // loads previous level
                gameController.LoadLevel(previous_level, previous_seed, false);
            }
        }

        public void Punish(int max_item_loss)
        {
            int n_items_lost = 0;

            while (n_items_lost < max_item_loss)
            {
                if (n_weapons > 0)
                {
                    n_weapons--;
                    n_items_lost++;
                }
                if (n_armors > 0)
                {
                    n_armors--;
                    n_items_lost++;
                }
                if (n_keys > 0)
                {
                    n_keys--;
                    n_items_lost++;
                }
                if (n_foods > 0)
                {
                    n_foods--;
                    n_items_lost++;
                }
                if (n_potions > 0)
                {
                    n_potions--;
                    n_items_lost++;
                }
                if (n_treasures > 0)
                {
                    n_treasures--;
                    n_items_lost++;
                }
                if (n_shields > 0)
                {
                    n_shields--;
                    n_items_lost++;
                }
            }
        }

        public void Move(Vector2 move, float delta)
        {
            if (!alive)
                return;

            //  vector.zero = no momevent
            if (!move.Equals(Vector2.Zero))
                walking = true;
            else
                walking = false;

            // if not walking, return
            if (!walking)
                return;

            //else update sprite
            Animate();

            if (!CheckCollision(move))
            {
                // updates last position
                lastPosition = Position;
                // and update player position
                Position += move;
                // updates collider
                //collider.UpdateCollider(collider.x + move.X, collider.y + move.Y, collider.width, collider.height);
            }
            else
            {
                int dirx = 0, diry = 0;
                
                if (move.X > 0)
                    dirx = 1;
                else if (move.X < 0)
                    dirx = -1;
                if (move.Y > 0)
                    diry = 1;
                else if (move.Y < 0)
                    diry = -1;

                bool found_direction = false;

                float newMove = moveSpeed * delta;

                // collision in a diagonal direction
                if (dirx != 0 && diry != 0)
                {
                    /*lets try to find a walkable direction between ^v>< */
                    // diagonal "v>" (tries to walk v or >)
                    if (dirx > 0 && diry > 0)
                    {
                        // tries "v" - ignores dir values because they were normalized
                        if (!CheckCollision(newMove * new Vector2(0, 1)))
                        {
                            direction = new Vector2(0, 1);
                            found_direction = true;
                        }
                        // tries ">" - ignores dir values because they were normalized
                        else if (!CheckCollision(newMove * new Vector2(1, 0)))
                        {
                            direction = new Vector2(1, 0);
                            found_direction = true;
                        }
                    }
                    // diagonal "^>" (tries to walk ^ or >)
                    else if (dirx > 0 && diry < 0)
                    {
                        // tries "^" - ignores dir values because they were normalized
                        if (!CheckCollision(newMove * new Vector2(0, -1)))
                        {
                            direction = new Vector2(0, -1);
                            found_direction = true;
                        }
                        // tries ">" - ignores dir values because they were normalized
                        else if (!CheckCollision(newMove * new Vector2(1, 0)))
                        {
                            direction = new Vector2(1, 0);
                            found_direction = true;
                        }
                    }
                    // diagonal "<^" (tries to walk ^ or <)
                    else if (dirx < 0 && diry < 0)
                    {
                        // tries "^" - ignores dir values because they were normalized
                        if (!CheckCollision(newMove * new Vector2(0, -1)))
                        {
                            direction = new Vector2(0, -1);
                            found_direction = true;
                        }
                        // tries "<" - ignores dir values because they were normalized
                        else if (!CheckCollision(newMove * new Vector2(-1, 0)))
                        {
                            direction = new Vector2(-1, 0);
                            found_direction = true;
                        }

                    }
                    // diagonal "<v" (tries to walk < or v)
                    else
                    {
                        // tries "v" - ignores dir values because they were normalized
                        if (!CheckCollision(newMove * new Vector2(0, 1)))
                        {
                            direction = new Vector2(0, 1);
                            found_direction = true;
                        }
                        // tries "<" - ignores dir values because they were normalized
                        else if (!CheckCollision(newMove * new Vector2(-1, 0)))
                        {
                            direction = new Vector2(-1, 0);
                            found_direction = true;
                        }
                    }

                    if (found_direction)
                    {
                        // updates last position
                        lastPosition = Position;
                        // and update player position
                        Vector2 m = newMove * direction * 1 / gameController.wall_friction;
                        Position += m;
                        // updates collider
                       // collider.UpdateCollider(collider.x + m.X, collider.y + m.Y, collider.width, collider.height);
                        // animate
                        Animate();
                    }

                }
                else
                {
                    walking = false;
                }
            }

        }

        private void CheckTrigger()
        {
            var triggers = gameController.GetSubscribedColliders(false);

            for (int i = 0; i < triggers.Count; i++)
            {
                // dont check trigger collision with itself
                if (triggers[i].Equals(trigger))
                    continue;

                if(triggers[i].GetParentClassName() == "Enemy")
                {
                    Enemy enemy = (Enemy)triggers[i].GetParent();
                    enemy.in_trigger = false;
                }

                if (triggers[i].CheckCollision(trigger.x, trigger.y, trigger.width, trigger.height))
                {
                    var other_type = triggers[i].GetParentClassName();
                    var other = triggers[i].GetParent();

                    switch (other_type)
                    {
                        // walked on an item
                        case "Item":
                            Item item = (Item)other;
                            // pick item up from ground
                            item.Pick();
                            // add item to player items
                            PickItem(item);
                            break;
                        // walked on a spike
                        case "Tile":
                            Tile tile = (Tile)other;
                            if (tile.type == Tile.Type.Spike)
                            {
                                // take damage if time between spike damage is achieved
                                if (tile.spike_damage_timer >= tile.time_between_spike_damage)
                                {
                                    tile.spike_damage_timer = 0f;
                                    // sound effect
                                    gameController.SubscribeSound(new Sound(gameController.Content, Sound.SoundType.Hit_Spike, 0.5f, 1f));
                                    // damage position (with offset)
                                    Vector2 damage_pos = new Vector2(Position.X - (camera.zoom * sprite.Width / 3) - 1,
                                                                    Position.Y - (camera.zoom * sprite.Height));
                                    // calculates damage
                                    Damage dmg = new Damage(damage_pos, Color.Red, gameController, gameController.Content);
                                    dmg.CalculateDamage(this, dmg.spike_damage, false);
                                    // subscribe damage to be drawn
                                    gameController.SubscribeDamage(dmg);
                                    // take damage
                                    TakeHit(dmg.value, false);
                                }
                            }
                            break;
                        case "Enemy":
                            Enemy enemy = (Enemy)other;
                            enemy.in_trigger = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void TakeHit(float damage, bool defend)
        {
            if (!defend && alive)
                health -= (int)Math.Round(damage);
        }

        public void TakeHit(string atk_dealer, Object Attacker)
        {
            if (!alive)
                return;

            // hit effect
            Effect effect = new Effect(gameController.Content, Effect.EffectType.Crack_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera, Position.X - camera.x - tilesize * (camera.zoom / 2f) + 6f * camera.zoom,
                                Position.Y - camera.y - tilesize * (camera.zoom / 2f) + 6f * camera.zoom,
                                gameController.effect_scale, gameController.time_between_clicks / 10f);

            // calculate damage
            Damage dmg = new Damage(new Vector2(Position.X - tilesize * camera.zoom /2 + 4f * camera.zoom, 
                                    Position.Y - tilesize * camera.zoom/2 - 7f * camera.zoom), Color.DarkRed, gameController, gameController.Content);

            // if player attacked
            if (atk_dealer == "enemy")
            {
                // calculates damage
                dmg.CalculateDamage((Enemy) Attacker, this);
            }

            health -= dmg.value;

            // add to the damage list to be drawn 
            gameController.SubscribeDamage(dmg);
        }

        // picks a item updating list of player items
        public void PickItem(Item item)
        {
            // check if player is alive
            if (!alive)
                return;

            // item pickup message
            string message = "You obtained one ";

            // add to items list
            items.Add(item);
            // update count of items
            switch(item.type)
            {
                case Item.ItemType.Armor:
                    message += "Armor";
                    n_armors++;
                    break;
                case Item.ItemType.Food:
                    message += "Food";
                    n_foods++;
                    break;
                case Item.ItemType.Key:
                    message += "Key";
                    n_keys++;
                    break;
                case Item.ItemType.Potion:
                    message += "Potion";
                    n_potions++;
                    // fullfill life
                    FullFillHealth();
                    break;
                case Item.ItemType.Shield:
                    message += "Shield";
                    n_shields++;
                    break;
                case Item.ItemType.Treasure:
                    message += "Treasure";
                    n_treasures++;
                    break;
                case Item.ItemType.Weapon:
                    message += "Weapon";
                    n_weapons++;
                    break;
                default:
                    Console.WriteLine("player picking item that does not exist");
                    break;
            }

            gameController.SubscribeMessage(new Message(message, gameController.HUDFont, gameController.hud_scale * 0.4f, 4f, gameController));
        }

        public void FullFillHealth()
        {
            health = 100 + (int)Math.Round((n_potions * 5.5f));
        }

        // pick lootbag
        public void PickLootBag(LootBag loot_bag)
        {
            n_armors += loot_bag.n_armors;
            n_foods += loot_bag.n_foods;
            n_potions += loot_bag.n_potions;
            n_shields += loot_bag.n_shields;
            n_treasures += loot_bag.n_treasures;
            n_weapons += loot_bag.n_weapons;
            n_keys += loot_bag.n_keys;

            // message
            string message = "";

            if(loot_bag.n_armors == 0 && loot_bag.n_foods == 0 && loot_bag.n_potions == 0 && 
                loot_bag.n_shields == 0 && loot_bag.n_weapons == 0 && loot_bag.n_keys == 0 && loot_bag.n_treasures == 0)
                message = "Empty loot bag\r\n";
            else
            {
                // compose message with item quantities
                if(loot_bag.n_weapons > 1)
                    message += "You obtained " + loot_bag.n_weapons + " weapons\r\n";
                else if(loot_bag.n_weapons == 1)
                    message += "You obtained " + loot_bag.n_weapons + " weapon\r\n";
                if (loot_bag.n_armors > 1)
                    message += "You obtained " + loot_bag.n_armors + " armors\r\n";
                else if (loot_bag.n_armors == 1)
                    message += "You obtained " + loot_bag.n_armors + " armor\r\n";
                if (loot_bag.n_shields > 1)
                    message += "You obtained " + loot_bag.n_shields + " shields\r\n";
                else if (loot_bag.n_shields == 1)
                    message += "You obtained " + loot_bag.n_shields + " shield\r\n";
                if (loot_bag.n_keys > 1)
                    message += "You obtained " + loot_bag.n_keys + " keys\r\n";
                else if (loot_bag.n_keys == 1)
                    message += "You obtained " + loot_bag.n_keys + " key\r\n";
                if (loot_bag.n_foods > 1)
                    message += "You obtained " + loot_bag.n_foods + " foods\r\n";
                else if (loot_bag.n_foods == 1)
                    message += "You obtained " + loot_bag.n_foods + " food\r\n";
                if (loot_bag.n_potions > 1)
                    message += "You obtained " + loot_bag.n_potions + " potions\r\n";
                else if (loot_bag.n_potions == 1)
                    message += "You obtained " + loot_bag.n_potions + " potion\r\n";
                if (loot_bag.n_treasures > 1)
                    message += "You obtained " + loot_bag.n_treasures + " treasures\r\n";
                else if (loot_bag.n_treasures == 1)
                    message += "You obtained " + loot_bag.n_treasures + " treasure\r\n";
            }

            // pots fullfill health
            if (loot_bag.n_potions > 0)
                FullFillHealth();

            // removes last "\r\n" that is not necessary
            message = message.Remove(message.Length - 2);

            gameController.SubscribeMessage(new Message(message, gameController.HUDFont, gameController.hud_scale * 0.4f, 4f, gameController));
        }

        // returns player list of items
        internal List<Item> GetItems()
        {
            return items;
        }

        public bool CheckCollision(Vector2 move)
        {
            if (godmode)
                return false;

            colliders = gameController.GetSubscribedColliders(true);

            for (int i = 0; i < colliders.Count; i++)
            {
                // dont check collision with itself
                if (colliders[i].Equals(collider))
                    continue;

                // check and return if player will collide with a solid entity if move is added
                if (colliders[i].CheckCollision(collider.x + move.X, collider.y + move.Y, collider.width, collider.height))
                {
                    return colliders[i].solid;
                }
            }

            return false;
        }


        public void Animate()
        {
            if (!alive)
                return;

            int src_sprite_x, src_sprite_y;

            // src sprite x dictates what animation frame is going to be used
            // depends on what char is being used
            src_sprite_x = tilesize * (((char_id % 4) * 3)) + (tilesize * frame);
            // src sprite x dicates what group of frames is going to be used
            // depends on what direction the player is going
            int dir_spr_idx = direction.Equals(new Vector2(0, 1)) ? 0 : // down
                                direction.Equals(new Vector2(0, -1)) ? 3 : // up
                                direction.Equals(new Vector2(-1, 0)) ? 1 : // left 
                                direction.Equals(new Vector2(1, 0)) ? 2 : // right
                                (direction.X > 0 && direction.Y > 0) ? 2 : // V>
                                (direction.X > 0 && direction.Y < 0) ? 2 : // ^>
                                (direction.X < 0 && direction.Y > 0) ? 1 : // <V
                                (direction.X < 0 && direction.Y < 0) ? 1 : 0; // <^

            
            src_sprite_y = tilesize * dir_spr_idx + 1;
            // shifts down for chars 4,5,6,7
            if (char_id > 3)
                src_sprite_y += (tilesize) * 4 ;

            sprite = new Rectangle(src_sprite_x, src_sprite_y, tilesize, tilesize);

            // updates time_between_frames on account of player speed changes
            time_between_frames = init_time_between_frames / moveSpeed * camera.zoom;

            // alternates frame if time_between_frames is achieved
            if (frame_timer >= time_between_frames)
            {
                frame_timer = 0f;
                frame++;
                frame = frame % n_frames;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // increases animation timer
            frame_timer += deltatime;

            // no momevent - reset to initial frame - return;
            if (!walking)
            {
                frame = initial_frame;
                int src_sprite_x = tilesize * (((char_id % 4) * 3)) + (tilesize * frame);
                sprite = new Rectangle(src_sprite_x, sprite.Y, tilesize, tilesize);
            }

            Color color = Color.White;
            float alpha = (global_light * 2);

            if(!alive)
            {
                color = Color.PaleVioletRed;
                alpha = 1f - (die_timer/die_time) * 2f;
            }

            // draws player
            Vector2 playerPosition = new Vector2(screenX - (Width/2), screenY - (Height/2));
            // updates layer
            gameController.player_layer = 0.1f + (0.0001f * playerPosition.Y) + (0.00005f * playerPosition.X);
            spriteBatch.Draw(spritesheet, playerPosition, sprite, color * alpha, 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, gameController.player_layer);
        }

        public int getPlayer_I()
        {
            int lines = grid.GetUpperBound(0) + 1;
            float tilezoomed = (tilesize * camera.zoom);
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            Double offsetY = -camera.y + startRow * tilezoomed;
            int i = (int)(Math.Floor(((screenY - offsetY) / tilezoomed) + startRow));

            // out of bounds clamp
            if (i < 0)
                i = 0;
            if (i >= lines)
                i = lines - 1;

            return i;
        }

        public int getPlayer_J()
        {
            int lines = grid.GetUpperBound(0) + 1;
            float tilezoomed = (tilesize * camera.zoom);
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            Double offsetX = -camera.x + startCol * tilezoomed;
            int j = (int)(Math.Floor(((screenX - offsetX) / tilezoomed) + startCol));

            // out of bounds clamp
            if (j < 0)
                j = 0;
            if (j >= lines)
                j = lines - 1;

            return j;
        }

        public Vector2 FindNearestFreeTile()
        {
            // spam token on the floor below player
            int p_i = getPlayer_I();
            int p_j = getPlayer_J();
            int lines = grid.GetUpperBound(0) + 1;

            // max area to search. if not found, return (0,0)
            int max_area = lines;

            // if player is on free place return player pos
            if (grid[p_i, p_j] == ' ')
                return new Vector2(p_i, p_j);
            // if there are free places near player
            if (grid[p_i + 1, p_j] == ' ')
                return new Vector2(p_i + 1, p_j);
            if (grid[p_i - 1, p_j] == ' ')
                return new Vector2(p_i - 1, p_j);
            if (grid[p_i, p_j + 1] == ' ')
                return new Vector2(p_i, p_j + 1);
            if (grid[p_i, p_j - 1] == ' ')
                return new Vector2(p_i, p_j - 1);
            if (grid[p_i + 1, p_j + 1] == ' ')
                return new Vector2(p_i + 1, p_j + 1);
            if (grid[p_i + 1, p_j - 1] == ' ')
                return new Vector2(p_i + 1, p_j - 1);
            if (grid[p_i - 1, p_j + 1] == ' ')
                return new Vector2(p_i - 1, p_j + 1);
            if (grid[p_i - 1, p_j - 1] == ' ')
                return new Vector2(p_i - 1, p_j - 1);

            // else run the algorithm
            for (int d = 1; d < max_area; d++)
            {
                for (int i = 1; i < d; i++)
                {
                    int x1 = p_i - i;
                    int y1 = p_j + d - i;

                    // Clamp to avoid out of bounds (not all are necessary to check)
                    if (x1 < 0) x1 = 0;
                    if (y1 < 0) y1 = 0;
                    if (x1 >= lines) x1 = lines - 1;
                    if (y1 >= lines) y1 = lines - 1;

                    // Check point (x1, y1)
                    if (grid[x1, y1] == ' ')
                        return new Vector2(x1, y1);

                    int x2 = p_i + d - i;
                    int y2 = p_j - i;

                    // Clamp to avoid out of bounds (not all are necessary to check)
                    if (x2 < 0) x2 = 0;
                    if (y2 < 0) y2 = 0;
                    if (x2 >= lines) x2 = lines - 1;
                    if (y2 >= lines) y2 = lines - 1;

                    // Check point (x2, y2)
                    if (grid[x2, y2] == ' ')
                        return new Vector2(x2, y2);
                }
                for (int i = 0; i < d + 1; i++)
                {
                    int x1 = p_i - d + i;
                    int y1 = p_j - i;

                    // Clamp to avoid out of bounds (not all are necessary to check)
                    if (x1 < 0) x1 = 0;
                    if (y1 < 0) y1 = 0;
                    if (x1 >= lines) x1 = lines - 1;
                    if (y1 >= lines) y1 = lines - 1;

                    // Check point (x1, y1)
                    if (grid[x1, y1] == ' ')
                        return new Vector2(x1, y1);

                    int x2 = p_i + d - i;
                    int y2 = p_j + i;

                    // Clamp to avoid out of bounds (not all are necessary to check)
                    if (x2 < 0) x2 = 0;
                    if (y2 < 0) y2 = 0;
                    if (x2 >= lines) x2 = lines - 1;
                    if (y2 >= lines) y2 = lines - 1;

                    // Check point (x2, y2)
                    if (grid[x2, y2] == ' ')
                        return new Vector2(x2, y2);
                }
            }

            // not found
            return new Vector2(0, 0);   
        }

        public void DropToken(bool cheat)
        {
            //Vector2 drop_pos = FindNearestFreeTile();
            //int i = (int)drop_pos.X;
            //int j = (int)drop_pos.Y;

            if (cheat)
            {
                Token token = new Token();
                token.Initialize(Token.TokenType.Gold, 46, 20, 0, 0, Position, tilesize, camera, gameController.Content, gameController);
                gameController.tokens.Add(token);
                return;
            }

            if (tokens.Count == 0)
                return;

            // free token
            tokens[0].Drop(Position);
            tokens.RemoveAt(0);
        }

        public void PickToken(Token token)
        {
            tokens.Add(token);

            // token pick message
            string message = "You found a ";

            if (token.token_type == Token.TokenType.Bronze)
                message += "Bronze";
            else if (token.token_type == Token.TokenType.Silver)
                message += "Silver";
            else if (token.token_type == Token.TokenType.Gold)
                message += "Gold";

            message += " Token";

            gameController.SubscribeMessage(new Message(message, gameController.HUDFont, gameController.hud_scale * 0.4f, 4f, gameController));

            if (tokens.Count > 1)
                DropToken(false);
        }
    }

}