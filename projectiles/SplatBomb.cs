using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SplatoonMod.Buffs;
using SplatoonMod.Dust;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles
{
    public class SplatBomb : ModProjectile
    {
        public Vector2 oldpos;
        private bool Contact = false;
        private int FrameSpeed;
        private int denom;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hero Splat Bomb");
            Main.projFrames[projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ignoreWater = false;
            drawOffsetX = 1;
            drawOriginOffsetY = -9;
            projectile.rotation = 0f;
            FrameSpeed = 5;             

        }
        public override bool PreAI()
        {
            denom = (int)Math.Abs(projectile.velocity.X) + 1;
            FrameSpeed = 50 / denom;
            return base.PreAI();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {

            Contact = true;
            return false;
        }
        public override void AI()
        {

            projectile.velocity.Y += 0.3f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
            
            if (projectile.velocity.X == 0f)
            {
                if (projectile.frame % 2 == 0)
                {
                    projectile.frame = 0;
                }
                else
                {
                    projectile.frame = 7;
                }
            }
            else
            {
                if (Contact)
                {
                    projectile.spriteDirection = -projectile.direction;
                    UpdateFrames(0, 6, FrameSpeed);
                }
                else
                {
                    projectile.spriteDirection = projectile.direction;
                    UpdateFrames(1, 6, FrameSpeed);

                }
            }
            if (Contact)
            {

                if (projectile.velocity.X <= -0.75f)
                {
                    projectile.velocity.X += 0.5f;
                }
                else if (projectile.velocity.X >= 0.75f)
                {
                    projectile.velocity.X -= 0.5f;
                }
                else
                {
                    projectile.velocity.X = 0f;
                }

                projectile.ai[0] += 1;
            }

            if (projectile.ai[0] == 1f)
            {
                Main.PlaySound(SoundLoader.customSoundType, projectile.oldPosition, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombAlert01"));
            }
            if (projectile.ai[0] == 60f)
            {
                projectile.light = 0.15f;
                projectile.ai[0] = 0;
                projectile.Kill();
            }
        }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent2Debuff>(), 300, false);
            if (projectile.Hitbox.Intersects(target.Hitbox))
            {
                PreKill(1);
            }
            projectile.active = false;
        }

        public override bool PreKill(int timeLeft)
        {
            Vector2 oldpos = projectile.oldPosition;
            Explode(oldpos);
            return base.PreKill(timeLeft);
        }


        public override void Kill(int timeLeft)
        {
            projectile.active = false;
        }
        private void Explode(Vector2 oldpos)
        {
            Vector2 vel = new Vector2(0f, 0f);
            Projectile.NewProjectile(oldpos, vel, ModContent.ProjectileType<SplatBombExplosion>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 3);
            Main.PlaySound(SoundLoader.customSoundType, oldpos, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombExplosion00"));
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(oldpos, (projectile.width / 2), (projectile.height / 2), ModContent.DustType<Agent2InkDroplet>(), 0f, 0f, 0, default, 2f);
                Main.dust[dustIndex].velocity.X = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity.Y = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity *= 8f;
                Main.dust[dustIndex].fadeIn = 8f;
                Main.dust[dustIndex].scale = 2f;
            }
        }
        protected void UpdateFrames(int startframe, int endframe, int framespeed)
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
                    projectile.frame = startframe;
                }
            }
        }
    }

}
