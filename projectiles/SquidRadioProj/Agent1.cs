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
    /*
     TODO: Added reference to owner in Summon attack to update timer and play animation
     */
    public class Agent1 : InklingSummon
    {

        private readonly int projectiles = 10;

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
            drawOriginOffsetY = -53;
            drawOffsetX = -20;
            maxspeed = 10f;
            defaultInertia = 20f;
            CooldownLimit = 0f;
            AttackTypes = new SummonAttack[] { new SummonAttack(this, 1, 3, 22f), new SummonAttack(this, 19, 21, 15f), new SummonAttack(this, 22, 24, 15f) };
            SpecialDuration = AttackTypes[2].GetDuration();
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
                        if (SubActive)
                        {
                            SetInklingState(InklingStates.SUB);
                        }
                        else
                        if ((Vector2.Distance(projectile.Center, target) <= 48f && (projectile.velocity.X < 7f || projectile.velocity.X > -7f)) || (InklingState != InklingStates.ROLLER_DOWN && Vector2.Distance(projectile.Center, target) > 48f))
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
                /*
                else if (Math.Abs(projectile.position.X - player.position.X) >= FollowRange)//distanceToIdlePosition > FollowRange)//distanceToIdlePosition > FollowRange || (player.velocity.X < -1f || player.velocity.X > 1f) && player.velocity.X != 0)
                {
                    SetInklingState(InklingStates.FOLLOW);
                }
                else if ((projectile.velocity.X < 1f || projectile.velocity.X > -1f) && (player.velocity.X < 1f || player.velocity.X > -1f))// && distanceToIdlePosition < FollowRange)//(Math.Abs(projectile.position.X - player.position.X) <= FollowRange)
                {
                    SetInklingState(InklingStates.IDLE);
                }*/
            }
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
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    break;
                case InklingStates.FOLLOW:
                    JumpOverTiles();
                    FollowPlayer(distanceToIdlePosition, vectorToIdlePosition, FollowRange);
                    
                    break;
                case InklingStates.LAND:
                    projectile.velocity.X = 0;
                    break;
                case InklingStates.FLYING:
                    UpdateInklingFlying(vectorToIdlePosition, distanceToIdlePosition);
                    break;
                case InklingStates.PRIMARY:
                    if (projectile.frame == AttackTypes[0].GetEndFrame())
                    {
                        SetInklingState(InklingStates.ROLLER_DOWN);
                    }
                    else
                    {
                        FaceTarget(target);
                        projectile.velocity.X *= 0.0f;
                        projectileVector = projectile.DirectionTo(target);
                        TimedAttack(target, projectileVector, AttackTypes[0].GetDuration(), 20, 23);
                        CooldownLimit = AttackTypes[0].GetDuration();
                    }
                   
                    break;
                case InklingStates.SUB:
                    FaceTarget(target);
                    projectile.velocity.X = 0f;
                    projectileVector = AimProjectile(target, 16f, 16f, 15f);
                    TimedAttack(target, projectileVector, AttackTypes[1].GetDuration(), 20, 23);
                    CooldownLimit = AttackTypes[1].GetDuration();
                    break;
                case InklingStates.SPECIAL:
                    FaceTarget(target);
                    projectileVector *= projectile.spriteDirection;
                    projectile.velocity.X = 0f;
                    TimedAttack(target, projectileVector, AttackTypes[2].GetDuration(), 20, 23);
                    CooldownLimit = AttackTypes[2].GetDuration();
                    break;
                case InklingStates.ROLLER_DOWN:
                    speed = 10f;
                    inertia = 30f;                   
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    break;
                case InklingStates.WAIT:
                    
                    break;
                default:
                    break;
            }
        }
       
        public override bool MinionContactDamage()
        {
            return InklingState == InklingStates.ROLLER_DOWN && (projectile.velocity.X > 7f || projectile.velocity.X < -7f);
        }

      
        protected override void DefaultAttack(Vector2 projVector, Vector2 targetposition)
        {
            if (projectile.direction < 0) 
            {
                CenteroffSet.X -= 30f;
            }
            else
            {
                CenteroffSet.X += 30f;
            }
            specialCounter++;
            if (projectile.frame == 1)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/RollerLift"), 0.5f);

            }
            else
            if (projectile.frame == 2)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/RollerSwing"), 0.5f);
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Roller/PlayerWeaponRollerSpray" + Main.rand.Next(0, 3)), 0.5f);
                Projectile.NewProjectile(CenteroffSet, Vector2.Zero, ModContent.ProjectileType<HeroRollerSwingProjectile>(), 140, 10f, projectile.owner);
              
                    Vector2[] positions = RandomSpreads(16f,16f, 90f, projectiles);
                    for (int i = 0; i < projectiles; i++)
                    {
                        positions[i] *= projVector;
                        CenteroffSet.Y -= Main.rand.NextFloat(1.5f, 5f);
                        Projectile.NewProjectile(CenteroffSet, positions[i], ModContent.ProjectileType<HeroRollerSplash>(), 125, 3f, projectile.owner);
                        specialCounter += 1;
                    }
                }
            }
        private Vector2[] RandomSpreads(float speedX,float speedY, float angle, int num)
        {

            var posArray = new Vector2[num];
            
            float spread = (float)(angle * 0.0174532925);
            float baseSpeed = (float)System.Math.Sqrt(speedX * speedX + speedY * speedY);
            double baseAngle = System.Math.Atan2(speedX, speedY);
            double randomAngle;
            for (int i = 0; i < num; ++i)
            {
                randomAngle = baseAngle + (Main.rand.NextFloat() - 0.5f) * spread;
                posArray[i] = new Vector2(baseSpeed * (float)System.Math.Sin(randomAngle), baseSpeed * (float)System.Math.Cos(randomAngle));
            }
            return (Vector2[])posArray;
        }
        protected override void SubAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<SuctionBomb>(), (projectile.damage), projectile.knockBack, projectile.owner);
                specialCounter += 10;
            }
            if (projectile.frame == AttackTypes[1].GetEndFrame()) 
            {
                SubActive = false;
            }
        }

        protected override void SpecialAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.direction < 0)
            {
                CenteroffSet.X -= 30f;
            }
            else
            {
                CenteroffSet.X += 30f;
            }
            CenteroffSet.Y -= projectile.height*0.5f;

            if (projectile.frame == throwingframe)
            {                
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Inkling/Callie/Callie_"+Main.rand.Next(0,5)), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<KillerWail>(), 50, projectile.knockBack, projectile.owner);
                specialCounter = 0;
            }
            specialused = projectile.frame == AttackTypes[2].GetEndFrame();


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
                    if (projectile.velocity.X > 1f || projectile.velocity.X < -1f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(4, 9);
                    }
                    else
                    {
                        projectile.frame = 0;
                        FacePlayer();
                    }
                    break;
                case InklingStates.PRIMARY:
                    FrameSpeed = (int)AttackTypes[0].GetDuration();
                    if (projectile.frame < AttackTypes[0].GetEndFrame())
                    {
                        projectile.velocity.X = 0f;
                    }
                    PlayerAnimation(AttackTypes[0].GetStartFrame(), AttackTypes[0].GetEndFrame());
                    //AnimateState(AttackTypes[0].GetStartFrame(), AttackTypes[0].GetEndFrame(), InklingStates.WAIT);
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
                    FrameSpeed = (int)AttackTypes[1].GetDuration();
                    PlayerAnimation(AttackTypes[1].GetStartFrame(), AttackTypes[1].GetEndFrame());
                    break;
                case InklingStates.SPECIAL:
                    FrameSpeed = (int)AttackTypes[2].GetDuration();
                    AnimateState(AttackTypes[2].GetStartFrame(), AttackTypes[2].GetEndFrame(),InklingStates.PRIMARY);
                    break;
                case InklingStates.ROLLER_DOWN:
                    FrameSpeedOnVelocity();
                    PlayerAnimation(10, 15);
                    if (projectile.velocity.X > 0.5f || projectile.velocity.X < -0.5f)
                    {
                        projectile.spriteDirection = projectile.direction;
                    }
                    break;
                default:
                    break;
            }
        }
       
        private void FrameSpeedOnVelocity()
        {
            if (projectile.velocity.X < -8f || projectile.velocity.X > 8f)
            {
                FrameSpeed = 5;
            }
            else if (projectile.velocity.X < -5f || projectile.velocity.X > 5f)
            {
                FrameSpeed = 7;
            }
            else if (projectile.velocity.X < -3f || projectile.velocity.X > 3f)
            {
                FrameSpeed = 9;
            }
            else if (projectile.velocity.X > -1f && projectile.velocity.X < 1f)
            {
                FrameSpeed = 24;
            }
        }

    }
}
