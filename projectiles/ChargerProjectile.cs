using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.Dust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles
{
    public class ChargerProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Agent2_Charger_Ink");
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
            //drawOffsetX = 5;
            projectile.rotation = projectile.ai[0];
            projectile.timeLeft = 60;

        }

        public override bool PreAI()
        {
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            return base.PreAI();
        }
        public override void AI()
        {
             if (projectile.ai[1] < 10f)
             {
                 projectile.ai[1] += 1f;
             }
             if (projectile.ai[1] >= 10f)
             {
                 projectile.velocity.Y += 0.3f;
             }
                 

            if (projectile.ai[1] > 2f)
            {               
                Projectile.NewProjectile(projectile.Center, Vector2.One * projectile.DirectionTo(projectile.velocity).ToRotation(), ModContent.ProjectileType<ChargerTrail>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.rotation); 
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent2Debuff>(), 300, false);
        }
        public override void Kill(int timeLeft)
        {
            Explode(projectile.oldPosition);
            base.Kill(timeLeft);
        }
        private void Explode(Vector2 oldpos)
        {
            for (int i = 0; i < 15; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(oldpos, (projectile.width / 2), (projectile.height / 2), ModContent.DustType<Agent2InkDroplet>(), 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity.X = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity.Y = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity *= 5f;
                Main.dust[dustIndex].fadeIn = 15f;
                Main.dust[dustIndex].scale = 1.5f;
            }
        }
    }
}
