using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.SquidPhoneProj
{
    public class SquidPhoneProj : InklingSummon
    {
        public override void AI()
        {
            SquidBuffType = ModContent.BuffType<SquidPhoneBuff>();
            base.AI();
        }
        /* public override void AI()
         {
             Player player = Main.player[projectile.owner];
             projectile.velocity.Y += Gravity;


             if (projectile.velocity.Y >= TerminalVelocity)
             {
                 projectile.velocity.Y = TerminalVelocity;
             }
             if (player.dead || !player.active)
             {
                 player.ClearBuff(ModContent.BuffType<SquidPhoneBuff>());
             }
             if (player.HasBuff(ModContent.BuffType<SquidPhoneBuff>()))
             {
                 projectile.timeLeft = 2;
             }
             Vector2 idlePosition = player.Center;


             //  float minionPositionOffsetX = (10 + projectile.minionPos * 40) * -player.direction;
             idlePosition.X += -player.direction * 32f; // Go behind the player


             Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
             float distanceToIdlePosition = vectorToIdlePosition.Length();
             vectorToIdlePosition.X += player.direction * -32f;
             vectorToIdlePosition.Y += -32f;


             if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
             {
                 // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                 // and then set netUpdate to true
                 projectile.position = idlePosition;
                 projectile.velocity *= 0.1f;
                 projectile.netUpdate = true;
             }
             UpdateInklingFlying(vectorToIdlePosition);
             // If your minion is flying, you want to do this independently of any conditions
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
        */

        /// <summary>
        /// A movement function used to specify speed and intertia of the projectile
        /// </summary>
        /// <param name="newspeed"></param>
        /// <param name="newinertia"></param>
        /// <param name="vectorToIdlePosition"></param>
        private void Move(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            InklingState = InklingStates.MOVING;
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity.X = Approach(vectorToIdlePosition).X;
            

        }



    }
}

