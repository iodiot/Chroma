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

    private Lightning lightning;
    private Vector2 lightningAim;

    private readonly ParticleManager pm;

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

      animOffset = SciHelper.GetRandom(0, 10000);

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });

      lightning = null;

      // Compute lightning aim
      var r = SciHelper.GetRandom(50f, 100f);
      lightningAim = Position + new Vector2(SciHelper.GetRandom(-r, r), 0f);
      var platform = core.GetPlayState().ActorManager.FindPlatformUnder(lightningAim);
      lightningAim.Y = (platform != null) ? platform.Y : SciHelper.GetRandom(0f, r * .5f);

      pm = new ParticleManager(core, .5f);
      pm.OnSpawn = OnParticleSpawn;
      pm.OnUpdate = OnParticleUpdate;
    }

    private void OnParticleSpawn(Particle particle)
    {
      particle.Position = Position + SciHelper.GetRandomVectorInCircle(5f);
      particle.Ttl = SciHelper.GetRandom(50, 75);
      particle.RotationSpeed = SciHelper.GetRandom(-.1f, .1f);
      particle.Color = MagicManager.MagicColors[color];
      particle.Velocity = new Vector2(SciHelper.GetRandom(-.1f, .1f), SciHelper.GetRandom(0f, -.5f));
      particle.Scale = new Vector2(2f, 2f);
    }

    private void OnParticleUpdate(Particle particle)
    {
      particle.Color *= .99f;
    }

    public override void Update(int ticks)
    {
      if (SciHelper.ChanceRoll(.05f))
      {
        lightning = new Lightning(core, Position, lightningAim);
      }

      if (SciHelper.ChanceRoll(.05f))
      {
        lightning = null;
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

      pm.Update();
        
      base.Update(ticks);
    }

    public override void Draw()
    {
      var color = MagicManager.MagicColors[this.color];

      for (var i = 0; i < BallsCount; ++i)
      {
        var ball = balls[i];

        var z = (int)(Math.Ceiling(ball.Z + 3) / 2);
        z = Math.Min(Math.Max(z, 1), 3);
        core.Renderer[4 - z].DrawSpriteW(ballSprites[z], Position + ball.XY() - new Vector2(4));
      } 

      if (lightning != null)
      {
        lightning.Draw(color);
      }

      pm.Draw();

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

          core.MessageManager.Send(new AddActorMessage(new FragmentActor(core, Position + ball.XY(), ballSprites[z], FragmentActor.Preset.Remains)), this);
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

