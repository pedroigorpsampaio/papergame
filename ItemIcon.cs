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
    public class ItemIcon
    {
        // bg frame
        Rectangle item_bg;
        // bg spritesheet
        Texture2D hudsheet;
        // item icon frame
        Rectangle item_icon;
        // item spritesheet
        Texture2D itemsheet;
        // itemicon position
        Vector2 position;
        // spriteBatch reference
        SpriteBatch spriteBatch;
        // content reference
        ContentManager content;
        // hud scale
        float scale;
        // reference to the player
        Player player;
        // font
        SpriteFont HUDFont;
        // total offset
        float offset;
        // max offset
        float max_offset;
        // number of items
        int n_items;
        // number of displayed items
        int n_display_items;
        // distance between icons
        int distance_between_icons;
        // item type
        Item.ItemType type;

        public void Initialize(ContentManager content, SpriteBatch spriteBatch, Vector2 position, float scale, Item.ItemType type,
                                Player player, SpriteFont HUDFont, Texture2D itemsheet, Rectangle item_icon)
        {
            item_bg = new Rectangle(0, 78, 44, 40);
            hudsheet = content.Load<Texture2D>("hud");
            this.spriteBatch = spriteBatch;
            this.content = content;
            this.position = position;
            this.scale = scale;
            this.player = player;
            this.HUDFont = HUDFont;
            this.itemsheet = itemsheet;
            this.item_icon = item_icon;
            this.type = type;

            // total offset initial
            offset = 0f;
            // number of item types in hud
            n_items = 7;
            // number of items show at the same time in hud
            n_display_items = 3;
            // distance between item icons
            distance_between_icons = 3;
            // offset
            float offset_x = -0.5f;

            // max offsetz
            max_offset = ((n_items - n_display_items) * (item_bg.Width + distance_between_icons)) + offset_x;
        }

        public void ScrollLeft(float amount)
        {
            // only moves if offset is bigger than 0
            if (offset > -max_offset)
            {
                position.X = position.X - amount;
                offset -= amount;
            }
        }

        public void ScrollRight(float amount)
        {
            if (offset < 0)
            {
                position.X = position.X + amount;
                offset += amount;
            }
        }

        public void Draw()
        {
            // draw item icon backgruond
            spriteBatch.Draw(hudsheet, new Vector2(position.X * scale, position.Y * scale), item_bg, Color.White, 0f, Vector2.Zero, 1f * scale, SpriteEffects.None, 0.951f);

            // draw icon
            float offset_x = (item_bg.Width * scale / 4) - 3f * scale;
            float offset_y = (item_bg.Width * scale / 4) - 5f * scale;

            spriteBatch.Draw(itemsheet, new Vector2(position.X * scale + offset_x, position.Y * scale + offset_y), item_icon, Color.White, 0f, Vector2.Zero, 0.85f * scale, SpriteEffects.None, 0.952f);
            // draw players item quantity
            // switch type to get exact quantity;
            float quantity;
            switch(type)
            {
                case Item.ItemType.Armor:
                    quantity = player.n_armors;
                    break;
                case Item.ItemType.Food:
                    quantity = player.n_foods;
                    break;
                case Item.ItemType.Key:
                    quantity = player.n_keys;
                    break;
                case Item.ItemType.Potion:
                    quantity = player.n_potions;
                    break;
                case Item.ItemType.Shield:
                    quantity = player.n_shields;
                    break;
                case Item.ItemType.Treasure:
                    quantity = player.n_treasures;
                    break;
                case Item.ItemType.Weapon:
                    quantity = player.n_weapons;
                    break;
                default:
                    quantity = 0;
                    break;
            }

            // draws quantity
            float offset_text_x = 44f * scale - (HUDFont.MeasureString("x" + quantity).Length() / 3.1f) * scale;
            float offset_text_y = 24f * scale;
            spriteBatch.DrawString(HUDFont, "x" + quantity, new Vector2(position.X * scale + offset_text_x+1, position.Y * scale + offset_text_y+1), Color.Black, 0f, Vector2.Zero, 0.30f * scale, SpriteEffects.None, 0.954f);
            spriteBatch.DrawString(HUDFont, "x" + quantity, new Vector2(position.X * scale + offset_text_x, position.Y * scale + offset_text_y), Color.White, 0f, Vector2.Zero, 0.30f * scale, SpriteEffects.None, 0.956f);

        }
    }
}
