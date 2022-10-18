using MGRBosses.Content.NPCs;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BladeMode.Content.Items
{
	public class Murasama : ModItem
	{
        public override string Texture => "MGRBosses/Content/Textures/Items/Murasama";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("VT-7 HF Blade");
		}

        public override void SetDefaults()
		{
			Item.damage = 40;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
			if (Main.npc.Count(x => x.ModNPC is MonsoonBoss && x.active) > 0)
				crit = 100;
        }

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.Register();
		}
    }
}