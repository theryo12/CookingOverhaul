using CookingOverhaul.Networking;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CookingOverhaul.Content.Items.Hoe
{
    public abstract class BaseHoe : ModItem
    {
        public static readonly Dictionary<int, int> TileToSoil = new()
        {
            { TileID.Dirt, TileID.Dirt },
        };
        public abstract void PrePlow(ref int area);

        public override bool? UseItem(Player player)
        {
            int area = 1;
            PrePlow(ref area);

            PlowArea(area);
            return true;
        }

        private static void PlowArea(int area)
        {
            int cursorTileX = Player.tileTargetX;
            int cursorTileY = Player.tileTargetY;

            for (int x = cursorTileX - area; x <= cursorTileX + area; x++)
            {
                Tile tile = Main.tile[x, cursorTileY];

                if (tile.HasTile && TileToSoil.TryGetValue(tile.TileType, out int soilType))
                {
                    if (!TilePlowable(tile))
                    {
                        continue;
                    }


                    PlowAction(x, cursorTileY, soilType);
                }
            }
        }

        public static bool TilePlowable(Tile tile)
        {
            if (tile.LiquidAmount > 0 || tile.Slope > 0 || tile.IsHalfBlock || tile.TopSlope)
            {
                return false;
            }

            return true;
        }

        public static void PlowAction(int x, int y, int soilType)
        {
            WorldGen.KillTile(x, y, noItem: true);
            WorldGen.PlaceTile(x, y, soilType); 
        }
    }

    public class WoodenHoe : BaseHoe
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.PickaxeAxe;

        public override void PrePlow(ref int area)
        {
            area = 5;
        }
    }
}
