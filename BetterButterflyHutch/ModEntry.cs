﻿using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Helpers;
using System.Linq;
using StardewValley.Internal;
//using xTile.Dimensions;

namespace BetterButterflyHutch
{
    public class ModEntry : Mod
    {
        internal static ModConfig Config;
        internal static bool Debug;
        internal static Logger Log;

        private const int MaxMaxButterflies = 64;
        private const int MaxMinButterflies = MaxMaxButterflies;
        private const int MinBatWings = 10;
        private const int MaxBatWings = 200;
        //private const int HutchIdx = 1971;
        private const String HutchQID = "(F)1971";

        internal static Random Rand;

        internal IModHelper MyHelper;

        internal String I18nGet(String str)
        {
            return MyHelper.Translation.Get(str);
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            MyHelper = helper;
            Log = new Logger(this.Monitor);


            MyHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            MyHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            MyHelper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            Rand = new Random(DateTime.Now.Millisecond);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static void NormalizeMinMax()
        {
            if (Config.MinIndoors > Config.MaxIndoors)
                Config.MaxIndoors = Config.MinIndoors;
            if (Config.MinOutdoors > Config.MaxOutdoors)
                Config.MaxOutdoors = Config.MinOutdoors;
        }

        /// <summary>Raised after the game has loaded and all Mods are loaded. Here we load the config.json file and setup GMCM </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Config = MyHelper.ReadConfig<ModConfig>();
            Config.MinIndoors = Clamp(Config.MinIndoors, 0, MaxMinButterflies);
            Config.MaxIndoors = Clamp(Config.MaxIndoors, 1, MaxMaxButterflies);
            Config.MinOutdoors = Clamp(Config.MinOutdoors, 0, MaxMinButterflies);
            Config.MaxOutdoors = Clamp(Config.MaxOutdoors, 1, MaxMaxButterflies);
            NormalizeMinMax();
            Config.NumBatWings = Math.Min(MaxBatWings, Math.Max(MinBatWings, Config.NumBatWings));

            Debug = Config.Debug;
#if DEBUG
            Debug = true;
#endif

            var harmony = new Harmony(this.ModManifest.UniqueID);
            System.Reflection.MethodInfo mInfo;

            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Locations.Desert), nameof(StardewValley.Locations.Desert.OnDesertTrader)),
                                  postfix: new HarmonyMethod(typeof(DesertTraderPatches), nameof(DesertTraderPatches.OnDesertTrader_Postfix))
                                 );

            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Objects.Furniture), nameof(StardewValley.Objects.Furniture.actionOnPlayerEntryOrPlacement)),
                                    prefix: new HarmonyMethod(typeof(FurniturePatches), nameof(FurniturePatches.actionOnPlayerEntryOrPlacement_Prefix)),
                                    postfix: new HarmonyMethod(typeof(FurniturePatches), nameof(FurniturePatches.actionOnPlayerEntryOrPlacement_Postfix))
                                    );

            // use GMCM in an optional manner.

            //IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            var gmcm = Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (gmcm != null)
            {
                gmcm.Register(ModManifest,
                              reset: () => Config = new ModConfig(),
                              save: () => Helper.WriteConfig(Config));

                gmcm.AddNumberOption(ModManifest,
                                     () => Config.MinIndoors,
                                     (int value) => { Config.MinIndoors = value; NormalizeMinMax(); },
                                     () => I18nGet("minIndoors.Label"),
                                     () => I18nGet("minIndoors.Tooltip"),
                                     min: 0,
                                     max: MaxMinButterflies);

                gmcm.AddNumberOption(ModManifest,
                                     () => Config.MaxIndoors,
                                     (int value) => { Config.MaxIndoors = value; NormalizeMinMax(); },
                                     () => I18nGet("maxIndoors.Label"),
                                     () => I18nGet("maxIndoors.Tooltip"),
                                     min: 1,
                                     max: MaxMaxButterflies);

                gmcm.AddNumberOption(ModManifest,
                                     () => Config.MinOutdoors,
                                     (int value) => { Config.MinOutdoors = value; NormalizeMinMax(); },
                                     () => I18nGet("minOutdoors.Label"),
                                     () => I18nGet("minOutdoors.Tooltip"),
                                     min: 0,
                                     max: MaxMinButterflies);

                gmcm.AddNumberOption(ModManifest,
                                     () => Config.MaxOutdoors,
                                     (int value) => { Config.MaxOutdoors = value; NormalizeMinMax(); },
                                     () => I18nGet("maxOutdoors.Label"),
                                     () => I18nGet("maxOutdoors.Tooltip"),
                                     min: 1,
                                     max: MaxMaxButterflies);

                gmcm.AddNumberOption(ModManifest,
                                     () => Config.NumBatWings,
                                     (int value) => Config.NumBatWings = value,
                                     () => I18nGet("numBatWings.Label"),
                                     () => I18nGet("numBatWings.Tooltip"),
                                     min: MinBatWings,
                                     max: MaxBatWings,
                                     interval: 10);

                gmcm.AddBoolOption(ModManifest,
                                   () => Config.WinterButterflies,
                                   (bool value) => Config.WinterButterflies = value,
                                   () => I18nGet("winterButterflies.Label"),
                                   () => I18nGet("winterButterflies.Tooltip"));

                gmcm.AddBoolOption(ModManifest,
                                   () => Config.IslandButterflies,
                                   (bool value) => Config.IslandButterflies = value,
                                   () => I18nGet("islandButterflies.Label"),
                                   () => I18nGet("islandButterflies.Tooltip"));

                //gmcm.AddBoolOption(ModManifest,
                //                   () => Config.ShakeHutch,
                //                   (bool value) => Config.ShakeHutch = value,
                //                   () => I18nGet("shakeHutch.Label"),
                //                   () => I18nGet("shakeHutch.Tooltip"));

                gmcm.AddBoolOption(ModManifest,
                                   () => Config.NightButterflies,
                                   (bool value) => Config.NightButterflies = value,
                                   () => I18nGet("nightButterflies.Label"),
                                   () => I18nGet("nightButterflies.Tooltip"));
            }
            else
            {
                Log.LogOnce("Generic Mod Config Menu not available.");
            };
        }

        /// <summary>Raised after a game save is loaded. Here we hook into necessary events for gameplay.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            MyHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
            MyHelper.Events.Player.Warped += Player_Warped;
        }

        /// <summary>Raised after a game has exited a game/save to the title screen.  Here we unhook our gameplay events.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            MyHelper.Events.Input.ButtonPressed -= Input_ButtonPressed;
            MyHelper.Events.Player.Warped -= Player_Warped;
        }

        private static bool IsHutchAtTile(GameLocation location, Vector2 tile)
        {
            foreach (StardewValley.Object obj in location.furniture)
            {
                if (
                    (obj.QualifiedItemId == HutchQID) &&
                    (obj.boundingBox.Y / Game1.tileSize == tile.Y) &&
                    (
                     (obj.boundingBox.X / Game1.tileSize == tile.X) ||
                     (((obj.boundingBox.X / Game1.tileSize)+1) == tile.X)
                    )
                   )
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CanSpawn(GameLocation loc)
        {
            if (loc.IsOutdoors)
            {
                // the game hutch code will
                //     spawn butterfies in the rain or snow or wind debris.
                //     spawn after dark
                //     spawn any season
                // Is...Here method return true for the Desert when it is raining/etc in the valley/town.
                // the Desert LocationContext is the same as valley/town. there are really only two contexts. valley/town and island.
                // we spawn in more sensible conditions.

                bool island = loc.InIslandContext();
                bool desert = loc.InDesertContext();
                bool isClear = !(Game1.IsRainingHere(loc) || Game1.IsLightningHere(loc) || Game1.IsSnowingHere(loc) || Game1.IsDebrisWeatherHere(loc));

                bool spawn = island || desert || (!Game1.currentSeason.Equals("winter", StringComparison.Ordinal) || Config.WinterButterflies);
                spawn = spawn && (isClear || desert);
                spawn = spawn && !Game1.isStartingToGetDarkOut(loc);

                return spawn;
            }
            else
            {
                if (Config.NightButterflies || !Game1.isStartingToGetDarkOut(loc))
                    return true;
                return false;
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            Vector2 tile = e.Cursor.GrabTile;
            GameLocation location = Game1.currentLocation;

            if (
                Context.IsPlayerFree &&
                SButtonExtensions.IsActionButton(e.Button) &&
                (location.furniture.Count > 0) &&
                IsHutchAtTile(location, tile)
                )
            {
                location.playSound("leafrustle");

                if (CanSpawn(location))
                {
                    bool island = location.InIslandContext();
                    if (!location.IsOutdoors && Config.IslandButterflies)
                        island = true;

                    do
                    {
                        Vector2 v = new Vector2(tile.X + (float)Game1.random.Next(1, 3), tile.Y - 2f + (float)Game1.random.Next(-1, 2));
                        location.addCritter(new Butterfly(location, v, islandButterfly: island, forceSummerButterfly: false).setStayInbounds(true));
                    } while (Game1.random.NextDouble() < 0.8);
                }
            }
        }

        private static int CountButterflies(GameLocation loc)
        {
            int count = 0;
            if (loc.critters != null)
            {
                for (int i = 0; i < loc.critters.Count; i++)
                {
                    if (loc.critters[i] is Butterfly)
                        count++;
                }
            }
            return count;
        }

        private static void RemoveButterflies(GameLocation loc)
        {
            // remove hutch spawned butterflies in instances we think they should not spawn
            // outdoors, the game will not spawn ambient butterfies in these conditions. so just remove all butterflies.
            // indoors, we remove after dark by config.
            // with Config island butterfly, then we may remove hutch spawned so we can replace with island.

            if (Debug)
                Log.Debug($"Remove Butterflies. critters={loc.critters.Count}");

            for (int i = loc.critters.Count - 1; i >= 0; i--)
            {
                if (loc.critters[i] is Butterfly)
                {
                    loc.critters.RemoveAt(i);
                    if (Debug)
                        Log.Debug($"    Remove Butterfly idx={i}");
                }
            }

        }

        private static void SpawnButterflies(GameLocation loc, int hutchCount, Rectangle? boundingBox)
        {
            // if the hutch did not spawn anything, then we will not
            if (hutchCount > 0)
            {
                bool island = loc.InIslandContext();
                int min = 0;
                int max = 0;

                if (!loc.IsOutdoors)
                {
                    if (Config.MinIndoors > 0)
                    {
                        if (CanSpawn(loc))
                        {
                            if (hutchCount < Config.MinIndoors)
                                min = Config.MinIndoors - hutchCount;
                            max = Config.MaxIndoors - hutchCount;
                            if (Config.IslandButterflies)
                            {
                                island = true;
                                RemoveButterflies(loc);//remove hutch spawned butterflies. they will not be island.
                                min = Config.MinIndoors;
                                max = Config.MaxIndoors;
                            }
                        }
                        else
                        {
                            RemoveButterflies(loc);
                        }
                    }
                }
                else
                {
                    if (Config.MinOutdoors > 0)
                    {
                        if (CanSpawn(loc))
                        {
                            if (hutchCount < Config.MinOutdoors)
                                min = Config.MinOutdoors - hutchCount;
                            max = Config.MaxOutdoors - hutchCount;
                        }
                        else
                        {
                            RemoveButterflies(loc);
                        }
                    }
                }
                max = Math.Max(max, 0);// possible for max to go <= 0 if the game spawns a ton of butterflies

                if (max > 0)
                {
                    int spawnCount = Rand.Next(min, max + 1);//result always < upper value.
                    if (Debug)
                    {
                        int x = -1;
                        int y = -1;
                        if (boundingBox.HasValue)
                        {
                            x = boundingBox.Value.X / Game1.tileSize;
                            y = boundingBox.Value.Y / Game1.tileSize;
                        }
                        Log.Debug($"Butterfly spawns={spawnCount}, hutchSpawned={hutchCount}, HutchAt={x},{y}");
                    }

                    for (int i = 0; i < spawnCount; i++)
                        loc.addCritter(new Butterfly(loc, loc.getRandomTile(), islandButterfly: island, forceSummerButterfly: false).setStayInbounds(true));
                }
            }
            //else
            //{
            //    if (Debug)
            //        Log.Debug("Hutch did not spawn butterflies");
            //}
        }

        /// <summary>Raised just after the player changes location. Here we spawn our Butterflies.
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (Context.IsPlayerFree)
            {
                GameLocation loc = e.NewLocation;

                // we can't distinguish from ambient and hutch spawns. this only matters outdoors.
                // if there are multiple hutches, we cannot distinguish spawns from individual hutches
                int count = CountButterflies(loc);

                foreach (StardewValley.Object obj in loc.furniture)
                {
                    if (obj.QualifiedItemId == HutchQID)
                    {
                        if (Debug)
                            Log.Debug($"Found Hutch at {loc.Name}, Outdoors={loc.IsOutdoors}, Game Butterflies={count}");

                        SpawnButterflies(loc, count, null);
                        return;
                    }
                }
            }
        }

        public class FurniturePatches
        {
            private static int Before;

            public static bool actionOnPlayerEntryOrPlacement_Prefix(StardewValley.Objects.Furniture __instance, GameLocation environment, bool dropDown)
            {
                try
                {
                    if ((__instance.QualifiedItemId == HutchQID) && !dropDown)
                        Before = CountButterflies(environment);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed in {nameof(actionOnPlayerEntryOrPlacement_Prefix)}:\n{ex}");
                    return true;
                }
            }

            public static void actionOnPlayerEntryOrPlacement_Postfix(StardewValley.Objects.Furniture __instance, GameLocation environment, bool dropDown)
            {
                try
                {
                    if ((__instance.QualifiedItemId == HutchQID) && !dropDown)
                        SpawnButterflies(environment, CountButterflies(environment) - Before, __instance.boundingBox.Value);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed in {nameof(actionOnPlayerEntryOrPlacement_Postfix)}:\n{ex}");
                }
            }
        }

        public class DesertTraderPatches
        {
            public static void OnDesertTrader_Postfix(StardewValley.Locations.Desert __instance)
            {
                try
                {
                    if (Game1.activeClickableMenu is ShopMenu shop)
                    {
                        foreach (var item in shop.forSale)
                        {
                            if (item.Name.Equals("Butterfly Hutch", StringComparison.OrdinalIgnoreCase))
                            {
                                var itemPriceAndStock = shop.itemPriceAndStock;
                                var stock = itemPriceAndStock[item];
                                stock.TradeItemCount = Config.NumBatWings;
                                itemPriceAndStock[item] = stock;
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed in {nameof(OnDesertTrader_Postfix)}:\n{ex}");
                }
            }
        }
    }
}

