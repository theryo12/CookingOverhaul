using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CookingOverhaul.Content.Items.Hoes.Base
{
    public class HoePreviewDrawLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.HeldItem.ModItem is BaseHoe;
        }

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeldItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;

            if (player.HeldItem.ModItem is not BaseHoe hoe)
                return;

            var cursorTileX = Player.tileTargetX; 
            var cursorTileY = Player.tileTargetY;

            for (var x = cursorTileX - hoe.Area; x <= cursorTileX + hoe.Area; x++)
            {
                if (Vector2.Distance(new Vector2(x, cursorTileY), player.Center.ToTileCoordinates().ToVector2()) > hoe.Area)
                    continue;

                var tile = Main.tile[x, cursorTileY];

                if (!tile.HasTile || !BaseHoe.TileToSoil.TryGetValue(tile.TileType, out _) || !BaseHoe.TilePlowable(tile))
                    continue;

                var screenPos = new Vector2(x * 16, cursorTileY * 16) - Main.screenPosition;
                var rect      = new Rectangle((int)screenPos.X, (int)screenPos.Y, 16, 16);

                drawInfo.DrawDataCache.Add(new DrawData(
                                                        TextureAssets.MagicPixel.Value,
                                                        rect,
                                                        null,
                                                        new Color(255, 255, 0, 128), // Semi-transparent fill
                                                        0f,
                                                        Vector2.Zero,
                                                        SpriteEffects.None
                                                       ));

                // Outline
                drawInfo.DrawDataCache.Add(new DrawData(
                                                        TextureAssets.MagicPixel.Value,
                                                        new Rectangle(rect.X, rect.Y, rect.Width, 1),
                                                        null,
                                                        Color.Yellow,
                                                        0f,
                                                        Vector2.Zero,
                                                        SpriteEffects.None
                                                       ));

                drawInfo.DrawDataCache.Add(new DrawData(
                                                        TextureAssets.MagicPixel.Value,
                                                        new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1),
                                                        null,
                                                        Color.Yellow,
                                                        0f,
                                                        Vector2.Zero,
                                                        SpriteEffects.None
                                                       ));

                drawInfo.DrawDataCache.Add(new DrawData(
                                                        TextureAssets.MagicPixel.Value,
                                                        new Rectangle(rect.X, rect.Y, 1, rect.Height),
                                                        null,
                                                        Color.Yellow,
                                                        0f,
                                                        Vector2.Zero,
                                                        SpriteEffects.None
                                                       ));

                drawInfo.DrawDataCache.Add(new DrawData(
                                                        TextureAssets.MagicPixel.Value,
                                                        new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height),
                                                        null,
                                                        Color.Yellow,
                                                        0f,
                                                        Vector2.Zero,
                                                        SpriteEffects.None
                                                       ));
            }
        }
    }
}
