using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles.HeroProjectiles;
using System;
using SplatoonMod.Util;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;



namespace SplatoonMod.projectiles.SquidRadioProj
{
    public class Agent2 : InklingSummon
    {
        public override void AI()
        {
            if (projectile.ai[1] == 1)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Inkling/Marie/Marie_" + Main.rand.Next(0, 4)), 0.5f);
            }
            SquidBuffType = ModContent.BuffType<SquidRadioBuff>();
            base.AI();
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Agent_2");
            Main.projFrames[projectile.type] = 21;
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
            drawOriginOffsetY = -49;
            drawOffsetX = -20;
            maxspeed = 10f;
            defaultInertia = 20f;
            CooldownLimit = 0f;
            primaryprojectileespeed = 35f;
            projectilespeed = 16f;
            MaxSubTime = 30f;
            primaryAccuracy = 0f;
            subAccuracy = 20f;
            specialAccuracy = 20f;
            distanceFromTarget = 320f;
            TargetDetectRange = 120f;
            AttackTypes = new SummonAttack[] { new SummonAttack(this, 1, 5, 90f), new SummonAttack(this, 15, 17, 15f), new SummonAttack(this, 18, 20, 10f) };
           
        }
        protected override void PerformState(float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Vector2 projectileVector = Vector2.One;

            switch (InklingState)
            {
                case InklingStates.IDLE:
                    projectile.velocity.X = 0f;
                    break;
                case InklingStates.RUN:
                    JumpOverTiles();
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    break;
                case InklingStates.FOLLOW:
                    JumpOverTiles();
                    FollowPlayer(distanceToIdlePosition, vectorToIdlePosition, FollowRange);
                    break;
                case InklingStates.LAND:
                    projectile.velocity.X *= 0;
                    break;
                case InklingStates.FLYING:
                    UpdateInklingFlying(vectorToIdlePosition, distanceToIdlePosition);
                    break;
                case InklingStates.PRIMARY:
                    FaceTarget(target);
                    
                    projectileVector = new Vector2(primaryprojectileespeed, primaryprojectileespeed);
                    Vector2 dest = target;
                    dest.X += (targetnpc.velocity.X * targetnpc.velocity.X) * targetnpc.direction;
                    dest.Y -= (targetnpc.velocity.Y * targetnpc.velocity.Y) * targetnpc.directionY;

                    float dist = projectile.Distance(dest);
                    float time = (dist / primaryprojectileespeed);
                    TargetingAngle = RadToDegree(projectile.DirectionTo(target).ToRotation());
                    dest.Y -=  (3f*time)*0.5f;
                    //dest.X += (time*projectile.direction);
                   
                    projectileVector *= projectile.DirectionTo(dest);
                    //projectileVector = AimProjectile(target, primaryprojectileespeed, primaryprojectileespeed, primaryAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[0].GetDuration(), 16, 19);
                    CooldownLimit = AttackTypes[0].GetDuration();
                    break;
                case InklingStates.SUB:
                    FaceTarget(target);
                    projectileVector = AimProjectile(target, projectilespeed, projectilespeed, subAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[1].GetDuration(), 16, 19);
                    CooldownLimit = AttackTypes[1].GetDuration();
                    break;
                case InklingStates.SPECIAL:
                    FaceTarget(target);
                    specialused = true;
                    projectileVector = AimProjectile(target, projectilespeed, projectilespeed, specialAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[2].GetDuration(), 16, 19);
                    CooldownLimit = AttackTypes[2].GetDuration();
                    break;
                case InklingStates.WAIT:
                    projectile.velocity.X = 0f;
                    break;
                default:
                    break;
            }
        }
        protected override void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            if (distanceToIdlePosition > MaxDistance)
            {
                SetInklingState(InklingStates.FLYING);
            }
            else if (InklingState != InklingStates.FLYING)
            {
                if (foundTarget)
                {
                    if (SpecialActive(specialReq))
                    {
                        SetInklingState(InklingStates.SPECIAL);
                    }
                    else
                    {
                        if (SubActive)//(projectile.Distance(target) <= 480f)//
                        {
                            SetInklingState(InklingStates.SUB);
                        }
                        else
                        {
                            SetInklingState(InklingStates.PRIMARY);
                        }
                    }



                }
                else if ((projectile.velocity.Y > 5f || projectile.velocity.Y < -5f) && (projectile.velocity.X < 1f || projectile.velocity.X > -1f))
                {
                    SetInklingState(InklingStates.JUMPING);
                }
                else
                {
                    SetInklingState(InklingStates.FOLLOW);
                }
            }
        }

        protected override void DefaultAttack(Vector2 projVector, Vector2 targetposition)
        {
            CenteroffSet = projectile.Center;
            CenteroffSet.Y -= projectile.height * 0.10f;
            specialCounter+=5;
            Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Charger/Marie_ChargerShot"), 0.5f);
            Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<ChargerProjectile>(), projectile.damage, projectile.knockBack, projectile.owner);                
            
        }

        protected override void SubAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<SplatBomb>(), (180), projectile.knockBack, projectile.owner);
                specialCounter += 15;
            }
            if (projectile.frame == throwingframe + 1)
            {
                SubActive = false;
            }
        }

        protected override void SpecialAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<SplatBomb>(), (180), projectile.knockBack, projectile.owner);
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
                case InklingStates.FOLLOW:
                    if (projectile.velocity.X > 1f || projectile.velocity.X < -1f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(6, 11);
                    }
                    else
                    {
                        projectile.frame = 0;
                        FacePlayer();
                    }
                    break;
                case InklingStates.RUN:
                    projectile.spriteDirection = projectile.direction;
                    PlayerAnimation(6, 11);
                    break;
                case InklingStates.JUMPING:
                    projectile.spriteDirection = projectile.direction;
                    projectile.frame = 12;
                    break;
                case InklingStates.LAND:
                    projectile.frame = 0;
                    break;
                case InklingStates.PRIMARY:
                    projectile.velocity.X = 0f;
                    SetAttackAnimation(TargetingAngle);
                    break;
                case InklingStates.FLYING:
                    PlayerAnimation(13, 14);
                    break;
                case InklingStates.SUB:
                    projectile.velocity.X = 0f;
                    FrameSpeed = (int)AttackTypes[1].GetDuration();
                    PlayerAnimation(AttackTypes[1].GetStartFrame(), AttackTypes[1].GetEndFrame());
                    break;
                case InklingStates.SPECIAL:
                    projectile.velocity.X = 0f;
                    FrameSpeed = (int)AttackTypes[2].GetDuration();
                    PlayerAnimation(AttackTypes[2].GetStartFrame(), AttackTypes[2].GetEndFrame());
                    break;
                default:
                    break;
            }
        }
    }
}
