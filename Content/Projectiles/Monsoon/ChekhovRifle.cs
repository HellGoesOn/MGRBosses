using MGRBosses.Content.NPCs;
using MGRBosses.Content.Players;
using MGRBosses.Content.Systems.Arenas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles.Monsoon
{
    // Looking kinda dogshit ngl
    // needs rewrite/cleanup?
    public class ChekhovRifle : ModProjectile
    {
        public override string Texture => "MGRBosses/Content/Textures/Monsoon/Pillar";

        private bool preparedForAttack;
        private bool hasPlayerDodged;
        private bool triedToBlendIn;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Metallic Pillar");
        }

        public float opacity, drawAngle;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.aiStyle = -1;
            Projectile.rotation = 0;

            opacity = -1;
            drawAngle = Projectile.rotation;
            Projectile.hide = true;
        }

        public override bool PreAI()
        {
            if (!triedToBlendIn) {
                triedToBlendIn = true;
                BossArena arena = BossArenaSystem.GetArenaByAlias("MonsoonArena");
                if (arena != null) {
                    arena.Participants.Add(Projectile);
                }
            }
            return base.PreAI();
        }

        public override void AI()
        {
            if (!Main.npc.Any(x => x.active && x.ModNPC is MonsoonBoss && x.whoAmI != Main.maxNPCs)) {
                Projectile.Kill();
                return;
            }

            BossArena arena = BossArenaSystem.GetArenaByAlias("MonsoonArena");
            if (arena != null) {
                if (Projectile.Bottom.Y + Projectile.velocity.Y >= arena.position.Y + arena.size.Y) {
                    Projectile.velocity *= 0.0f;
                }
            }
            NPC monsoon = Main.npc[(int)Projectile.knockBack];
            Player player = Main.LocalPlayer;

            if (preparedForAttack && Projectile.ai[1] < 250) {
                if (player.controlHook) {
                    hasPlayerDodged = true;
                    player.RemoveAllGrapplingHooks();
                    (monsoon.ModNPC as MonsoonBoss).magnetizedTime = 0;
                    Projectile.ai[1] = 249.5f;
                }
            }

            if (Projectile.ai[1] == 251f && hasPlayerDodged) {
                player.velocity.Y = -12f;
            }

            if (Projectile.ai[1] == 250) {
                if (!hasPlayerDodged)
                    Projectile.damage = 60000;

                Projectile.rotation = (Projectile.Center - Main.player[monsoon.target].Center).SafeNormalize(-Vector2.UnitY).ToRotation() + MathHelper.PiOver2;
                Main.LocalPlayer.GetModPlayer<MGRPlayer>().SetCameraTarget(Projectile.Center + new Vector2(0, 32), 0.2f, Projectile);
                Projectile.velocity = -new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) * 26f;
            }

            if (hasPlayerDodged && Projectile.velocity == Vector2.Zero && Projectile.ai[1] >= 251) {
                float dist = Vector2.Distance(monsoon.Center, player.Center);
                float horizontalDist = (float)Math.Abs(monsoon.Center.X - player.Center.X);

                if (horizontalDist > 40) {
                    var y = Math.Clamp(player.Center.Y, 0, Projectile.Center.Y - 300 + dist * 0.5f);
                    player.Center = new Vector2(player.Center.X, y);
                } else {
                    Projectile.Kill();
                }
            }

            if (opacity < 1)
                opacity += 0.05f;
            else if (Projectile.ai[1] <= 0)
                Projectile.ai[1] = 1;

            Projectile.timeLeft = 240;

            if (Projectile.ai[0] == 0) {
                Projectile.velocity.Y += 4f;
            }

            var colResult = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, false, false, 1);
            Projectile.velocity = colResult;

            if (colResult.Y == 0)
                Projectile.velocity.X *= 0;

            if (!preparedForAttack) {
                drawAngle = Projectile.rotation;
                Main.LocalPlayer.GetModPlayer<MGRPlayer>().SetCameraTarget(Projectile.Center + new Vector2(0, 32), 0.2f, Projectile);
                if (Projectile.ai[1] > 0 && Projectile.ai[1] < 40) {
                    Projectile.velocity.Y = 0;
                    Projectile.Center += new Vector2(Main.rand.Next(-4, 5), -Main.rand.Next(5));
                    Projectile.ai[1] += 0.5f;
                } else if (Projectile.ai[1] >= 40) {
                    float targetAngle = (Projectile.Center - Main.player[monsoon.target].Center).SafeNormalize(-Vector2.UnitY).ToRotation();
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, targetAngle - MathHelper.PiOver2, 0.025f);
                    Projectile.velocity.Y = 0;
                    Projectile.ai[0] = 20;
                    if (Projectile.Center.Y > monsoon.Center.Y)
                        Projectile.position.Y -= 2f;
                    else if (Vector2.Distance(Projectile.Center, monsoon.Center + new Vector2(-100 * -monsoon.direction, 0)) > 12)
                        Projectile.position.X -= 4f * -monsoon.direction;
                    else
                        preparedForAttack = true;
                }
                return;
            }

            Projectile.ai[1] += 0.5f;

            if (Projectile.ai[1] < 240) {
                drawAngle = Projectile.rotation;
                float targetAngle = (Projectile.Center - Main.player[monsoon.target].Center).SafeNormalize(-Vector2.UnitY).ToRotation();
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, targetAngle - MathHelper.PiOver2, 0.025f);
            }

            if (Projectile.ai[1] >= 240 && Projectile.ai[1] < 250) {
                drawAngle = Projectile.rotation;
                Projectile.Center += new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) * 1.22f;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            SpriteEffects effects = (int)(Projectile.velocity.X) % 4 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, drawAngle, new Vector2(59, 128), 1, effects, 1);
        }
    }
}
