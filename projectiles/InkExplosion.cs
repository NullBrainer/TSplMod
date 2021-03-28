using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles
{
    public class InkExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.Name = "Ink Explosion";
            projectile.width = 100;
            projectile.height = 100;
            projectile.timeLeft = 10;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.ranged = true;
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;

        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {            
            target.AddBuff(ModContent.BuffType<InkedBuff>(), 300, false);

        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Rectangle hitbox = projectile.Hitbox;
            if (hitbox.Intersects(target.Hitbox))
            {
                int X = (int)(hitbox.Center.X - target.Center.X);
                int Y = (int)(hitbox.Center.Y - target.Center.Y);

                int dist = (int)Math.Sqrt((X * X) + (Y * Y));
                if (dist < 0)
                {
                    dist = 1;
                }
                else if (dist > 1024)
                {
                    dist = 2;
                }
                damage -= (int)(((projectile.damage - dist) * 0.9f) * 0.85f);
            }
        }

    }
}
