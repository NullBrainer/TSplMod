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
    public class SplatterShotProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ink");
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
        }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<InkedBuff>(), 300, false);
        }

        public override void AI()
        {
            projectile.knockBack -= 0.1f;
            if (projectile.knockBack <= 0.5f)
            {
            projectile.knockBack = 0.5f;
            }
            projectile.velocity.Y += 0.3f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            base.AI();
        }
    }
}
