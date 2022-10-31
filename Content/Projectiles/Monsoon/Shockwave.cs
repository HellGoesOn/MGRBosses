using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles.Monsoon
{
    public class Shockwave : ModProjectile
    {
        private readonly int RIPPLE_COUNT = 1;
        private readonly int RIPPLE_SIZE = 8;
        private readonly float RIPPLE_SPEED = 3f;
        private readonly float DISTORT_STRENGTH = 1000f;

        public override string Texture => "MGRBosses/Content/Textures/Monsoon/RedPhosphor";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.damage = 0;
        }

        public override void AI()
        {
            base.AI();

            if (Main.dedServ)
                return;

            if (!Filters.Scene["Shockwave"].IsActive()) {
                Filters.Scene.Activate("Shockwave", Projectile.Center).GetShader().UseColor(RIPPLE_COUNT, RIPPLE_SIZE, RIPPLE_SPEED).UseTargetPosition(Projectile.Center);
            }

            float progress = (90f - Projectile.timeLeft) / 60f;

            Filters.Scene["Shockwave"].GetShader().UseTargetPosition(Projectile.Center);
            Filters.Scene["Shockwave"].GetShader().UseColor(RIPPLE_COUNT, RIPPLE_SIZE, progress * 10);
            Filters.Scene["Shockwave"].GetShader().UseProgress(progress).UseOpacity(DISTORT_STRENGTH * (1 - progress / 3f));
        }

        public override void Kill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            Filters.Scene["Shockwave"].Deactivate();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
