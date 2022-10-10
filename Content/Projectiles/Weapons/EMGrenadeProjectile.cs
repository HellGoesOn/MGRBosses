using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace MGRBosses.Content.Projectiles.Weapons
{
    public class EMGrenadeProjectile : ModProjectile
    {
        public override string Texture => "MGRBosses/Content/Textures/Items/EMGrenade";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.4f;
            Projectile.velocity.X *= 0.99f;
        }

        public override bool PreKill(int timeLeft)
        {
            int id = Projectile.NewProjectile(Projectile.GetSource_None(), Projectile.Center, Vector2.Zero, ProjectileID.Electrosphere, 10, 1f, Projectile.owner);
            Main.projectile[id].timeLeft = 30;
            return base.PreKill(timeLeft);
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
        }
    }
}
