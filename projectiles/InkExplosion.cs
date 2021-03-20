using Microsoft.Xna.Framework;
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
            projectile.width = 125;
            projectile.height = 125;
            projectile.timeLeft = 10;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.ranged = true;
            projectile.aiStyle = 0;
            projectile.velocity = new Vector2(0f, 0f);

        }
    }
}
