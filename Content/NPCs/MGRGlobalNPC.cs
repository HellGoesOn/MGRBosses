using MGRBosses.Content.Systems.BladeMode;
using MGRBosses.Content.Systems.Cinematic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public class MGRGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool PreAI(NPC npc)
        {
            if (CinematicScene.IsActor(npc, out var res))
                return !res.SequenceBlocksInput;

            return base.PreAI(npc);
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            base.OnHitByItem(npc, player, item, damage, knockback, crit);
            HurtTooMuch(npc);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            base.OnHitByProjectile(npc, projectile, damage, knockback, crit);
            HurtTooMuch(npc);
        }

        public void HurtTooMuch(NPC me)
        {
            if(me.life <= (int)(me.lifeMax * 0.1f)) {
                Weakspot.Create(me, Vector2.Zero, new Vector2(20));
            }
        }
    }
}
