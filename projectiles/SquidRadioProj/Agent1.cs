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
            projectile.width = 45;//66
            projectile.height = 45; //70
            drawOriginOffsetY = -39;
            drawOffsetX = -20;
            maxspeed = 10f;
            defaultInertia = 20f;
            CooldownLimit = 24f;
            AttackTypes = new SummonAttack[] { new SummonAttack(this, 1, 3, 20f), new SummonAttack(this, 19, 21, 15f), new SummonAttack(this, 22, 24, 15f) };
        }
        protected override void SetStates(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
            Vector4 SlopedCollision = Collision.SlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, 1f, false);
            projectile.position = SlopedCollision.XY();
            projectile.velocity = SlopedCollision.ZW();
            MovementContext(distanceToIdlePosition, vectorToIdlePosition);
            CheckCondition(player, distanceToIdlePosition, vectorToIdlePosition);
        }
        protected void CheckCondition(Player player, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            if (distanceToIdlePosition > MaxDistance)
            {
                SetInklingState(InklingStates.FLYING);
            }
            else if (InklingState != InklingStates.FLYING)
            {
                if (foundTarget)
                {

                    if (SubActive)
                    {
                        SetInklingState(InklingStates.SUB);
                    }
                    else if ((Vector2.Distance(projectile.Center, target) <= 32f && (projectile.velocity.X < 0.05f || projectile.velocity.X > -0.05f)) || (InklingState != InklingStates.ROLLER_DOWN && Vector2.Distance(projectile.Center, target) > 32f))
                    {

                        SetInklingState(InklingStates.PRIMARY);
                    }
                    CombatContext();
                }
                else if ((projectile.velocity.Y > 5f || projectile.velocity.Y < -5f) && (projectile.velocity.X < 1f || projectile.velocity.X > -1f))
                {
                    SetInklingState(InklingStates.JUMPING);
                }
                else if (Math.Abs(projectile.position.X - player.position.X) >= FollowRange)//distanceToIdlePosition > FollowRange)//distanceToIdlePosition > FollowRange || (player.velocity.X < -1f || player.velocity.X > 1f) && player.velocity.X != 0)
                {
                    SetInklingState(InklingStates.FOLLOW);
                }
                else if ((projectile.velocity.X < 1f || projectile.velocity.X > -1f) && (player.velocity.X < 1f || player.velocity.X > -1f))// && distanceToIdlePosition < FollowRange)//(Math.Abs(projectile.position.X - player.position.X) <= FollowRange)
                {
                    SetInklingState(InklingStates.IDLE);
                }
            }
        }
        /*protected override void MovementContext(float distanceToIdlePosition, Vector2 vectorToIdlePosition)
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
                case InklingStates.LAND:
                    projectile.velocity.X = 0;
                    break;
                case InklingStates.FLYING:
                    UpdateInklingFlying(vectorToIdlePosition, distanceToIdlePosition);
                    break;
                default:
                    break;
            }
        }*/

        protected override void CombatContext()
        {
                Vector2 projectileVector = Vector2.One;
            switch (InklingState)
            {
                case InklingStates.PRIMARY:
                    projectile.velocity.X = projectile.direction * 0.001f;
                    TimedAttack(target, projectileVector, 20f, 20, 23);
                    if (projectile.frame == 3)
                    {
                        SetInklingState(InklingStates.ROLLER_DOWN);
                    }
                    break;
                case InklingStates.SUB:
                    projectile.velocity.X = projectile.direction * 0.001f;
                    TimedAttack(target, projectileVector, 15f, 20, 23);
                    break;
                case InklingStates.SPECIAL:
                    projectile.velocity.X = projectile.direction * 0.001f;
                    break;
                case InklingStates.ROLLER_DOWN:
                    speed = 10f;
                    inertia = 35f;
                    projectile.velocity.X = Approach(target - projectile.Center).X;
                    break;
                default:
                    break;
            }
        }
        public override bool MinionContactDamage()
        {
            return InklingState == InklingStates.ROLLER_DOWN && (projectile.velocity.X > 7f || projectile.velocity.X < -7f);
        }

        protected override void TimedAttack(Vector2 targetposition, Vector2 projVector, float Cooldown, int subthrowframe, int specialthrowframe)
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
                projVector = RandomSpread(16f, 16f, 15f);
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
                        SubAttack(projVector, targetposition, subthrowframe);
                        break;
                    case InklingStates.SPECIAL:
                        SpecialAttack(projVector, targetposition, specialthrowframe);
                        break;
                    case InklingStates.WAIT:
                        break;
                    default:
                        break;
                }
            }
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

                Vector2[] positions = RandomSpreads(16f, 75f, projectiles);
                for (int i = 0; i < projectiles; i++)
                {
                    positions[i] *= projectile.DirectionTo(target);
                    projVector = projectile.DirectionTo(target)* RandomSpread(12f, 12f, 90f);
                    CenteroffSet.Y -= Main.rand.NextFloat(1.5f, 5f);
                    Projectile.NewProjectile(CenteroffSet, positions[i], ModContent.ProjectileType<HeroRollerSplash>(), 125, 3f, projectile.owner);
                }
            }
        }
        private Vector2[] RandomSpreads(float speed, float angle, int num)
        {
            var posArray = new Vector2[num];
            float spread = (float)(angle * 0.0174532925);
            speed = (float)Math.Sqrt((speed * speed) + (speed * speed));
            double baseAngle = System.Math.Atan2(speed, speed);
            double randomAngle;
            for (int i = 0; i < num; ++i)
            {
                randomAngle = baseAngle + (Main.rand.NextFloat() - 0.5f) * spread;
                posArray[i] = new Vector2(speed * (float)Math.Cos(randomAngle), speed * (float)Math.Sin(randomAngle));
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
                    FrameSpeed = (int)AttackTypes[0].GetDuration();
                    AnimateState(AttackTypes[0].GetStartFrame(), AttackTypes[0].GetEndFrame(), InklingStates.WAIT);
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
                    PlayerAnimation(AttackTypes[2].GetStartFrame(), AttackTypes[1].GetEndFrame());
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
