using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;


namespace SplatoonMod.Dust
{
    public class Agent2InkDroplet : ModDust
    {
        public override void OnSpawn(Terraria.Dust dust)
        {
            dust.velocity.X = 0f;
            dust.velocity.Y = Main.rand.NextFloat(0.5f, 3f);
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = Main.rand.NextFloat(0.6f, 1f);
        }

        public override bool Update(Terraria.Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.Y * 0.15f;
            dust.scale -= (dust.fadeIn * 0.01f);
            dust.alpha++;
            if (dust.alpha >= 255 || dust.scale <= 0.1f)
            {
                dust.active = false;
            }

            return false;
        }

    }
}
