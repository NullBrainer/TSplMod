using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles.HeroProjectiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.SquidRadioProj
{
    public class Agent1 : InklingSummon
    {
        
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Agent1Debuff>(), 300, false);

        }
        public override void AI()
        {
            SquidBuffType = ModContent.BuffType<SquidRadioBuff>();
            base.AI();
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Agent_1");
            Main.projFrames[projectile.type] = 25;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            CenteroffSet = projectile.Center;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.netImportant = true;
            projectile.width = 45;//66
            projectile.height = 45; //70
            drawOriginOffsetY = -40;
            drawOffsetX = -20;
            maxspeed = 10f;
            defaultInertia = 20f;
        }
        protected override void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);
            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();

            CheckConditions(player, distanceToIdlePosition, vectorToIdlePosition);
            if (distanceToIdlePosition > MaxDistance)
            {
                SetInklingState(InklingStates.FLYING);
            }
            else if (InklingState != InklingStates.FLYING)
            {
                if (foundTarget)
                {
                    SetInklingState(InklingStates.ROLLER_DOWN);
                    if (Vector2.Distance(projectile.Center, target) < 64f && (projectile.velocity.X < 1.5f && projectile.velocity.X > -1.5f))
                    {
                        SetInklingState(InklingStates.PRIMARY);
                    }

                }
                else if ((projectile.velocity.Y > 5f || projectile.velocity.Y < -5f) && (projectile.velocity.X < 1f || projectile.velocity.X > -1f))
                {
                    SetInklingState(InklingStates.JUMPING);
                }
                else if (Math.Abs(projectile.position.X - player.position.X) >= FollowRange)//distanceToIdlePosition > FollowRange)//distanceToIdlePosition > FollowRange || (player.velocity.X < -1f || player.velocity.X > 1f) && player.velocity.X != 0)
                {
                    SetInklingState(InklingStates.FOLLOW);
                }
                else if ((projectile.velocity.X < 1f || projectile.velocity.X > -1f))// && distanceToIdlePosition < FollowRange)//(Math.Abs(projectile.position.X - player.position.X) <= FollowRange)
                {
                    SetInklingState(InklingStates.IDLE);
                }

            }
            

        }
        protected void CheckConditions(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            switch (InklingState)
            {
                case InklingStates.IDLE:
                    projectile.velocity.X = 0f;
                    break;
                case InklingStates.RUN:
                    break;
                case InklingStates.FOLLOW:
                    JumpOverTiles();
                    FollowPlayer(distanceToIdlePosition, vectorToIdlePosition, FollowRange);
                    break;
                case InklingStates.PRIMARY:
                    projectile.velocity.X = projectile.direction * 0.01f;
                    TimedAttack(target, 20f, 20, 23);
                    break;
                case InklingStates.JUMPING:
                    break;
                case InklingStates.LAND:
                    projectile.velocity.X = 0;
                    break;
                case InklingStates.FLYING:
                    UpdateInklingFlying(vectorToIdlePosition, distanceToIdlePosition);
                    break;
                case InklingStates.SUB:
                    break;
                case InklingStates.SPECIAL:
                    break;
                case InklingStates.ROLLER_DOWN:
                    speed = 10f;
                    inertia = 30f;
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    break;
                default:
                    break;
            }

        }
        public override bool MinionContactDamage()
        {
            return InklingState == InklingStates.ROLLER_DOWN && (projectile.velocity.X > 5f || projectile.velocity.X < -5f);
        }

        protected override void DefaultAttack(Vector2 projVector, Vector2 targetposition)
        {
            if (projectile.direction < 0)
            {
                CenteroffSet.X -= 25f;
            }
            else
            {
                CenteroffSet.X += 25f;
            }
            specialCounter++;
            if (projectile.frame == 1)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/RollerLift"), 0.5f);

            }
            if (projectile.frame == 2)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/RollerSwing"), 0.5f);
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/PlayerWeaponRollerSpray" + Main.rand.Next(0, 3)), 0.5f);
                Projectile.NewProjectile(CenteroffSet, Vector2.Zero, ModContent.ProjectileType<HeroRollerSwingProjectile>(), 20, 10f, projectile.owner);
            }

        }

        protected override void SubAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<HeroBurstBomb>(), (projectile.damage), projectile.knockBack, projectile.owner);
                SubActive = false;
                specialCounter += 10;
            }
        }

        protected override void SpecialAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<HeroBurstBomb>(), (projectile.damage), projectile.knockBack, projectile.owner);
            }
        }

        protected override void Animate(InklingStates state)
        {
            switch (state)
            {
                case InklingStates.IDLE:
                    projectile.frame = 0;
                    FacePlayer();
                    break;
                case InklingStates.RUN:
                    if (projectile.velocity.X != 0f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(4, 9);
                    }
                    break;
                case InklingStates.FOLLOW:
                    if (projectile.velocity.X != 0f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(4, 9);
                    }
                    break;
                case InklingStates.PRIMARY:
                    FrameSpeed = 20;
                    AnimateState(0, 3, InklingStates.ROLLER_DOWN);
                    break;
                case InklingStates.JUMPING:
                    projectile.frame = 16;
                    break;
                case InklingStates.LAND:
                    break;
                case InklingStates.FLYING:
                    PlayerAnimation(17, 18);
                    break;
                case InklingStates.SUB:
                    PlayerAnimation(19, 21);
                    break;
                case InklingStates.SPECIAL:
                    PlayerAnimation(22, 24);
                    break;                
                case InklingStates.ROLLER_DOWN:
                    FrameSpeedOnVelocity();
                    projectile.spriteDirection = projectile.direction;
                    PlayerAnimation(10, 15);
                    break;
                default:
                    break;
            }
        }
        private void AnimateState(int startframe, int endframe, InklingStates newstate)
        {

            if (projectile.frame <= startframe || projectile.frame > endframe)
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
                    SetInklingState(newstate);
                }
            }
        }
        private void FrameSpeedOnVelocity()
        {
            if (projectile.velocity.X < -8f || projectile.velocity.X > 8f)
            {
                FrameSpeed = 4;
            }
            else if (projectile.velocity.X < -5f || projectile.velocity.X > 5f)
            {
                FrameSpeed = 6;
            }
            else if (projectile.velocity.X < -3f || projectile.velocity.X > 3f)
            {
                FrameSpeed = 8;
            }
            else if (projectile.velocity.X > -1f && projectile.velocity.X < 1f)
            {
                FrameSpeed = 24;
            }
        }

    }
}
