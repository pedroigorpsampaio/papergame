using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gamerator.AI;

namespace Gamerator
{
    public class Enemy
    {
        // Position of the enemy relative to the upper left side of the screen
        public Vector2 Position;
        // stores last position to help in collisions
        public Vector2 lastPosition;
        // reference to camera
        private Camera camera;
        // enemy rotation
        public float angle = 0;
        // enemy origin for rotationg
        Vector2 origin;
        // State of the enemy
        public bool active;
        // Amount of hit points that enemy has
        public int health;
        // enemy's max hp
        public int max_health;
        // spritesheet
        public Texture2D spritesheet;
        // spritesheet id
        public int spritesheet_id;
        // char rectangle sprite in spritesheet
        public Rectangle sprite;
        // tilesize - same as spritesize
        public int tilesize;
        // enemy movement direction
        public Vector2 direction;
        // initial animation frame;
        public int initial_frame = 1;
        // animation frame;
        public int frame = 1;
        // spritesheet number of frames
        public int n_frames = 3;
        // number of animation cycles;
        public int n_cycles = 1;
        // initial time between frames (without enemy speed)
        public float init_time_between_frames = 20f;
        // time between frames;
        public float time_between_frames;
        // frame time counter
        public float frame_timer = 0f;
        // enemy movement speed
        public float moveSpeed;
        // bool that represents if a enemy is walking
        public bool walking = false;

        // key that opens all chests with one click
        public bool hasKey = false;
        // enemy's list of tokens
        public List<Token> tokens;
        // a reference to the grid structure of the level
        public char[,] grid;
        // enemy ID
        public int id;
        // enemy spritesheet local id
        public int local_id;
        // enemy Type
        public Type type;
        public enum Type { Dragon, Blob, Succubus, Plant, Insect, Animal, Mutant, Aquatic };
        // enemy collider
        public Collider collider;
        // reference to gameController
        public GameController gameController;
        // reference to the list of colliders
        public List<Collider> colliders;
        // coll x coord offset // dont bother with camera zoom
        private float coll_offset_x = 4;
        // coll y coord offset // dont bother with camera zoom
        private float coll_offset_y = 17;
        // coll width scale
        private float coll_width_scale = 0.75f;
        // coll height scale
        private float coll_height_scale = 0.4f;
        private string name;
        private bool target;
        // trigger collider
        private Collider trigger;
        // trigger x coord offset // dont bother with camera zoom
        private float trigger_offset_x = 1;
        // trigger y coord offset // dont bother with camera zoom
        private float trigger_offset_y = 10;
        // trigger width scale
        private float trigger_width_scale = 0.956f;
        // trigger height scale
        private float trigger_height_scale = 0.86f;

        // for hud display
        // hud sheet
        public Texture2D hudsheet;
        // rectangle of hud sprite
        public Rectangle hud;
        // health bar
        public Rectangle health_bar;
        // target arrow
        public Rectangle target_arrow;
        // for arrow animation
        public float arrow_animation;

        // enemy defense
        public float defense;
        public float defense_const = 1f;
        // enemy attack
        public float attack;
        public float attack_const = 1.2f;
        // health const
        public float health_const = 2.5f;
        // enemy loot bag
        public LootBag loot_bag;

        // gets width of enemy
        public int Width
        {
            get { return sprite.Width; }
        }
        // Get the height of the enemy
        public int Height
        {
            get { return sprite.Height; }
        }

        public void Initialize(int id, string name, float[] personality, int health, Vector2 position, int tilesize, Camera camera, char[,] grid, ContentManager content, GameController gameController)
        {
            this.id = id;
            this.tilesize = tilesize;
            this.camera = camera;
            this.grid = grid;
            this.health = health;
            this.gameController = gameController;
            this.name = name;
            colliders = gameController.GetSubscribedColliders(true);
            collision = false;

            position.Y -= 10f * camera.zoom;

            // Initialize AI
            InitializeAI(new Personality(personality));

            spritesheet_id = (int)Math.Floor(id / 8f) + 1;

            spritesheet = content.Load<Texture2D>("Enemies_"+spritesheet_id);

            local_id = id - (8 * (spritesheet_id-1));

            // enemy attack
            attack = gameController.level  * attack_const + (id + 1)/5f * gameController.level;
            // enemy defense
            defense = gameController.level * defense_const + (id + 1) / 5f * gameController.level;

            // gets enemy type
            if (id == 0 || id == 4)
                type = Type.Animal;
            else if (id == 1)
                type = Type.Mutant;
            else if (id == 2 || id == 6 || id == 7)
                type = Type.Insect;
            else if (id == 3 || id == 5)
                type = Type.Aquatic;
            else if (id == 8 || id == 12)
                type = Type.Plant;
            else if (id == 9 || id == 13)
                type = Type.Dragon;
            else if (id == 10 || id == 14)
                type = Type.Blob;
            else if (id == 11 || id == 15)
                type = Type.Succubus;
            else
                type = Type.Blob;

            moveSpeed = 140.67f;

            // one size collider for all types of enemies (as of now)
            float tilezoomed = camera.zoom * tilesize;
            collider = new Collider(position.X + coll_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + coll_offset_y * camera.zoom, 
                                     tilezoomed * coll_width_scale, tilezoomed * coll_height_scale, true, this, gameController.GraphicsDevice);
            // subscribe collider
            gameController.SubscribeCollider(collider);

            // enemy trigger collider
            trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + trigger_offset_y * camera.zoom,
                         tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            // subscribe trigger collider (goes to trigger list instead of collider list)
            gameController.SubscribeCollider(trigger);

            // gets the sprite frame from spritesheet
            sprite = new Rectangle((tilesize * (((local_id % 4) * 3) + 1)), tilesize * 0, tilesize, tilesize); 

            // Set the starting position of the enemy around the middle of the screen and to the back
            Position = position;
            lastPosition = position;

            // Set the enemy to be active
            active = true;

            // Set the enemy health
            this.health = (int)Math.Round(health * health_const * (gameController.level * 1.66));
            max_health = this.health;

            // Set current and previous token as none
            tokens = new List<Token>();

            // initial direction (sprite looking down)
            direction = new Vector2(0, 1);

            // sets origin
            origin = new Vector2(0, 0);

            // initialize lootbag
            loot_bag = new LootBag();
            loot_bag.Initialize(position, tilesize, camera, content, gameController);

            target = false;

            // hud
            hudsheet = content.Load<Texture2D>("hud");
            hud = new Rectangle(0, 123, 121, 26);
            health_bar = new Rectangle(0, 0, 106, 12);
            target_arrow = new Rectangle(107, 0, 9, 9);
            arrow_animation = 0f;

            // makes sure that sprite is correct
            Animate();
        }

        float test_timer = 0f;
        float time_between_tests = 0.5f;
        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // check if enemy is alive still
            if (health <= 0)
                Die();

            // check if enemy is the players target
            Object player_target = gameController.player_target;
            if (player_target != null)
            {
                string target_class = player_target.ToString().Split('.')[1];
                if (target_class == "Enemy")
                {
                    if (this.Equals((Enemy)player_target))
                        target = true;
                    else
                        target = false;
                }
                else
                    target = false;
            }

            // if target updates arrow animation
            if (target)
            {
                if (arrow_animation <= 5f * camera.zoom)
                    arrow_animation += 6f * delta * camera.zoom;
                else
                    arrow_animation = 0f;
            }

            /*** camera framing ****/
            float target_y = Position.Y - camera.y;
            float target_x = Position.X - camera.x;

            float tilezoomed = tilesize * camera.zoom;

            collider.x = target_x + coll_offset_x * camera.zoom;
            collider.y = target_y + coll_offset_y * camera.zoom;

            trigger.x = target_x + trigger_offset_x * camera.zoom;
            trigger.y = target_y + trigger_offset_y * camera.zoom;

            // check if enemy is visible on camera
            active = camera.IsOnCamera(tilesize, target_x, target_y);

            // update subscriber colliders (if enemy is active)
            if (active)
            {
                // increases animation timer
                frame_timer += delta;

                gameController.UpdateSubscription(collider, collider.x, collider.y, tilezoomed * coll_width_scale, tilezoomed * coll_height_scale);
                gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
            }

            UpdateAI(delta);
        }

        public void TakeHit(Point hit, string atk_dealer, Object Attacker)
        {
            // hit effect
            Effect effect = new Effect(gameController.Content, Effect.EffectType.Crack_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera, hit.X - (camera.zoom * 11f), hit.Y - (camera.zoom * 4f),
                                gameController.effect_scale, gameController.time_between_clicks / 10f);

            // calculate damage
            Damage dmg = new Damage(new Vector2(Position.X + 3f * camera.zoom, Position.Y), Color.OrangeRed, gameController, gameController.Content);

            // if player attacked
            if (atk_dealer == "player")
            {
                // calculates damage
                dmg.CalculateDamage(gameController.player, this);
                // dispatch player attack event for AI
                AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerAttack);
            }

            health -= dmg.value;

            if (health < 0)
                health = 0;

            // add to the damage list to be drawn 
            gameController.SubscribeDamage(dmg);
        }

        public void Die()
        {
            // destruction effect
            Effect effect = new Effect(gameController.Content, Effect.EffectType.Smoke_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera,
                                Position.X - camera.x - coll_offset_y / 10 / camera.zoom,
                                Position.Y - camera.y + coll_offset_y / 3 / camera.zoom,
                                gameController.effect_scale * 1.75f, gameController.effect_duration * 0.5f);

            // remove me from list of chests
            gameController.UnsubscribeEnemy(this);
            // remove my colliders from list of colliders;
            gameController.UnsubscribeCollider(collider);
            gameController.UnsubscribeCollider(trigger);

            // drop loot bag (loot will be generated in lootbag.drop)
            // enemy dying, so it isnt a reward (2x drops)
            loot_bag.Drop(false, Position);
        }

        private void DrawHUD(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // hud scale
            float hud_scale = gameController.hud_scale;

            // hud bg
            spriteBatch.Draw(hudsheet, new Vector2((camera.width / 2) - (hud.Width / 2 * hud_scale), 20 * hud_scale), hud,
                            Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, 0.92f);
            // healthbar
            float health_scale = health / (float)max_health;
            float offset_x = 7f * hud_scale;
            float offset_y = 6f * hud_scale;
            spriteBatch.Draw(hudsheet, new Rectangle((int)Math.Round((camera.width / 2) - (hud.Width / 2 * hud_scale) + offset_x),
                                                    (int)Math.Round(20 * hud_scale + offset_y), (int)Math.Round(108 * health_scale * hud_scale),
                                                    (int)Math.Round(14 * hud_scale)), health_bar, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.93f);
            // target arrow
            float target_y = (float)Math.Round(Position.Y) - camera.y;
            float target_x = (float)Math.Round(Position.X) - camera.x;
            float t_arrow_off_x = (sprite.Width * camera.zoom / 2) - (target_arrow.Width * camera.zoom / 2);
            float t_arrow_off_y = -(arrow_animation * hud_scale);
            spriteBatch.Draw(hudsheet, new Vector2(target_x + t_arrow_off_x, target_y + t_arrow_off_y), target_arrow,
                                                    Color.White, 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, 0.891f);

            // draw health info
            float health_offset_x = (max_health.ToString().Length - 3) * hud_scale * 0.27f
            + (health.ToString().Length - 3) * hud_scale * 0.27f;
            spriteBatch.DrawString(gameController.HUDFont, "HP: " + health + " / " + max_health, new Vector2((int)Math.Round(266 * hud_scale +
                                   (266 * hud_scale / 2f - gameController.HUDFont.MeasureString("HP: " + health + " / " + max_health).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                                    health_offset_x / 2f), (int)Math.Round(30 * hud_scale)), Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            // draw chest name
            float name_offset_x = (name.ToString().Length - 3) * hud_scale * 0.27f;
            spriteBatch.DrawString(gameController.HUDFont, name, new Vector2((int)Math.Round(266 * hud_scale +
                                   (266 * hud_scale / 2f - gameController.HUDFont.MeasureString(name).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                                    health_offset_x / 2f), (int)Math.Round(23 * hud_scale)),
                                    Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            // draw emotion/mental state tag in HUD
            string emotion_tag = GetEmotionTag();
            string mental_state_tag = GetMentalStateTag();

            spriteBatch.DrawString(gameController.HUDFont, mental_state_tag, new Vector2((int)Math.Round(266 * hud_scale +
                       (266 * hud_scale / 2f - gameController.HUDFont.MeasureString(emotion_tag).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                        health_offset_x / 2f), (int)Math.Round(43 * hud_scale)),
                        Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            // debug for personality
            string debug_personality = "Personality | O:" + personality.Openness + " C:" + personality.Conscientiousness + " E:" + personality.Extraversion +
                                " A:" + personality.Agreeableness + " N:" + personality.Neuroticism;

            spriteBatch.DrawString(gameController.HUDFont, debug_personality, new Vector2((int)Math.Round(266 * hud_scale +
                   (266 * hud_scale / 2f - gameController.HUDFont.MeasureString(emotion_tag).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                    health_offset_x / 2f), (int)Math.Round(54 * hud_scale)),
                    Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);
        }

        public void Move(Vector2 move)
        {
            // only moves enemies if its active
            if (active)
            {
                //  vector.zero = no momevent
                if (!move.Equals(Vector2.Zero))
                    walking = true;
                else
                    walking = false;

                direction.X = 0;
                direction.Y = 0;

                if (move.X > 0)
                    direction.X = 1f;
                if (move.Y > 0)
                    direction.Y = 1f;
                if (move.X < 0)
                    direction.X = -1f;
                if (move.Y < 0)
                    direction.Y = -1f;

                // if not walking, return
                if (!walking)
                    return;

                //else update sprite
                Animate();

                // check collision
                bool solid = CheckCollision(move);
                if (solid)
                    return;

                // updates last position
                lastPosition = Position;

                // and update enemy position
                Position += move;

                // updates collider
                collider.UpdateCollider(collider.x + move.X, collider.y + move.Y, collider.width, collider.height);
            }
        }

        public bool CheckCollision(Vector2 move)
        {
            colliders = gameController.GetSubscribedColliders(true);

            for (int i = 0; i < colliders.Count; i++)
            {
                // dont check collision with itself
                if (colliders[i].Equals(collider))
                    continue;

                if (colliders[i].CheckCollision(collider.x + move.X, collider.y + move.Y, collider.width, collider.height))
                {
                   // Console.WriteLine(colliders[i].box + " : " + colliders[i].GetParentClass());
                    if(colliders[i].solid)
                    {
                        collision = true;
                    }
                    else
                    {
                        collision = false;
                    }
                    return colliders[i].solid;
                }
            }

            return false;
        }

        public void Animate()
        {
            int src_sprite_x, src_sprite_y;

            // src sprite x dictates what animation frame is going to be used
            // depends on what char is being used
            src_sprite_x = tilesize * (((local_id % 4) * 3)) + (tilesize * frame);
            // src sprite x dicates what group of frames is going to be used
            // depends on what direction the enemy is going
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
            if (local_id > 3)
                src_sprite_y += (tilesize) * 4;

            sprite = new Rectangle(src_sprite_x, src_sprite_y, tilesize, tilesize);

            // updates time_between_frames on account of enemy speed changes
            time_between_frames = init_time_between_frames / moveSpeed * camera.zoom;

            //Console.WriteLine(frame + " / " + frame_timer + " / " + time_between_frames);
            // alternates frame if time_between_frames is achieved
            if (frame_timer >= time_between_frames)
            {
                frame_timer = 0f;
                frame++;
                frame = frame % n_frames;
            }
        }
        private Vector2 getPointInScreen(int i, int j)
        {
            Vector2 destination;
            float tilezoomed = camera.zoom * tilesize;
            int startCol = (int)(Math.Floor(camera.x / tilezoomed));
            int startRow = (int)(Math.Floor(camera.y / tilezoomed));
            Double offsetX = -camera.x + startCol * tilezoomed;
            Double offsetY = -camera.y + startRow * tilezoomed;

            var target_x = (j - (Math.Floor(camera.x / tilezoomed))) * tilezoomed + offsetX;
            var target_y = (i - (Math.Floor(camera.y / tilezoomed))) * tilezoomed + offsetY;

            destination = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));

            return destination;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // if enemy is active (enemies within camera), draw it
            if (active)
            {
                // show health bar if attacked
                if (target)
                    DrawHUD(spriteBatch, camera, deltatime, global_light);

                // no momevent - reset to initial frame - return;
                if (current_state != AI.StateMachine.ProcessState.Patrolling && current_state != AI.StateMachine.ProcessState.Attacking && !walking)
                {
                    frame = initial_frame;
                    int src_sprite_x = tilesize * (((local_id % 4) * 3)) + (tilesize * frame);
                    sprite = new Rectangle(src_sprite_x, sprite.Y, tilesize, tilesize);
                }

                /*** camera framing ****/
                float target_y = (float)Math.Round(Position.Y) - camera.y;
                float target_x = (float)Math.Round(Position.X) - camera.x;
                
                // draws enemy
                Vector2 enemyPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                spriteBatch.Draw(spritesheet, enemyPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, 0.1f + (0.0001f * target_y) + (0.00005f * target_x));
            }
        }

        #region AI
        /// AI

        private AI.Brain brain;
        private Emotion emotion;
        private Personality personality;
        private MentalState mental_state;
        private Plan plan;
        private Event evt;
        private bool in_sight;
        public bool in_reach;
        public bool in_trigger;
        public bool in_follow_range;
        public float follow_range;
        private float patrol_timer;
        private float time_between_patrol;
        private float attack_timer;
        private float time_between_attacks;
        private AI.StateMachine.ProcessState current_state;
        private bool collision;

        // later to be changed with personality, current emotion, etc..
        public void InitializeAI(Personality personality)
        {
            // personality generated when enemy is created
            this.personality = personality;
            // event aux 
            evt = new Event();
            // emotion
            emotion = new Emotion();
            emotion.Initialize(evt);
            // mental state
            mental_state = new MentalState();
            mental_state.Initialize(this);
            // plan
            plan = new Plan();
            plan.Initialize();

            in_sight = false;
            in_reach = false;
            in_trigger = false;
            in_follow_range = false;
            follow_range = 2.75f * tilesize * camera.zoom;
            patrol_timer = 0f;
            attack_timer = 0f;
            time_between_patrol = 3f;
            time_between_attacks = 1f;
            brain = new AI.Brain(this);
            brain.Initialize();
        }
        
        public AI.Brain GetBrain()
        {
            return brain;
        }

        public Emotion GetEmotion()
        {
            return emotion;
        }

        public MentalState GetMentalState()
        {
            return mental_state;
        }

        public Personality GetPersonality()
        {
            return personality;
        }

        public Plan GetPlan()
        {
            return plan;
        }

        internal StateMachine.ProcessState GetCurrentState()
        {
            return current_state;
        }


        /// <summary>
        /// Returns the current mental state tag of an enemy
        /// </summary>
        /// <returns></returns>
        public string GetMentalStateTag()
        {
            return GetMentalState().CurrentMentalState.ToString();
        }

        /// <summary>
        /// Returns the current emotion tag of an enemy
        /// </summary>
        /// <returns></returns>
        public string GetEmotionTag()
        {
            return EventsController.Convert_EmotionToTag(EventsController.FindMostInfluentEmotion((GetEmotion().currentEmotion)));
        }

        public bool PlayerInTrigger()
        {
            return in_trigger;
        }

        public bool PlayerInFollowRange()
        {
            Vector2 player_position = gameController.player.Position;
            Vector2 centered_player_position = new Vector2(player_position.X - tilesize * (camera.zoom / 2f) + 6f * camera.zoom,
                                                           player_position.Y - tilesize * (camera.zoom / 2f) + 6f * camera.zoom);

            if (Vector2.Distance(centered_player_position, Position) < follow_range)
                return true;
            else
                return false;
        }

        public void UpdateAI(float delta)
        {
            // update events
            if (active)
            {
                // update in sight
                if (!in_sight)
                {
                    in_sight = true;
                    // dispatch player in sight event for AI
                    AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerInSight);
                }
                // update in reach
                if (!in_reach)
                {
                    if (PlayerInTrigger())
                    {
                        in_reach = true;
                        // dispatch player within reach event for ai
                        AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerWithinReach);
                    }
                }
                else
                {
                    if (!PlayerInTrigger())
                    {
                        in_reach = false;
                        AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerOutOfReach);
                    }
                }
                if(!in_follow_range)
                {
                    if(PlayerInFollowRange())
                    {
                        in_follow_range = true;
                        // dispatch player within follow range event for ai
                        AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerWithinFollowRange);
                    }
                }
                else
                {
                    if (!PlayerInFollowRange())
                    {
                        in_follow_range = false;
                        // dispatch player within follow range event for ai
                        AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerOutOfFollowRange);
                    }
                }

                // if low health dispatch low health event
                if(health < max_health * 0.15f)
                    AI.EventsController.DispatchEvent(this, AI.EventsController.Type.lowHealth);

                // update timers
                patrol_timer += delta;
                attack_timer += delta;

                // update MentalState and Emotion
                mental_state.Update();
                emotion.Update(delta);

                // peforms next action
                Act(brain.GetState(), delta);
            }
            else
            {
                if (in_sight)
                {
                    in_sight = false;
                    AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerOutOfSight);
                }      
            }
        }

        private void Act(AI.StateMachine.ProcessState state, float delta)
        {
            current_state = state;

            switch (state)
            {
                case AI.StateMachine.ProcessState.Patrolling:
                    Patrol();
                    break;
                case AI.StateMachine.ProcessState.Attacking:
                    Attack();
                    break;
                case AI.StateMachine.ProcessState.Following:
                    Follow(delta);
                    break;
                case AI.StateMachine.ProcessState.Running:
                    Run(delta);
                    break;
                case AI.StateMachine.ProcessState.Terminated:
                    Reward();
                    break;
                default:
                    Patrol();
                    break;
            }
        }

        private void Patrol()
        {

            if (patrol_timer >= time_between_patrol)
            {
                patrol_timer = 0f;
                
                int n = gameController.random_noseed.Next(0,100);
                if (n < 25)
                    direction = new Vector2(0, 1);
                else if (n < 50)
                    direction = new Vector2(0, -1);
                else if (n < 75)
                    direction = new Vector2(1, 0);
                else
                    direction = new Vector2(-1, 0);   
            }

            Animate();
        }

        private void Attack()
        {
            if (in_reach)
            {
                // face player
                FacePlayer();
                // guarantee animation
                Animate();

                if (attack_timer >= time_between_attacks)
                {
                    attack_timer = 0f;
                    gameController.player.TakeHit("enemy", this);
                }
            }
            else
                AI.EventsController.DispatchEvent(this, AI.EventsController.Type.playerOutOfReach);
        }

        private void Reward()
        {
            
        }

        Vector2 move_avoid;
        bool avoid = false;
        float avoid_desloc = 0f;

        private void Follow(float delta)
        {
            // move on players direction
            Vector2 player_position = gameController.player.Position;
            Vector2 centered_player_position = new Vector2(player_position.X - tilesize * (camera.zoom / 2f) + 6f * camera.zoom,
                                                           player_position.Y - tilesize * (camera.zoom / 2f) + 6f * camera.zoom);
            Vector2 move_dir = (centered_player_position - Position);
            Vector2 distance = move_dir;
            move_dir.Normalize();
            Vector2 move = moveSpeed * delta * move_dir;

            if (!avoid)
            {

                if (CheckCollision(move))
                {
                    Console.WriteLine("collision 1");
                    move_avoid = AvoidCollision(move, centered_player_position, delta);
                    avoid = true;
                    return;
                }

                Move(move);
            }
            else
            {
                if (CheckCollision(move) && avoid_desloc < tilesize*camera.zoom)
                {
                    Console.WriteLine("collision 2");
                    Move(move_avoid);
                    avoid_desloc += moveSpeed * delta;
                }
                else
                {
                    avoid = false;
                    avoid_desloc = 0f;
                }
            }
        }

        private void Run(float delta)
        {
            Vector2 player_position = gameController.player.Position;
            Vector2 centered_player_position = new Vector2(player_position.X - tilesize * (camera.zoom / 2f) + 6f * camera.zoom,
                                               player_position.Y - tilesize * (camera.zoom / 2f) + 6f * camera.zoom);

            float force_value = moveSpeed * delta;

            var move_left = new Vector2(-1, 0) * force_value;
            var move_right = new Vector2(1, 0) * force_value;
            var move_up = new Vector2(0, -1) * force_value;
            var move_down = new Vector2(0, 1) * force_value;

            // closest move vars
            var farther_dist = 0f;
            var farther_move = new Vector2(0, 0);

            if (!CheckCollision(move_left))
            {
                if (Vector2.Distance(centered_player_position, Position + move_left) > farther_dist)
                {
                    farther_dist = Vector2.Distance(centered_player_position, Position + move_left);
                    farther_move = move_left;
                }
            }
            if (!CheckCollision(move_right))
            {
                if (Vector2.Distance(centered_player_position, Position + move_right) > farther_dist)
                {
                    farther_dist = Vector2.Distance(centered_player_position, Position + move_right);
                    farther_move = move_right;
                }
            }
            if (!CheckCollision(move_up))
            {
                if (Vector2.Distance(centered_player_position, Position + move_up) > farther_dist)
                {
                    farther_dist = Vector2.Distance(centered_player_position, Position + move_up);
                    farther_move = move_up;
                }
            }
            if (!CheckCollision(move_down))
            {
                if (Vector2.Distance(centered_player_position, Position + move_down) > farther_dist)
                {
                    farther_dist = Vector2.Distance(centered_player_position, Position + move_down);
                    farther_move = move_down;
                }
            }

            Move(farther_move);
        }

        public void FacePlayer()
        {
            Player player = gameController.player;

            Vector2 dir = player.Position - Position;
            dir.Normalize();
            direction = dir;
        }

        public Vector2 AvoidCollision(Vector2 move, Vector2 centered_player_position, float delta)
        {
            Vector2 force_axis = Vector2.Transform(move, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));
            Vector2 force_1 = Vector2.Normalize(force_axis);
            force_1.X = (int)((move.X) * Math.Cos(MathHelper.ToRadians(90)) - move.Y * Math.Sin(MathHelper.ToRadians(90)));
            force_1.Y = (int)((move.X) * Math.Sin(MathHelper.ToRadians(90)) + move.Y * Math.Cos(MathHelper.ToRadians(90)));
            force_1 = Vector2.Normalize(force_1);
            Vector2 force_2 = Vector2.Negate(force_1) ;
            Vector2 newdir_1 = Vector2.Normalize(move * force_1);
            Vector2 newdir_2 = Vector2.Normalize(move * force_2);
            float force_value = moveSpeed * delta;
            Vector2 result_1 = newdir_1 * force_value;
            Vector2 result_2 = newdir_2 * force_value;

            var move_left = new Vector2(-1, 0) * force_value;
            var move_right = new Vector2(1, 0) * force_value;
            var move_up = new Vector2(0, -1) * force_value;
            var move_down = new Vector2(0, 1) * force_value;

            //Console.WriteLine(move + " / " + force_1);

            // closest move vars
            var closest_dist = 1000000f;
            var closest_move = new Vector2(0, 0);

            if (!CheckCollision(move_left))
            {
                if (Vector2.Distance(centered_player_position, Position + move_left * 10f) < closest_dist)
                {
                    closest_dist = Vector2.Distance(centered_player_position, Position + move_left * 10f);
                    closest_move = move_left;
                }
            }
            if (!CheckCollision(move_right))
            {
                if(Vector2.Distance(centered_player_position, Position + move_right * 10f) < closest_dist)
                {
                    closest_dist = Vector2.Distance(centered_player_position, Position + move_right * 12f);
                    closest_move = move_right;
                }
            }
            if (!CheckCollision(move_up))
            {
                if (Vector2.Distance(centered_player_position, Position + move_up * 10f) < closest_dist)
                {
                    closest_dist = Vector2.Distance(centered_player_position, Position + move_up * 10f);
                    closest_move = move_up;
                }
            }
            if (!CheckCollision(move_down))
            {
                if (Vector2.Distance(centered_player_position, Position + move_down * 10f) < closest_dist)
                {
                    closest_dist = Vector2.Distance(centered_player_position, Position + move_down * 10f);
                    closest_move = move_down;
                }
            }

            return closest_move;
        }
        #endregion
    }
}
