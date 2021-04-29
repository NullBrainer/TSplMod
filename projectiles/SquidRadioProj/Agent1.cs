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
            drawOriginOffsetY = -16;
            drawOffsetX = -10;
        }
        public override void AI()
        {
            SquidBuffType = ModContent.BuffType<SquidRadioBuff>();
            base.AI();
        }
        protected override void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);
            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();

            switch (InklingState)
            {
                case InklingStates.FOLLOW:
                    JumpOverTiles();
                    FollowPlayer(distanceToIdlePosition, vectorToIdlePosition, FollowRange);
                    break;
                case InklingStates.IDLE:
                    projectile.velocity.X = 0f;
                    break;
                case InklingStates.LAND:
                    projectile.velocity.X = 0;
                    break;
                case InklingStates.FLYING:
                    UpdateInklingFlying(vectorToIdlePosition, distanceToIdlePosition);
                    break;
                default:
                    break;
            }
            if (distanceToIdlePosition > MaxDistance)
            {
                SetInklingState(InklingStates.FLYING);
            }
            else if (InklingState != InklingStates.FLYING)
            {
                if (foundTarget)
                {
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    SetInklingState(InklingStates.RUN);
                    if (SpecialActive(specialReq))
                    {
                        projectile.ai[1] += 1f;
                        if (projectile.ai[1] >= 360f)
                        {
                            specialCounter = 0;
                            projectile.ai[1] = 0f;
                        }
                    }
                    if (Math.Abs(projectile.position.X - target.X) <= 320f && Math.Abs(projectile.position.Y - target.Y) <= projectile.Distance(target))
                    {
                        projectile.velocity.X = 0;
                        if (SpecialActive(specialReq))
                        {
                            SetInklingState(InklingStates.SPECIAL);
                            TimedAttack(target, 6f);
                        }
                        else
                        {
                            if (SubActive)
                            {
                                SetInklingState(InklingStates.SUB);
                                TimedAttack(target, 6f);
                            }
                            else
                            {
                                SetInklingState(InklingStates.PRIMARY);
                                TimedAttack(target, 6f);
                            }
                        }
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


        protected override void DefaultAttack(Vector2 projVector, Vector2 targetposition)
        {
            specialCounter++;
            Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Shooter0" + Main.rand.Next(0, 2)), 0.5f);
            Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<HeroShotProjectile>(), projectile.damage, projectile.knockBack, projectile.owner);
            TargetingAngle = DegreeToRad(projectile.DirectionTo(targetposition).ToRotation());
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
                    break;
                case InklingStates.FOLLOW:
                    if (projectile.velocity.X != 0f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(4, 9);
                    }
                    break;
                case InklingStates.PRIMARY:
                    PlayerAnimation(1, 3);
                    break;
                case InklingStates.JUMPING:
                    projectile.frame = 17;
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
                    PlayerAnimation(22,24);
                    break;
                case InklingStates.ROLLER_UP:
                    if (projectile.velocity.X != 0f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(4, 9);
                    }
                    break;
                case InklingStates.ROLLER_DOWN:
                    projectile.spriteDirection = projectile.direction;
                    PlayerAnimation(10, 15);
                    break;
                default:
                    break;
            }
        }

    }
}
