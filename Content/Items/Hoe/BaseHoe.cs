using CookingOverhaul.Networking;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CookingOverhaul.Content.Items.Hoe
{
    public abstract class BaseHoe : ModItem
    {
        private static readonly Dictionary<int, int> s_tileToSoil = new Dictionary< int, int >
        {
            { TileID.Dirt, TileID.Dirt },
        };

        protected abstract void PrePlow(ref int area);

        public override bool? UseItem(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null);
                return true;
            }

            var area = 1;
            PrePlow(ref area);

            PlowArea(area);
            return true;
        }

        private static void PlowArea(int area)
        {
            var cursorTileX = Player.tileTargetX;
            var cursorTileY = Player.tileTargetY;

            for (var x = cursorTileX - area; x <= cursorTileX + area; x++)
            {
                var tile = Main.tile[x, cursorTileY];

                if (!tile.HasTile || !s_tileToSoil.TryGetValue(tile.TileType, out var soilType))
                {
                    continue;
                }
                if (!TilePlowable(tile))
                {
                    continue;
                }


                PlowAction(x, cursorTileY, soilType);
            }
        }

        private static bool TilePlowable(Tile tile)
        {
            return tile is { LiquidAmount: <= 0, Slope: <= 0, IsHalfBlock: false, TopSlope: false };
        }

        private static void PlowAction(int x, int y, int soilType)
        {
            WorldGen.KillTile(x, y, noItem: true);
            WorldGen.PlaceTile(x, y, soilType); 
            
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, x, y, soilType);
            }
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

        protected override void PrePlow(ref int area)
        {
            area = 5;
        }
    }
}
