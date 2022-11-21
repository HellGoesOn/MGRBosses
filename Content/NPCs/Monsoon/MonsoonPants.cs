using MGRBosses.Content.Buffs;
using MGRBosses.Content.Projectiles.Monsoon;
using MGRBosses.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public class MonsoonPants : ModNPC
    {
        private bool init;

        public override string Texture => "MGRBosses/Content/Textures/Monsoon/PH";

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = false;
            NPC.friendly = false;
            NPC.width = 26;
            NPC.height = 44;
            NPC.life = NPC.lifeMax = 10000;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.chaseable = false;
            speed = 8f;
        }

        public override bool PreAI()
        {
            if(!init) {
                init = true; 
                NPC.AddOnParryAction(() =>
                {
                    NPC.velocity.X = -NPC.direction * 2.4f;
                    DoParry(/*player*/);
                });
            }

            return base.PreAI();
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(true);
            }

            if (Main.player[NPC.target].dead)

            {
                NPC.Center -= new Vector2(0, 40);
                NPC.EncourageDespawn(10);
            }

            Attack_Direction = PlayerTarget.Center.X < NPC.Center.X ? -1 : 1;

            if (Attack_AimTime > 30)
            {
                NPC.velocity.X *= 0.25f;
            }

            if (DistanceFromTarget > 10000 && Attack_AimTime <= 30)
            {
                MoveTowardsPlayer();
                return;
            }

            if (DistanceFromTarget <= 10000 && Attack_AimTime > 20 && Attack_AimTime < 30)
                NPC.velocity.X *= 0f;

            if (Attack_AimTime == 24)
            {
                Attack_AttemptCount--;
                if (Attack_AttemptCount > 0)
                    MGRBosses.TriggerParry(NPC.Center + new Vector2(0, 10));
            }

            if (Attack_AimTime == 12)
            {
                NPC.damage = 8;
                float velY = ((PlayerTarget.Center - NPC.Center).SafeNormalize(-Vector2.UnitY) * 16).Y;
                float vel =  12f;
                NPC.velocity.X = vel * Attack_Direction;
                NPC.velocity.Y = velY;
            }

            NPC.direction = NPC.Center.X > PlayerTarget.Center.X ? -1 : 1;

            if (Attack_AimTime == 2)
            {
                NPC.velocity.X *= 0f;
                NPC.damage = 0;
            }

            if (Attack_AimTime > 0)
                Attack_AimTime--;
            else
            {
                Attack_AimTime = 32;
            }

            if (Attack_AttemptCount <= 0)
            {
                NPC.StrikeNPCNoInteraction(10000, 0f, 0, false, true);
            }
        }

        private void BlockDamage()
        {
            NPC.damage = 0;
            SoundEngine.PlaySound(SoundID.Item37, NPC.position);
            Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
        }

        private void DoParry()
        {
            BlockDamage();

            if (Attack_AttemptCount > 0)
                Attack_AimTime = 38;
            else
                Attack_AimTime = 2;

            NPC.velocity *= 0f;
        }

        private float speed;

        private void MoveTowardsPlayer()
        {
            int dir = PlayerTarget.Center.X < NPC.Center.X ? -1 : 1;

            NPC.velocity *= 0.5f;

            if (NPC.velocity.X < 0.01f)
                NPC.velocity.X = 0;

            if (DistanceFromTarget > 8000)
                NPC.velocity.X = speed * dir;

            NPC.velocity.Y = 8f;
        }

        private Player PlayerTarget => Main.player[NPC.target];

        private float DistanceFromTarget => Vector2.DistanceSquared(NPC.Center, PlayerTarget.Center);
        private float Attack_AttemptCount
        {
            get => NPC.ai[1];
            set
            {
                NPC.ai[1] = value;
            }
        }

        private float Attack_AimTime
        {
            get => NPC.ai[2];
            set
            {
                NPC.ai[2] = value;
            }
        }

        private float Attack_Direction
        {
            get => NPC.ai[3];
            set
            {
                NPC.ai[3] = value;
            }
        }
    }
}
