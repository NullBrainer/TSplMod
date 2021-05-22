using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using SplatoonMod.Dust;

namespace SplatoonMod.projectiles.HeroProjectiles
{
    public class HeroRollerSplash : ModProjectile
    {
        private bool Gravity = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("RollerSplash");
        }

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.ignoreWater = false;
            projectile.velocity = Vector2.Zero;
            projectile.rotation = Main.rand.NextFloat(0,2f) * Main.rand.NextFloatDirection();
        }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent1Debuff>(), 300, false);

        }
        public override void Kill(int timeLeft)
        {
            Explode(projectile.oldPosition);
            base.Kill(timeLeft);
        }

        public override void AI()
        {
            projectile.rotation += 0.1f;
            if (projectile.damage > 25)
            {
                projectile.damage -= 1;
            }
            if (!Gravity)
            {
                projectile.ai[0] += 1;
                Gravity = projectile.ai[0] >= 4;
                if (Gravity)
                {
                    projectile.ai[0] = 0;
                }
            }
            else
            {
                projectile.velocity.Y += 0.3f;
                if (projectile.velocity.Y > 16f)
                {
                    projectile.velocity.Y = 16f;
                }
            }
           
            base.AI();
        }
        private void Explode(Vector2 oldpos)
        {
            for (int i = 0; i < 15; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(oldpos, (projectile.width / 2), (projectile.height / 2), ModContent.DustType<Agent1InkDroplet>(), 0f, 0f, 100,default, 2f);
                Main.dust[dustIndex].velocity.X = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity.Y = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity *= 5f;
                Main.dust[dustIndex].fadeIn = 15f;
                Main.dust[dustIndex].scale = 1.5f;
            }
        }
    }
}
