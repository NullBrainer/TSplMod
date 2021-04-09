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
    }
}

