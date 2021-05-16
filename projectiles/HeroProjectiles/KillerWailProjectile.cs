using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SplatoonMod.Buffs;
using SplatoonMod.Dust;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using Terraria.Enums;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;

namespace SplatoonMod.projectiles.HeroProjectiles
{
    public class KillerWailProjectile : ModProjectile
    {
		private const float Distance = 1000f;
		private const float MOVE_DISTANCE = 33f;
		private Vector2 Origin;
		private Point OriginPoint;
		private Point
			BodyPosition = new Point(0, 15),
			TailPosition = new Point(0, 90),
			BodyDimensions = new Point(100, 70),
			TailDimension = new Point(100, 10);
		
		private int BeamFrame = 0;

		
		public override void SetDefaults()
        {
			projectile.width = 100;
			projectile.height = 100;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.minion = true;			
			projectile.timeLeft = 120;
			OriginPoint = projectile.position.ToPoint();
		}
        public override void AI()
		{
			Origin = Main.projectile[(int)projectile.ai[1]].Center;
			HandleBeamSegmentAnimation();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
				DrawLaser(spriteBatch, Main.projectileTexture[projectile.type], Origin,
					projectile.velocity, 10, -1.57f, 1f, Color.White, (int)MOVE_DISTANCE);
			return false;
		}

		public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step,  float rotation = 0f, float scale = 1f,  Color color = default(Color), int transDist = 50)
		{
			float r = unit.ToRotation() + rotation;

			//tail
			spriteBatch.Draw(texture, start + unit * (transDist - (2*step)) - Main.screenPosition, new Rectangle(TailPosition.X, TailPosition.Y, TailDimension.X, TailDimension.Y), Color.White, r, new Vector2(TailDimension.X * .5f, TailDimension.Y * .5f), scale, 0, 0);
			//body
			for (float i = transDist; i <= Distance; i += step)
			{
				var origin = start + i * unit;
				if (BeamFrame > 0)
				{
					spriteBatch.Draw(texture, origin - Main.screenPosition, BeamRectangleAnimatedSegment1(), color, rotation, new Vector2(BodyDimensions.X * .5f, BodyDimensions.Y * .5f), scale, 0, 0f);
					spriteBatch.Draw(texture, origin + (BeamFrame * projectile.velocity) - Main.screenPosition, BeamRectangleAnimatedSegment2(), color, rotation, new Vector2(BodyDimensions.X * .5f, BodyDimensions.Y * .5f), scale, 0, 0f);
				}
				else
				{
					spriteBatch.Draw(texture, origin - Main.screenPosition, new Rectangle(BodyPosition.X,BodyPosition.Y,BodyDimensions.X,BodyDimensions.Y), color, rotation, new Vector2(BodyDimensions.X * .5f, BodyDimensions.Y * .5f), scale, 0, 0f);
				}
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[projectile.owner] = 5;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 unit = projectile.velocity;
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Origin,
				Origin + unit * Distance, projectile.height, ref point);
		}

		public Rectangle BeamRectangleAnimatedSegment1()
		{
			return new Rectangle(BodyPosition.X, BodyPosition.Y, BodyDimensions.X, BodyDimensions.Y - BeamFrame);
		}

		public Rectangle BeamRectangleAnimatedSegment2()
		{
			return new Rectangle(BodyPosition.X, BodyPosition.Y, BodyDimensions.X, BodyDimensions.Y - BeamFrame);
		}
		private void HandleBeamSegmentAnimation()
		{
				BeamFrame += 10;
				if (BeamFrame >= BodyDimensions.Y)
				{
					BeamFrame = 0;
				}
		}

	}
}

