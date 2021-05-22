using SplatoonMod.projectiles;
using SplatoonMod.projectiles.SquidRadioProj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.Buffs
{
    public class SquidRadioBuff : ModBuff
    {
        private int[] ProjectileIDS;
        public override void SetDefaults()
        {
            ProjectileIDS =  new int[] { ModContent.ProjectileType<SquidRadioProj>(), ModContent.ProjectileType<Agent1>(), ModContent.ProjectileType<Agent2>() };
            DisplayName.SetDefault("Squid Radio");
            Description.SetDefault("The Agents will fight for you.");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ProjectileIDS[0]] > 0 || player.ownedProjectileCounts[ProjectileIDS[1]] > 0 || player.ownedProjectileCounts[ProjectileIDS[2]] > 0)
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
