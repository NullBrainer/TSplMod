using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SplatoonMod.Dust;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles
{
    public class BurstBomb : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.arrow = true;
            projectile.width = 30;
            projectile.height = 30;
            projectile.aiStyle = 1;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.ignoreWater = false;
        }
        public override void AI()
        {
            projectile.rotation *= 0.5f;
            projectile.velocity.Y += 0.1f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
            base.AI();
        }
        public override void Kill(int timeLeft)
        {

            Explode();
        }

        private void Explode()
        {
            Main.PlaySound(SoundLoader.customSoundType, projectile.position, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BurstBombExplosion"));

            Vector2 vel = new Vector2(0f, 0f);
            Projectile.NewProjectile(projectile.Center, vel, ModContent.ProjectileType<InkExplosion>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 1);

            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, ModContent.DustType<InkDropletOrange>(), 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity.X = 1f;
                Main.dust[dustIndex].velocity *= 5f;
                Main.dust[dustIndex].scale = 5f;
            }
            
        }
    }
}
