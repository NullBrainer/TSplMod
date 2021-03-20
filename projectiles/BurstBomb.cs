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
            base.SetDefaults();
        }
        public override void AI()
        {
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
                int dustIndex = Terraria.Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }

            for (int i = 0; i < 80; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, ModContent.DustType<InkDroplet>(), 0f, 0f, 100, default(Color), 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= Main.rand.NextFloat(4f);
                dustIndex = Terraria.Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, ModContent.DustType<InkDroplet>(), 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity *= Main.rand.NextFloat(2f);
            }
        }
    }
}
