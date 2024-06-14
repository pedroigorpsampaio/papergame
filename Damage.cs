using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Gamerator
{
    public class Damage
    {
        internal Vector2 position;
        internal int value;
        internal Color color;
        internal float shift;
        internal bool critical;
        internal ContentManager content;
        internal Rectangle sprite_damaged;

        internal float damage_animation_speed = 30f;
        internal float damage_animation_threshold = 60f;

        // a reference to the game controller
        private GameController gameController;

        // random aux
        private Random random_noseed;

        // constant damages (only to be multipled by world level)
        internal float spike_damage = 5f;

        public Damage(Vector2 position, Color color, GameController gameController, ContentManager content)
        {
            this.position = position;
            this.color = color;
            this.gameController = gameController;
            this.content = content;
            random_noseed = gameController.random_noseed;
            shift = 0f;
            critical = false;
        }

        /* Damage calculation */
        /// damage formula (player x enemy)
        public void CalculateDamage(Player player, Enemy enemy)
        { 
            sprite_damaged = enemy.sprite;

            // check if damage was a critical hit
            if (random_noseed.Next(0, 100) < player.critical_chance)
            {
                critical = true;
                gameController.SubscribeSound(new Sound(content, Sound.SoundType.Critical_Hit_1, 0.5f, gameController.volume * gameController.hit_volume * 1.5f));
            }

            float actual_damage = random_noseed.Next((int)Math.Round(player.attack / 1.2f), (int)Math.Round(player.attack * 1.2f)) -
                             random_noseed.Next((int)Math.Round(enemy.defense / 8f), (int)Math.Round(enemy.defense / 4f));

            if (critical)
                actual_damage *= player.critical_multiplier * 1.1f;

            value = (int)Math.Round(actual_damage);

            // clamp if damage goes below 0 (defense >>>>> attack)
            if (value < 0)
                value = 0;
        }

        /// damage formula (enemy x player)
        public void CalculateDamage(Enemy enemy, Player player)
        {    
            sprite_damaged = player.sprite;

            float actual_damage = random_noseed.Next((int)Math.Round(enemy.attack / 1.4f), (int)Math.Round(enemy.attack * 2.1f)) -
                            random_noseed.Next((int)Math.Round(player.defense / 20f), (int)Math.Round(player.defense / 15f));

            value = (int)Math.Round(actual_damage);

            // damage halves and sield breaks
            if (player.n_shields > 0)
            {
                value /= 2;
                player.n_shields--;
            }

            // clamp if damage goes below 0 (defense >>>>> attack)
            if (value < 0)
                value = 0;
        }

        /// damage formula (player x chest)
        public void CalculateDamage(Player player, Chest chest)
        {  
            sprite_damaged = chest.sprite;

            // check if damage was a critical hit
            if (random_noseed.Next(0, 100) < player.critical_chance)
            {
                critical = true;
                gameController.SubscribeSound(new Sound(content, Sound.SoundType.Critical_Hit_1, 0.5f, gameController.volume * gameController.hit_volume * 1.5f));
            }

            float actual_damage = random_noseed.Next((int)Math.Round(player.attack / 1.5f), (int)Math.Round(player.attack * 1.5f)) -
                             random_noseed.Next((int)Math.Round(chest.defense / 10f), (int)Math.Round(chest.defense / 5f));

            if(critical)
                actual_damage *= player.critical_multiplier * 1.1f;

            value = (int)Math.Round(actual_damage);

            // clamp if damage goes below 0 (defense >>>>> attack)
            if (value < 0)
                value = 0;
        }

        /// damage formula (enemy x chest)
        public void CalculateDamage(Enemy enemy, Chest chest)
        {  
            sprite_damaged = chest.sprite;
            value = 0;
        }

        /// generic damage on player
        public void CalculateDamage(Player player, float damage, bool defend)
        {
            sprite_damaged = player.sprite;
            if (!defend)
                value = (int)Math.Round(damage * gameController.level);

            // damage halves and sield breaks
            if (player.n_shields > 0)
            {
                value /= 2;
                player.n_shields--;
            }

            // clamp if damage goes below 0 (defense >>>>> attack)
            if (value < 0)
                value = 0;
        }

        /// generic damage on enemy
        public void CalculateDamage(Enemy enemy, float damage, bool defend)
        {
            sprite_damaged = enemy.sprite;
            if (!defend)
                value = (int)Math.Round(damage * gameController.level);

            // clamp if damage goes below 0 (defense >>>>> attack)
            if (value < 0)
                value = 0;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime)
        {
            /*** camera framing ****/
            float target_y = (float)Math.Round(position.Y) - camera.y;
            float target_x = (float)Math.Round(position.X) - camera.x;

            float fontsize = 0.32f;
            Color main_color = Color.White;

            if (critical)
            {
                fontsize *= 1.25f;
                main_color = Color.Yellow;
            }

            float center_offset = (sprite_damaged.Width / 2) - (gameController.HUDFont.MeasureString(value.ToString()).X * fontsize * gameController.hud_scale) / 3f;

            Vector2 damagePosition = new Vector2((float)Math.Round(target_x + center_offset), (float)Math.Round(target_y));

            spriteBatch.DrawString(gameController.HUDFont, value.ToString(), damagePosition,
                                    main_color, 0f, Vector2.Zero, fontsize * gameController.hud_scale, SpriteEffects.None, 0.91f);

            spriteBatch.DrawString(gameController.HUDFont, value.ToString(), new Vector2(damagePosition.X + 1, damagePosition.Y + 1),
                    color, 0f, Vector2.Zero, fontsize * gameController.hud_scale, SpriteEffects.None, 0.90f);
        }
    }
}