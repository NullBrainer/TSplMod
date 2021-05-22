using Microsoft.Xna.Framework;
using SplatoonMod.Dust;
using Terraria;
using Terraria.ModLoader;

namespace SplatoonMod.Buffs
{
    public class Agent2Debuff : ModBuff
    {
        public override void SetDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            canBeCleared = true;
        }


        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.stepSpeed *= 0.65f;
            npc.velocity.X *= 0.65f;
            int dustid = Terraria.Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Agent2InkDroplet>(), 0f, -3f, 0, default);
            Main.dust[dustid].noGravity = true;
            Main.dust[dustid].fadeIn = 5f;
        }
    }
}
