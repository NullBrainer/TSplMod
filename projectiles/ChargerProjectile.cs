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
    public class ChargerProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Agent2_Charger_Ink");
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
            projectile.rotation = projectile.ai[0];

        }

        public override bool PreAI()
        {
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            return base.PreAI();
        }
        public override void AI()
        {
           
            projectile.ai[1] += 1f;
            if (projectile.ai[1] > 3f)
            {
            if(projectile.velocity.X == projectile.oldVelocity.X || projectile.velocity.Y == projectile.oldVelocity.Y)
            {
            Projectile.NewProjectile(projectile.position, projectile.velocity, ModContent.ProjectileType<ChargerTrail>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.rotation);
            }                      
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent2Debuff>(), 300, false);
        }
    }
}
