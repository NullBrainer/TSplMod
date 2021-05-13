using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles.HeroProjectiles;
using System;
using SplatoonMod.Util;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.Util
{
    public class SummonAttack
    {

        private int StartFrame, EndFrame;
        private float Duration;
        private float Timer;
        private ModProjectile projectile;
        public SummonAttack(ModProjectile proj, int startFrame, int endFrame, float duration = 0f)
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            Duration = duration;
            Timer = 0f;
            projectile = proj;
        }

        public SummonAttack(ModProjectile proj, int startFrame, int endFrame)
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            Duration =  0f;
            Timer = 0f;
            projectile = proj;
        }

        public int GetStartFrame()
        {
            return StartFrame;
        }
        
        public int GetEndFrame()
        {
            return EndFrame;
        }
        
        public float GetDuration()
        {
            return Duration;
        }
        public void Update()
        {
            Timer += 1f;
        }
        public void IsFinished(out bool updatedbool)
        {
            updatedbool = Timer >= Duration;
            Timer = 0f;
        }       
    }
}
