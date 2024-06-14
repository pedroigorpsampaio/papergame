using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Gamerator
{
    public class Totem
    {
        public int value;
        private Camera camera;
        private Texture2D tileset;
        private Collider collider;
        private float collider_offset_x = 6.5f;
        private float collider_offset_y = 18f;
        private float collider_width_scale = 0.55f;
        private float collider_height_scale = 0.36f;
        private Collider trigger;
        private float trigger_offset_x = 6f;
        private float trigger_offset_y = 1f;
        private float trigger_width_scale = 0.65f;
        private float trigger_height_scale = 0.96f;
        private Rectangle sprite;
        private int tilesize;
        private GameController gameController;
        private Vector2 position;
        private ContentManager content;
        private bool active;

        // Next Level HUD
        private Player player;
        private bool is_next_level_accessible;
        public bool drawHUD;
        private int n_items_next_level;
        private int n_items_selected;
        private Vector2 hudPosition;
        private Vector2 nextLevelPosition;
        private float hud_scale;
        private SpriteFont HUDFont;
        private Texture2D hudsheet;
        private Texture2D itemsheet;
        private Rectangle hud_bg_sprite;
        private Rectangle token_sprite;
        private int token_ind_x;
        private int token_ind_y;
        private float token_alpha;
        private Color token_color;
        private int token_value;
        private Rectangle treasure_sprite;
        private Rectangle armor_sprite;
        private Rectangle weapon_sprite;
        private Rectangle shield_sprite;
        private Rectangle food_sprite;
        private Rectangle potion_sprite;
        private Rectangle key_sprite;
        private Rectangle next_level_sprite;
        private Rectangle next_level_not_sprite;
        private Rectangle up_arrow_sprite;
        private Rectangle down_arrow_sprite;
        private Rectangle quantity_bg;
        private Collider hud_trigger;
        private Collider armor_up_trigger;
        private Collider armor_down_trigger;
        private Collider shield_up_trigger;
        private Collider shield_down_trigger;
        private Collider food_up_trigger;
        private Collider food_down_trigger;
        private Collider potion_up_trigger;
        private Collider potion_down_trigger;
        private Collider key_up_trigger;
        private Collider key_down_trigger;
        private Collider weapon_up_trigger;
        private Collider weapon_down_trigger;
        private Collider next_level_trigger;
        private Collider close_hud_trigger;
        private int n_armor_chosen;
        private int n_weapon_chosen;
        private int n_shield_chosen;
        private int n_key_chosen;
        private int n_food_chosen;
        private int n_potion_chosen;

        // HUD interaction timer
        private float hud_timer = 0f;
        private float time_between_interaction = .2f;

        public void Initialize(int totem_ind_x, int totem_ind_y, int off_x, int off_y, Vector2 position,
                        int tilesize, Camera camera, ContentManager content, GameController gameController)
        {
            drawHUD = false;
            this.position = position;
            this.tilesize = tilesize;
            this.camera = camera;
            this.content = content;
            this.gameController = gameController;
            HUDFont = gameController.HUDFont;
            player = gameController.player;
            is_next_level_accessible = false;
            n_items_next_level = 0;
            n_items_selected = 0;
            token_ind_x = 0;
            token_ind_y = 0;
            token_color = Color.White;
            token_alpha = 1f;
            token_value = 0;
            n_armor_chosen = 0;
            n_weapon_chosen = 0;
            n_shield_chosen = 0;
            n_key_chosen = 0;
            n_food_chosen = 0;
            n_potion_chosen = 0;

            tileset = content.Load<Texture2D>("tiles");

            float tilezoomed = camera.zoom * tilesize;
            collider = new Collider(position.X + collider_offset_x * camera.zoom, position.Y + collider_offset_y * camera.zoom,
                     tilezoomed * collider_width_scale, tilezoomed * collider_height_scale, true, this, gameController.GraphicsDevice);
            // subscribe collider
            gameController.SubscribeCollider(collider);

            trigger = new Collider(position.X + trigger_offset_x * camera.zoom, position.Y + trigger_offset_y * camera.zoom,
                     tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale, false, this, gameController.GraphicsDevice);
            // subscribe trigger
            gameController.SubscribeCollider(trigger);

            // totem sprite
            sprite = new Rectangle(tilesize * totem_ind_x + off_x, tilesize * totem_ind_y + off_y, tilesize, tilesize);

            // hudsheet
            hudsheet = content.Load<Texture2D>("hud");
            itemsheet = content.Load<Texture2D>("items");
            hud_scale = gameController.hud_scale;
            // hud sprites
            hud_bg_sprite = new Rectangle(227, 0, 224, 252);
            next_level_not_sprite = new Rectangle(0, 212, 80, 26);
            next_level_sprite = new Rectangle(0, 152, 80, 26);
            treasure_sprite = new Rectangle(0,8*34,34,34);
            weapon_sprite = new Rectangle(0 * 34, 3 * 34, 34, 34);
            food_sprite = new Rectangle(1 * 34, 1 * 34, 34, 34);
            key_sprite = new Rectangle(7 * 34, 10 * 34, 34, 34);
            potion_sprite = new Rectangle(0 * 34, 2 * 34, 34, 34);
            shield_sprite = new Rectangle(12 * 34, 7 * 34, 34, 34);
            armor_sprite = new Rectangle(3 * 34, 10 * 34, 34, 34);
            up_arrow_sprite = new Rectangle(123, 124, 24, 24);
            down_arrow_sprite = new Rectangle(149, 124, 24, 24);
            quantity_bg = new Rectangle(114, 183, 86, 24);
            // hud positions
            hudPosition = new Vector2(gameController.GraphicsDevice.Viewport.Width / 2 - (hud_bg_sprite.Width * hud_scale) / (2.5f),
                       gameController.GraphicsDevice.Viewport.Height / 2 - (hud_bg_sprite.Height * hud_scale) / (2f));
            nextLevelPosition = new Vector2(hudPosition.X + (hud_bg_sprite.Width * hud_scale) / 2f - (next_level_sprite.Width * hud_scale)/1.45f, 
                                            hudPosition.Y + (hud_bg_sprite.Height * hud_scale) - (next_level_sprite.Height * hud_scale) - (10f * hud_scale));
            // hud triggers
            hud_trigger = new Collider(hudPosition.X, hudPosition.Y, hud_bg_sprite.Width * hud_scale - (32f * hud_scale),
                                        hud_bg_sprite.Height * hud_scale, false, new Button(Button.Type.Null, Button.Group.TotemHUD),
                                        gameController.GraphicsDevice, 1f);

            close_hud_trigger = new Collider(hudPosition.X + (175f * hud_scale), hudPosition.Y + (9f * hud_scale),  40f * hud_scale, 46f * hud_scale, 
                                                false, new Button(Button.Type.CloseTotemHUD, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            next_level_trigger = new Collider(nextLevelPosition.X, nextLevelPosition.Y, next_level_sprite.Width * hud_scale, next_level_sprite.Height * hud_scale, 
                                              false, new Button(Button.Type.NextLevel, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            Vector2 firstQuantityBG = hudPosition + new Vector2(15f * hud_scale, (95f * hud_scale)) + new Vector2(-5f * hud_scale, 34f * hud_scale);
            float updown_distance = quantity_bg.Width * 0.5f * hud_scale - (quantity_bg.Height * 0.5f * hud_scale);

            float distanceX = quantity_bg.Width * 0.5f * hud_scale + (22f * hud_scale);
            float distanceY = armor_sprite.Height * hud_scale + (25f * hud_scale);


            weapon_down_trigger = new Collider(firstQuantityBG.X, firstQuantityBG.Y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                                                false, new Button(Button.Type.DownWeapon, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            weapon_up_trigger = new Collider(firstQuantityBG.X + updown_distance, firstQuantityBG.Y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                                    false, new Button(Button.Type.UpWeapon, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            armor_down_trigger = new Collider(weapon_down_trigger.x + distanceX, weapon_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                                    false, new Button(Button.Type.DownArmor, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            armor_up_trigger = new Collider(armor_down_trigger.x + updown_distance, armor_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                        false, new Button(Button.Type.UpArmor, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            shield_down_trigger = new Collider(armor_down_trigger.x + distanceX, armor_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                         false, new Button(Button.Type.DownShield, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            shield_up_trigger = new Collider(shield_down_trigger.x + updown_distance, shield_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                                false, new Button(Button.Type.UpShield, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            key_down_trigger = new Collider(firstQuantityBG.X, firstQuantityBG.Y + distanceY, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                                    false, new Button(Button.Type.DownKey, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            key_up_trigger = new Collider(key_down_trigger.x + updown_distance, key_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                            false, new Button(Button.Type.UpKey, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            food_down_trigger = new Collider(key_down_trigger.x + distanceX, key_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                            false, new Button(Button.Type.DownFood, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            food_up_trigger = new Collider(food_down_trigger.x + updown_distance, key_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                             false, new Button(Button.Type.UpFood, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);

            potion_down_trigger = new Collider(food_down_trigger.x + distanceX, key_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                             false, new Button(Button.Type.DownPotion, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);
                
            potion_up_trigger = new Collider(potion_down_trigger.x + updown_distance, key_down_trigger.y, quantity_bg.Height * 0.5f * hud_scale, quantity_bg.Height * 0.5f * hud_scale,
                            false, new Button(Button.Type.UpPotion, Button.Group.TotemHUD), gameController.GraphicsDevice, 1.1f);
        }

        public void Update(GameTime gameTime)
        {
            /*** camera framing ****/
            float target_y = position.Y - camera.y;
            float target_x = position.X - camera.x;
            float tilezoomed = tilesize * camera.zoom;

            collider.x = target_x + collider_offset_x * camera.zoom + 0f * camera.zoom;
            collider.y = target_y + collider_offset_y * camera.zoom + 0f * camera.zoom;

            trigger.x = target_x + trigger_offset_x * camera.zoom + 0f * camera.zoom;
            trigger.y = target_y + trigger_offset_y * camera.zoom + 0f * camera.zoom;

            // check if totem is visible on camera
            active = camera.IsOnCamera(tilesize, target_x, target_y);

            // update trigger collider subscription if visible on camera
            if (active)
            {
                gameController.UpdateSubscription(collider, collider.x, collider.y, tilezoomed * collider_width_scale, tilezoomed * collider_height_scale);
                gameController.UpdateSubscription(trigger, trigger.x, trigger.y, tilezoomed * trigger_width_scale, tilezoomed * trigger_height_scale);
            }

            if(drawHUD)
                UpdateHUD(gameTime);

            // don't interact with player if he isnt alive
            if(!player.alive)
            {
                drawHUD = false;
            }
        }

        // updates related to the hud
        private void UpdateHUD(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // updates timer
            hud_timer += delta;

            // update n of items selected
            n_items_selected = n_armor_chosen + n_food_chosen + n_key_chosen + 
                                n_potion_chosen + n_shield_chosen + n_weapon_chosen;

            if (n_items_selected > n_items_next_level)
                Console.WriteLine("NItem selected Explosion!");

            // updates current player token and token display
            if (player.tokens.Count > 0)
            {
                if (player.tokens[0].token_type == Token.TokenType.Bronze)
                {
                    token_ind_x = 37; token_ind_y = 20;
                }
                else if (player.tokens[0].token_type == Token.TokenType.Silver)
                {
                    token_ind_x = 43; token_ind_y = 20;
                }
                else if (player.tokens[0].token_type == Token.TokenType.Gold)
                {
                    token_ind_x = 46; token_ind_y = 20;
                }
                // token value
                token_value = player.tokens[0].value;
                token_color = Color.White;
                token_alpha = 1f;
            }
            else
            {
                token_alpha = 0.5f;
                token_ind_x = 46; token_ind_y = 20;
                token_color = Color.Black;
            }

            // calculates how many items player can choose as bonus
            n_items_next_level = token_value + TreasureItemConversion();

            // updates player next level accessability
            if (player.tokens.Count > 0 && n_items_selected == n_items_next_level)
                is_next_level_accessible = true;
            else
                is_next_level_accessible = false;
        }

        // input handle
        public void HandleInput(Button button)
        {
            if (hud_timer >= time_between_interaction)
            {
                hud_timer = 0f;

                switch (button.type)
                {
                    case Button.Type.CloseTotemHUD:
                        drawHUD = false;
                        UnsubscribeButtons();
                        ResetInfo();
                        break;
                    case Button.Type.NextLevel:
                        if (is_next_level_accessible)
                        {
                            // update player items
                            player.n_armors += n_armor_chosen;
                            player.n_foods += n_food_chosen;
                            player.n_potions += n_potion_chosen;
                            player.n_weapons += n_weapon_chosen;
                            player.n_keys += n_key_chosen;
                            player.n_shields += n_shield_chosen;
                            // reset treasure count (traded for items)
                            player.n_treasures = 0;
                            gameController.LoadLevel(++gameController.level, ++gameController.seed, false);
                        }
                        else
                        {
                            string msg_text = "You can't go to the next level until you have a token and choose all item bonuses";
                            gameController.SubscribeMessage(new Message(msg_text, HUDFont, hud_scale * 0.4f, 5f, gameController));
                        }
                        break;
                    case Button.Type.UpArmor:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_armor_chosen++;
                        break;
                    case Button.Type.UpFood:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_food_chosen++;
                        break;
                    case Button.Type.UpKey:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_key_chosen++;
                        break;
                    case Button.Type.UpPotion:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_potion_chosen++;
                        break;
                    case Button.Type.UpShield:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_shield_chosen++;
                        break;
                    case Button.Type.UpWeapon:
                        if (n_items_selected < n_items_next_level && n_items_next_level > 0)
                            n_weapon_chosen++;
                        break;
                    case Button.Type.DownArmor:
                        if (n_armor_chosen > 0)
                            n_armor_chosen--;
                        break;
                    case Button.Type.DownFood:
                        if (n_food_chosen > 0)
                            n_food_chosen--;
                        break;
                    case Button.Type.DownKey:
                        if (n_key_chosen > 0)
                            n_key_chosen--;
                        break;
                    case Button.Type.DownPotion:
                        if (n_potion_chosen > 0)
                            n_potion_chosen--;
                        break;
                    case Button.Type.DownShield:
                        if (n_shield_chosen > 0)
                            n_shield_chosen--;
                        break;
                    case Button.Type.DownWeapon:
                        if (n_weapon_chosen > 0)
                            n_weapon_chosen--;
                        break;
                    default:
                        break;
                }
            }
        }

        // subscribe buttons triggers
        public void SubscribeButtons()
        {
            gameController.SubscribeCollider(armor_up_trigger);
            gameController.SubscribeCollider(armor_down_trigger);
            gameController.SubscribeCollider(shield_up_trigger);
            gameController.SubscribeCollider(shield_down_trigger);
            gameController.SubscribeCollider(food_up_trigger);
            gameController.SubscribeCollider(food_down_trigger);
            gameController.SubscribeCollider(potion_up_trigger);
            gameController.SubscribeCollider(potion_down_trigger);
            gameController.SubscribeCollider(key_up_trigger);
            gameController.SubscribeCollider(key_down_trigger);
            gameController.SubscribeCollider(weapon_up_trigger);
            gameController.SubscribeCollider(weapon_down_trigger);
            gameController.SubscribeCollider(next_level_trigger);
            gameController.SubscribeCollider(close_hud_trigger);
            gameController.SubscribeCollider(hud_trigger);
        }

        // unsubscribe buttons triggers
        public void UnsubscribeButtons()
        {
            gameController.UnsubscribeCollider(armor_up_trigger);
            gameController.UnsubscribeCollider(armor_down_trigger);
            gameController.UnsubscribeCollider(shield_up_trigger);
            gameController.UnsubscribeCollider(shield_down_trigger);
            gameController.UnsubscribeCollider(food_up_trigger);
            gameController.UnsubscribeCollider(food_down_trigger);
            gameController.UnsubscribeCollider(potion_up_trigger);
            gameController.UnsubscribeCollider(potion_down_trigger);
            gameController.UnsubscribeCollider(key_up_trigger);
            gameController.UnsubscribeCollider(key_down_trigger);
            gameController.UnsubscribeCollider(weapon_up_trigger);
            gameController.UnsubscribeCollider(weapon_down_trigger);
            gameController.UnsubscribeCollider(next_level_trigger);
            gameController.UnsubscribeCollider(close_hud_trigger);
            gameController.UnsubscribeCollider(hud_trigger);
        }

        public void ResetInfo()
        {
            is_next_level_accessible = false;
            n_items_next_level = 0;
            n_items_selected = 0;
            token_ind_x = 0;
            token_ind_y = 0;
            token_color = Color.White;
            token_alpha = 1f;
            token_value = 0;
            n_armor_chosen = 0;
            n_weapon_chosen = 0;
            n_shield_chosen = 0;
            n_key_chosen = 0;
            n_food_chosen = 0;
            n_potion_chosen = 0;
        }

        public void DrawHUD(SpriteBatch spriteBatch)
        {
            // draws HUD BG
            float hudLayer = 0.99995f;
            spriteBatch.Draw(hudsheet, hudPosition, hud_bg_sprite, Color.White, 0f, Vector2.Zero, hud_scale,
                                SpriteEffects.None, hudLayer);

            // draw token icon on HUD
            Rectangle source = new Rectangle(tilesize * token_ind_x + 1, tilesize * token_ind_y + 1, tilesize - 1, tilesize -1);
            spriteBatch.Draw(tileset, new Vector2(hudPosition.X + 20f * hud_scale, hudPosition.Y + 15f * hud_scale), source,
                            token_color * token_alpha, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);

            // draw token value info (if player has a token)
            if(player.tokens.Count > 0)
                spriteBatch.DrawString(HUDFont, token_value+" items", new Vector2(hudPosition.X + 20f * hud_scale, hudPosition.Y + 15f * hud_scale)
                                                        + new Vector2(23.5f * hud_scale, 24f * hud_scale), Color.White, 0f, Vector2.Zero,
                        hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw treasure icon o HUD
            Color c = Color.White; float a = 1f;
            if (player.n_treasures <= 0)
            {
                c = Color.Black;
                a = 0.5f;
            }
            spriteBatch.Draw(itemsheet, new Vector2(hudPosition.X + 20f * hud_scale, hudPosition.Y + 15f * hud_scale) + new Vector2(75f * hud_scale, 0),
                treasure_sprite, c * a, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);

            // draw treasures value in items info (if player has at least one treasure)
            if (player.n_treasures > 0)
                spriteBatch.DrawString(HUDFont, TreasureItemConversion() + " items", new Vector2(hudPosition.X + 20f * hud_scale, hudPosition.Y + 15f * hud_scale) + new Vector2(75f * hud_scale, 0)
                                                        + new Vector2(23.5f * hud_scale, 24f * hud_scale), Color.White, 0f, Vector2.Zero,
                        hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draws info about how many items player can select
            string infoMessage = "You can choose " + (n_items_next_level - n_items_selected) + " items";
            Vector2 infoPosition = hudPosition;
            float messageWidth = HUDFont.MeasureString(infoMessage).X * hud_scale * 0.30f;
            infoPosition += new Vector2((hud_bg_sprite.Width/2 + 6f * hud_scale) - messageWidth / 2, (75f * hud_scale)); 
            spriteBatch.DrawString(HUDFont, infoMessage, infoPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw weapon icon
            Vector2 weaponPosition = hudPosition + new Vector2(15f * hud_scale, (95f * hud_scale));
            spriteBatch.Draw(itemsheet, weaponPosition, weapon_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw weapon quantity bg
            Vector2 quantityBGPosition = weaponPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw weapon quantity
            Vector2 quantityPosition = quantityBGPosition;
            string quantityString = n_weapon_chosen.ToString();
            float quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw armor icon
            float distanceX = quantity_bg.Width * 0.5f * hud_scale + (22f * hud_scale);
            Vector2 armorPosition = weaponPosition + new Vector2(distanceX, 0f);
            spriteBatch.Draw(itemsheet, armorPosition, armor_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw armor quantity bg
            quantityBGPosition = armorPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw armor quantity
            quantityPosition = quantityBGPosition;
            quantityString = n_armor_chosen.ToString();
            quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw shield icon
            Vector2 shieldPosition = armorPosition + new Vector2(distanceX, 0f);
            spriteBatch.Draw(itemsheet, shieldPosition, shield_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw shield quantity bg
            quantityBGPosition = shieldPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw shield quantity
            quantityPosition = quantityBGPosition;
            quantityString = n_shield_chosen.ToString();
            quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw key icon
            float distanceY =  armor_sprite.Height * hud_scale + (25f * hud_scale);
            Vector2 keyPosition = weaponPosition + new Vector2(0f, distanceY);
            spriteBatch.Draw(itemsheet, keyPosition, key_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw key quantity bg
            quantityBGPosition = keyPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw key quantity
            quantityPosition = quantityBGPosition;
            quantityString = n_key_chosen.ToString();
            quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw food icon
            Vector2 foodPosition = weaponPosition + new Vector2(distanceX, distanceY);
            spriteBatch.Draw(itemsheet, foodPosition, food_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw food quantity bg
            quantityBGPosition = foodPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw food quantity
            quantityPosition = quantityBGPosition;
            quantityString = n_food_chosen.ToString();
            quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

            // draw key icon
            Vector2 potionPosition = armorPosition + new Vector2(distanceX, distanceY);
            spriteBatch.Draw(itemsheet, potionPosition, potion_sprite, Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            // draw key quantity bg
            quantityBGPosition = potionPosition + new Vector2(-5f * hud_scale, 34f * hud_scale);
            spriteBatch.Draw(hudsheet, quantityBGPosition, quantity_bg, Color.White, 0f, Vector2.Zero, hud_scale * 0.5f, SpriteEffects.None, hudLayer + 0.00001f);
            // draw key quantity
            quantityPosition = quantityBGPosition;
            quantityString = n_potion_chosen.ToString();
            quantityWidth = HUDFont.MeasureString(quantityString).X * hud_scale * 0.30f;
            quantityPosition += new Vector2((quantity_bg.Width * hud_scale * 0.5f) / 2 - (quantityWidth / 2), 0f);
            spriteBatch.DrawString(HUDFont, quantityString, quantityPosition, Color.White, 0f, Vector2.Zero,
                                            hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);


            // draws accessible next level button if player has token and has chosen all items
            // and unaccessible next level button otherwise
            if (is_next_level_accessible)
            {
                spriteBatch.Draw(hudsheet, nextLevelPosition, next_level_sprite,
                            Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            }
            else
            {
                spriteBatch.Draw(hudsheet, nextLevelPosition, next_level_not_sprite,
                    Color.White, 0f, Vector2.Zero, hud_scale, SpriteEffects.None, hudLayer + 0.00001f);
            }

            // draw next level string
            c = Color.White; a = 1f;
            if(!is_next_level_accessible)
            {
                c = Color.Black;
                a = 0.25f;
            }
            spriteBatch.DrawString(HUDFont, "Next Level", nextLevelPosition + new Vector2(17.5f*hud_scale,6f*hud_scale), c * a, 0f, Vector2.Zero,
                                    hud_scale * 0.30f, SpriteEffects.None, hudLayer + 0.00002f);

        }

        public int TreasureItemConversion()
        {
            return (int)Math.Round((player.n_treasures + 1) / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, float deltatime, float global_light)
        {
            if (active)
            {
                if (drawHUD)
                    DrawHUD(spriteBatch);

                /*** camera framing ****/
                float target_y = (float)Math.Round(position.Y) - camera.y + 0f * camera.zoom;
                float target_x = (float)Math.Round(position.X) - camera.x + 0f * camera.zoom;

                // draws token
                Vector2 totemPosition = new Vector2((float)Math.Round(target_x), (float)Math.Round(target_y));
                spriteBatch.Draw(tileset, totemPosition, sprite, Color.White * (global_light * 2), 0f, Vector2.Zero, camera.zoom,
                                    SpriteEffects.None, 0.1f + (0.0001f * target_y) + (0.00005f * target_x));
            }
            else
            {
                if (drawHUD == true)
                {
                    drawHUD = false;
                    UnsubscribeButtons();
                    ResetInfo();
                }
            }
        }
    }
}