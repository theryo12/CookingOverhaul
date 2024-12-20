using CookingOverhaul.Content.Items.Hoes.Base;
using Terraria.ID;
using Terraria.ModLoader;

namespace CookingOverhaul.Content.Items.Hoes
{
    public class WoodenHoe : BaseHoe
    {
        public override void SetDefaults()
        {
            Item.width        = 20;
            Item.height       = 20;
            Item.useTime      = 120;
            Item.useAnimation = 120;
            Item.damage       = 7;
            Item.useStyle     = ItemUseStyleID.Swing;
            Item.rare         = ItemRarityID.White;
            Item.UseSound     = SoundID.Item1;
            Item.autoReuse    = true;
        }

        public override void AddRecipes()
        {
            ModContent.GetInstance< WoodenHoe >().CreateRecipe()
                      .AddRecipeGroup(RecipeGroupID.Wood, 8)
                      .AddIngredient(ItemID.Cobweb)
                      .AddTile(TileID.WorkBenches)
                      .Register();
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.PickaxeAxe;

        public override int Area => 0;

        public override int Range  => 5;
    }
}
