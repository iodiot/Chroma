using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class ZapperActor : Actor
  {
    private const int BallsCount = 4;

    private readonly Dictionary<int, Sprite> ballSprites;
    private readonly List<Vector3> balls;
    private MagicColor color;

    private int animOffset;

    public ZapperActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;
      boundingBox = new Rectangle(-15, -15, 30, 30);
      this.Position.Y -= 55;

      ballSprites = new Dictionary<int, Sprite>();
      ballSprites.Add(1, core.SpriteManager.GetSprite(SpriteName.zapper_ball_1));
      ballSprites.Add(2, core.SpriteManager.GetSprite(SpriteName.zapper_ball_2));
      ballSprites.Add(3, core.SpriteManager.GetSprite(SpriteName.zapper_ball_3));

      balls = new List<Vector3>();
      for (var i = 0; i < BallsCount; i++)
      {
        balls.Add(new Vector3(0));
      }

      CanMove = true;
      CanFall = false;
      CanLick = true;

      animOffset = ScienceHelper.GetRandom(0, 10000);

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });
    }

    public override void Update(int ticks)
    {
      ticks += animOffset;

      Velocity.X = 0.1f * (float)Math.Cos((ticks) / 20) - 0.2f;
      Velocity.Y = 0.2f * (float)Math.Cos((ticks) / 17);

      for (var i = 0; i < BallsCount; i++)
      {
        var ball = balls[i];
        ball.X = (float)Math.Sin((double)ticks / (23 + 3 * i)) * 10;
        ball.Y = (float)Math.Sin((double)ticks / (13 + 5 * i)) * 10;
        ball.Z = (float)Math.Sin((double)ticks / (17 + 7 * i)) * 3;
        balls[i] = ball;
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      var color = MagicManager.MagicColors[this.color];
      core.Renderer[2].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(10), color, scale: new Vector2(0.35f));

      for (var i = 0; i < BallsCount; i++)
      {
        var ball = balls[i];

        var z = (int)(Math.Ceiling(ball.Z + 3) / 2);
        z = Math.Min(Math.Max(z, 1), 3);
        core.Renderer[4 - z].DrawSpriteW(ballSprites[z], Position + ball.XY() - new Vector2(4));
      } 

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is ProjectileActor && ((ProjectileActor)other).color == this.color)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);

        for (var i = 0; i < BallsCount; i++)
        {
          var ball = balls[i];
          var z = (int)(Math.Ceiling(ball.Z + 3) / 2);
          z = Math.Min(Math.Max(z, 1), 3);

          core.MessageManager.Send(new AddActorMessage(new FragmentActor(core, Position + ball.XY(), ballSprites[z])), this);
        }

        DropCoin();
      }

      if (other is PlayerActor)
      {
        (other as PlayerActor).Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

