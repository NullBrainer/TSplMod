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
            projectile.minion = true;
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;

        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<InkedBuff>(), 300, false);

        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            int X = (int)(projectile.Center.X - target.Center.X);
            int Y = (int)(projectile.Center.Y - target.Center.Y);

            int dist = (int)Math.Sqrt((X * X) + (Y * Y));
            if (dist > projectile.Hitbox.Width / 3)
            {
                damage = (int)(damage * 0.59f);
            }
            else if (dist >= projectile.Hitbox.Width / 2)
            {
                damage = (int)(damage * 0.72f);
            }
        }

    }
}
