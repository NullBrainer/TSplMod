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
        private bool Shooting = false;
        private bool SoundOn = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Killer_Wail");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 50;
            projectile.height = 50;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ignoreWater = false;

        }

        public override void AI()
        {
            projectile.velocity.Y = 0f;
            if (projectile.velocity.X < 0)
            {
                projectile.velocity.X = -0.000001f;
                projectile.direction = -1;
            }
            else
            {
                projectile.velocity.X = 0f;
                projectile.direction = 1;
            }
            projectile.spriteDirection = projectile.direction;
            if (!SoundOn)
            {
                Sound();
            }
            projectile.ai[0] += 1;
            if (!Shooting && projectile.ai[0] > 60f)
            {
                ShootBeam();
                Shooting = true;
            }
            if (Shooting)
            {
                Animate(0, 1, 2);
            }
            if (projectile.ai[0] > 240f)
            {
                projectile.ai[0] = 0;
                projectile.Kill();
            }
        }
        private void ShootBeam()
        {
            Vector2 vel = new Vector2(2f, 0f);
            vel.X *= projectile.spriteDirection;
            Projectile.NewProjectile(projectile.position, vel, ModContent.ProjectileType<KillerWailProjectile>(), projectile.damage, 0f, projectile.owner, 0f, (float)projectile.whoAmI);

        }
        private void Sound()
        {
            SoundOn = true;
            Main.PlaySound(SoundLoader.customSoundType, projectile.position, mod.GetSoundSlot(SoundType.Custom, "Sounds/Specials/BigLaser10"));

        }
        private void Animate(int startframe, int endframe, int FrameSpeed)
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
        public override bool CanDamage()
        {
            return false;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}
