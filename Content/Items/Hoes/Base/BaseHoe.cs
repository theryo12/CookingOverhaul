using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CookingOverhaul.Content.Items.Hoes.Base {

    /// <summary>
    ///     Represents the base functionality for a modded hoe item.
    ///     Provides mechanisms for area-based tile manipulation and plowing functionality.
    /// </summary>
    public abstract class BaseHoe : ModItem
    {
        /// <summary>
        ///     A dictionary mapping specific tile types to their corresponding soil types.
        /// </summary>
        /// <remarks>
        ///     This dictionary is used to determine which tiles can be plowed and what type of soil they should become.
        ///     Extend this dictionary to support additional tile-to-soil transformations.
        /// </remarks>
        public static readonly Dictionary<int, int> TileToSoil = new Dictionary< int, int >
        {
            { TileID.Dirt, TileID.Adamantite },
        };
        
        /// <summary>
        ///     Gets the area of effect for this hoe.
        /// </summary>
        /// <remarks>
        ///     The area determines the horizontal extent of tiles that will be plowed, centered on the target tile.
        ///     For example, if this value is set to 1, the hoe will plow the target tile and one tile to the left
        ///     and right, resulting in a total width of 3 tiles being plowed. Derived classes must implement this
        ///     property to define their specific area of effect.
        /// </remarks>
        public abstract int Area { get; }
        
        /// <summary>
        ///     Gets the maximum range within which the player can plow tiles.
        /// </summary>
        /// <remarks>
        ///     This property determines how far (in tiles) the player can target tiles for plowing.
        ///     It does not affect the number of tiles plowed but limits the distance from the player's position
        ///     to the target tile that can be interacted with. Derived classes must implement this property
        ///     to define their specific range.
        /// </remarks>
        public abstract int Range { get; }
        
        /// <summary>
        ///     Executes any pre-processing logic before plowing tiles.
        /// </summary>
        /// <remarks>
        ///     Override this method in derived classes to implement custom behavior before the plow operation begins.
        /// </remarks>
        protected virtual void PrePlow() {}
        
        /// <summary>
        ///     Processes tiles within a specified area, applying an action to eligible tiles.
        /// </summary>
        /// <param name="area">The radius (in tiles) to process around the cursor position.</param>
        /// <param name="player">The player performing the action.</param>
        /// <param name="action">The action to apply to each eligible tile.</param>
        /// <remarks>
        ///     This method loops through all tiles within the specified area and applies the given action to each plowable tile.
        /// </remarks>
        private static void ProcessTiles(int area, int radius, Player player, Action<int, int, int> action)
        {
            var cursorTileX = Player.tileTargetX;
            var cursorTileY = Player.tileTargetY;

            for (var x = cursorTileX - area; x <= cursorTileX + area; x++)
            {
                var playerTilePos = player.Center.ToTileCoordinates();
                if (Vector2.Distance(new Vector2(x, cursorTileY), playerTilePos.ToVector2()) > radius)
                    continue;


                var tile = Main.tile[x, cursorTileY];

                if (!tile.HasTile || !TileToSoil.TryGetValue(tile.TileType, out var soilType) || !TilePlowable(tile))
                {
                    continue;
                }

                action(x, cursorTileY, soilType);
            }
        }
        
        /// <summary>
        ///     Determines whether a tile can be plowed.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if the tile is plowable; otherwise, false.</returns>
        /// <remarks>
        /// A tile is considered plowable if it has no liquid, no slope, and is not a half block or top slope.
        /// </remarks>
        public static bool TilePlowable(Tile tile)
        {
            return tile is { LiquidAmount: <= 0, Slope: <= 0, IsHalfBlock: false, TopSlope: false };
        }
        
        /// <summary>
        ///     The action performed to plow a tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <param name="soilType">The type of soil to place after plowing.</param>
        private static void PlowAction(int x, int y, int soilType)
        {
            WorldGen.KillTile(x, y, noItem: true);
            WorldGen.PlaceTile(x, y, soilType); 
            
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, x, y, soilType);
            }
        }
        
                
        public override bool? UseItem(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileManipulation);
                return true;
            }
            
            PrePlow();
            
            ProcessTiles(Area, Range, player, PlowAction);
            return true;
        }
    }
}
