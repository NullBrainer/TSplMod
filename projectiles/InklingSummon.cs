﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using SplatoonMod.Util;

namespace SplatoonMod.projectiles
{
    public class InklingSummon : ModProjectile
    {
        protected InklingStates InklingState;
        protected Vector2 CenteroffSet;
        protected readonly float Gravity = 0.6f, TerminalVelocity = 10f;
        protected int SquidBuffType;
        protected float TargetingAngle, TargetDetectRange, targetDistance;
        protected float speed, inertia, maxspeed, defaultInertia, defaultspeed, projectilespeed, primaryprojectileespeed, primaryAccuracy, subAccuracy, specialAccuracy;
        protected Vector2 target, FiringPosition;
        protected NPC targetnpc;
        protected readonly float FollowRange = 64f, MaxDistance = 800f;
        protected int FrameSpeed = 7;
        protected bool SubActive = false, specialused = false, foundTarget = false, FiringDestinationReached = false, FiringDestSet = false;
        protected int specialReq = 180;
        protected int specialCounter = 0;
        protected float CooldownLimit, SubChanceTimer, MaxSubTime, SpecialDuration;
        protected float Timer;

        protected SummonAttack[] AttackTypes;

        protected void SetInklingState(InklingStates newstate)
        {
            InklingState = newstate;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Inkling_Girl");
            Main.projFrames[projectile.type] = 19;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {

            projectile.netImportant = true;
            projectile.width = 30;//66
            projectile.height = 30; //70
            projectile.tileCollide = true;
            projectile.friendly = true;
            projectile.minion = true;
            projectile.minionSlots = 1f;
            projectile.penetrate = -1;
            SetInklingState(InklingStates.JUMPING);
            drawOriginOffsetY = -27;
            drawOffsetX = -20;
            speed = 3f;
            defaultInertia = 20f;
            inertia = 20f;
            maxspeed = 6f;
            defaultspeed = 5f;
            SubChanceTimer = 0f;
            CooldownLimit = 0f;
            MaxSubTime = 30f;
            Timer = 0f;
            SpecialDuration = 300f;
            projectilespeed = 16f;
            primaryprojectileespeed = 16f;
            primaryAccuracy = 20f;
            subAccuracy = 25f;
            specialAccuracy = 25f;
            TargetDetectRange = 400f;
            targetDistance = 80f;
            AttackTypes = new SummonAttack[] { new SummonAttack(this, 1, 5, 6f), new SummonAttack(this, 16, 18, 10f), new SummonAttack(this, 16, 18, 6f) };//i dont wana put it here :C
        }


        public override bool MinionContactDamage()
        {
            return false;
        }


        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (InklingState != InklingStates.FLYING)
            {
                projectile.velocity.Y += Gravity;
            }

            if (projectile.velocity.Y >= TerminalVelocity)
            {
                projectile.velocity.Y = TerminalVelocity;
            }
            if (player.dead || !player.active)
            {
                player.ClearBuff(SquidBuffType);
            }
            if (player.HasBuff(SquidBuffType))
            {
                projectile.timeLeft = 2;
            }
            Vector2 idlePosition = player.Center;

            // The index is projectile.minionPos
            float minionPositionOffsetX = (40 + projectile.minionPos * 40) * -player.direction;
            idlePosition.X += minionPositionOffsetX; // Go behind the player
            //idlePosition.X += (player.direction * -48f);

            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();


            if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
            {
                projectile.position = idlePosition;
                projectile.velocity *= 0.001f;
                projectile.netUpdate = true;
            }
            //PositionSelf(player);
            if (InklingState != InklingStates.RUN || InklingState != InklingStates.PRIMARY || InklingState != InklingStates.SUB || InklingState != InklingStates.SPECIAL)
            {
                //FixOverlap(0.16f);
            }


            target = projectile.position;
            foundTarget = false;
            target = FindTarget(player, targetDistance, target);



            Timer += 1f;
            SubChanceTimer += 1f;
            if (!SubActive && SubChanceTimer >= MaxSubTime)
            {
                SubChanceTimer = 0f;
                SubActive = Main.rand.Next(1, 10) == 1;
            }

            if (specialused)
            {
                projectile.ai[1] += 1f;
                if (projectile.ai[1] >= SpecialDuration)
                {
                    specialCounter = 0;
                    projectile.ai[1] = 0f;
                    specialused = false;
                }
            }
            if (projectile.velocity != Vector2.Zero && InklingState != InklingStates.FLYING)
            {
                UpdateSlopeMovement();
                JumpOverTiles();
            }
            if (Timer >= CooldownLimit)
            {
                CooldownLimit = 0f;
                Timer = 0f;
                SetStates(player, distanceToIdlePosition, vectorToIdlePosition);
                PerformState(distanceToIdlePosition, vectorToIdlePosition);
            }
            Animate(InklingState);
            ResetConditions();
        }
        protected virtual void ResetConditions()
        {
            if (speed >= maxspeed)
            {
                speed = maxspeed;
            }
            else
            {
                speed = defaultspeed;
            }
            FrameSpeed = 7;
            inertia = defaultInertia;
            CenteroffSet = projectile.Center;
            CenteroffSet.Y -= (projectile.height*0.5f);

        }
        protected virtual void UpdateInklingFlying(Vector2 playerposition, float distanceToIdlePosition)
        {

            //projectile.tileCollide = !Collision.CanHitLine(projectile.position, projectile.width, projectile.height, playerposition, projectile.width / 2, projectile.height / 2);
            if (distanceToIdlePosition < FollowRange && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
            {
                projectile.rotation = 0f;
                projectile.tileCollide = true;
                SetInklingState(InklingStates.JUMPING);
            }
            else
            {
                Fly(16f, 20f, playerposition);
                projectile.rotation = projectile.spriteDirection != 1
                ? (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 3.14f
                : (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);
                projectile.tileCollide = false;

            }

        }

        protected virtual void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            if (distanceToIdlePosition > MaxDistance)
            {
                SetInklingState(InklingStates.FLYING);
            }
            else if (InklingState != InklingStates.FLYING)
            {
                if (foundTarget)
                {
                    SetFiringDestination(player);
                    /*//lessthan 320, but move to min distance of 120 when reaching 320
                    //if dis is 120
                    if dist 
                     */
                    float distX = Math.Abs(projectile.position.X - target.X);
                    float distY = Math.Abs(projectile.position.Y - target.Y);
                    float firposdist = (int)Math.Abs(projectile.position.X - FiringPosition.X);
                    FiringDestinationReached = firposdist <= (projectile.width / 2f);// && projectile.velocity.X < 0.5f || projectile.velocity.X > -0.5f;
                    FiringDestSet = distX < 320f && distY < 400f;
                    if ((FiringDestinationReached && FiringDestSet))
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
                            {
                                SetInklingState(InklingStates.PRIMARY);
                            }
                        }
                    }
                    else
                    {
                        SetInklingState(InklingStates.RUN);
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
        private void SetFiringDestination(Player player)
        {
            if (!FiringDestSet)
            {
                FiringPosition = target;
                if (player.position.X > FiringPosition.X)
                {
                    FiringPosition.X += 160f;
                }
                else
                {
                    FiringPosition.X -= 160f;
                }
                FiringDestSet = true;
            }
        }



        /// <summary>
        /// A helper method used for SetStates. Implemented to determine what attack to use
        /// when a target is spotted.
        /// </summary>


        protected void UpdateSlopeMovement()
        {
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);
            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();
        }
        /// <summary>
        /// A helper method used for SetStates. Implemented to determine what movements to use
        /// depending on distance and position.
        /// </summary>
        /// <param name="distanceToIdlePosition"></param>
        /// <param name="vectorToIdlePosition"></param>
        protected virtual void PerformState(float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Vector2 projectileVector = Vector2.One;

            switch (InklingState)
            {
                case InklingStates.RUN:
                    projectile.velocity.X = Approach(FiringPosition - projectile.position).X;
                    break;
                case InklingStates.FOLLOW:
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
                    projectileVector = AimProjectile(target, primaryprojectileespeed, primaryprojectileespeed, primaryAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[2].GetDuration(), 17, 17);
                    CooldownLimit = AttackTypes[0].GetDuration();
                    break;
                case InklingStates.SUB:
                    FaceTarget(target);

                    projectileVector = AimProjectile(target, projectilespeed, projectilespeed, subAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[1].GetDuration(), 17, 17);
                    CooldownLimit = AttackTypes[1].GetDuration();
                    break;
                case InklingStates.SPECIAL:
                    FaceTarget(target);
                    specialused = true;
                    projectileVector = AimProjectile(target, projectilespeed, projectilespeed, specialAccuracy);
                    TimedAttack(target, projectileVector, AttackTypes[0].GetDuration(), 17, 17);
                    CooldownLimit = AttackTypes[0].GetDuration();
                    break;
                case InklingStates.WAIT:
                    projectile.velocity.X = 0f;
                    break;
                default:
                    break;
            }
        }
        protected virtual void Fly(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity = Approach(vectorToIdlePosition);
        }



        protected virtual int RandomPosNeg()
        {
            int result = 1;
            if (Main.rand.NextFloatDirection() < 0)
            {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// A movement function that adjusts speed and inertia based on distance between projectile and player
        /// </summary>
        /// <param name="distanceToIdlePosition"></param>
        /// <param name="vectorToIdlePosition"></param>
        /// <param name="dist"></param>
        protected void FollowPlayer(float distanceToIdlePosition, Vector2 vectorToIdlePosition, float dist)
        {
            inertia = 5f;
            if (distanceToIdlePosition < 5f)
            {
              speed = Math.Abs(Main.player[projectile.owner].velocity.X)*0.1f;               

            }
            else if (distanceToIdlePosition > dist)
            {
                speed += 1f;
            }
            projectile.velocity.X = Approach(vectorToIdlePosition).X;


            int denom = (int)Math.Abs(projectile.velocity.X) + 1;
            FrameSpeed = 30 / denom;

        }



        /// <summary>
        /// The immediate range around the target (when it passively floats about)
        /// Returns a vector representing the speed of the projectile
        /// </summary>
        /// <param name="destination"></param>
        protected virtual Vector2 Approach(Vector2 destination)
        {
            destination.Normalize();
            destination *= speed;
            return (projectile.velocity * (inertia - 1) + destination) / inertia;
        }
        /// <summary>
        /// Fixes overlap with other minions.
        /// Used during AI(). 
        /// </summary>
        /// <param name="overlapVelocity"> </param>
        protected void FixOverlap(float overlapVelocity)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
                {
                    if (projectile.position.X < other.position.X) projectile.velocity.X -= overlapVelocity;
                    else projectile.velocity.X += overlapVelocity;

                    if (projectile.position.Y < other.position.Y) projectile.velocity.Y -= overlapVelocity;
                    else projectile.velocity.Y += overlapVelocity;
                }
            }
        }


        /// <summary>
        /// Vanilla targeting behavior that includes independent searching and player made targeting.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="distfromtarget"></param>
        /// <param name="targetCenter"></param>
        /// <returns></returns>
        protected virtual Vector2 FindTarget(Player player, float distfromtarget, Vector2 targetCenter)
        {
            // This code is required if your minion weapon has the targeting feature
            if (player.HasMinionAttackTargetNPC && projectile.OwnerMinionAttackTargetNPC.CanBeChasedBy(projectile, false))//added projectile.OwnerMinionAttackTargetNPC.CanBeChasedBy(projectile,false)
            {
                NPC target = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(target.Center, projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < TargetDetectRange)//1000f
                {
                    distfromtarget = between;
                    targetCenter = target.Center;
                    if (Collision.CanHit(projectile.position, projectile.width / 2, projectile.height / 2, target.position, target.width / 2, target.height / 2))
                    {
                        foundTarget = true;
                    }

                }
            }
            if (!foundTarget)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC minionTarget = Main.npc[i];// another instance
                    if (minionTarget.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(minionTarget.Center, projectile.Center);
                        bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
                        bool inRange = between < distfromtarget;
                        bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width / 2, projectile.height / 2, minionTarget.position, minionTarget.width / 2, minionTarget.height / 2);

                        //bool closeThroughWall = between < 100f;
                        if (((closest && inRange) || !foundTarget) && lineOfSight)
                        {
                            distfromtarget = between;
                            targetCenter = minionTarget.Center;
                            targetnpc = minionTarget;
                            foundTarget = true;
                        }
                    }
                }
            }
            return targetCenter;
        }

        /// <summary>
        /// Projectile spawning that includes sprite adjustment
        /// </summary>
        /// <param name="targetposition"></param>
        protected virtual void TimedAttack(Vector2 targetposition, Vector2 projVector, float Cooldown, int subthrowframe, int specialthrowframe)
        {
            //when in the combat state, check again after it's finished
            AdjustProjectileSpawn();
            projectile.netUpdate = true;

            switch (InklingState)
            {
                case InklingStates.PRIMARY:
                    DefaultAttack(projVector, targetposition);
                    break;
                case InklingStates.SUB:
                    SubAttack(projVector, targetposition, subthrowframe);
                    break;
                case InklingStates.SPECIAL:
                    SpecialAttack(projVector, targetposition, specialthrowframe);
                    break;
                default:
                    break;
            }
        }
        protected void FaceTarget(Vector2 targetposition)
        {
            if (targetposition.X - projectile.Center.X < 0)
            {
                projectile.direction = -1;
            }
            else
            {
                projectile.direction = 1;
            }
            projectile.spriteDirection = projectile.direction;

        }

        protected float RadToDegree(float Degree)
        {
            return (180f / (float)Math.PI) * Degree;
        }

        /// <summary>
        /// Used for Attack function
        /// </summary>
        /// <param name="targetposition"></param>
        /// <returns></returns>


        /// <summary>
        /// Non inclusive comparison and inclusive comparison.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <returns>True if value is greater than min up to max</returns>
        protected bool WithinValues(float min, float value, float max)
        {
            return (value > min && value <= max);
        }

        /// <summary>
        /// Assigns projectile frame based on angle using degrees
        /// </summary>
        /// <param name="angle"></param>
        protected void SetAttackAnimation(float angle)
        {

            if (WithinValues(-120f, angle, -70f))
            {
                projectile.frame = 1;
            }
            else if (WithinValues(-70f, angle, -30f) || WithinValues(-150, angle, -120))
            {
                projectile.frame = 2;
            }
            else if ((angle >= -30 && angle <= 30f) || (angle >= -180 && angle <= -150) || (angle >= 150 && angle <= 180))
            {
                projectile.frame = 3;
            }
            else if (WithinValues(30f, angle, 70f) || WithinValues(120, angle, 150))
            {
                projectile.frame = 4;
            }
            else if (WithinValues(70f, angle, 120f))
            {
                projectile.frame = 5;
            }
        }

        protected virtual void DefaultAttack(Vector2 projVector, Vector2 targetposition)
        {
            specialCounter++;
            Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Shooter0" + Main.rand.Next(0, 2)), 0.5f);
            Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<SplatterShotProjectile>(), projectile.damage, projectile.knockBack, projectile.owner);
            TargetingAngle = RadToDegree(projectile.DirectionTo(targetposition).ToRotation());
        }
        protected virtual void SubAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<BurstBomb>(), (projectile.damage), projectile.knockBack, projectile.owner);
                specialCounter += 10;
            }
            if (projectile.frame == throwingframe + 1)
            {
                SubActive = false;
            }
        }

        protected virtual void SpecialAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {

            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<BurstBomb>(), (projectile.damage), projectile.knockBack, projectile.owner);
            }
        }

        protected virtual void AdjustProjectileSpawn()
        {
            if (projectile.direction < 0)
            {
                CenteroffSet.X += 5f;
            }
            else
            {
                CenteroffSet.X -= 5f;
            }
        }
        protected virtual Vector2 AimProjectile(Vector2 targetposition, float Xcomp, float Ycomp, float accuracy)
        {
            Vector2 projVector = RandomSpread(Xcomp, Ycomp, accuracy);
            if (accuracy == 0f)
            {
                projVector = new Vector2(Xcomp, Ycomp);
            }
            /** Vector2 dest = targetposition;
             dest.Y -= PositionOffset(targetposition) + targetnpc.velocity.Y;//(targetnpc.velocity.Y * targetnpc.velocity.Y) * targetnpc.directionY; 
             dest.X += targetnpc.velocity.X * targetnpc.velocity.X * targetnpc.direction;
             projVector *= projectile.DirectionTo(dest);
            */
            //
            Vector2 dest = targetposition;
            dest.X += (targetnpc.velocity.X * targetnpc.velocity.X) * targetnpc.direction;
            dest.Y -= (targetnpc.velocity.Y * targetnpc.velocity.Y) * targetnpc.directionY;

            float dist = projectile.Distance(dest);
            float time = (dist / primaryprojectileespeed);
            //TargetingAngle = RadToDegree(projectile.DirectionTo(dest).ToRotation());
            dest.Y -= (3f * time) * 0.5f;
            //dest.X += (time*projectile.direction);

            projVector *= projectile.DirectionTo(dest);

            return projVector;
        }

        protected float PositionOffset(Vector2 targetposition)
        {
            float dist = projectile.Distance(targetposition);
            float magnitude = (float)Math.Sqrt((dist * dist) + (dist * dist)) * 0.16f;
            return magnitude;
        }

        protected static Vector2 RandomSpread(float speedX, float speedY, float angle)
        {

            float spread = (float)(angle * 0.0174532925);
            float baseSpeed = (float)System.Math.Sqrt((speedX * speedX) + (speedY * speedY));
            double baseAngle = System.Math.Atan2(speedX, speedY);
            double randomAngle = baseAngle + (Main.rand.NextFloat() - 0.5f) * spread;
            Vector2 altaredvector = new Vector2(baseSpeed * (float)Math.Sin(randomAngle), baseSpeed * (float)Math.Cos(randomAngle));

            return altaredvector;
        }

        protected bool SpecialActive(int Max)
        {
            return specialCounter >= Max;
        }

        /// <summary>
        /// Projectile frame is adjusted depending on the current state.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void Animate(InklingStates state)
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
                        FacePlayer();
                        projectile.frame = 0;
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
                    projectile.frame = 13;
                    break;
                case InklingStates.PRIMARY:
                    projectile.velocity.X = 0f;
                    SetAttackAnimation(TargetingAngle);
                    break;
                case InklingStates.FLYING:
                    PlayerAnimation(14, 15);
                    break;
                case InklingStates.SUB:
                    projectile.velocity.X = 0;
                    FrameSpeed = (int)AttackTypes[1].GetDuration();
                    PlayerAnimation(16, 18);
                    break;
                case InklingStates.SPECIAL:
                    projectile.velocity.X = 0;
                    FrameSpeed = (int)AttackTypes[2].GetDuration();
                    PlayerAnimation(16, 18);
                    break;
                default:
                    break;
            }
        }

        protected void FacePlayer()
        {
            if (projectile.position.X < Main.player[projectile.owner].position.X)
            {
                projectile.direction = 1;
            }
            else
            {
                projectile.direction = -1;
            }
            projectile.spriteDirection = projectile.direction;
        }

        /// <summary>
        /// Animates the projectile by playing frames within parameters given
        /// Frame rate is affected by class property FrameSpeed.
        /// </summary>
        /// <param name="startframe">Starting frame of animation</param>
        /// <param name="endframe">Ending frame of animation</param>
        protected virtual void PlayerAnimation(int startframe, int endframe)
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

        protected void AnimateState(int startframe, int endframe, InklingStates newstate)
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
                    SetInklingState(newstate);
                }
            }
        }


        /// <summary>
        /// Tile detection and vertical adjustment over obstacles
        /// </summary>
        protected virtual void JumpOverTiles()
        {
            var XVector = (int)((double)projectile.position.X + (double)(projectile.width / 2)) / 16;
            var yVec = (int)((double)projectile.position.Y + (double)(projectile.height / 2)) / 16;

            if (projectile.direction < 0)
                --XVector;
            if (projectile.direction > 0)
                ++XVector;

            if (((double)projectile.velocity.X < 0.0 || (double)projectile.velocity.X > 0.0))
            {
                var i = (int)((double)projectile.position.X + (double)(projectile.width / 2)) / 16;
                var jey = (int)((double)projectile.position.Y + (double)(projectile.height / 2)) / 16 + 1;
                WorldGen.SolidTile(i, jey);
            }
            if (WorldGen.SolidTile(XVector + (int)projectile.velocity.X, yVec))
            {
                var i1 = (int)((double)projectile.position.X + (double)(projectile.width / 2)) / 16;
                var j = (int)((double)projectile.position.Y + (double)projectile.height) / 16 + 1;
                if (WorldGen.SolidTile(i1, j) || Main.tile[i1, j].halfBrick() || (Main.tile[i1, j].slope() > (byte)0))
                {

                    try
                    {
                        var Xvector = (int)((double)projectile.position.X + (double)(projectile.width / 2)) /
                                   16;
                        var yVector = (int)((double)projectile.position.Y + (double)(projectile.height / 2)) /
                                   16;
                        if (projectile.direction < 0)
                            --Xvector;
                        if (projectile.direction > 0)
                            ++Xvector;
                        var i2 = Xvector + (int)projectile.velocity.X;
                        if (!WorldGen.SolidTile(i2, yVector - 1) && !WorldGen.SolidTile(i2, yVector - 2))
                            projectile.velocity.Y = -6.1f;
                        else if (!WorldGen.SolidTile(i2, yVector - 2))
                            projectile.velocity.Y = -8.1f;
                        else if (WorldGen.SolidTile(i2, yVector - 5))
                            projectile.velocity.Y = -13.1f;
                        else if (WorldGen.SolidTile(i2, yVector - 4))
                            projectile.velocity.Y = -15.1f;
                        else
                            projectile.velocity.Y = -10.1f;


                    }
                    catch
                    {
                        projectile.velocity.Y = -10.1f;
                    }


                    if (projectile.type == (int)sbyte.MaxValue)
                        projectile.ai[0] = 1f;
                }
            }
        }

    }
}