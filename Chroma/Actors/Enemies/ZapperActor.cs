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
  class LightingVertex
  {
    public Vector2 Center;
    public float Amplitude;
    public float Phase;
    public float PhaseSpeed;
    public Vector2 Axis;

    public Vector2 Position {get { return Center + Amplitude * (float)Math.Sin(Phase) * Axis; }}
  }

  class Lighting
  {
    public int Ttl { get; private set; }

    private readonly Core core;
    private readonly List<LightingVertex> vertices;

    public Lighting(Core core, Vector2 from)
    {
      this.core = core;

      Ttl = ScienceHelper.GetRandom(30, 50);

      // Compute aim of lighting
      var r = ScienceHelper.GetRandom(50f, 100f);
      var to = from + new Vector2(ScienceHelper.GetRandom(-r, r), 0f);
      var platform = core.GetPlayState().ActorManager.FindPlatformUnder(to);
      to.Y = (platform != null) ? platform.Y : ScienceHelper.GetRandom(0f, r * .5f);

      vertices = GenerateVertices(from, to, 5, 10f);
    }

    private static List<LightingVertex> GenerateVertices(Vector2 from, Vector2 to, int count, float deviation)
    {
      var result = new List<LightingVertex>();

      var length = (to - from).Length();
      var step = length / (float)count;
      var dir = (to - from);
      dir.Normalize();

      result.Add(new LightingVertex() { Center = from });

      Vector2 perp;
      perp.X = 1f;
      perp.Y = -(dir.X * perp.X) / dir.Y;

      for (var i = 1; i < count; ++i)
      {
        var axis = new Vector2(ScienceHelper.GetRandom(-1f, 1f), ScienceHelper.GetRandom(-1f, 1f));
        axis.Normalize();

        var amp = ScienceHelper.GetRandom(.5f * deviation, deviation);

        result.Add(new LightingVertex() {
          Center = from + dir * i * step + perp * amp,
          Amplitude = amp,
          PhaseSpeed = ScienceHelper.GetRandom(0, .5f), // (float)Math.Sin(i * .01f) * 5f, //
          Axis = axis,
          //Phase = ScienceHelper.GetRandom(0f, 100f)
        });
      }

      result.Add(new LightingVertex() { Center = to });

      return result;
    }

    private void UpdateVertices()
    {
      foreach (var v in vertices)
      {
        if (v.Amplitude > 0f)
        {
          v.Phase += v.PhaseSpeed;
        }
      }
    }

    public void Update()
    {
      if (Ttl > 0)
      {
        --Ttl;

        UpdateVertices();
      }
    }

    public void Draw(Vector2 from, Color color)
    {
      vertices[0].Center = from;

      for (var i = 1; i < vertices.Count; ++i)
      {
        core.Renderer[1000].DrawLineW(vertices[i - 1].Position, vertices[i].Position, color);
      }

      for (var i = 0; i < vertices.Count; ++i)
      {
       // core.Renderer[1001].DrawRectangleW(vertices[i].Position - new Vector2(1.5f, 1.5f), 3f, 3f, Color.Red);
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
        lighting.Update();

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
        lighting.Draw(Position, color);
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

