using SplatoonMod.projectiles;
using SplatoonMod.projectiles.SquidPhoneProj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.Buffs
{
    public class SquidPhoneBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Squid Phone");
            Description.SetDefault("The Inkling will fight for you.");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;

        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SquidPhoneProj>()] > 0 )
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
