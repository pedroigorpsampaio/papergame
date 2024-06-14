using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Gamerator
{
    public class Chest : IDisposable
    {

        // item inside chest
        public Item item;
        // chest collider
        public Collider collider;
        // chest trigger collider
        public Collider trigger;
        // reference to gameController
        public GameController gameController;
        // reference to the list of colliders
        public List<Collider> colliders;
        // coll x coord offset // dont bother with camera zoom
        private float coll_offset_x = 2;
        // coll y coord offset // dont bother with camera zoom
        private float coll_offset_y = 17;
        // coll width scale
        private float coll_width_scale = 0.75f;
        // coll height scale
        private float coll_height_scale = 0.4f;
        // trigger x coord offset // dont bother with camera zoom
        private float trigger_offset_x = 1;
        // trigger y coord offset // dont bother with camera zoom
        private float trigger_offset_y = 10;
        // trigger width scale
        private float trigger_width_scale = 0.856f;
        // trigger height scale
        private float trigger_height_scale = 0.66f;
        // box health
        public int hitpoints;
        // box max health
        public int max_hp;
        // chest position
        public Vector2 position;
        // chest_id
        public int chest_id;
        // tilesize
        public int tilesize;
        // chest spritesheet
        public Texture2D spritesheet;
        // chest sprite in spritesheet
        public Rectangle sprite;
        // a reference to the camera;
        internal Camera camera;
        // index of first chest in tilesheet
        int box_ind_x = 43;
        int box_ind_y = 45;
        // a reference to the content
        private ContentManager content;
        // bool attacked (to show health)
        public bool target;
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
        // chest name
        public string name;
        // chest defense
        public float defense;
        public float defense_const = 1f;
        // a reference to the player
        Player player;

        public void Initialize(int id, string name, int hitpoints, Item item, Vector2 position, int tilesize, Camera camera, char[,] grid, ContentManager content, GameController gameController)
        {
            this.tilesize = tilesize;
            this.camera = camera;
            this.gameController = gameController;
            this.position = position;
            chest_id = id;
            this.hitpoints = (int)Math.Round(hitpoints * 10 * (gameController.level * 1.5));
            max_hp = this.hitpoints;
            this.item = item;
            this.content = content;
            colliders = gameController.GetSubscribedColliders(true);
            player = gameController.player;

            // chest defense
            defense = gameController.level * (id + 1) * defense_const;

            // generate name
            this.name = name;

            target = false;

            spritesheet = content.Load<Texture2D>("tiles");
            sprite = new Rectangle(tilesize * (box_ind_x + chest_id), tilesize * box_ind_y, tilesize - 1, tilesize);

            // hud
            hudsheet = content.Load<Texture2D>("hud");
            hud = new Rectangle(0, 123, 121, 26);
            health_bar = new Rectangle(0, 0, 106, 12);
            target_arrow = new Rectangle(107, 0, 9, 9);
            arrow_animation = 0f;

            // one size collider for all types of boxes (as of now)
            float tilezoomed = camera.zoom * tilesize;
            collider = new Collider(position.X + coll_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + coll_offset_y * camera.zoom,
                                     tilezoomed * coll_width_scale, tilezoomed * coll_height_scale, true, this, gameController.GraphicsDevice);
            // subscribe collider
            gameController.SubscribeCollider(collider);

            // chest trigger collider
            trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + (tilezoomed / 2) + trigger_offset_y * camera.zoom,
                         tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            // subscribe trigger collider (goes to trigger list instead of collider list)
            gameController.SubscribeCollider(trigger);
        }

        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // destroy chest if hitpoints decreases below 0
            if (hitpoints <= 0)
                Destroy();

            // check if chest is the players target
            Object player_target = gameController.player_target;
            if (player_target != null)
            {
                string target_class = player_target.ToString().Split('.')[1];
                if (target_class == "Chest")
                {
                    if (this.Equals((Chest)player_target))
                        target = true;
                    else
                        target = false;
                }
                else
                    target = false;
            }

            // if target updates arrow animation
            if(target)
            {
                if (arrow_animation <= 5f * camera.zoom)
                    arrow_animation += 6f * delta * camera.zoom;
                else
                    arrow_animation = 0f;
            }

            /*** camera framing ****/
            float target_y = position.Y - camera.y;
            float target_x = position.X - camera.x;

            float tilezoomed = tilesize * camera.zoom;

            collider.x = target_x + coll_offset_x * camera.zoom;
            collider.y = target_y + coll_offset_y * camera.zoom;

            trigger.x = target_x + trigger_offset_x * camera.zoom;
            trigger.y = target_y + trigger_offset_y * camera.zoom;

            // check if enemy is visible on camera
            active = camera.IsOnCamera(tilesize, target_x, target_y);

            // only update colliders if active
            if (active)
            {
                // update collider subscription 
                gameController.UpdateSubscription(collider, collider.x, collider.y, tilezoomed * coll_width_scale, tilezoomed * coll_height_scale);
                // update trigger collider subscription 
                gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
            }
        }

        public void TakeHit(Point hit, string atk_dealer, Object Attacker)
        {
            // hit effect
            Effect effect = new Effect(content, Effect.EffectType.Crack_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera, hit.X - (camera.zoom * 11f), hit.Y - (camera.zoom * 4f),
                                gameController.effect_scale, gameController.time_between_clicks / 10f);

            // calculate damage
            Damage dmg = new Damage(new Vector2(position.X + 1f * camera.zoom, position.Y), Color.RosyBrown, gameController, content);

            if(atk_dealer == "player")
                dmg.CalculateDamage(player, this);
            else
                dmg.CalculateDamage((Enemy)Attacker, this);

            hitpoints -=  dmg.value;

            // add to the damage list to be drawn 
            gameController.SubscribeDamage(dmg);
        }

        public void UseKey(Point hit)
        {
            Effect effect = new Effect(content, Effect.EffectType.Crack_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera, hit.X - (camera.zoom * 11f), hit.Y - (camera.zoom * 4f),
                                gameController.effect_scale, gameController.time_between_clicks / 10f);

            hitpoints = 0;
        }

        public void Destroy()
        {
            // destruction effect
            Effect effect = new Effect(content, Effect.EffectType.Smoke_1);
            effect.CreateEffect(gameController, gameController.spriteBatch, camera,
                                position.X - camera.x - coll_offset_y / 10 / camera.zoom,
                                position.Y - camera.y + coll_offset_y/3 / camera.zoom, 
                                gameController.effect_scale * 1.75f, gameController.effect_duration * 0.5f);

            // remove me from list of chests
            gameController.UnsubscribeChest(this);
            // remove my colliders from list of colliders;
            gameController.UnsubscribeCollider(collider);
            gameController.UnsubscribeCollider(trigger);
            // drop item (only if chest has a item)
            item.Drop();

            Dispose();
        }

        private void DrawHUD(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // hud scale
            float hud_scale = gameController.hud_scale;

            // hud bg
            spriteBatch.Draw(hudsheet, new Vector2((camera.width / 2) - (hud.Width / 2 * hud_scale), 20 * hud_scale), hud,
                            Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, 0.92f);
            // healthbar
            float health_scale = hitpoints / (float)max_hp;
            float offset_x = 7f * hud_scale;
            float offset_y = 6f * hud_scale;
            spriteBatch.Draw(hudsheet, new Rectangle((int)Math.Round((camera.width / 2) - (hud.Width / 2 * hud_scale) + offset_x), 
                                                    (int)Math.Round(20 * hud_scale + offset_y) ,  (int)Math.Round(108 * health_scale * hud_scale),
                                                    (int)Math.Round(14 * hud_scale)), health_bar, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.93f);
            // target arrow
            float target_y = (float)Math.Round(position.Y) - camera.y;
            float target_x = (float)Math.Round(position.X) - camera.x;
            float t_arrow_off_x = (sprite.Width * camera.zoom / 2) - (target_arrow.Width * camera.zoom / 2) - 2f * camera.zoom;
            float t_arrow_off_y = -(arrow_animation * hud_scale);
            spriteBatch.Draw(hudsheet, new Vector2(target_x + t_arrow_off_x, target_y + t_arrow_off_y), target_arrow, 
                                                    Color.White, 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, 0.891f);

            // draw health info
            float health_offset_x = (max_hp.ToString().Length - 3) * hud_scale * 0.27f
            + (hitpoints.ToString().Length - 3) * hud_scale * 0.27f;
            spriteBatch.DrawString(gameController.HUDFont, "HP: " + hitpoints + " / " + max_hp, new Vector2((int)Math.Round(266 * hud_scale +
                                   (266 * hud_scale / 2f - gameController.HUDFont.MeasureString("HP: " + hitpoints + " / " + max_hp).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                                    health_offset_x / 2f), (int)Math.Round(30 * hud_scale)), Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);

            // draw chest name
            float name_offset_x = (name.ToString().Length - 3) * hud_scale * 0.27f;
            spriteBatch.DrawString(gameController.HUDFont, name, new Vector2((int)Math.Round(266 * hud_scale +
                                   (266 * hud_scale / 2f - gameController.HUDFont.MeasureString(name).X * hud_scale * 0.27f / 2 + 12 * hud_scale) -
                                    health_offset_x / 2f), (int)Math.Round(23 * hud_scale)), 
                                    Color.White, 0f, Vector2.Zero, 0.27f * hud_scale, SpriteEffects.None, 0.9999f);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            // only draw if active (visible on camera)
            if (active)
            {
                // show health bar if attacked
                if (target)
                    DrawHUD(spriteBatch, camera, deltatime, global_light);

                /*** camera framing ****/
                float target_y = (float)Math.Round(position.Y) - camera.y;
                float target_x = (float)Math.Round(position.X) - camera.x;

                // draws chest
                Vector2 chestPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                // update layer
                gameController.chest_layer = 0.1f + (0.0001f * target_y) + (0.00005f * target_x);
                spriteBatch.Draw(spritesheet, chestPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom, SpriteEffects.None, gameController.chest_layer);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private bool active;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Chest() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
