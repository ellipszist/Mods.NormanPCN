﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ToolBelt
{
    internal class ToolBeltMenu : IClickableMenu
    {
        private List<ToolBeltButton> buttons = new List<ToolBeltButton>();
        protected int buttonRadius;
        protected int lowestButtonY = -1;
        protected int animtime;
        protected int age;
        protected float animProgress = 0f;

        protected bool gamepadMode;
        protected bool moved = false;
        protected int moveAge = 0;
        protected int delay = 250;

        protected int wheelIndex = -1;

        IModHelper Helper;
        IMonitor Monitor;
        ModConfig Config;
        ModEntry Mod;

        public ToolBeltMenu(IModHelper helper, ModEntry mod, ModConfig config, IMonitor Monitor)
        {
            Helper = helper;
            Mod = mod;
            Config = config;
            this.Monitor = Monitor;

            animtime = Config.AnimationMilliseconds;
            buttonRadius = 26;

            width = 256;
            height = 256;
            xPositionOnScreen = (int)((float)(Game1.viewport.Width / 2) - (float)width / 2f);
            yPositionOnScreen = (int)((float)(Game1.viewport.Height / 2) - (float)height / 2f);
            snapToPlayerPosition();
        }

        public void updateToolList(SortedDictionary<Item, int> dict)
        {
            //essentially re init the menu
            gamepadMode = false;
            buttons.Clear();
            age = 0;
            animProgress = 0f;
            delay = 250;
            moved = false;

            foreach (KeyValuePair<Item, int> kv in dict)
            {
                buttons.Add(new ToolBeltButton(kv.Value, kv.Key, Helper));
                if (kv.Value == Game1.player.CurrentToolIndex)
                {
                    wheelIndex = buttons.Count - 1;
                    buttons[wheelIndex].select();
                }
            }

            snapToPlayerPosition();
        }

        public int closeAndReturnSelected()
        {
            exitThisMenu();
            if (wheelIndex < 0) return wheelIndex;
            return buttons[wheelIndex].getIndex();
        }

        public override void update(GameTime time)
        {
            age += time.ElapsedGameTime.Milliseconds;
            if (age > animtime)
            {
                animProgress = 1f;
            }
            else if (animtime > 0)
            {
                animProgress = (float)age / (float)animtime;
            }
            if (wheelIndex >= 0) buttons[wheelIndex].select();

            snapToPlayerPosition();
            Vector2 offset = default(Vector2);
            float xState = 0;
            float yState = 0;

            if (Config.LeftStickSelection)
            {
                xState = Game1.input.GetGamePadState().ThumbSticks.Left.X;
                yState = Game1.input.GetGamePadState().ThumbSticks.Left.Y;
            }
            else
            {
                xState = Game1.input.GetGamePadState().ThumbSticks.Right.X;
                yState = Game1.input.GetGamePadState().ThumbSticks.Right.Y;
            }

            if (!gamepadMode && Game1.options.gamepadControls && (Math.Abs(xState) > 0.5f || Math.Abs(yState) > 0.5f))
            {
                gamepadMode = true;
            }

            if (gamepadMode)
            {
                if (Config.HorizontalSelect)
                {
                    //horizontal selection style
                    if (wheelIndex <= 0) wheelIndex = 0;
                    if (!moved && xState > 0.7f)
                    {
                        moved = true;
                        moveAge = age;
                        buttons[wheelIndex].deSelect();
                        wheelIndex = (wheelIndex + 1) % buttons.Count;

                    }
                    if (!moved && xState < -0.7f)
                    {
                        moved = true;
                        moveAge = age;
                        buttons[wheelIndex].deSelect();
                        wheelIndex = wheelIndex == 0 ? buttons.Count - 1 : wheelIndex - 1;
                    }
                    if (moved && age - moveAge > delay)
                    {
                        moved = false;
                        delay = 80;
                    }

                    if (Math.Abs(xState) < 0.3f)
                    {
                        moved = false;
                        delay = 250;
                    }

                    Monitor.Log(
                        "\n update" +
                        "\n age - move age = " + (age - moveAge) +
                        "\n age = " + age +
                        "\n delay = " + delay, LogLevel.Trace);

                }
                else
                {
                    //radial selection style
                    if (Math.Abs(xState) > 0.5f || Math.Abs(yState) > 0.5f)
                    {
                        offset = new Vector2(xState, yState);
                        offset.Y *= -1f;
                        offset.Normalize();
                        float highest_dot = -1f;
                        int tempIndex = 0;
                        for (int j = 0; j < buttons.Count; j++)
                        {
                            float dot = Vector2.Dot(value2: new Vector2((float)buttons[j].bounds.Center.X - ((float)xPositionOnScreen + (float)width / 2f), (float)buttons[j].bounds.Center.Y - ((float)yPositionOnScreen + (float)height / 2f)), value1: offset);

                            if (dot > highest_dot)
                            {
                                highest_dot = dot;
                                tempIndex = j;

                            }
                        }
                        buttons[wheelIndex].deSelect();
                        wheelIndex = tempIndex;
                        buttons[wheelIndex].select();
                    }
                    else
                    {
                        Mod.swapItem(closeAndReturnSelected());
                    }
                }
            }

        }

        public override void performHoverAction(int x, int y)
        {
            if (gamepadMode)
            {
                return;
            }

            x = (int)Utility.ModifyCoordinateFromUIScale(x);
            y = (int)Utility.ModifyCoordinateFromUIScale(y);

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].containsPoint(x, y))
                {
                    wheelIndex = i;
                    buttons[wheelIndex].select();
                    return;
                }
                else
                {
                    buttons[i].deSelect();
                    wheelIndex = -1;
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            x = (int)Utility.ModifyCoordinateFromUIScale(x);
            y = (int)Utility.ModifyCoordinateFromUIScale(y);
            Mod.swapItem(closeAndReturnSelected());
            base.receiveLeftClick(x, y, playSound);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            wheelIndex = -1;
            receiveLeftClick(x, y, playSound);
        }

        protected void snapToPlayerPosition()
        {
            if (Game1.player != null)
            {
                Vector2 player_position = Game1.player.getLocalPosition(Game1.viewport) + new Vector2((float)(-width) / 2f, (float)(-height) / 2f);
                xPositionOnScreen = (int)player_position.X + 32;
                yPositionOnScreen = (int)player_position.Y - 64;
                if (xPositionOnScreen + width > Game1.viewport.Width)
                {
                    xPositionOnScreen -= xPositionOnScreen + width - Game1.viewport.Width;
                }
                if (xPositionOnScreen < 0)
                {
                    xPositionOnScreen -= xPositionOnScreen;
                }
                if (yPositionOnScreen + height > Game1.viewport.Height)
                {
                    yPositionOnScreen -= yPositionOnScreen + height - Game1.viewport.Height;
                }
                if (yPositionOnScreen < 0)
                {
                    yPositionOnScreen -= yPositionOnScreen;
                }

                if (Config.HorizontalSelect)
                {
                    yPositionOnScreen -= 3 * Game1.player.GetBoundingBox().Height;
                }
                repositionButtons();
            }
        }

        protected void repositionButtons()
        {
            if (Config.HorizontalSelect)
            {
                positionHorizontal();
            }
            else
            {
                positionRadial();
            }
        }

        private void positionRadial()
        {
            updateButtonRadius();
            lowestButtonY = -1;
            int x;
            int y;
            for (int i = 0; i < buttons.Count; i++)
            {
                ClickableTextureComponent button = buttons[i];
                float radians = Utility.Lerp(0f, (float)Math.PI * 2f, ((float)i / (float)buttons.Count));
                x = (int)((float)(xPositionOnScreen + width / 2 + (int)(Math.Cos(radians) * (double)buttonRadius) * 4) - (float)button.bounds.Width / 2f);
                y = (int)((float)(yPositionOnScreen + height / 2 + (int)((0.0 - Math.Sin(radians)) * (double)buttonRadius) * 4) - (float)button.bounds.Height / 2f);
                button.bounds.X = x;
                button.bounds.Y = y;
                if (lowestButtonY < y) lowestButtonY = y;
            }
        }
        private void positionHorizontal()
        {
            int x;
            int y = (yPositionOnScreen + height / 2);
            int buttonCount = buttons.Count;
            int spacing = 15;

            for (int i = 0; i < buttonCount; i++)
            {
                ClickableTextureComponent button = buttons[i];
                x = (xPositionOnScreen + width / 2) + (spacing + button.bounds.Width) * i;
                x = x - (((buttonCount * (button.bounds.Width + spacing)) - spacing) / 2);

                button.bounds.X = x;
                button.bounds.Y = y;
            }
        }

        private void updateButtonRadius()
        {
            if (buttons.Count > 6)
            {
                buttonRadius = buttons.Count * 5;
            }
            else
            {
                buttonRadius = 26;
            }
            buttonRadius = (int)(animProgress * (float)buttonRadius);

        }

        public override void draw(SpriteBatch b)
        {
            Game1.StartWorldDrawInUI(b);



            foreach (ToolBeltButton button in buttons)
            {
                button.draw(b, (float)Math.Pow(animProgress, 8), Config.UseBackdrop);
            }

            if (!gamepadMode)
            {
                Game1.mouseCursorTransparency = 1f;
                drawMouse(b);
            }


            if (animProgress >= 1 && wheelIndex >= 0)
            {
                if (Config.HorizontalSelect)
                {
                    SpriteText.drawStringWithScrollCenteredAt(b, buttons[wheelIndex].toolName(), xPositionOnScreen + width / 2, (yPositionOnScreen + height / 2) - buttons[wheelIndex].bounds.Height); ;
                }
                else
                {
                    SpriteText.drawStringWithScrollCenteredAt(b, buttons[wheelIndex].toolName(), xPositionOnScreen + width / 2, lowestButtonY + buttons[wheelIndex].bounds.Height + 20); ;
                }

            }
            Game1.EndWorldDrawInUI(b);
        }

        public override bool autoCenterMouseCursorForGamepad()
        {
            return false;
        }

    }
}