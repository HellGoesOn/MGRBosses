﻿using MGRBosses.Content.Systems.Arenas;
using MGRBosses.Content.Systems.BladeMode;
using MGRBosses.Content.Systems.Cinematic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using MGRBosses.Core;
using MGRBosses.Content.Projectiles.Monsoon;
using System.Collections.Generic;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        enum AttacksFrom
        {
            Left,
            TopLeft,
            TopRight,
            Right
        }

        public AIState state;
        public float rotation;

        private static float DifficultyScale {
            get {
                if (Main.masterMode)
                    return 5;
                if (Main.expertMode)
                    return 2.5f;
                return 1f;
            }
        }

        private readonly int CURRENT_AIM_TIME_MAX = 80;
        private readonly string[] smokeQuotes = new string[] {
                "HAVE A SMOKE!",
            "I HOPE YOU CHOKE!",
            "WHO KNOWS WHERE I’LL COME FROM?",
            "CATCH ME IF YOU CAN"
            };
        private readonly AIState[] attacks = new AIState[]
        {
                AIState.SmokePrepare,
                AIState.AttackChain,
                AIState.AttackChain,
                AIState.AttackChain,
                AIState.Idle
        };

        private int magneticSwitchCounter;
        private int arenaId;
        private int nextMoveDelayPlusOne = 30;
        private int currentAttack;
        private bool instaKill;
        private bool firstPhase;
        private bool initializedArena;
        private bool switchedToPhase2;
        private float speed = 4f;
        private float monsoonOpacity = 1f;

        public override string Texture => "MGRBosses/Content/Textures/Monsoon/PH";

        private Player PlayerTarget => Main.player[NPC.target];

        private float DistanceFromTarget => Vector2.DistanceSquared(NPC.Center, PlayerTarget.Center);
        private float Attack_AttemptCount {
            get => NPC.ai[1];
            set {
                NPC.ai[1] = value;
            }
        }

        private float Attack_AimTime {
            get => NPC.ai[2];
            set {
                NPC.ai[2] = value;
            }
        }

        private float Attack_Direction {
            get => NPC.ai[3];
            set {
                NPC.ai[3] = value;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Monsoon");
        }

        public override void SetDefaults()
        {
            animations = new Dictionary<string, SpriteAnimation>();
            InitDrawing();
            NPC.npcSlots = 200f;
            NPC.noGravity = false;
            NPC.friendly = false;
            NPC.width = 90;
            NPC.height = 82;
            NPC.life = NPC.lifeMax = 10000;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.ai[0] = 0;
            NPC.boss = true;
            NPC.chaseable = false;
            switchedToPhase2 = false;
            firstPhase = false;

            totalProjectileCount = 0;
            currentThrownProjectileCount = 0;
            magneticSwitchCounter = 0;
            beganTenPercentAttack = false;
            pantsId = -1;
        }

        public override void OnKill()
        {
            Main.StopRain();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (state == AIState.SmokeAttack)
                return false;

            return base.DrawHealthBar(hbPosition, ref scale, ref position);
        }

        public override bool PreAI()
        {
            if(!initializedArena && NPC.HasPlayerTarget) {
                NPC.AddOnParryAction(() =>
                {
                    if (state == AIState.SmokeAttack) {
                        Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
                        Attack_AimTime = (float)(CURRENT_AIM_TIME_MAX * 0.25);
                    }

                    if (state == AIState.AttackChain) {
                        NPC.velocity.X = -NPC.direction * 2.4f;
                        DoParry(/*player*/);
                    }

                    if (state == AIState.MagneticSpin) {
                        BlockDamage();

                    }
                });
                initializedArena = true;

                var targetData = NPC.GetTargetData();
                arenaId = BossArenaSystem.CreateArena(new(targetData.Position + targetData.Size - new Vector2(1200, 1600), new Vector2(2400, 800), NPC, Main.player[NPC.target]) { Alias = "MonsoonArena"});
                var cutscene = CinematicSystem.AddCinematicScene();
                cutscene.Actors.Add(NPC);
                foreach(Player plr in Main.player.Where(x => x.whoAmI != 255)) {
                    cutscene.Actors.Add(plr);
                }

                Main.StartRain();
                var arena = BossArenaSystem.GetArenaByAlias("MonsoonArena");
                cutscene.screenPosition = arena.Center + new Vector2(0, 240);
                cutscene.AddSequence(60, () => {
                    NPC.position = arena.position + NPC.Size;
                    NPC.velocity *= 0;
                });
                cutscene.AddSequence(1, () =>
                {
                    Main.NewText("How pleased are you to chop away, Jack the Woodchipper?");
                });
                cutscene.AddSequence(180, () =>
                {
                    NPC.position = arena.position + NPC.Size;
                    NPC.velocity *= 0;
                });
                cutscene.AddSequence(60, () =>
                {
                    cutscene.Actors.Where(x => x is Player).ToList().ForEach(x => x.direction = -1);
                    cutscene.screenPosition += (arena.Center + new Vector2(-180, 60) - cutscene.screenPosition) * 0.025f;
                });
                cutscene.AddSequence(60, () =>
                {
                    cutscene.Actors.Where(x => x is Player).ToList().ForEach(x => x.direction = 1);
                    cutscene.screenPosition += (arena.Center + new Vector2(180, 60) - cutscene.screenPosition) * 0.025f;
                });
                cutscene.AddSequence(180, () =>
                {
                    cutscene.Actors.Where(x => x is Player).ToList().ForEach(x => x.direction = -1);
                    NPC.position = arena.position + NPC.Size;
                    NPC.velocity *= 0;
                    cutscene.screenPosition += (NPC.Center - cutscene.screenPosition) * 0.05f;
                    });
                cutscene.AddSequence(1, () =>
                {
                    Main.NewText("But enough cock rating, time for the action test");
                });
                cutscene.AddSequence(180, () =>
                {
                    NPC.velocity *= 0.98f;
                    cutscene.screenPosition += (NPC.Center - cutscene.screenPosition) * 0.05f;
                });
                cutscene.AddSequence(120, () =>
                {
                    cutscene.screenPosition += (Main.LocalPlayer.Center - cutscene.screenPosition) * 0.05f;
                }, false);
            }

            return base.PreAI();
        }

        public override void AI()
        {
            base.AI();

            if (pantsId != -1 && (!Main.npc[pantsId].active || Main.npc[pantsId].life <= 0))
                pantsId = -1;

            if (state != AIState.SmokeAttack && fogDensity > 0)
                fogDensity -= 0.025f;

            #region boring stuff
            NPC.noTileCollide = false;
            NPC.netAlways = true;
            NPC.netUpdate = true;

            NPC.velocity.Y += 0.38f;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                NPC.TargetClosest(true);
            }

            NPC.direction = NPC.Center.X > PlayerTarget.Center.X ? -1 : 1;

            if (PlayerTarget.dead) {
                monsoonOpacity = 0f;
                NPC.Center -= new Vector2(0, 40);
                NPC.EncourageDespawn(10);
            }


            if (NPC.life >= NPC.lifeMax * 0.4 && !firstPhase) {
                firstPhase = true;

                if (!Main.dedServ) {
                    Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/StainsOfTimeInstrumental");
                }

                switchedToPhase2 = false;
            }
            if (NPC.life <= NPC.lifeMax * 0.4 && !switchedToPhase2) {
                switchedToPhase2 = true;

                if (!Main.dedServ) {
                    Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/StainsOfTime3");
                }
            }
            #endregion

            if (IsMagnetized) {

                if (!Weakspot.ExistsFor(NPC)) {
                    var spot = Weakspot.Create(NPC, new(-8, -16), new(16));
                    spot.OnWeakspotCut += delegate (Entity owner)
                    {
                        magnetizedTime = 0;
                    };
                }
                magnetizedTime--;
            }
            if (NPC.life < (int)(NPC.lifeMax * 0.69f) && magneticSwitchCounter <= 0) {
                magneticSwitchCounter++;
                ForceMagneticAttack();
            }

            if (NPC.life < (int)(NPC.lifeMax * 0.55f) && magneticSwitchCounter <= 1) {
                magneticSwitchCounter++;
                ForceMagneticAttack();
            }

            if (NPC.life < (int)(NPC.lifeMax * 0.4f) && magneticSwitchCounter <= 2) {
                magneticSwitchCounter++;
                ForceMagneticAttack();
            }

            if (NPC.life < (int)(NPC.lifeMax * 0.1f) && magneticSwitchCounter <= 3) {
                magneticSwitchCounter++;

                state = AIState.TenPercentLeft;
            }

            if (NPC.HasPlayerTarget) {
                switch (state) {
                    case AIState.Spawn:
                        state = AIState.Run;
                        NPC.ai[0] = 5f;
                        NPC.Center = new Vector2(PlayerTarget.Center.X + 700 * PlayerTarget.direction, PlayerTarget.Center.Y - 16);
                        break;
                    case AIState.Idle:
                        Idle();
                        MoveTowardsPlayer();
                        break;
                    case AIState.Run:
                        Run();
                        MoveTowardsPlayer();
                        break;
                    case AIState.SmokePrepare:
                        SmokePrepare();
                        break;
                    case AIState.SmokeAttack:
                        SmokeAttack();
                        break;
                    case AIState.AttackChain:
                        AttackChain();
                        break;
                    case AIState.Retreat:
                        Retreat();
                        break;
                    case AIState.MagneticWreckageThrowAttack:
                        ThrowAttack();
                        break;
                    case AIState.TenPercentLeft:
                        TenPercentRule();
                        break;
                    case AIState.MagneticSpin:
                        MagneticSpin();
                        break;
                    case AIState.PantsAttack:
                        PantsAttack();
                        break;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (state == AIState.SmokeAttack) {
                Say("DOES IT HURT?");
            }
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            int defMod = (int)(target.statDefense * 0.5f);
            if (Main.expertMode)
                defMod = (int)(target.statDefense * 0.75f);
            if (Main.masterMode)
                defMod = target.statDefense;
            damage = NPC.damage + defMod;
        }

        private void Run()
        {
            speed = 10f;
            NPC.damage = 0;

            if (DistanceFromTarget < 8000 && state != AIState.AttackChain)
                state = AIState.Idle;
        }


        private void ForceRetreat(int nextAttackDelay)
        {
            nextMoveDelayPlusOne = nextAttackDelay + 1;

            state = AIState.Retreat;
            ResetAI(nextMoveDelayPlusOne);
        }

        private void Idle()
        {
            currentAnimation = "Idle";
            rotation = Utils.AngleLerp(rotation, 0f, 0.12f);

            monsoonOpacity = 1f;
            NPC.damage = 0;
            speed = 0.5f;

            if (Math.Abs(PlayerTarget.velocity.X) > 10f || DistanceFromTarget > 400000) {
                state = AIState.SmokePrepare;
                ResetAI(60);
                Attack_AimTime = 90;
            }

            if (NPC.ai[0] > 0)
                NPC.ai[0] -= 0.5f;
            else
                PickNextAttack();
        }

        private void MoveTowardsPlayer()
        {
            int dir = PlayerTarget.Center.X < NPC.Center.X ? -1 : 1;

            NPC.velocity *= 0.5f;

            if (NPC.velocity.X < 0.01f)
                NPC.velocity.X = 0;

            if (DistanceFromTarget > 8000)
                NPC.velocity.X = speed * dir;

            NPC.velocity.Y = 8f;

            currentAnimation = "Walk";
            animations[currentAnimation].Update();
        }

        private void PickNextAttack()
        {
            AIState nextAttack = attacks[currentAttack];

            if (currentAttack++ >= attacks.Length - 1)
                currentAttack = 0;
            if (!IsMagnetized)
                state = nextAttack;
            else
                state = (AIState)Main.rand.Next((int)AIState.MagneticSpin, (int)AIState.PantsAttack + 1);

            switch (state) {
                case AIState.SmokePrepare:
                    ResetAI(60);
                    Attack_AimTime = 90;
                    break;
                case AIState.AttackChain:
                    ResetAI(60);
                    NPC.velocity *= 0;
                    Attack_Direction = PlayerTarget.Center.X < NPC.Center.X ? -1 : 1;
                    Attack_AimTime = 40;
                    Attack_AttemptCount = 2 + currentAttack;
                    break;
                case AIState.Idle:
                    ResetAI(150);
                    break;
                case AIState.MagneticSpin:
                    ResetAI(30, 4, 120);
                    break;
                case AIState.PantsAttack:
                    ResetAI(30, 1, 242);
                    break;
            }
        }

        private void ForceMagneticAttack()
        {
            prepedThrowAttack = false;
            ResetAI(60);
            magnetizedTime = 1800;
            state = AIState.MagneticWreckageThrowAttack;

        }

        private void Retreat()
        {
            if (NPC.ai[0] == nextMoveDelayPlusOne) {
                NPC.velocity = new Vector2(-8 * NPC.direction, -12.5f);
                NPC.ai[0]--;
            }
            if (NPC.ai[0] > 0)
                NPC.ai[0]--;
            else {
                ResetAI(10);
                state = AIState.Idle;
            }

            currentAnimation = "Jump";
        }

        private void Say(string thing)
        {
            int c = CombatText.NewText(new Rectangle((int)(NPC.Hitbox.X + NPC.velocity.X * 4), NPC.Hitbox.Y, 0, 0), Color.Red, 5, true);
            Main.combatText[c].text = thing;
        }

        private void SetDamage(float damage)
        {
            NPC.damage = (int)(PlayerTarget.statLifeMax2 * damage);
        }

        public void ResetAI(float time, float attackAttempts = 0f, float attackAimTime = 0f, float attackDirection = 0f)
        {
            rotation = 0f;
            NPC.ai[0] = time;
            NPC.ai[1] = attackAttempts;
            NPC.ai[2] = attackAimTime;
            NPC.ai[3] = attackDirection;
        }
    }
}
