using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles.Agent3Proj;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.SquidRadioProj
{
    public class SquidRadioProj : InklingSummon
    {

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            projectile.velocity.Y += Gravity;


            if (projectile.velocity.Y >= TerminalVelocity)
            {
                projectile.velocity.Y = TerminalVelocity;
            }
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<SquidRadioBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<SquidRadioBuff>()))
            {
                projectile.timeLeft = 2;
            }

            Vector2 idlePosition = player.Center;
            idlePosition.X += -player.direction * 32f;

            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            vectorToIdlePosition.X += player.direction * -32f;
            vectorToIdlePosition.Y += -32f;


            if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
            {
                projectile.position = idlePosition;
                projectile.velocity *= 0.1f;
                projectile.netUpdate = true;
            }
            UpdateInklingFlying(vectorToIdlePosition);
            FixOverlap(0.04f);

            // Starting search distance
            float distanceFromTarget = 320f;
            target = projectile.position;
            foundTarget = false;
            target = FindTarget(player, distanceFromTarget, target);

            speed = 5f;
            inertia = 3f;
            SetStates(player, distanceToIdlePosition, vectorToIdlePosition);
            Animate(InklingState);
        }

        private void Move(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            SetInklingState( InklingStates.FOLLOWING);
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity.X = Approach(vectorToIdlePosition).X;


        }

    }
}

