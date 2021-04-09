using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace SplatoonMod.projectiles
{
    public class InklingSummon : ModProjectile
    {
        protected InklingStates InklingState;
        protected Vector2 CenteroffSet;
        protected readonly float Gravity = 0.6f;
        protected readonly float TerminalVelocity = 10f;
        protected int SquidBuffType;
        protected float TargetingAngle;
        protected float speed, inertia, maxspeed;
        protected Vector2 target;
        protected NPC targetnpc;
        protected bool foundTarget = false;
        protected readonly float FollowRange = 48f;
        protected readonly float MaxDistance = 800f;
        protected int FrameSpeed = 7;
        protected bool SubActive = false;
        protected int specialReq = 180;
        protected int specialCounter = 0;
        



        protected void SetInklingState(InklingStates newstate)
        {
            InklingState = newstate;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Inkling_Girl");
            Main.projFrames[projectile.type] = 22;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            CenteroffSet = projectile.Center;
        }


        public override void SetDefaults()
        {

            projectile.netImportant = true;
            projectile.width = 40;//66
            projectile.height = 40; //70
            projectile.tileCollide = true;
            projectile.friendly = true;
            projectile.minion = true;
            projectile.minionSlots = 1f;
            projectile.penetrate = -1;
            SetInklingState(InklingStates.JUMPING);
            drawOriginOffsetY = -30;
            drawOffsetX = -20;
            projectile.scale = 0.65f;
            speed = 3f;
            inertia = 20f;
            maxspeed = 6f;
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
            float minionPositionOffsetX = (10 + projectile.minionPos * 40) * -player.direction;
            idlePosition.X += minionPositionOffsetX; // Go behind the player
            idlePosition.X += (player.direction * -48f);

            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();


            if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
            {
                projectile.position = idlePosition;
                projectile.velocity *= 0.1f;
                projectile.netUpdate = true;
            }

            FixOverlap(0.04f);


            // Starting search distance
            float distanceFromTarget = 320f;
            target = projectile.position;
            foundTarget = false;
            target = FindTarget(player, distanceFromTarget, target);

           


            if (!SubActive)
            {
                SubActive = Main.rand.Next(1, 300) == 1;
            }


            SetStates(player, distanceToIdlePosition, vectorToIdlePosition);
            Animate(InklingState);
            ResetConditions();
        }
        protected virtual void ResetConditions()
        {
            if (speed >= maxspeed)
            {
                speed = maxspeed;
            }
            FrameSpeed = 7;            
            inertia = 20f;
        }
        protected virtual void UpdateInklingFlying(Vector2 playerposition, float distanceToIdlePosition)
        {

            //projectile.tileCollide = !Collision.CanHitLine(projectile.position, projectile.width, projectile.height, playerposition, projectile.width / 2, projectile.height / 2);
            if (distanceToIdlePosition >= 0 && distanceToIdlePosition < FollowRange && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
            {
                projectile.rotation = 0f;
                projectile.tileCollide = true;
                SetInklingState(InklingStates.JUMPING);
            }
            else
            {
                Fly(16f, 8f, playerposition);
                projectile.rotation = projectile.spriteDirection != 1
                ? (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 3.14f
                : (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);
                projectile.tileCollide = false;//active only specific frames?

            }
        }

        protected virtual void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);
            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();
            
            switch (InklingState)
            {
                case InklingStates.FOLLOW:
                    JumpOverTiles();
                    FollowPlayer(distanceToIdlePosition,vectorToIdlePosition,FollowRange);
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
                else if ( (projectile.velocity.X < 1f || projectile.velocity.X > -1f))// && distanceToIdlePosition < FollowRange)//(Math.Abs(projectile.position.X - player.position.X) <= FollowRange)
                {
                        SetInklingState(InklingStates.IDLE);
                }

            }

        }

        protected virtual void Fly(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity = Approach(vectorToIdlePosition);
        }

        protected virtual void Idle()
        {
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= Main.rand.NextFloat(30f, 180f))
            {
                projectile.ai[0] = 0f;
                projectile.netUpdate = true;
                if (InklingState == InklingStates.IDLE)
                {
                    projectile.velocity.X = (speed * RandomPosNeg());
                    SetInklingState(InklingStates.RUN);
                }
                else
                {
                    projectile.velocity.X = 0;
                    SetInklingState(InklingStates.IDLE);
                }
            }
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
        protected virtual void FollowPlayer(float distanceToIdlePosition, Vector2 vectorToIdlePosition, float dist)
        {
            if (distanceToIdlePosition < dist)
            {
                speed -= 0.01f;
            }
            else if(distanceToIdlePosition == dist)
            {
                speed = Math.Abs(Main.player[projectile.owner].velocity.X);
            }
            else
            {
                speed += 0.01f;
            }
            if (projectile.velocity.X > 4f || projectile.velocity.X < -4f)
            {
                FrameSpeed = 6;
            }else if (projectile.velocity.X > 2f || projectile.velocity.X < -2f)
            {
                FrameSpeed = 9;
            }
            else
            {
                FrameSpeed = 12;
            }
            if (projectile.velocity.X > 1f || projectile.velocity.X < -1f)
            {
                projectile.velocity.X = Approach(vectorToIdlePosition).X;
            }
            else
            {
                if (projectile.position.X < Main.player[projectile.owner].position.X)
                {
                    projectile.direction = 1;
                }
                else
                {
                    projectile.direction = -1;
                }
                projectile.velocity.X = projectile.direction*1.25f;
            }
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
        /// <param name="distanceFromTarget"></param>
        /// <param name="targetCenter"></param>
        /// <returns></returns>
        protected virtual Vector2 FindTarget(Player player, float distanceFromTarget, Vector2 targetCenter)
        {
            // This code is required if your minion weapon has the targeting feature
            if (player.HasMinionAttackTargetNPC && projectile.OwnerMinionAttackTargetNPC.CanBeChasedBy(projectile, false))//added projectile.OwnerMinionAttackTargetNPC.CanBeChasedBy(projectile,false)
            {
                NPC target = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(target.Center, projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 1000f)//1000f
                {
                    distanceFromTarget = between;
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
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width / 2, projectile.height / 2, minionTarget.position, minionTarget.width / 2, minionTarget.height / 2);

                        //bool closeThroughWall = between < 100f;
                        if (((closest && inRange) || !foundTarget) && lineOfSight)
                        {
                            distanceFromTarget = between;
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
        protected virtual void TimedAttack(Vector2 targetposition, float Cooldown)
        {

            projectile.ai[0] += 1f;
            if (targetposition.X - projectile.Center.X < 0)
            {
                projectile.direction = -1;
            }
            else
            {
                projectile.direction = 1;
            }
            projectile.spriteDirection = projectile.direction;

            if (projectile.ai[0] >= Cooldown)
            {
                projectile.ai[0] = 0f;
                projectile.netUpdate = true;
                Vector2 projVector = RandomSpread(16f, 16f, 15f);
                targetposition.Y -= PositionOffset(targetposition);
                projVector *= projectile.DirectionTo(targetposition);
                CenteroffSet = projectile.Center;
                CenteroffSet.Y -= projectile.height * 0.10f;
                if (projectile.direction < 0)
                {
                    CenteroffSet.X += 5f;
                }
                else
                {
                    CenteroffSet.X -= 5f;
                }

                switch (InklingState)
                {
                    case InklingStates.PRIMARY:
                        DefaultAttack(projVector, targetposition);
                        break;
                    case InklingStates.SUB:
                        SubAttack(projVector, targetposition, 17);
                        break;
                    case InklingStates.SPECIAL:
                        SpecialAttack(projVector, targetposition, 20);
                        break;
                    default:
                        break;
                }
            }
        }


        protected float DegreeToRad(float Degree)
        {
            return (180f / (float)Math.PI) * Degree;
        }

        /// <summary>
        /// Used for Attack function
        /// </summary>
        /// <param name="targetposition"></param>
        /// <returns></returns>
        protected float PositionOffset(Vector2 targetposition)
        {
            float Xcomp = projectile.Distance(targetposition);
            float ycomp = targetnpc.height;// targetnpc.height;
            float magnitude = (float)Math.Sqrt((Xcomp * Xcomp) + (ycomp * ycomp));
            //20 tiles
            return (magnitude * 0.2f);
        }

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
            TargetingAngle = DegreeToRad(projectile.DirectionTo(targetposition).ToRotation());
        }
        protected virtual void SubAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {
            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<BurstBomb>(), (projectile.damage ), projectile.knockBack, projectile.owner);
                SubActive = false;
                specialCounter += 10;
            }
        }

        protected virtual void SpecialAttack(Vector2 projVector, Vector2 targetposition, int throwingframe)
        {

            if (projectile.frame == throwingframe)
            {
                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Bombs/BombFly"), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<BurstBomb>(), (projectile.damage ), projectile.knockBack, projectile.owner);
            }
        }

        protected static Vector2 RandomSpread(float speedX, float speedY, float angle)
        {

            float spread = (float)(angle * 0.0174532925);
            float baseSpeed = (float)System.Math.Sqrt(speedX * speedX + speedY * speedY);
            double baseAngle = System.Math.Atan2(speedX, speedY);
            double randomAngle;

            randomAngle = baseAngle + (Main.rand.NextFloat() - 0.5f) * spread;
            Vector2 altaredvector = new Vector2(baseSpeed * (float)Math.Sin(randomAngle), baseSpeed * (float)Math.Cos(randomAngle));

            return altaredvector;
        }

        protected virtual bool SpecialActive(int Max)
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
                    if(projectile.velocity.X != 0f)
                    {
                        projectile.spriteDirection = projectile.direction;
                        PlayerAnimation(6, 11);
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
                    SetAttackAnimation(TargetingAngle);
                    break;
                case InklingStates.FLYING:
                    projectile.tileCollide = false;
                    PlayerAnimation(14, 15);
                    break;
                case InklingStates.SUB:
                    FrameSpeed = 6;
                    PlayerAnimation(16, 18);
                    break;
                case InklingStates.SPECIAL:
                    FrameSpeed = 6;
                    PlayerAnimation(19, 21);
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
                if (projectile.frame < endframe)//frames don't increment
                {
                    projectile.frame++;
                }
                else if (projectile.frame == endframe)
                {
                    projectile.frame = startframe;
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