using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles.Agent3Proj;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.projectiles.SquidRadioProj
{
    public class SquidRadioProj : ModProjectile
    {
        private readonly float TerminalVelocity = 16f;
        private InklingStates InklingState;
        private Vector2 target;
        private NPC targetnpc;
        private float TargetingAngle;
        private float speed;
        private float inertia;
        private bool foundTarget = false;
        private readonly float Gravity = 0.6f;
        private readonly float FollowRange = 48f;
        private readonly float MaxDistance = 480;
        private Vector2 CenteroffSet;
        int frameSpeed = 6;


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Inkling");
            Main.projFrames[projectile.type] = 18;
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
            InklingState = InklingStates.JUMPING;
            projectile.scale = 0.89f;

            drawOriginOffsetY = -30;
            drawOffsetX = -20;


        }

        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }

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

        private void UpdateInklingFlying(Vector2 playerposition)
        {
            if (InklingState == InklingStates.FLYING && !Collision.CanHitLine(projectile.position, projectile.width, projectile.height, playerposition, projectile.width / 2, projectile.height / 2))
            {

                projectile.rotation = projectile.spriteDirection != 1
                                ? (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 3.14f
                                : (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);
                projectile.velocity.Y -= Gravity;
                projectile.tileCollide = false;
            }
            else
            {

                projectile.rotation = 0f;
                projectile.tileCollide = true;
            }
        }

        private void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            float StepSpeed = 2f, gfxOffY = 0f;
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref StepSpeed, ref gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);

            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();
            JumpOverTiles();

            if (foundTarget && targetnpc != null)
            {

                projectile.velocity.X = Approach(target - projectile.Center).X;
                InklingState = InklingStates.FOLLOWING;
                if (projectile.Distance(target) <= 320f)
                {
                    projectile.velocity.X = 0;
                    Attack(target);
                }
            }
            else if (InklingState == InklingStates.FLYING && distanceToIdlePosition <= 0f)
            {
                InklingState = InklingStates.JUMPING;
            }
            else if (InklingState == InklingStates.JUMPING && projectile.oldVelocity.Y == 0 && distanceToIdlePosition <= 0f)
            {
                InklingState = InklingStates.LAND;
                projectile.velocity.X = 0f;
            }
            else if (distanceToIdlePosition > MaxDistance)
            {
                Fly(16f, 2f, vectorToIdlePosition);
            }
            else if (player.velocity.X != 0f)
            {
                FollowPlayer(distanceToIdlePosition, vectorToIdlePosition, FollowRange);
            }
            else if (InklingState != InklingStates.IDLE && (projectile.velocity.X <= 1f && projectile.velocity.X >= -1f))
            {
                projectile.velocity.X = 0f;
                InklingState = InklingStates.IDLE;
            }
            else
            {
                Idle();
            }
        }


        private void Move(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            InklingState = InklingStates.FOLLOWING;
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity.X = Approach(vectorToIdlePosition).X;


        }


        private void Fly(float newspeed, float newinertia, Vector2 vectorToIdlePosition)
        {
            speed = newspeed;
            inertia = newinertia;
            projectile.velocity = Approach(vectorToIdlePosition);
            InklingState = InklingStates.FLYING;

        }
        private void Idle()
        {
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= Main.rand.NextFloat(30f, 180f))
            {
                projectile.ai[0] = 0f;
                projectile.netUpdate = true;
                if (InklingState == InklingStates.IDLE)
                {
                    projectile.velocity.X = (5f * RandomPosNeg());
                    InklingState = InklingStates.FOLLOWING;
                }
                else
                {
                    projectile.velocity.X = 0;
                    InklingState = InklingStates.IDLE;
                }
            }
        }
        private int RandomPosNeg()
        {
            int result = 1;
            if (Main.rand.NextFloatDirection() < 0)
            {
                result = -1;
            }
            return result;
        }
        private void FollowPlayer(float distanceToIdlePosition, Vector2 vectorToIdlePosition, float maxdistance)
        {
            InklingState = InklingStates.FOLLOWING;
            if (distanceToIdlePosition > maxdistance)
            {
                speed = 6f;
                inertia = 2f;

            }
            else
            {
                speed = 5f;
                inertia = 2f;
            }
            projectile.velocity.X = Approach(vectorToIdlePosition).X;

        }

        /// <summary>
        /// The immediate range around the target (when it passively floats about)
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="speed"></param>
        /// <param name="inertia"></param>
        /// <returns></returns>
        private Vector2 Approach(Vector2 destination)
        {
            destination.Normalize();
            destination *= speed;
            return (projectile.velocity * (inertia - 1) + destination) / inertia;
        }

        private void FixOverlap(float overlapVelocity)
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

        private Vector2 FindTarget(Player player, float distanceFromTarget, Vector2 targetCenter)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(target.Center, projectile.Center);
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = target.Center;
                    foundTarget = true;

                }
            }
            if (!foundTarget)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC minionTarget = Main.npc[i];
                    if (minionTarget.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(minionTarget.Center, projectile.Center);
                        bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, minionTarget.position, minionTarget.width, minionTarget.height);
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


        private void Attack(Vector2 targetposition)
        {
            InklingState = InklingStates.FIGHTING;

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
            if (projectile.ai[0] >= 6f)
            {
                projectile.ai[0] = 0f;
                projectile.netUpdate = true;
                Vector2 projVector = RandomSpread(16f, 16f, 25f);

                targetposition.Y -= PositionOffset(targetposition);
                projVector *= projectile.DirectionTo(targetposition);
                CenteroffSet = projectile.Center;
                CenteroffSet.Y -= projectile.height * 0.10f;
                if (projectile.direction < 0)
                {
                    CenteroffSet.X += 10f;
                }
                else
                {
                    CenteroffSet.X -= 10f;
                }

                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.position.X, (int)projectile.position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Weapon/Shooter0" + Main.rand.Next(0, 2)), 0.5f);
                Projectile.NewProjectile(CenteroffSet, projVector, ModContent.ProjectileType<HeroShotProjectile>(), projectile.damage, projectile.knockBack, projectile.owner);
                TargetingAngle = DegreeToRad(projectile.DirectionTo(targetposition).ToRotation());
            }
        }
        private float DegreeToRad(float Degree)
        {
            return (180f / (float)Math.PI) * Degree;
        }
        private float PositionOffset(Vector2 targetposition)
        {
            
            float Xcomp = projectile.Distance(targetposition);
            float ycomp = targetnpc.height;           
            float magnitude = (float)Math.Sqrt((Xcomp * Xcomp) + (ycomp * ycomp));
            return (magnitude * 0.16f) + (targetnpc.height * 0.25f);

        }

        private bool WithinValues(float min, float value, float max)
        {
            return (value > min && value <= max);
        }
        private void SetAttackAnimation(float angle)
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
            else if (WithinValues(30f, angle, 60f) || WithinValues(120, angle, 150))
            {
                projectile.frame = 4;
            }
            else if (WithinValues(70f, angle, 120f))
            {
                projectile.frame = 5;
            }
        }

        public static Vector2 RandomSpread(float speedX, float speedY, float angle)
        {
            _ = new Vector2();
            float spread = (float)(angle * 0.0174532925);
            float baseSpeed = (float)System.Math.Sqrt(speedX * speedX + speedY * speedY);
            double baseAngle = System.Math.Atan2(speedX, speedY);
            double randomAngle;

            randomAngle = baseAngle + (Main.rand.NextFloat() - 0.5f) * spread;
            Vector2 altaredvector = new Vector2(baseSpeed * (float)Math.Sin(randomAngle), baseSpeed * (float)Math.Cos(randomAngle));

            return altaredvector;
        }

        private void Animate(InklingStates state)
        {
            switch (state)
            {
                case InklingStates.IDLE:
                    projectile.frame = 0;
                    break;
                case InklingStates.FOLLOWING:
                    projectile.spriteDirection = projectile.direction;
                    PlayerAnimation(6, 11);
                    break;
                case InklingStates.JUMPING:
                    projectile.frame = 12;
                    break;
                case InklingStates.LAND:
                    projectile.frame = 13;
                    break;
                case InklingStates.FIGHTING:
                    SetAttackAnimation(TargetingAngle);
                    break;
                case InklingStates.FLYING:
                    PlayerAnimation(14, 17);
                    break;
                default:
                    break;
            }
        }
        private void PlayerAnimation(int startframe, int endframe)
        {
            if (projectile.frame < startframe || projectile.frame >= endframe)
            {
                projectile.frame = startframe;
            }
            projectile.frameCounter++;
            if (projectile.frameCounter >= frameSpeed)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
            }

        }

        private void JumpOverTiles()
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

                        InklingState = InklingStates.JUMPING;
                    }
                    catch
                    {
                        projectile.velocity.Y = -10.1f;
                        InklingState = InklingStates.JUMPING;
                    }


                    if (projectile.type == (int)sbyte.MaxValue)
                        projectile.ai[0] = 1f;
                }
            }
        }

    }
}

