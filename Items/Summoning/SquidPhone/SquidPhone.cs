using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles;
using SplatoonMod.projectiles.SquidPhoneProj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.Items.Summoning.SquidPhone
{
    public class SquidPhone : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Calls an inkling to fight for you.");
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
        }
        public override void SetDefaults()
        {
            item.damage = 10;
            item.knockBack = 2.5f;
            item.mana = 10;
            item.width = 25;
            item.height = 32;
            item.useTime = 35;
            item.useAnimation = 35;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.rare = ItemRarityID.Orange;
            item.UseSound = SoundID.Item44;
            item.value = Terraria.Item.buyPrice(0,10,60,0);

            item.noMelee = true;
            item.summon = true;
            item.buffType = ModContent.BuffType<SquidPhoneBuff>();
            item.shoot = ModContent.ProjectileType<SquidPhoneProj>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            player.AddBuff(item.buffType, 2,true);
            position = Main.MouseWorld;
            return true;

        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood,1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
