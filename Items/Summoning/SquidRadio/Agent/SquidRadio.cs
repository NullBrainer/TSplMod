using Microsoft.Xna.Framework;
using SplatoonMod.Buffs;
using SplatoonMod.projectiles;
using SplatoonMod.projectiles.SquidPhoneProj;
using SplatoonMod.projectiles.SquidRadioProj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SplatoonMod.Items.Summoning.SquidRadio.Agent
{
    public class SquidRadio : ModItem
    {
        private int[] Projectiles;
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Calls an Agent to fight for you.");
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
        }
        public override void SetDefaults()
        {
            Projectiles = new int[]{ ModContent.ProjectileType<SquidRadioProj>() , ModContent.ProjectileType<Agent1>(), ModContent.ProjectileType<Agent2>() };
            item.damage = 35;
            item.knockBack = 2.5f;
            item.mana = 10;
            item.width = 25;
            item.height = 32;
            item.useTime = 35;
            item.useAnimation = 35;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.rare = ItemRarityID.Orange;
            item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Inkling/SquidRadio");
            item.value = Terraria.Item.buyPrice(0,10,60,0);
            item.melee = false;

            item.noMelee = true;
            item.summon = true;
            item.buffType = ModContent.BuffType<SquidRadioBuff>();
            item.shoot = ModContent.ProjectileType<SquidRadioProj>();
            
            base.SetDefaults();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            player.AddBuff(item.buffType, 2);
            position = Main.MouseWorld;
            int agentID = 2;
            if (agentID == 1)
            {
                damage = 140;
            } else if (agentID == 2)
            {
                damage = 160;
            }

            Projectile.NewProjectile(position, Vector2.Zero, Projectiles[agentID], damage, knockBack, player.whoAmI);
            return false;
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
