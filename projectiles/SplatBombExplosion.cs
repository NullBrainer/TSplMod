using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles
{
    public class SplatBombExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("SplatBombExplosion");
            Main.projFrames[projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            projectile.Name = "Ink Explosion";
            projectile.width = 150;
            projectile.height = 150;
            projectile.timeLeft = 10;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.minion = true;
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;
            projectile.light = 2f;
        }
        public override void AI()
        {
            UpdateFrames(0, 7, 1);

            base.AI();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent2Debuff>(), 300, false);

        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            int X = (int)(projectile.Center.X - target.Center.X);
            int Y = (int)(projectile.Center.Y - target.Center.Y);

            int dist = (int)Math.Sqrt((X * X) + (Y * Y));
            if (dist > (projectile.Hitbox.Width / 4)*3)
            {
                damage = 30;
            }           
        }

        protected virtual void UpdateFrames(int startframe, int endframe, int framespeed)
        {

            if (projectile.frame < startframe || projectile.frame > endframe)
            {
                projectile.frame = startframe;
            }
            projectile.frameCounter++;
            if (projectile.frameCounter >= framespeed)
            {
                projectile.frameCounter = 0;
                if (projectile.frame < endframe)
                {
                    projectile.frame++;
                }
                else if (projectile.frame == endframe)
                {
                    projectile.Kill();
                }
            }
        }
    }
}
