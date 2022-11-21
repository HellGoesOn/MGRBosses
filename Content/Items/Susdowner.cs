using MGRBosses.Content.NPCs;
using MGRBosses.Content.NPCs.Sundowner;
using MGRBosses.Content.Systems.Arenas;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BladeMode.Content.Items
{
	public class Susdowner : ModItem
	{
        public override string Texture => "MGRBosses/Content/Textures/Items/Meme";

        public override void SetStaticDefaults()
        {
			Tooltip.SetDefault("Red Sus" +
				"\nSummons Sundowner");
        }

        public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
		}

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
			if (Main.npc.Any(x => x.ModNPC is MonsoonBoss))
				crit = 100;
        }

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.Register();
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(ModContent.NPCType<Sundowner>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				SoundEngine.PlaySound(SoundID.Roar, player.position);

				int type = ModContent.NPCType<Sundowner>();

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.SpawnOnPlayer(player.whoAmI, type);
				}
				else
				{
					NetMessage.SendData(MessageID.SpawnBoss, number: player.whoAmI, number2: type);
				}

			}

			return true;
		}
	}
}