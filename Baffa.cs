using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

/// <summary>
/// FOR CLASS:
/// 1 - Show Baffa Sprite
/// 2 - Make It Follow Camera
/// 3 - Animate Sprite
/// 4 - Make It Follow Player
/// 5 - Add Trigger Collider
/// 6 - Make it deal damage to player on trigger enter
/// </summary>

/*within Gamerator namespace*/
namespace Gamerator
{
    /// <summary>
    /// Class created for the hands-on class
    /// showing some of the motor features
    /// </summary>
    class Baffa
    {
        // the image that contains the sprites (spritesheet)
        Texture2D baffasheet;
        // the rectangular cut that will select the sprite from the spritesheet
        Rectangle sprite;
        // the position (x,y - 2D World) for this object in the game world
        Vector2 position;
        // the camera position (x,y - Screen) for this object in the screen
        Vector2 cameraPosition;
        // a reference to the game`s camera - with that, we`ll be able to relate
        // our object with the game`s camera (camera position, camera zoom...)
        Camera camera;
        // a reference to the game`s main module
        GameController gameController;
        // baffa sprite scale (can be a Vector2 also, for (x,y) scaling)
        float scale;
        // size of each sprite
        float spritesize;
        // bool that represents if this object is active (on camera)
        bool active;
        // baffa`s current target
        Object target;
        // baffa`s current target type
        string target_type;
        // baffa`s movement speed
        float movementSpeed;
        // baffa`s movement direction;
        Vector2 direction;
        // baffa`s attack
        float attack;
        // baffa`s reach distance
        float reach_distance;
        // baffa`s attack speed vars
        float attack_speed;
        float time_between_attacks;
        float attack_timer;

        /* baffa`s trigger collider */
        private Collider trigger;
        // trigger collider positioning and scaling
        private float trigger_offset_x = 0f;
        private float trigger_offset_y = 0f;
        private float trigger_width_scale = 1f;
        private float trigger_height_scale = 1f;

        /* animation variables */
        // timer that will control frame transitions
        float frame_timer;
        // time between each frame transition
        float time_between_frames;
        // current animation frame
        float frame;
        // total number of animation frames
        float n_frames;

        /// <summary>
        /// Game Loop: In general, initializes common C# variables and configurations
        /// Important: Will be called only once by GameController`s GameLoop Initialize, 
        ///            at the beginning of the program`s(game) execution.
        /// </summary>
        /// <param name="x">Position X of the object in the game`s world</param>
        /// <param name="y">Position Y of the object in the game`s world</param>
        /// <param name="camera">The reference to the game`s camera</param>
        /// <param name="gameController">The reference to the game`s main module</param>
        public void Initialize(float x, float y, float scale, Camera camera, GameController gameController)
        {
            // initializes position Vector2 with x and y params
            position = new Vector2(x, y);
            // initialize sprite scale with scale parameter
            this.scale = scale;
            // initializes camera position using auxiliar function
            // camera.TransformToCameraPosition that converts
            // world position to camera position
            cameraPosition = camera.TransformToCameraPosition(position);
            // stores the reference to the game`s camera in the respective variable
            this.camera = camera;
            // stores the reference to the game`s main module 
            this.gameController = gameController;
            // initializes our sprite size var (sizeXsize - equal size for width and height)
            spritesize = 128;

            // initializes baffa`s movement speed
            movementSpeed = 140.67f;
            // initializes baffa`s attack
            attack = 5f;
            // initializes baffa`s reach distance
            reach_distance = 10f;
            // initializes baffa`s attack speed vars
            attack_speed = 5f;
            attack_timer = 0f;
            time_between_attacks = 1f / attack_speed;

            /* initialization of animation vars */
            frame_timer = 0f;
            time_between_frames = 1f;
            frame = 0;
            n_frames = 2;

            // trigger collider initialization
            // notice that bool solid is false, indicating that this is a trigger collider
            // all colliders management is done by using gameController management, via
            // a subscription pattern (subscribe, unsubscribe, update...).
            // it is important to pass the object parent of the collider to the collider,
            // so when a collision happens we can retrieve the object that collided.
            // we pas gameController`s GraphicsDevice for debugging colliders with visual boxes
            float trigger_def_size = spritesize * camera.zoom * scale;
            trigger = new Collider(position.X + trigger_offset_x, position.Y + trigger_offset_y,
                                     trigger_def_size * trigger_width_scale, trigger_def_size * trigger_height_scale, 
                                        false, this, gameController.GraphicsDevice);
            // subscribe trigger in the list of trigger colliders (done via gameController methods)
            // SusbscribeCollider method is used in both triggers and physics (solid) colliders.
            gameController.SubscribeCollider(trigger);
        }

        /// <summary>
        /// Game Loop: In general, loads resources related specifically to MonoGame
        /// Important: Will be called only once by GameController`s GameLoop LoadContent, 
        ///            at the beginning of the program`s(game) execution and after Initialize.
        /// </summary>
        /// <param name="content">A reference to the ContentManager that controls interactions
        ///                       with the project Content resources</param>
        public void LoadContent(ContentManager content)
        {
            // loads from Content the source image for the spritesheet
            baffasheet = content.Load<Texture2D>("baffasheet");
            // cuts the spritesheet in a rectangular area to select 
            // the desired initial sprite of this object
            sprite = new Rectangle(0, 0, baffasheet.Bounds.Width / 2, baffasheet.Bounds.Height);
        }

        /// <summary>
        /// Game Loop: In general, unloads resources related specifically to MonoGame that
        ///            won`t be disposed by C#`s garbage collector
        /// Important: Will be called only once by GameController`s GameLoop UnloadContent, 
        ///            when graphics resources need to be unloaded, either when the program
        ///            is closing or when creating a new level (to unload unecessary old resources).
        /// </summary>
        /// <param name="content">A reference to the ContentManager that controls interactions
        ///                       with the project Content resources</param>
        public void UnloadContent(ContentManager content)
        {
            // dispose the image loaded into baffasheet variable
            baffasheet.Dispose();
            // make sure all content related resources are unloaded
            content.Unload();
        }

        /// <summary>
        /// Sets Baffa`s current target
        /// </summary>
        /// <param name="target">an object in the game</param>
        /// <param name="target_type">type of object in the game</param>
        public void SetCurrentTarget(Object target, string target_type)
        {
            this.target = target;
            this.target_type = target_type;
        }

        /// <summary>
        /// Game Loop: In general, calculates the physics of the object, updates its variables
        ///            and other dynamic configurations.
        /// Important: Will be called during the game`s execution by GameController`s GameLoop Update, 
        ///            when the gameloop has determined that game logic needs to be processed.
        ///            isFixedStep = true -> 1/60th of a second between calls.
        /// Caution:   Avoid loading resources in Update, they probably can be lodaded in LoadContent,
        ///            getting lodaded only once and not all the time, which would happen if its lodaded in Update,
        ///            resulting in performance drops, memory leaks, etc...
        /// </summary>
        /// <param name="gameTime">the deltatime between last call and current</param>
        public void Update(GameTime gameTime)
        {
            // converts to gameTime to a float value in the desired metric
            // delta is very important to synchronize game physics and
            // updates in general on different graphics card, with different
            // performances, and must be used as a scale to normalize values
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // updates camera position always, with the object dynamic world position
            // tending to change, we must make sure that our camera coordinates are
            // updated so the object is properly displayed in the screen 
            camera.UpdateCameraPosition(position, ref cameraPosition);

            // check if baffa is visible on camera
            active = camera.IsOnCamera(spritesize * scale * camera.zoom, cameraPosition.X, cameraPosition.Y);

            // if active (on camera)
            if (active)
            {
                // activates trigger
                trigger.active = true;
                // updates attack timer
                attack_timer += delta;
                // animate sprite
                Animate(delta);
                // follow current target;
                Follow(delta);
                // updates trigger collider position
                gameController.UpdateSubscription(trigger, cameraPosition.X + trigger_offset_x, cameraPosition.Y + trigger_offset_y, 
                                                    trigger.width, trigger.height);

                // checks if baffa trigger collided with other trigger
                // We will check if baffa trigger has collided with
                // player`s trigger collider and make the 
                // player take damage if so.
                // Note: if we were to check a physic collision,
                // preventing baffa from move any longer, we`d have
                // to check collision before the movement was made,
                // passing the move value and checking if baffa would
                // collide if moved with move value, and only then we
                // would move or not move depending if collision happened or not
                CheckTriggerCollision();
            }
            else // not active
            {
                // deactivates trigger
                trigger.active = false;
            }
        }

        /// <summary>
        /// follows current target position
        /// </summary>
        public void Follow(float delta)
        {
            // different casts for different target types
            switch (target_type)
            {
                case "Player":
                    // cast object type
                    Player player = (Player)target;
                    // player position anchor isnt centered!
                    Vector2 centered_position = new Vector2(player.Position.X - (player.tilesize * camera.zoom) / 2,
                                                            player.Position.Y - (player.tilesize * camera.zoom) / 2);
                    // move in direction of player
                    direction = Vector2.Normalize(centered_position - position);
                    // only move if can`t reach target yet
                    if(!CanReachTarget(centered_position))
                        Move(delta);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// returns if baffa`s can reach current target
        /// </summary>
        public bool CanReachTarget(Vector2 targetPosition)
        {
            float distance = Vector2.Distance(targetPosition, position);

            if (distance > reach_distance)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Moves baffa`s in the current direction
        /// by the current movementSpeed syncing with delta
        /// </summary>
        /// <param name="delta">deltatime between frames</param>
        public void Move(float delta)
        {
            // Vector2 that represents the move of Baffa`s
            Vector2 currMove = movementSpeed * delta * direction;

            // applies the move to baffa`s position (without collision check)
            // for this example, we will add only  a trigger collider, so there is no
            // need to check for a physic collision before moving baffa
            position += currMove;
        }

        /// <summary>
        /// check collision between trigger colliders
        /// </summary>
        public void CheckTriggerCollision()
        {
            // retrieve the list of triggers from gameController management
            List<Collider> triggers = gameController.GetSubscribedColliders(false);

            // scans all triggers in the list of triggers from gameController management
            for (int i = 0; i < triggers.Count; i++)
            {
                // dont check collision with itself
                if (triggers[i].Equals(triggers))
                    continue;

                // check if collider with a trigger from the list
                // using auxiliar method present in any collider 
                // CheckCollision, which only needs an external collider
                // to check if there is a collision with itself
                if (triggers[i].CheckCollision(trigger.x, trigger.y, trigger.width, trigger.height))
                {
                    // deal with trigger collision
                    OnTriggerCollision(triggers[i]);
                }
            }
        }

        /// <summary>
        /// deals with collision of triggers
        /// </summary>
        /// <param name="trigger">trigger that collided</param>
        void OnTriggerCollision(Collider trigger)
        {
            // retrieve parent type of trigger
            string hit_type = trigger.GetParentClassName();
            // retrieve parent object
            Object hit_object = trigger.GetParent();

            // different reactions to different types of collision
            switch(hit_type)
            {
                case "Player":
                    // deal damage if time between atacks is surpassed by the timer
                    if (attack_timer >= time_between_attacks)
                    {
                        // cast object to Player
                        Player player = (Player)hit_object;

                        // if player is dead, dont attack
                        if (!player.alive)
                            return;

                        // resets attack timer
                        attack_timer = 0f;

                        // sound effect - created using Sound management
                        gameController.SubscribeSound(new Sound(gameController.Content, Sound.SoundType.Hit_Spike, 0.5f, 1f));
                        // damage position (centered in player)
                        Vector2 damage_pos = new Vector2(player.Position.X - (player.tilesize * camera.zoom) / 2 + 5f * camera.zoom,
                                                        player.Position.Y - (player.tilesize * camera.zoom) / 2 - 5f * camera.zoom);
                        // calculates damage - formulas in Damage class
                        Damage dmg = new Damage(damage_pos, Color.Red, gameController, gameController.Content);
                        dmg.CalculateDamage(player, attack, false);
                        // subscribe damage to be drawn - GameController management
                        gameController.SubscribeDamage(dmg);
                        // take damage - player method to take damage
                        player.TakeHit(dmg.value, false);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Animates baffa sprite
        /// Simple animation, only changes between two sprite
        /// and ocurres all the time (if visible on camera)
        /// </summary>
        private void Animate(float delta)
        {
            // increases animation`s frame timer
            frame_timer += delta;

            // alternates frame if time_between_frames is achieved
            if (frame_timer >= time_between_frames)
            {
                frame_timer = 0f;
                frame++;
                frame = frame % n_frames;
            }

            // updates current sprite rectangular cut:
            // only needs to cycle in the X axis because
            // spritesheet contains the two frames side by sidse
            // and both have the same size.
            // Observation: Spritesheet precision is important,
            // and saves a lot of trouble, but if spritesheet sprites
            // aren`t well positioned, it is still possible to 
            // adjust it here by adding offsets/scales to the cut.
            sprite.X = (int)Math.Round(spritesize * frame);
        }

        /// <summary>
        /// Game Loop: In general, it is responsible for all game rendering.
        /// Important: Will be called during the game`s execution by GameController`s GameLoop Draw, 
        ///            when the gameloop determines it is time to draw a frame.
        ///            isFixedStep = true -> Draw will be called as often as possible.
        /// Caution:   Avoid loading resources in Draw, they probably can be lodaded in LoadContent,
        ///            getting lodaded only once and not all the time, which would happen if its lodaded in Draw,
        ///            resulting in performance drops, memory leaks, etc...
        /// </summary>
        /// <param name="spriteBatch">The batch of sprites that are drawn with specific configurations</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // only draws baffa sprite if it is active (on camera)
            if (active)
            {
                // to avoid desync in drawing  due to update and draw syncing
                // we should guarantee that cameraPosition is updated before drawing
                camera.UpdateCameraPosition(position, ref cameraPosition);

                // adds to the sprite batch a new sprite to be rendered using the specified 
                // texture, position, source rectangle, color, rotation, origin, scale, effects, and layer
                // Important: Color must be white for unadulterated image displaying. You can multiply by an alpha to 
                //            add transparency to the sprite if desired.
                //            Scale must be related to the camera.zoom, even though we are not changing the zoom during
                //            game`s execution
                //            Layer is the result of orthogonalization relate to both axis (x,y), by adding cameraPosition.x and .y
                //            as a factor to the layer. If orthogonalization of an object is not desirable, just define the layer
                //            statically as a float value between (0,1) - 1 being the top layer, and 0 being the background layer.
                spriteBatch.Draw(baffasheet, cameraPosition, sprite, Color.White, 0f, Vector2.Zero, camera.zoom * scale, 
                                    SpriteEffects.None, camera.GetOrthogonalLayer(cameraPosition));
            }
        }
    }
}
