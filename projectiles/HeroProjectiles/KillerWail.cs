using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SplatoonMod.Buffs;
using SplatoonMod.Dust;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.HeroProjectiles
{
    public class KillerWail : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Killer_Wail");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.arrow = true;
            projectile.width = 10;
            projectile.height = 10;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.ignoreWater = false;
            projectile.velocity = Vector2.Zero;
        }

        public override void AI()
        {
            Animate(0, 1, 2);
            projectile.ai[0] += 1;
            if (projectile.ai[0] > 240f)
            {
                projectile.ai[0] = 0;
                projectile.Kill();
            }
        }
        private  void Animate(int startframe, int endframe, int FrameSpeed)
        {
            if (projectile.frame < startframe || projectile.frame > endframe)
            {
                projectile.frame = startframe;
            }
            projectile.frameCounter++;
            if (projectile.frameCounter >= FrameSpeed)
            {
                projectile.frameCounter = 0;
                if (projectile.frame < endframe)
                {
                    projectile.frame++;
                }
                else if (projectile.frame == endframe)
                {
                    projectile.frame = startframe;
                }
            }
        }
    }
}
