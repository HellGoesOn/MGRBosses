using MGRBosses.Content.Systems.BladeMode;
using MGRBosses.Content.Systems.Cinematic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public class MGRGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Action OnParry;

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

        public override bool PreKill(NPC npc)
        {
            OnParry = null;

            return base.PreKill(npc);
        }

        public static void HurtTooMuch(NPC me)
        {
            if(me.life <= (int)(me.lifeMax * 0.1f)) {
                Weakspot.Create(me, Vector2.Zero, new Vector2(20));
            }
        }

        public static void DoOnParry(NPC target) => target.GetGlobalNPC<MGRGlobalNPC>().OnParry?.Invoke();

        public static void ClearOnParry(NPC target) => target.GetGlobalNPC<MGRGlobalNPC>().OnParry = null;
    }
}
