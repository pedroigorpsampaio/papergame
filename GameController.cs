using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Gamerator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameController : Game
    {
        GraphicsDeviceManager graphics;
        internal SpriteBatch spriteBatch;
        // Represents the player
        internal Player player;
        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;
        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;
        // player char id
        int player_char_id = 2;
        // represents the map 
        internal Map map;
        /**
         *  cool seeds
         *  136136131
         *  136136133
         *  1
         *  54
         */
        // map config
        public int seed;
        // tileset's tilesize
        int tilesize = 32;
        // max hits to destroy a chest
        public int max_chest_hits = 60;
        // map data structure
        char[,] grid;
        // represents the camera of the game
        Camera camera;
        // fps counter
        FPSCounterComponent fps;
        // sprite font for fps
        private SpriteFont font;
        // wall friction to slow players walking against walls
        public float wall_friction;
        // number of bgms to be played
        private int bgm_size = 3;
        // bgm order
        private int[] bgm_order;
        // current bgm id
        private int bgm_current = 0;
        // current bgm
        private SoundEffect bgm;
        // current bgm instance;
        SoundEffectInstance bgm_instance;
        // sound effects list
        private List<Sound> sounds;
        // audio config
        internal float volume = 1.0f;
        internal float pitch = 0.0f;
        internal float pan = 0.0f;
        // custom mouses
        Point mousePosition;
        private Cursor cursor_arrow_1;
        private Cursor cursor_arrow_2;
        private Cursor cursor_current;
        private Cursor cursor_attack;
        private Cursor cursor_grab_1;
        private Cursor cursor_grab_2;
        private Cursor cursor_pointer_1;
        private Cursor cursor_pointer_2;
        private Cursor cursor_look_1;
        private Cursor cursor_look_2;
        private float mouse_scale = 0.75f;
        private Vector2 cursor_pos;
        // shatter sprites
        private Shatter shatter;
        // effects
        private List<Effect> effects;
        internal float effect_duration = 1f;
        internal float effect_scale = 1f;
        // input control
        private float input_timer = 0f;
        private float time_between_inputs = 10f;
        private float click_timer = 0f;
        internal float time_between_clicks = 1f;
        private float attack_timer = 0f;
        internal float time_between_attacks = 50f;
        private float drop_timer = 0f;
        private float time_between_drops = 10f;
        internal float pick_timer = 0f;
        private float time_between_picks = .5f;
        // global light alpha
        internal float global_light = 1f;
        // individual volumes
        internal float hit_volume = 0.66f;
        internal float pickup_volume = 0.84f;
        internal float destruction_volume = 0.79f;
        // chests list
        internal List<Chest> chests;
        // enemies list
        internal List<Enemy> enemies;
        // list of items in scene
        internal List<Item> items;
        // list of lootbags in scene
        internal List<LootBag> lootbags;
        // list of tokens in scene
        internal List<Token> tokens;
        // list of generic tiles
        internal List<Tile> tiles;
        // totems (end level)
        internal List<Totem> totems;
        // list of game messages to be displayed
        internal List<Message> messages;
        // list of all game colliders (including triggers - non solid colliders)
        private List<Collider> colliders;
        // list of all game triggers (to improve raycast speed)
        private List<Collider> triggers;
        private bool initial_draw;

        // layers
        public float player_layer = 0.1f;
        public float chest_layer = 0.1f;

        // Chest drop rates - simple implementation: tries to generate from most rare to least rare
        internal int chest_food_chance = 20;
        internal int chest_key_chance = 15;
        internal int chest_potion_chance = 13;
        internal int chest_weapon_chance = 11;
        internal int chest_shield_chance = 9;
        internal int chest_armor_chance = 6;
        internal int chest_treasure_chance = 5;
        private Texture2D itemsheet;
        // HUD Sheet
        private Texture2D hudsheet;
        // font for hud text
        public SpriteFont HUDFont;
        // HUD scale
        internal float hud_scale = 1.25f;
        // list of item types for icons
        private List<ItemIcon> item_icons;

        // hud item icons scroll speed
        internal float icon_scroll_speed = 128f;

        // map level info
        public int level;

        // player current target
        public Object player_target;

        // list of damages to be shown
        public List<Damage> damages;

        // random auxiliar (without seed effect)
        public Random random_noseed;

        // baffa object - Hands-on class
        private Baffa baffa;

        // DEBUG
        public bool debugColliders = false;

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        }

        public GameController(int seed, int level, bool fullscreen)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.IsFullScreen = fullscreen;
            this.seed = seed;
            this.level = level;

            // loads game from file (level saved)
            if (gameLoadRequest == false)
            {
                gameLoadRequest = true;
                result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
            }
        }

        #region CollidersManagement
        internal void SubscribeCollider(Collider collider)
        {
            // if its solid subscribe it colliders
            if (collider.solid)
            {
                // susbscribe collider
                if (!colliders.Contains(collider))
                {
                    colliders.Add(collider);
                    return;
                }
            }
            else
            {
                // subscribe trigger
                if (!colliders.Contains(collider))
                {
                    // if its not, add it to triggers
                    // mantaining layer order and then Y order for raycasting
                    int i = triggers.Count;
                    for (i = 0; i < triggers.Count; i++)
                    {
                        // if layer is still bigger, go to next
                        if (triggers[i].layer > collider.layer)
                            continue;
                        // if layer is the same, check Y order
                        else if (triggers[i].layer == collider.layer)
                        {
                            if (triggers[i].y < collider.y)
                                break;
                        }
                        // if layer is smaller, put in front
                        else
                            break;
                    }

                    triggers.Insert(i, collider);
                }
            }

        }


        internal void UnsubscribeCollider(Collider collider)
        {
            // if its solid remove it from colliders
            if (collider.solid)
            {
                colliders.Remove(collider);
            } 
            // else remove it from triggers
            else
            {
                triggers.Remove(collider);
            }
        }

        internal List<Collider> GetSubscribedColliders(bool solid)
        {
            if(solid)
                return colliders;
            else
                return triggers;
        }

        internal bool IsSubscribed(Collider collider)
        {
            if (collider.solid)
                return colliders.Contains(collider);
            else
                return triggers.Contains(collider);
        }

        internal void UpdateSubscription(Collider collider, float x, float y, float width, float height)
        {
            List<Collider> collList;
            if(collider.solid)
                collList = colliders;
            else
                collList = triggers;

            for (int i = 0; i < collList.Count; i++)
            {
                if (collider.Equals(collList[i]))
                {
                    float old_y = collList[i].y;
                    collList[i].UpdateCollider(x, y, width, height);

                    // if there is an update on y pos, update trigger collider position in list
                    if(old_y != y)
                    {
                        Collider trigger = collList[i];
                        collList.RemoveAt(i);
                        SubscribeCollider(trigger);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// cast a point (ray) in the screen and return trigger collider hit
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <returns>
        /// null if does not hit any trigger or
        /// trigger if does hit a trigger
        /// </returns>
        internal Collider RayCast(int x, int y)
        {
            // search for the list of triggers(Y-ordered) for a hit 
            for (int i = 0; i < triggers.Count; i++)
            {
                // the first one to hit is the correct one (Y-ordered list)
                if (triggers[i].CheckCollision(new Point(x,y)))
                    return triggers[i];
            }
            // didnt hit any trigger
            return null;
        }
        #endregion

        #region Subscribers
        internal void SubscribeEffect(Effect effect)
        {
            effects.Add(effect);
        }

        internal void SubscribeSound(Sound sound)
        {
            sounds.Add(sound);
        }

        internal void SubscribeChest(Chest chest)
        {
            chests.Add(chest);
        }

        internal void UnsubscribeChest(Chest chest)
        {
            chests.Remove(chest);
        }

        internal void SubscribeEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }

        internal void UnsubscribeEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        internal void SubscribeItem(Item item)
        {
            items.Add(item);
        }

        internal void UnsubscribeItem(Item item)
        {
            items.Remove(item);
        }

        internal void SubscribeLootBag(LootBag lootbag)
        {
            lootbags.Add(lootbag);
        }

        internal void UnsubscribeLootBag(LootBag lootbag)
        {
            lootbags.Remove(lootbag);
        }

        internal void SubscribeToken(Token token)
        {
            tokens.Add(token);
        }

        internal void UnsubscribeToken(Token token)
        {
            tokens.Remove(token);
        }

        internal void SubscribeMessage(Message message)
        {
            // insert messages always on beginning
            // so older messages goes up in visualization
            messages.Insert(0, message);
        }

        internal void UnsubscribeMessage(Message message)
        {
            messages.Remove(message);
        }

        internal void SubscribeDamage(Damage damage)
        {
            damages.Add(damage);
        }
        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // Initialize the player class
            player = new Player();
            // Set player constant move speed
            // Initialize map class
            map = new Map();
            // Initialize camera class
            camera = new Camera();
            // Initialize shatter class
            shatter = new Shatter(GraphicsDevice);
            // Initialize point mouse position
            mousePosition = new Point();
            // Initialize mouse pos
            cursor_pos = new Vector2();
            // Initialize list of effects
            effects = new List<Effect>();
            // Initialize list of sounds
            sounds = new List<Sound>();
            // Initialize list of generic tiles
            tiles = new List<Tile>();
            // Initialize list of enemies;
            enemies = new List<Enemy>();
            // Initialize list of chests;
            chests = new List<Chest>();
            // Initialize list of items;
            items = new List<Item>();
            // Initialize list of lootbags;
            lootbags = new List<LootBag>();
            // Initialize list of item icons;
            item_icons = new List<ItemIcon>();
            // initialize list of tokens;
            tokens = new List<Token>();
            // intialize totems;
            totems = new List<Totem>();
            // intialize list of game messages;
            messages = new List<Message>();
            // list that will contain damages received by chest
            damages = new List<Damage>();

            // Initialize list of colliders;
            colliders = new List<Collider>();
            // Initialize list of trigger colliders;
            triggers = new List<Collider>();

            // initialize auxiliar random
            random_noseed = new Random();

            // wall friction
            wall_friction = 1.0f;

            // initial state draw
            initial_draw = false;

            // initializes baffa object
           // baffa = new Baffa();
            //baffa.Initialize(464, 100, 0.5f, camera, this);
            // sets player as baffa`s target
            //baffa.SetCurrentTarget(player, player.ToString().Split('.')[1]);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load₢
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load Baffa`s content
            //baffa.LoadContent(Content);

            // Becareful to Avoid screen choke, flickering etc..
            player.moveSpeed = 256 / camera.zoom;

            // initializes map config
            map.Initialize(Content.Load<Texture2D>("tiles"), tilesize, chests, enemies, tiles);
            // generates the map
            grid = map.GenerateMap(level, seed);
            // initializes other configs 
            map.SetInitialConfig(grid, camera, Content, this);
            // inititializes player within start location (if not saved game load)
            Vector2 playerPosition = player.Position;

            if (player.Position == Vector2.Zero)
                playerPosition = new Vector2(map.initialPosition.X - 5f * camera.zoom, map.initialPosition.Y - 5f * camera.zoom);
           
            player.Initialize(Content.Load<Texture2D>("chars"), player_char_id, playerPosition, tilesize, camera, grid, this);
            // initializes camera config
            int line = grid.GetUpperBound(0) + 1;
            int col = grid.GetUpperBound(1) + 1;

            camera.Initialize(playerPosition.X, playerPosition.Y, GraphicsDevice.Viewport.Height, GraphicsDevice.Viewport.Width, col, line, tilesize);

            // adjust speed to the zoom of camera
            player.moveSpeed *= camera.zoom;

            // bgm random order
            bgm_order = Enumerable.Range(1, bgm_size).OrderBy(r => random_noseed.Next()).ToArray();
            // plays first bgm
            bgm = Content.Load<SoundEffect>(bgm_order[0].ToString());
            bgm_instance = bgm.CreateInstance();
            bgm_instance.Pan = pan;
            bgm_instance.Volume = volume;
            bgm_instance.Pitch = pitch;
            bgm_instance.Play();

            // custom mouses
            cursor_arrow_1 = new Cursor(Content, Cursor.CursorType.Arrow_1);
            cursor_attack =  new Cursor(Content, Cursor.CursorType.Sword_1);
            cursor_grab_1 = new Cursor(Content, Cursor.CursorType.Grab_1);
            cursor_grab_2 = new Cursor(Content, Cursor.CursorType.Grab_2);
            cursor_pointer_1 = new Cursor(Content, Cursor.CursorType.Pointer_1);
            cursor_pointer_2 = new Cursor(Content, Cursor.CursorType.Pointer_2);
            cursor_arrow_2 = new Cursor(Content, Cursor.CursorType.Arrow_2);
            cursor_look_1 = new Cursor(Content, Cursor.CursorType.Look_1);
            cursor_look_2 = new Cursor(Content, Cursor.CursorType.Look_2);
            cursor_current = cursor_arrow_1;

            // fps display
            font = Content.Load<SpriteFont>("FPSFont");
            // for HUD items display
            itemsheet = Content.Load<Texture2D>("items");
            // for HUD text display
            HUDFont = Content.Load<SpriteFont>("HUDFont");
            // hud textures
            hudsheet = Content.Load<Texture2D>("hud");
            // load list of item icons;
            loadItemIcons();

            fps = new FPSCounterComponent(this, spriteBatch, font);
            Components.Add(fps);
        }

        private void loadItemIcons()
        {
            // dont worry about * HUD scale 
            Rectangle item_bg_hud = new Rectangle(0, 78, 44, 40);
            ItemIcon icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72, 68), hud_scale, Item.ItemType.Weapon, player, HUDFont, itemsheet,
                            new Rectangle(34 * map.weapon_idx_x, 34 * map.weapon_idx_y, 34, 34));
            item_icons.Add(icon);
            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 44 + 3, 68), hud_scale, Item.ItemType.Armor, player, HUDFont, itemsheet,
                            new Rectangle(34 * (map.armor_idx_x + 3), 34 * map.armor_idx_y, 34, 34));
            item_icons.Add(icon);
            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 88 + 6, 68), hud_scale, Item.ItemType.Shield, player, HUDFont, itemsheet,
                            new Rectangle(34 * (map.shield_idx_x + 12), 34 * map.shield_idx_y, 34, 34));
            item_icons.Add(icon);

            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 132 + 9, 68), hud_scale, Item.ItemType.Key, player, HUDFont, itemsheet,
                            new Rectangle(34 * map.key_idx_x, 34 * map.key_idx_y, 34, 34));
            item_icons.Add(icon);

            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 176 + 12, 68), hud_scale, Item.ItemType.Food, player, HUDFont, itemsheet,
                            new Rectangle(34 * (map.food_idx_x + 1), 34 * (map.food_idx_y + 1), 34, 34));
            item_icons.Add(icon);

            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 220 + 15, 68), hud_scale, Item.ItemType.Potion, player, HUDFont, itemsheet,
                            new Rectangle(34 * map.potion_idx_x, 34 * map.potion_idx_y, 34, 34));
            item_icons.Add(icon);

            icon = new ItemIcon();
            icon.Initialize(Content, spriteBatch, new Vector2(72 + 264 + 18, 68), hud_scale, Item.ItemType.Treasure, player, HUDFont, itemsheet,
                            new Rectangle(34 * map.treasure_idx_x, 34 * map.treasure_idx_y, 34, 34));
            item_icons.Add(icon);

            // add triggers to items hud bg buttons
            Collider left_scroll = new Collider(54 * hud_scale, 76 * hud_scale, 13 * hud_scale, 23 * hud_scale, 
                                    false, new Button(Button.Type.LeftScroll, Button.Group.ItemScroll), GraphicsDevice, 1f);
            SubscribeCollider(left_scroll);
            Collider right_scroll = new Collider(214 * hud_scale, 76 * hud_scale, 13 * hud_scale, 23 * hud_scale, 
                                     false, new Button(Button.Type.RightScroll,  Button.Group.ItemScroll), GraphicsDevice, 1f);
            SubscribeCollider(right_scroll);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (spriteBatch != null)
                spriteBatch.Dispose();

            // unload`s baffa content
            //baffa.UnloadContent(Content);
            
            base.Dispose();
        }

        public void UnloadRemains()
        {
            if (bgm_instance != null)
                bgm_instance.Dispose();

            if (spriteBatch != null)
                spriteBatch.Dispose();
            
            Components.Remove(fps);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // increase locks timer
            input_timer++;
            click_timer++;
            attack_timer++;
            drop_timer++;

            // updates baffa object
           // baffa.Update(gameTime);

            //Update keyboard 
            UpdateKeyboard(gameTime);
            //Update the player
            UpdatePlayer(gameTime);
            //Updates the camera
            UpdateCamera(gameTime);
            //Updates bgm
            UpdateBGM(gameTime);
            //Updates mouse
            UpdateMouse(gameTime);
            // Updates tiles
            UpdateTiles(gameTime);
            // Updates enemies
            UpdateEnemies(gameTime);
            // Updates chests
            UpdateChests(gameTime);
            // Updates items
            UpdateItems(gameTime);
            // Updates lootbags
            UpdateLootBags(gameTime);
            // Updates tokens
            UpdateTokens(gameTime);
            // Updates totems
            UpdateTotems(gameTime);
            // Updates messages
            UpdateMessages(gameTime);
            // Updates damages
            UpdateDamages(gameTime);
            // Update game save/load
            UpdateGameSave();
            

            //graphics.ApplyChanges();
            base.Update(gameTime);
        }

        private void UpdateMessages(GameTime gameTime)
        {
            for (int i = 0; i < messages.Count; i++)
                messages[i].Update(gameTime);
        }

        private void UpdateTotems(GameTime gameTime)
        {
            for (int i = 0; i < totems.Count; i++)
                totems[i].Update(gameTime);
        }

        private void UpdateGameSave()
        {
            // If a save is pending, save as soon as the
            // storage device is chosen
            if ((gameSaveRequest) && (result.IsCompleted))
            {
                StorageDevice device = StorageDevice.EndShowSelector(result);
                if (device != null && device.IsConnected)
                {
                    SaveGame(device);

                }
                // Reset the request flag
                gameSaveRequest = false;
            }
            // if game load is pending, load as soon as
            // storage device is chosen
            if ((gameLoadRequest) && (result.IsCompleted))
            {
                StorageDevice device = StorageDevice.EndShowSelector(result);
                if (device != null && device.IsConnected)
                {
                    save = LoadGame(device);
                    // there is a saved game
                    if(save != null)
                        LoadLevel(save.level, save.seed, true);
                }
                // Reset the request flag
                gameLoadRequest = false;
            }
        }

        private void UpdateTokens(GameTime gameTime)
        {
            for (int i = 0; i < tokens.Count; i++)
                tokens[i].Update(gameTime);
        }

        private void UpdateDamages(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // if there is damage to be displayed update animation
            for (int i = 0; i < damages.Count; i++)
            {
                damages[i].position.Y -= damages[i].damage_animation_speed * delta * camera.zoom;
                damages[i].shift += damages[i].damage_animation_speed * delta * camera.zoom;
                if (damages[i].shift >= damages[i].damage_animation_threshold)
                    damages.RemoveAt(i);
            }
        }

        private void UpdateItems(GameTime gameTime)
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Update(gameTime);
        }

        private void UpdateLootBags(GameTime gameTime)
        {
            for (int i = 0; i < lootbags.Count; i++)
                lootbags[i].Update(gameTime);
        }

        private void UpdateChests(GameTime gameTime)
        {
            for (int i = 0; i < chests.Count; i++)
                chests[i].Update(gameTime);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].Update(gameTime);
        }

        private void UpdateTiles(GameTime gameTime)
        {
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].Update(gameTime);
        }

        private void UpdateKeyboard(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // light cheat +
            if (currentKeyboardState.IsKeyDown(Keys.LeftControl) && currentKeyboardState.IsKeyDown(Keys.Up))
            {
                global_light += 0.01f;
                if (global_light >= 1f)
                    global_light = 1f;
            }
            // light cheat -
            if (currentKeyboardState.IsKeyDown(Keys.LeftControl) && currentKeyboardState.IsKeyDown(Keys.Down))
            {
                global_light -= 0.01f;
                if (global_light <= 0.4f)
                    global_light = 0.4f;
            }

            // save game
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && currentKeyboardState.IsKeyDown(Keys.S) && input_timer > time_between_inputs)
            {
                input_timer = 0f;
                if (gameSaveRequest == false)
                {
                    gameSaveRequest = true;
                    result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                }
            }

            // load game
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && currentKeyboardState.IsKeyDown(Keys.E) && input_timer > time_between_inputs)
            {
                input_timer = 0f;
                if (gameLoadRequest == false)
                {
                    gameLoadRequest = true;
                    result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                }
            }

            // load next level
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && currentKeyboardState.IsKeyDown(Keys.L) && input_timer > time_between_inputs)
            {
                LoadLevel(++level, ++seed, false);
            }

            // load first level
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && currentKeyboardState.IsKeyDown(Keys.F12) && input_timer > time_between_inputs)
            {
                LoadLevel(1, 1, false);
            }

            // health cheat +
            if (currentKeyboardState.IsKeyDown(Keys.H))
            {
                player.max_health += 1000;
            }

            // treasure cheat +
            if (currentKeyboardState.IsKeyDown(Keys.T))
            {
                player.n_treasures += 1;
            }

            // food cheat +
            if (currentKeyboardState.IsKeyDown(Keys.F) && input_timer > time_between_inputs)
            {
                input_timer = 0f;
                player.n_foods += 10;
                for (int i = 0; i < triggers.Count; i++)
                    Console.WriteLine(triggers[i].layer);
            }

            // potion cheat +
            if (currentKeyboardState.IsKeyDown(Keys.P))
            {
                player.n_potions += 10;
            }

            // weapon cheat +
            if (currentKeyboardState.IsKeyDown(Keys.R))
            {
                player.n_weapons += 10;
            }

            // health cheat +
            if (currentKeyboardState.IsKeyDown(Keys.X))
            {
                for(int i = 0; i < item_icons.Count; i++)
                    item_icons[i].ScrollLeft(icon_scroll_speed * delta);
            }

            // health cheat +
            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                for (int i = 0; i < item_icons.Count; i++)
                    item_icons[i].ScrollRight(icon_scroll_speed * delta);
            }

            // atk cheat +
            if (currentKeyboardState.IsKeyDown(Keys.N))
            {
                player.attack += 5f;
            }

            // def cheat +
            if (currentKeyboardState.IsKeyDown(Keys.M))
            {
                player.defense += 5f;
            }

            // atk speed cheat +
            if (currentKeyboardState.IsKeyDown(Keys.L))
            {
                player.attack_speed += 0.1f;
            }

            // key cheat 
            if (currentKeyboardState.IsKeyDown(Keys.Q))
            {
                if (player.n_keys < 1000)
                    player.n_keys = 10000;
                else
                    player.n_keys = 0;
            }

            // drop token (cheat spawn gold)
            if (currentKeyboardState.IsKeyDown(Keys.C) && currentKeyboardState.IsKeyDown(Keys.LeftControl) && drop_timer >= time_between_drops)
            {
                player.DropToken(true);
                drop_timer = 0f;
            }

            // drop token
            if (currentKeyboardState.IsKeyDown(Keys.C) && drop_timer >= time_between_drops)
            {
                player.DropToken(false);
                drop_timer = 0f;
            }

            // debug collider
            if (currentKeyboardState.IsKeyDown(Keys.F1) && currentKeyboardState.IsKeyDown(Keys.LeftShift) && input_timer >= time_between_drops)
            {
                input_timer = 0f;
                if (debugColliders)
                    debugColliders = false;
                else
                    debugColliders = true;
            }

            // move all enemies for collision test (not normalized diagonals)
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (i % 2 == 0)
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(1, 0));
                    else
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(-1, 0));
                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (i % 2 == 0)
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(-1, 0));
                    else
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(1, 0));
                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if(i%2 == 0)
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(0, -1));
                    else
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(0, 1));

                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (i % 2 == 0)
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(0, 1));
                    else
                        enemies[i].Move(player.moveSpeed * delta * new Vector2(0, -1));
                }
            }
            else
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].walking = false;
                }
            }
        }

        // update mouse related
        private void UpdateMouse(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var mouseState = Mouse.GetState();
            mousePosition.X = mouseState.X;
            mousePosition.Y = mouseState.Y;

            // updates mouse position for drawing
            cursor_pos.X = mouseState.X;
            cursor_pos.Y = mouseState.Y;

            int lines = grid.GetUpperBound(0) + 1;
            float tilezoomed = (tilesize * camera.zoom);
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            Double offsetX = -camera.x + startCol * tilezoomed -5;
            Double offsetY = -camera.y + startRow * tilezoomed -10;

            int i = (int)(Math.Floor(((mousePosition.Y - offsetY) / tilezoomed) + startRow));
            int j = (int)(Math.Floor(((mousePosition.X - offsetX) / tilezoomed) + startCol));

            // out of bounds clamp
            if (i < 0)
                i = 0;
            if (j >= lines)
                j = lines - 1;
            if (i >= lines)
                i = lines - 1;
            if (j < 0)
                j = 0;

            // default cursor
            cursor_current = cursor_arrow_1;

            // mouse offset - adjust raycast point precision
            int x_off = 4;
            int y_off = 4;
            int ray_x = (int)Math.Round(mousePosition.X + x_off * camera.zoom);
            int ray_y = (int)Math.Round(mousePosition.Y + y_off * camera.zoom);

            // cast ray to see if mouse is on any trigger
            Collider hit = RayCast(ray_x, ray_y);

            // something was hit
            if (hit != null)
            {
                string hit_object_type = hit.GetParentClassName();
                Object hit_object = hit.GetParent();

                // change cursor on hover
                if (hit_object_type == "Enemy" || hit_object_type == "Chest")
                    cursor_current = cursor_arrow_2;
                else if (hit_object_type == "Token" || hit_object_type == "LootBag")
                    cursor_current = cursor_grab_2;
                else if (hit_object_type == "Button")
                    cursor_current = cursor_pointer_1;
                else if (hit_object_type == "Totem")
                    cursor_current = cursor_look_2;
                // change cursor if player can reach entity
                if (checkReach(cursor_pos.X, cursor_pos.Y))
                {
                    if (hit_object_type == "Enemy" || hit_object_type == "Chest")
                        cursor_current = cursor_attack;
                    else if (hit_object_type == "Token" || hit_object_type == "LootBag")
                        cursor_current = cursor_grab_1;
                    else if (hit_object_type == "Totem")
                        cursor_current = cursor_look_1;
                }

                // if button null go back to normal arrow
                if(hit_object_type == "Button")
                {
                    Button b = (Button)hit_object;
                    if (b.type == Button.Type.Null)
                        cursor_current = cursor_arrow_1;
                }

                // Left mouse button pressed on an trigger collider
                if (mouseState.LeftButton == ButtonState.Pressed && click_timer >= time_between_clicks)
                {
                    click_timer = 0f;

                    // if player isnt alive return
                    if (!player.alive)
                        return;

                    switch (hit_object_type)
                    {
                        // clicked on a chest
                        case "Chest":
                            // change current target
                            player_target = hit_object;

                            // check if player reaches entity
                            if (!checkReach(cursor_pos.X, cursor_pos.Y))
                                return;

                            // attack speed
                            if (attack_timer >= time_between_attacks / player.attack_speed)
                            {
                                attack_timer = 0f;

                                Chest chest = (Chest)hit_object;
                                // destroy chest with one hit
                                if (player.n_keys > 0)
                                {
                                    chest.UseKey(new Point(ray_x, ray_y));
                                    // decrease player n_keys
                                    player.n_keys--;
                                }
                                // decrease chest hitpoints
                                else
                                    chest.TakeHit(new Point(ray_x, ray_y), "player", player);
                            }
                            break;
                        case "Enemy":
                            // change current target
                            player_target = hit_object;

                            // check if player reaches entity
                            if (!checkReach(cursor_pos.X, cursor_pos.Y))
                                return;

                            if (attack_timer >= time_between_attacks / player.attack_speed)
                            {
                                attack_timer = 0f;

                                Enemy enemy = (Enemy)hit_object;
                                enemy.TakeHit(new Point(ray_x, ray_y), "player", player);
                            }
                            break;
                        case "Button":
                            Button button = (Button)hit_object;

                            // if null button, dont do nothing
                            if (button.type == Button.Type.Null)
                                return;

                            // change cursor to pointer that represents clicked button
                            cursor_current = cursor_pointer_2;

                            // if button is from totem HUD, let totem deal with input
                            if(button.group == Button.Group.TotemHUD)
                            {
                                totems[0].HandleInput(button);
                                return;
                            }

                            if(button.type == Button.Type.LeftScroll)
                            {
                                for (int k = 0; k < item_icons.Count; k++)
                                    item_icons[k].ScrollRight(icon_scroll_speed * delta);
                            }
                            else if(button.type == Button.Type.RightScroll)
                            {
                                for (int k = 0; k < item_icons.Count; k++)
                                    item_icons[k].ScrollLeft(icon_scroll_speed * delta);
                            }
                            break;
                        case "Token":
                            // check if player reaches entity
                            if (!checkReach(cursor_pos.X, cursor_pos.Y))
                                return;

                            if (drop_timer >= time_between_drops)
                            {
                                drop_timer = 0f;
                                Token token = (Token)hit_object;
                                token.Pick();
                                player.PickToken(token);
                            }
                            break;
                        case "LootBag":
                            // check if player reaches entity
                            if (!checkReach(cursor_pos.X, cursor_pos.Y))
                                return;

                            if (pick_timer >= time_between_picks)
                            {
                                pick_timer = 0f;
                                LootBag lootbag = (LootBag)hit_object;
                                lootbag.Pick();
                                player.PickLootBag(lootbag);
                            }
                            break;
                        case "Totem":
                            // check if player reaches entity
                            if (!checkReach(cursor_pos.X, cursor_pos.Y))
                                return;

                            if (drop_timer >= time_between_drops)
                            {
                                drop_timer = 0f;
                                Totem totem = (Totem)hit_object;
                                if (!totem.drawHUD)
                                {
                                    totem.drawHUD = true;
                                    totem.SubscribeButtons();
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private bool checkReach(float entity_X, float entity_Y)
        {
            float tilezoomed = (tilesize * camera.zoom);
            float entity_x_center = entity_X + tilezoomed / 2;
            float entity_y_center = entity_Y + tilezoomed / 2;
            float player_x_center = player.screenX + tilezoomed / 2;
            float player_y_center = player.screenY + tilezoomed / 2;
            float distance = Vector2.Distance(new Vector2(player_x_center, player_y_center),
                                                new Vector2(entity_x_center, entity_y_center));

            if (distance < tilezoomed * 1.2f)
                return true;
            else
                return false;
        }

        private void UpdateBGM(GameTime gameTime)
        {
            // check if song has ended, plays the next one
            if (bgm_instance.State == SoundState.Stopped)
            {
                bgm_current = bgm_current % bgm_size;
                bgm = Content.Load<SoundEffect>(bgm_order[bgm_current].ToString());
                bgm_instance.Dispose();
                bgm_instance = bgm.CreateInstance();
                bgm_instance.Pan = pan;
                bgm_instance.Volume = volume;
                bgm_instance.Pitch = pitch;
                bgm_instance.Play();
            }
        }

        /// <summary>
        /// Updates related to the player
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdatePlayer(GameTime gameTime)
        {
            // updates colliders
            player.Update(gameTime);

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            int dirx = 0;
            int diry = 0;

            // speed cheat
            if (currentKeyboardState.IsKeyDown(Keys.Up) && currentKeyboardState.IsKeyDown(Keys.LeftShift) && input_timer >= time_between_inputs)
            {
                if (player.moveSpeed == 512)
                    player.moveSpeed = 128;
                else
                    player.moveSpeed = 512;
                input_timer = 0f;
            }

            // god mode cheat
            if (currentKeyboardState.IsKeyDown(Keys.G) && currentKeyboardState.IsKeyDown(Keys.LeftShift) && input_timer >= time_between_inputs)
            {
                if (player.godmode)
                    player.godmode = false;
                else
                    player.godmode = true;
                input_timer = 0f;
            }

            // char change cheat
            if (currentKeyboardState.IsKeyDown(Keys.D1) || currentKeyboardState.IsKeyDown(Keys.NumPad1) && input_timer >= time_between_inputs)
            {
                player.char_id = 0;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D2) || currentKeyboardState.IsKeyDown(Keys.NumPad2) && input_timer >= time_between_inputs)
            {
                player.char_id = 1;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D3) || currentKeyboardState.IsKeyDown(Keys.NumPad3) && input_timer >= time_between_inputs)
            {
                player.char_id = 2;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D4) || currentKeyboardState.IsKeyDown(Keys.NumPad4) && input_timer >= time_between_inputs)
            {
                player.char_id = 3;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D5) || currentKeyboardState.IsKeyDown(Keys.NumPad5) && input_timer >= time_between_inputs)
            { 
                player.char_id = 4;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D6) || currentKeyboardState.IsKeyDown(Keys.NumPad6) && input_timer >= time_between_inputs)
            {
                player.char_id = 5;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D7) || currentKeyboardState.IsKeyDown(Keys.NumPad7) && input_timer >= time_between_inputs)
            {
                player.char_id = 6;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D8) || currentKeyboardState.IsKeyDown(Keys.NumPad8) && input_timer >= time_between_inputs)
            {
                player.char_id = 7;
                // guarantee refresh
                player.Animate();
                input_timer = 0f;
            }

            Vector2 dir;
            // Use the Keyboard / Dpad
            // left
            if (currentKeyboardState.IsKeyDown(Keys.A) || currentGamePadState.DPad.Left == ButtonState.Pressed && input_timer >= time_between_inputs)
            {
                dirx = -1;
                input_timer = 0f;
            }
            // right
            if (currentKeyboardState.IsKeyDown(Keys.D) || currentGamePadState.DPad.Right == ButtonState.Pressed && input_timer >= time_between_inputs)
            {
                dirx = 1;
                input_timer = 0f;
            }
            // down
            if (currentKeyboardState.IsKeyDown(Keys.S) || currentGamePadState.DPad.Down == ButtonState.Pressed && input_timer >= time_between_inputs)
            {
                diry = 1;
                input_timer = 0f;
            }
            // up
            if (currentKeyboardState.IsKeyDown(Keys.W) || currentGamePadState.DPad.Up == ButtonState.Pressed && input_timer >= time_between_inputs)
            {
                diry = -1;
                input_timer = 0f;
            }

            dir = new Vector2(dirx, diry);
            if (dirx != 0 && diry != 0)
                 dir.Normalize();

            // move player
            // there is no new momevent, mantain old direction
            if(dirx != 0 || diry != 0)
                player.direction = dir;
            player.Move(player.moveSpeed * delta * dir, delta);

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, camera.max_x - player.Width + camera.width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, camera.max_y - player.Height + camera.height);
        }

        private int get_I_InGrid(float y)
        {
            float tilezoomed = (tilesize * camera.zoom);
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            Double offsetY = -camera.y + startRow * tilezoomed;
            return (int)(Math.Floor(((y - offsetY) / tilezoomed) + startRow));
        }

        private int get_J_InGrid(float x)
        {
            float tilezoomed = (tilesize * camera.zoom);
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            Double offsetX = -camera.x + startCol * tilezoomed;
            return (int)(Math.Floor(((x - offsetX) / tilezoomed) + startCol));
        }

        /// <summary>
        /// Updates related to the camera
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateCamera(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // assume followed sprite should be placed at the center of the screen
            // whenever possible
            player.screenX = camera.width / 2 - (tilesize * camera.zoom / 2) + 16;
            player.screenY = camera.height / 2 - (tilesize * camera.zoom / 2);

            // old values
            float old_cam_x = camera.x;
            float old_cam_y = camera.y;

            // make the camera follow the sprite
            camera.x = player.Position.X - camera.width / 2;
            camera.y = player.Position.Y - camera.height / 2;

            // clamp camera to avoid getting out of bounds
            camera.x = MathHelper.Clamp(camera.x, 0, camera.max_x);
            camera.y = MathHelper.Clamp(camera.y, 0, camera.max_y);

            // left and right sides
            if (player.Position.X < camera.width / 2 ||
                player.Position.X > camera.max_x + camera.width / 2)
            {
                player.screenX = (float)Math.Round(player.Position.X - camera.x - 8);
            }
            // top and bottom sides
             if (player.Position.Y < camera.height / 2 ||
                player.Position.Y > camera.max_y + camera.height / 2)
            {
                player.screenY = (float)Math.Round(player.Position.Y - camera.y - 24);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Start drawing
            spriteBatch.Begin(SpriteSortMode.FrontToBack);

            // draws the map initial state
            if (!initial_draw)
            {
                map.InitialDraw(spriteBatch, grid, camera, global_light, this);
                initial_draw = true;
            }
   
            // Draws the Player
            player.Draw(spriteBatch, camera, delta, global_light);

            // draw effects
            DrawEffects(delta);   

            // play sound effects alive
            PlaySounds(delta);

            // draws HUD
            DrawHUD();

            // draw generic tiles
            for(int i = 0; i < tiles.Count; i++)
                tiles[i].Draw(spriteBatch, delta, global_light);

            // draw chests
            for (int i = 0; i < chests.Count; i++)
                chests[i].Draw(spriteBatch, camera, delta, global_light);

            // draw items
            for (int i = 0; i < items.Count; i++)
                items[i].Draw(spriteBatch, camera, delta, global_light);

            // draw lootbags
            for (int i = 0; i < lootbags.Count; i++)
                lootbags[i].Draw(spriteBatch, camera, delta, global_light);

            // draw tokens
            for (int i = 0; i < tokens.Count; i++)
                tokens[i].Draw(spriteBatch, camera, delta, global_light);

            // draw totems
            for (int i = 0; i < totems.Count; i++)
                totems[i].Draw(spriteBatch, camera, delta, global_light);

            // draw enemies
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].Draw(spriteBatch, camera, delta, global_light);

            // draw messages
            DrawMessages();

            // draw damages
            for (int i = 0; i < damages.Count; i++)
                damages[i].Draw(spriteBatch, camera, delta);

            // draw baffa object
            //baffa.Draw(spriteBatch);

            // draw game info messages;

            // debug: draw colliders (high consumption - debug only)
            if(debugColliders)
                DrawColliders();

            // Stop drawing
            spriteBatch.End();

            //Set up the spritebatch to draw using scissoring (for item icons cropping)
            RasterizerState _rasterizerState = new RasterizerState() { ScissorTestEnable = true };

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                              null, null, _rasterizerState);

            //Copy the current scissor rect so we can restore it after
            Rectangle currentRect = spriteBatch.GraphicsDevice.ScissorRectangle;

            // tile icons scissor rectangle
            Rectangle scissorRectangle = new Rectangle((int)Math.Round(72 * hud_scale), (int)Math.Round(66 * hud_scale),
                                                        (int)Math.Round(138 * hud_scale), (int)Math.Round(44 * hud_scale)); 

            //Set the current scissor rectangle
            spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;

            // draw item icons in scissor area
            for (int i = 0; i < item_icons.Count; i++)
                item_icons[i].Draw();

            //Reset scissor rectangle to the saved value
            spriteBatch.GraphicsDevice.ScissorRectangle = currentRect;

            //End the spritebatch
            spriteBatch.End();

            // on top of everything (mouse)
            spriteBatch.Begin();

            // draws custom mouse
            spriteBatch.Draw(cursor_current.texture, cursor_pos, cursor_current.frame,
                                Color.White, 0f, Vector2.Zero, camera.zoom * mouse_scale, SpriteEffects.None, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMessages()
        {
            // messages to be displayed
            // sends index of relative position messages
            // to be displayed on to of another (ignoring
            // fixed position ones)
            int index = 0; float y_offset = 0f;
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].Draw(spriteBatch, y_offset);
                // update index if not fixed position
                // with the amount of lines by each message
                if (!messages[i].FixedPosition)
                    y_offset = -(messages[i].GetMessageHeight());
            }
        }

        private void DrawEffects(float delta)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                Effect effect = effects[i];
                // removes finished effect from list
                if (!effect.active)
                {
                    effects.Remove(effect);
                }

                effect.Draw(this, delta, player, global_light);
            }

        }

        private void DrawHUD()
        {
            // draw HUD
            Rectangle health_bar = new Rectangle(0, 0 , 106, 12);
            Rectangle main_hud = new Rectangle(0, 13, 224, 64);
            Rectangle token_bg_hud = new Rectangle(0, 78, 44,40);
            Rectangle item_bg_hud = new Rectangle(45, 78, 173, 44);
            
            // main hud and health bar
            float health_scale = player.health / (float)player.max_health;
            spriteBatch.Draw(hudsheet, new Vector2(0, 0), main_hud, Color.White, 0f, Vector2.Zero, 1f * hud_scale, SpriteEffects.None, 0.951f);
            spriteBatch.Draw(hudsheet, new Rectangle((int)Math.Round(85 * hud_scale), (int)Math.Round(6 * hud_scale), (int)Math.Round(106 * health_scale * hud_scale), 
                                                    (int)Math.Round(12 * hud_scale)), health_bar, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.952f);
            // draw item Hud
            // token bg
            spriteBatch.Draw(hudsheet, new Vector2(9 * hud_scale, 68 * hud_scale), token_bg_hud, Color.White, 0f, Vector2.Zero, 1f * hud_scale, SpriteEffects.None, 0.951f);
            // items bg
            spriteBatch.Draw(hudsheet, new Vector2(54 * hud_scale, 66 * hud_scale), item_bg_hud, Color.White, 0f, Vector2.Zero, 1f * hud_scale, SpriteEffects.None, 0.951f);

            // draw health / attack / defense info
            float health_offset_x = (player.max_health.ToString().Length - 3) * hud_scale * 0.27f  
                        + (player.health.ToString().Length - 3) * hud_scale * 0.27f;

            spriteBatch.DrawString(HUDFont, "HP: " + player.health + " / " + player.max_health, new Vector2((int)Math.Round(85 * hud_scale + 
                                   (85*hud_scale/2f - HUDFont.MeasureString("HP: " + player.health + " / " + player.max_health).X * hud_scale * 0.27f / 2 + 12*hud_scale) - 
                                    health_offset_x/2f), (int)Math.Round(6 * hud_scale)), Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            float attack_offset_x = (player.attack.ToString().Length - 2) * hud_scale * 0.27f
            + (player.attack.ToString().Length - 2) * hud_scale * 0.27f;

            spriteBatch.DrawString(HUDFont, "ATK: " + player.attack, new Vector2((int)Math.Round(85 * hud_scale +
                                   (85 * hud_scale / 2f - HUDFont.MeasureString("ATK: " + player.attack).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                                    attack_offset_x / 2f), (int)Math.Round(26.2f * hud_scale)), Color.DarkGreen, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);


            float defense_offset_x = (player.defense.ToString().Length - 3) * hud_scale * 0.27f  
            + (player.defense.ToString().Length - 3) * hud_scale * 0.27f;

            spriteBatch.DrawString(HUDFont, "DEF: " + player.defense, new Vector2((int)Math.Round(85 * hud_scale +
                                   (85 * hud_scale / 2f - HUDFont.MeasureString("DEF: " + player.defense).X * hud_scale * 0.27f / 2 + 11 * hud_scale) -
                                    defense_offset_x / 2f), (int)Math.Round(46.4f * hud_scale)), Color.Black, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            // draws player character on HUD
            Rectangle player_sprite = new Rectangle(tilesize * (((player.char_id % 4) * 3) + 1), tilesize * (((player.char_id / 4) * (3 + 1))), tilesize, tilesize);
            spriteBatch.Draw(player.spritesheet, new Rectangle((int)Math.Round(15 * hud_scale), (int)Math.Round(13 * hud_scale), (int)Math.Round(player.sprite.Width * hud_scale), 
                                                                (int)Math.Round(player.sprite.Height * hud_scale)), player_sprite, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.99991f);

            // write world level on HUD
            float offset_level_x = 32f * hud_scale - (HUDFont.MeasureString(level.ToString()).X * 0.24f * hud_scale) / 2f;
            float offset_level_y = 45f * hud_scale;

            spriteBatch.DrawString(HUDFont, level.ToString(), new Vector2((int)Math.Round(offset_level_x), (int)Math.Round(offset_level_y)), Color.White, 0f, Vector2.Zero, 0.24f * hud_scale, SpriteEffects.None, 0.99992f);
            spriteBatch.DrawString(HUDFont, level.ToString(), new Vector2((int)Math.Round(offset_level_x) + 1, (int)Math.Round(offset_level_y) + 1), Color.Black, 0f, Vector2.Zero, 0.24f * hud_scale, SpriteEffects.None, 0.99991f);

            // draw token icons on HUD
            if (player.tokens.Count > 0)
            {
                int ind_x = 0, ind_y = 0;

                if (player.tokens[0].token_type == Token.TokenType.Bronze)
                {
                    ind_x = 37; ind_y = 20;
                }
                else if (player.tokens[0].token_type == Token.TokenType.Silver)
                {
                    ind_x = 43; ind_y = 20;
                }
                else if (player.tokens[0].token_type == Token.TokenType.Gold)
                {
                    ind_x = 46; ind_y = 20;
                }

                Rectangle source = new Rectangle(tilesize * ind_x, tilesize * ind_y, tilesize, tilesize);
                spriteBatch.Draw(map.tileset, new Vector2(15 * hud_scale, 71f * hud_scale), source, Color.White, 0f, Vector2.Zero, 1f * hud_scale, SpriteEffects.None, 0.9982f);
            }
         
        }

        private void PlaySounds(float delta)
        {
            // play sound effects
            for (int i = 0; i < sounds.Count; i++)
            {
                Sound sound = sounds[i];

                // fade out for sounds
                sound.sound_timer += delta;
                float minValue = 0.2f;
                float vol = sound.volume - (sound.sound_timer * sound.volume_factor);
                if (vol < minValue)
                    vol = minValue;
                sound.getInstance.Volume = vol;

                // removes finished effect from list
                
                if (sound.sound_timer >= sound.duration || sound.volume <= minValue)
                {
                    sound.Stop();
                    sounds.Remove(sound);
                }

                if (sound.getInstance.State != SoundState.Playing)
                    sound.Play();
            }
        }

        // loads level (level + seed)
        public void LoadLevel(int level, int seed, bool from_file)
        {
            // saves player before reset in case of new level from end level
            Player saved_player = player;

            // sets world seed and level
            this.seed = seed;
            this.level = level;

            // unload remains (bgm..)
            UnloadRemains();

            // resets player, camera, lists and other things;
            Initialize();
   
            if(from_file)
            {
                player.n_armors = save.n_armors;
                player.n_weapons = save.n_weapons;
                player.n_treasures = save.n_treasures;
                player.n_shields = save.n_shields;
                player.n_potions = save.n_potions;
                player.n_keys = save.n_keys;
                player.n_foods = save.n_foods;
                player.Position.X = save.player_pos_x;
                player.Position.Y = save.player_pos_y;
            }
            else
            {
                player.n_armors = saved_player.n_armors;
                player.n_weapons = saved_player.n_weapons;
                player.n_treasures = saved_player.n_treasures;
                player.n_shields = saved_player.n_shields;
                player.n_potions = saved_player.n_potions;
                player.n_keys = saved_player.n_keys;
                player.n_foods = saved_player.n_foods;
            }

            // Updates player health
            player.health =  100 + (int)Math.Round((player.n_potions * 5.5f));

            // displays message of game loaded
            string msg_text = "Game Loaded";
            SubscribeMessage(new Message(msg_text, HUDFont, hud_scale * 0.5f, 3f, this));

            // saves game (level save politics)
            if (gameSaveRequest == false)
            {
                gameSaveRequest = true;
                result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
            }
        }

        #region saveGameManagement
        StorageDevice device;
        IAsyncResult result;
        bool gameSaveRequest = false;
        bool gameLoadRequest = false;
        SaveGameData save;

        public void SaveGame(StorageDevice device)
        {
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer("StorageDemo", null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";

            // Check to see whether the save exists.
            if (container.FileExists(filename))
                // Delete it so that we can create one fresh.
                container.DeleteFile(filename);

            // Create the file.
            Stream stream = container.CreateFile(filename);

            // creates the data to be stored
            SaveGameData data = new SaveGameData();
            data.level = level; data.seed = seed; data.n_armors = player.n_armors;
            data.n_foods = player.n_foods; data.n_keys = player.n_keys;
            data.n_potions = player.n_potions; data.n_shields = player.n_shields;
            data.n_treasures = player.n_treasures; data.n_weapons = player.n_weapons;
            data.player_pos_x = player.Position.X; data.player_pos_y = player.Position.Y;

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));

            // Call Serialize, and then pass the Stream and the data to serialize.
            serializer.Serialize(stream, data);

            // Close the file.
            stream.Close();

            // Dispose the container, to commit changes.
            container.Dispose();

            // displays message informing that map was saved
            //string msg_text = "Game Saved";
            //SubscribeMessage(new Message(msg_text, HUDFont, hud_scale * 0.5f, 3f, this));
        }

        public SaveGameData LoadGame(StorageDevice device)
        {
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer("StorageDemo", null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";

            // Check to see whether the save exists.
            if (!container.FileExists(filename))
            {
                // If not, dispose of the container and return.
                container.Dispose();
                return null;
            }

            // Open the file.
            Stream stream = container.OpenFile(filename, FileMode.Open);

            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));

            SaveGameData data = (SaveGameData)serializer.Deserialize(stream);

            // Close the file.
            stream.Close();

            // Dispose the container.
            container.Dispose();

            return data;
        }
        #endregion


        // draw colliders for debug (high consumption - debug only)
        public void DrawColliders()
        {
            for(int i = 0; i < colliders.Count; i++)
            {
                if (!colliders[i].active)
                    continue;

                int width = (int)colliders[i].width;
                int height = (int)colliders[i].height;

                 spriteBatch.Draw(colliders[i].visual_box, new Rectangle((int)Math.Round(colliders[i].x), (int)Math.Round(colliders[i].y), width, height), 
                               null, Color.White * 0.6f, 0f, Vector2.Zero, SpriteEffects.None, 0.99999999f);

            }
            for (int i = 0; i < triggers.Count; i++)
            {
                if (!triggers[i].active)
                    continue;

                int width = (int)Math.Round(triggers[i].width);
                int height = (int)Math.Round(triggers[i].height);

                spriteBatch.Draw(triggers[i].visual_box, new Rectangle((int)Math.Round(triggers[i].x), (int)Math.Round(triggers[i].y), width, height),
                                null, Color.Purple * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, 0.999999991f);
            }
        }
    }
}
