using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.Actors
{
  public class PlayerActor : CollidableActor
  {
    private readonly Animation animation; 

    private int jumpTtl;
    private int hurtTtl;
    private int fireTtl;

    private float groundLevel;

    public PlayerActor(Core core, Vector2 position) : base(core, position)
    {
      Handle = "player";

      boundingBox = new Rectangle(5, 0, 12, 21);

      animation = new Animation();
      animation.Add("walk", core.SpriteManager.GetFrames("druid_walk_", new List<int>{ 1, 2, 3, 4 }));
      animation.Add("raise", core.SpriteManager.GetFrames("druid_raise_", new List<int>{ 1, 2 }));

      animation.Play("walk");

      jumpTtl = 0;
      hurtTtl = 0;
      fireTtl = 0;
      groundLevel = position.Y;
    }

    public override void Update(int ticks)
    {
      HandleInput();

      if (jumpTtl > 0)
      {
        --jumpTtl;
        Y = groundLevel - 30.0f * (float)Math.Sin((double)jumpTtl / 25.0 * Math.PI);

        if (jumpTtl == 0)
        {
          animation.Play("walk");
        }
      }

      if (fireTtl > 0)
      {
        --fireTtl;
      }

      if (hurtTtl > 0)
      {
        --hurtTtl;
      }

      animation.Update(ticks);

      X += 1.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      var tint = (hurtTtl / 5) % 2 == 0 ? Color.White : Color.Red;
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, tint);

      base.Draw();
    }

    private void HandleInput()
    {
      var touchState = TouchPanel.GetState();

      if (touchState.Count != 1)
      {
        return;
      }

      if ((touchState[0].Position.X < core.Renderer.ScreenWidth / 2) && (jumpTtl == 0))
      {
        jumpTtl = 25;
        animation.Play("raise");
      }

      if ((touchState[0].Position.X >= core.Renderer.ScreenWidth / 2) && (fireTtl == 0))
      {
        fireTtl = 25;

        core.MessageManager.Send(
          new AddActorMessage(
            new ProjectileActor(core, Position + new Vector2(25, 10))
          ),
          this
        );
      }
    }

    public override void OnCollide(CollidableActor other)
    {
      if ((other is GolemActor) && (hurtTtl == 0))
      {
        hurtTtl = 25;
      }

      base.OnCollide(other);
    }
  }
}

