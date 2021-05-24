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
    public class SuctionBomb : ModProjectile
    {
        private bool stuck = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suction_Bomb");
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {

            projectile.tileCollide = true;
            projectile.width = 19;
            projectile.height = 19;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ignoreWater = false;
            projectile.rotation = 0f;
        }

       
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Vector2 collisionvector = Collision.TileCollision(projectile.position, projectile.velocity, projectile.height, default, default, default);
            projectile.position += collisionvector;

            collisionvector.Normalize();
            collisionvector.X = (float)Math.Round(collisionvector.X);
            collisionvector.Y = (float)Math.Round(collisionvector.Y);

            if (projectile.velocity.X != oldVelocity.X)
            {
                if (collisionvector.X == -1 && (collisionvector.Y == 0 || collisionvector.Y == 1))
                {
                    projectile.rotation = 3 * MathHelper.PiOver2;//left
                }
                if (collisionvector.X == 1 && (collisionvector.Y == 0 || collisionvector.Y == 1))
                {
                    projectile.rotation = MathHelper.PiOver2;//right           
                }
            }
            if (projectile.velocity.X == oldVelocity.X) // landed on the floor but traveling on x
            {
                if (projectile.velocity.Y < oldVelocity.Y)
                {
                    projectile.rotation = MathHelper.Pi;//down
                }
                else
                {
                    projectile.rotation = MathHelper.TwoPi;//up
                }
            }
            stuck = true;
            projectile.velocity = Vector2.Zero;
            //SuctionBombContact
            Main.PlaySound(SoundLoader.customSoundType, projectile.oldPosition, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/SuctionBombContact"));
            return false;
        }

        public override void AI()
        {
            if (projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            }
            if (!stuck)
            {
                projectile.velocity.Y += 0.3f;
            }
            else
            {
                projectile.ai[0] += 1;
                if (projectile.ai[0] == 60f)
                {
                    projectile.light = 0.75f;
                    Main.PlaySound(SoundLoader.customSoundType, projectile.oldPosition, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombAlert01"));
                }
                if (projectile.ai[0] > 120f)
                {
                    projectile.ai[0] = 0;
                    projectile.Kill();
                }
            }

            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
           

        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
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
            projectile.light = 1.5f;
            Vector2 vel = new Vector2(0f, 0f);
            Projectile.NewProjectile(oldpos, vel, ModContent.ProjectileType<SuctionBombExplosion>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 3);
            Main.PlaySound(SoundLoader.customSoundType, oldpos, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombExplosion00"));
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(oldpos, (projectile.width / 2), (projectile.height / 2), ModContent.DustType<Agent1InkDroplet>(), 0f, 0f, 0, default, 2f);
                Main.dust[dustIndex].velocity.X = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity.Y = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity *= 8f;
                Main.dust[dustIndex].fadeIn = 8f;
                Main.dust[dustIndex].scale = 2f;
            }
        }

    }
}
