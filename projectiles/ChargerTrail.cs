using SplatoonMod.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;


namespace SplatoonMod.projectiles
{
    public class ChargerTrail : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Agent2_Charger_Ink_Trail");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.ignoreWater = false;
            drawOffsetX = 5;


        }
        public override bool PreAI()
        {
            projectile.velocity.X = 0f;
            if (projectile.velocity.Y < 0f)
            {
                projectile.velocity.Y = 0f;
            }
            projectile.rotation = projectile.ai[0];
            return true;
        }

        public override void AI()
        {

            projectile.velocity.Y += 0.3f;
            projectile.ai[1] += 1f;

            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
/*            if (projectile.ai[1] >= 30f )
            {
                projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            }*/

        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent2Debuff>(), 300, false);
        }
    }
}
