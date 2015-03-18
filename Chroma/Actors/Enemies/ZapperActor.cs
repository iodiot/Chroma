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
  class Lighting
  {
    public int Ttl { get; private set; }

    private Core core;
    private List<Vector2> points;

    public Lighting(Core core, Vector2 from, Vector2 to)
    {
      this.core = core;

      points = new List<Vector2>();

      GeneratePoints(from, to);

      Ttl = 10;
    }

    public Lighting(Core core, Vector2 from)
    {
      this.core = core;

      points = new List<Vector2>();

      const float R = 100f;
      var to = from + new Vector2(ScienceHelper.GetRandom(-R, R), ScienceHelper.GetRandom(.5f * R, R));

      GeneratePoints(from, to);

      Ttl = 50;
    }

    private void GeneratePoints(Vector2 from, Vector2 to)
    {
      var length = (to - from).Length();
      var step = length * 0.2f;
      var stepsCount = (int)(length / step);
      var dir = (to - from);
      dir.Normalize();

      const float R = 5f;

      for (var i = 0; i <= stepsCount; ++i)
      {
        var r = (i != 0 && i != stepsCount) ? new Vector2(ScienceHelper.GetRandom(-R, R), ScienceHelper.GetRandom(-R, R)) : Vector2.Zero;
        points.Add(from + dir * i * step + r);
      }
    }

    public void Update(int ticks)
    {
      if (Ttl > 0)
      {
        --Ttl;
      }
    }

    public void Draw(Vector2 startPoint)
    {
      points[0] = startPoint;

      for (var i = 1; i < points.Count; ++i)
      {
        core.Renderer.DrawLineW(points[i - 1], points[i], Color.Yellow);
      }

      for (var i = 0; i < points.Count; ++i)
      {
        core.Renderer.DrawRectangleW(points[i] - new Vector2(1.5f, 1.5f), 3f, 3f, Color.Red);
      }
    }
  }

  public class ZapperActor : Actor
  {
    private const int BallsCount = 4;

    private readonly Dictionary<int, Sprite> ballSprites;
    private readonly List<Vector3> balls;
    private MagicColor color;

    private int animOffset;

    private Lighting lighting;

    public ZapperActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;
      boundingBox = new Rectangle(-15, -15, 30, 30);
      Y -= 55;

      ballSprites = new Dictionary<int, Sprite>();
      ballSprites.Add(1, core.SpriteManager.GetSprite(SpriteName.zapper_ball_1));
      ballSprites.Add(2, core.SpriteManager.GetSprite(SpriteName.zapper_ball_2));
      ballSprites.Add(3, core.SpriteManager.GetSprite(SpriteName.zapper_ball_3));

      balls = new List<Vector3>();
      for (var i = 0; i < BallsCount; ++i)
      {
        balls.Add(Vector3.Zero);
      }

      CanMove = true;
      CanFall = false;
      CanLick = true;

      animOffset = ScienceHelper.GetRandom(0, 10000);

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });

      lighting = null;
    }

    public override void Update(int ticks)
    {
      if (lighting == null && ScienceHelper.ChanceRoll(0.05f))
      {
        lighting = new Lighting(core, Position);
      }

      if (lighting != null)
      {
        lighting.Update(ticks);

        if (lighting.Ttl == 0)
        {
          lighting = null;
        }
      }

      ticks += animOffset;

      Velocity.X = 0.1f * (float)Math.Cos((ticks) / 20) - 0.2f;
      Velocity.Y = 0.2f * (float)Math.Cos((ticks) / 17);

      balls.Clear();
      for (var i = 0; i < BallsCount; ++i)
      {
        balls.Add(new Vector3(
            (float)Math.Sin((double)ticks / (23 + 3 * i)) * 10,
            (float)Math.Sin((double)ticks / (13 + 5 * i)) * 10,
            (float)Math.Sin((double)ticks / (17 + 7 * i)) * 3
          ));
      }
        
      base.Update(ticks);
    }

    public override void Draw()
    {
      var color = MagicManager.MagicColors[this.color];
      core.Renderer[2].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(10), color, scale: new Vector2(0.35f));

      for (var i = 0; i < BallsCount; ++i)
      {
        var ball = balls[i];

        var z = (int)(Math.Ceiling(ball.Z + 3) / 2);
        z = Math.Min(Math.Max(z, 1), 3);
        core.Renderer[4 - z].DrawSpriteW(ballSprites[z], Position + ball.XY() - new Vector2(4));
      } 

      if (lighting != null)
      {
        lighting.Draw(Position);
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

