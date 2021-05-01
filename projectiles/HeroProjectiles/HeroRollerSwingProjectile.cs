using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.HeroProjectiles
{
    public class HeroRollerSwingProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("InkedDown");
        }

        public override void SetDefaults()
        {
            projectile.arrow = true;
            projectile.width = 60;
            projectile.height = 60;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ignoreWater = false;
            projectile.timeLeft = 10;
            projectile.velocity = Vector2.Zero;
        }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent1Debuff>(), 300, false);

        }

        public override void AI()
        {            
            base.AI();
        }

    }
}
