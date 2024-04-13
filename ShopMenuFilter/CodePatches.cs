﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ShopMenuFilter
{
    public partial class ModEntry
    {
        private static string lastFilterString = "";
        private static TextBox filterField = null;
        private static List<ISalable> allItems = null;

        // new Type[] { typeof(string), typeof(ShopData), typeof(ShopOwnerData), typeof(NPC), typeof(Func<ISalable, Farmer, int, bool>), typeof(Func<ISalable, bool>), typeof(bool)}
        // new Type[] {typeof(string), typeof(Dictionary<ISalable, ItemStockInformation>), typeof(int), typeof(string), typeof(Func<ISalable, Farmer, int, bool>), typeof(Func<ISalable, bool>), typeof(bool)}
        // new Type[] {typeof(string), typeof(List<ISalable>), typeof(int), typeof(string), typeof(Func<ISalable, Farmer, int, bool>), typeof(Func<ISalable, bool>), typeof(bool)}

        public static void Shopmenu_Constructor_Postfix(ShopMenu __instance)
        {
            lastFilterString = "";
            filterField = null;
            allItems = null;

            if (Config.ModEnabled)
            {
                allItems = new List<ISalable>(__instance.forSale);
                filterField = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = __instance.xPositionOnScreen + 28,
                    Y = __instance.yPositionOnScreen + __instance.height - 88,
                    Text = "",
                };
            }
        }

        public static void ShopMenu_applyTab_Postfix(ShopMenu __instance)
        {
            //can and will be called before Constructor Postfix method is called. constructor calls applytab.
            if (Config.ModEnabled && filterField != null)
            {
                filterField.Text = "";
                allItems = new List<ISalable>(__instance.forSale);
            }
        }

        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.gameWindowSizeChanged))]
        public class ShopMenu_gameWindowSizeChanged_Patch
        {
            public static void Postfix(ShopMenu __instance, Rectangle oldBounds, Rectangle newBounds)
            {
                if (Config.ModEnabled && (filterField != null))
                {
                    filterField.X = __instance.xPositionOnScreen + 28;
                    filterField.Y = __instance.yPositionOnScreen + __instance.height - 88;
                }
            }
        }

        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.drawCurrency))]
        public class ShopMenu_drawCurrency_Patch
        {

            public static void Postfix(ShopMenu __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                if (lastFilterString != filterField.Text)
                {
                    lastFilterString = filterField.Text;

                    foreach (var i in __instance.forSale)
                    {
                        if (!allItems.Contains(i))
                            allItems.Add(i);
                    }
                    for (int i = allItems.Count - 1; i >= 0; i--)
                    {
                        if (!__instance.itemPriceAndStock.ContainsKey(allItems[i]))
                            allItems.RemoveAt(i);
                    }
                    __instance.forSale.Clear();
                    if (filterField.Text == "")
                    {
                        __instance.forSale.AddRange(allItems);
                        return;
                    }
                    foreach (var i in allItems)
                    {
                        if (__instance.itemPriceAndStock.ContainsKey(i) && i.DisplayName.ToLower().Contains(filterField.Text.ToLower()))
                            __instance.forSale.Add(i);
                    }
                    __instance.currentItemIndex = 0;

                    //__instance.gameWindowSizeChanged(Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.graphics.GraphicsDevice.Viewport.Bounds);
                }
                filterField.Draw(b);
                SpriteText.drawStringHorizontallyCenteredAt(b, SHelper.Translation.Get("filter"), __instance.xPositionOnScreen + 128, __instance.yPositionOnScreen + __instance.height - 136, 999999, -1, 999999, 1, 0.88f, false, Config.LabelColor, 99999);
            }
        }
        
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.receiveLeftClick))]
        public class ShopMenu_receiveLeftClick_Patch
        {
            public static void Postfix(ShopMenu __instance)
            {
                if (Config.ModEnabled && (filterField != null))
                    filterField.Update();
            }
        }

        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.receiveKeyPress))]
        public class ShopMenu_receiveKeyPress_Patch
        {
            public static bool Prefix(ShopMenu __instance, Keys key)
            {
                if (Config.ModEnabled && (filterField != null))
                {
                    if (!filterField.Selected || key == Keys.Escape)
                        return true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.performHoverAction))]
        public class ShopMenu_performHoverAction_Patch
        {
            public static void Postfix(ShopMenu __instance, int x, int y)
            {
                if (Config.ModEnabled && (filterField != null))
                    filterField.Hover(x, y);
            }
        }
    }
}