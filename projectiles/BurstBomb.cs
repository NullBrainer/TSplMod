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
    public class BurstBomb : ModProjectile
    {
        public Vector2 oldpos;

        public override void SetDefaults()
        {
            //projectile.arrow = true;
            projectile.width = 20;
            projectile.height = 20;
            projectile.aiStyle = 0;
            projectile.minion = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ignoreWater = false;
            drawOffsetX = 1;
            drawOriginOffsetY = -9;
            projectile.rotation = 1f;
        }
        public override void AI()
        {
            projectile.rotation += 0.1f;
            projectile.velocity.Y += 0.3f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int) (damage * 1.72f);            
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
            Vector2 vel = new Vector2(0f, 0f);
            Projectile.NewProjectile(oldpos, vel, ModContent.ProjectileType<InkExplosion>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 3);
            Main.PlaySound(SoundLoader.customSoundType, oldpos, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BurstBombExplosion"));
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Terraria.Dust.NewDust(oldpos, (projectile.width / 2), (projectile.height / 2), ModContent.DustType<InkDropletOrange>(), 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity.X = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity.Y = Main.rand.NextFloat(-1, 1);
                Main.dust[dustIndex].velocity *= 5f;
                Main.dust[dustIndex].fadeIn = 15f;
                Main.dust[dustIndex].scale = 2f;
            }
        }
    }
}
