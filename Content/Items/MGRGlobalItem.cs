using BladeMode.Content.Items;
using MGRBosses.Content.Buffs;
using MGRBosses.Content.Projectiles.Monsoon;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.Items
{
    public class MGRGlobalItem : GlobalItem
    {
        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            /*
            List<Projectile> projectiles = Main.projectile.Where(x => x.ModProjectile is Wreckage && x.active).ToList();
            
            if(player.HasBuff<ParryBuff>())
            foreach(Projectile proj in projectiles)
            {
                if(hitbox.Intersects(proj.Hitbox))
                {
                        int dropItemType = ModContent.ItemType<EMGrenade>();
                        int newItem = Item.NewItem(proj.GetSource_DropAsItem(), proj.Hitbox, dropItemType);
                        Main.item[newItem].noGrabDelay = 0;

                        if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
                        }
                        proj.Kill();
                }
            }*/
        }
    }
}
