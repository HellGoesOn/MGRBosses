using MGRBosses.Content.NPCs;
using MGRBosses.Content.Projectiles.Weapons;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BladeMode.Content.Items
{
	public class EMGrenade : ModItem
	{
        public override string Texture => "MGRBosses/Content/Textures/Items/EMGrenade";

        public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("E.M. Grenade");
        }

        public override void SetDefaults()
		{
			Item.maxStack = 9999;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = 10000;
			Item.damage = 10;
			Item.rare = ItemRarityID.Green;
			Item.consumable = true;
			Item.shootSpeed = 16f;
			Item.shoot = ModContent.ProjectileType<EMGrenadeProjectile>();
		}

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.Register();
		}
	}
}