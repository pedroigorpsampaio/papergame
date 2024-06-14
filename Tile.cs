using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Gamerator
{
    public class Tile
    {
        // types of generic tiles
        public enum Type { Ground, Unused, Wall, Start, Spike };
        // type of tile
        internal Type type;

        // wall position
        Vector2 Position;
        // wall collider
        public Collider collider;
        // reference to gameController
        public GameController gameController;
        // reference to the list of colliders
        public List<Collider> colliders;
        // wall id in spritesheet
        public int id;
        // wall tilesize
        public int tilesize;
        // a reference to the camera
        public Camera camera;
        // the map grid
        char[,] grid;
        // the spritesheet for the walls
        Texture2D tileset;
        // the sprite rectangle in spritesheet
        Rectangle sprite;
        // a bool to define if tile should be layered as a orthogonal tile
        bool orthogonal;
        // layer of tile 
        float layer;
        // if a tile is solid or not
        public bool solid;
        private bool active;

        // coll x coord offset // dont bother with camera zoom
        private float coll_offset_x = 5;
        // coll y coord offset // dont bother with camera zoom
        private float coll_offset_y = 18;
        // coll width scale
        private float coll_width_scale = 0.75f;
        // coll height scale
        private float coll_height_scale = 0.5f;

        // spike trigger
        public Collider trigger;
        // trigger x coord offset // dont bother with camera zoom
        private float trigger_offset_x = 4;
        // trigger y coord offset // dont bother with camera zoom
        private float trigger_offset_y = 10;
        // trigger width scale
        private float trigger_width_scale = 0.70f;
        // trigger height scale
        private float trigger_height_scale = 0.4f;
        //timer for spike attack
        internal float spike_damage_timer;
        // time between spike attacks
        internal float time_between_spike_damage = 0.5f;

        public void Initialize(Type type, int id, int spr_ind_x, int spr_ind_y, int off_x, int off_y, bool solid, bool orthogonal, float layer, Vector2 position,
                                int tilesize, Camera camera, char[,] grid, ContentManager content, GameController gameController)
        {
            this.type = type;
            this.id = id;
            this.tilesize = tilesize;
            this.camera = camera;
            this.grid = grid;
            this.gameController = gameController;
            this.orthogonal = orthogonal;
            this.layer = layer;
            this.solid = solid;
            colliders = gameController.GetSubscribedColliders(true);

            spike_damage_timer = 0f;

            // set tileset
            if (type == Type.Wall)
            {
                tileset = content.Load<Texture2D>("tree");
                tilesize = 40;
            }
            else if (type == Type.Ground)
                tileset = content.Load<Texture2D>("tileset");
            else
                tileset = content.Load<Texture2D>("tiles");        

            // Set the starting position of the tile
            Position = position;

            // if tiles is solid, need do add a collider
            if (solid)
            {
                // one size collider for all types of walls (as of now)
                float tilezoomed = camera.zoom * tilesize;
                collider = new Collider(position.X + coll_offset_x * camera.zoom, position.Y  + coll_offset_y * camera.zoom,
                         tilezoomed * coll_width_scale, tilezoomed * coll_height_scale, true, this, gameController.GraphicsDevice);
                // subscribe collider
                gameController.SubscribeCollider(collider);
            }
            // if spike, add trigger
            if (type == Type.Spike)
            {
                // one size trigger for all types of spikes (as of now)
                float tilezoomed = camera.zoom * tilesize;
                trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + trigger_offset_y * camera.zoom,
                         tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
                // subscribe trigger
                gameController.SubscribeCollider(trigger);
            }

            if(type != Type.Wall)
                sprite = new Rectangle(tilesize * (spr_ind_x + id) + off_x, tilesize * spr_ind_y + off_y, tilesize, tilesize);  
            else
                sprite = new Rectangle(0, 0, tilesize, tilesize);
        }

        // update colliders;
        public void Update(GameTime gameTime)
        {
            /*** camera framing ****/
            float target_y = Position.Y - camera.y - 14f * camera.zoom;
            float target_x = Position.X - camera.x - 6f * camera.zoom;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // check if tile is visible on camera
            active = camera.IsOnCamera(tilesize, target_x, target_y);

            // no need to update collider if it isnt solid or spike
            if (!solid && type != Type.Spike)
                return;

            float tilezoomed = tilesize * camera.zoom;

            if (solid)
            {
                collider.x = target_x + coll_offset_x * camera.zoom;
                collider.y = target_y + coll_offset_y * camera.zoom;
            }
            if (type == Type.Spike)
            {
                // updates spike damage timer
                spike_damage_timer += delta;
                // updates spike trigger
                trigger.x = target_x + trigger_offset_x * camera.zoom;
                trigger.y = target_y + trigger_offset_y * camera.zoom;
            }

            // only updates colliders if active (visible on camera)
            if (active)
            {
                if(solid)
                    gameController.UpdateSubscription(collider, collider.x, collider.y, tilezoomed * coll_width_scale, tilezoomed * coll_height_scale);
                if(type == Type.Spike)
                    gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float deltatime, float global_light)
        {
            // only draw if its visible in camera
            if(active)
            {
                /*** camera framing ****/
                float tilezoomed = tilesize * camera.zoom;
                float target_y = (float)Math.Round(Position.Y) - camera.y - 14f * camera.zoom;
                float target_x = (float)Math.Round(Position.X) - camera.x - 6f * camera.zoom;

                // draws tiles
                Vector2 tilePosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));

                float draw_layer = layer;
                // if orthogonal tile
                if (orthogonal)
                    draw_layer = layer + (0.0001f * tilePosition.Y) + (0.00005f * tilePosition.X);

                spriteBatch.Draw(tileset, tilePosition, sprite, Color.White * global_light * global_light, 0f,
                                Vector2.Zero, camera.zoom * (1f + camera.zoom / 49),SpriteEffects.None, draw_layer);
            }
        }
    }
}
