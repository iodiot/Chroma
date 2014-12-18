using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;

namespace Chroma.Actors
{
  class PlayerActor : BodyActor
  {
    private readonly Animation animation; 

    private int jumpTtl;
    private float groundLevel;

    public PlayerActor(Core core, Vector2 position) : base(core, position)
    {
      Width = core.SpriteManager.GetSprite("druid_walk_1").Width;
      Height = core.SpriteManager.GetSprite("druid_walk_1").Height;

      Handle = "player";

      animation = new Animation();
      animation.Add("walk", core.SpriteManager.GetFrames("druid_walk_", new List<int>{ 1, 2, 3, 4 }));
      animation.Add("raise", core.SpriteManager.GetFrames("druid_raise_", new List<int>{ 1, 2 }));

      animation.Play("walk");

      jumpTtl = 0;
      groundLevel = position.Y;
    }

    public override void Update(int ticks)
    {
      HandleInput();

      if (jumpTtl > 0)
      {
        --jumpTtl;
        Y = groundLevel - 25.0f * (float)Math.Sin((double)jumpTtl / 25.0 * Math.PI);

        if (jumpTtl == 0)
        {
          animation.Play("walk");
        }
      }

      animation.Update(ticks);

      X += 1.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, Color.White);

      base.Draw();
    }

    private void HandleInput()
    {
      var touchState = TouchPanel.GetState();

      if (touchState.Count == 1 && (jumpTtl == 0))
      {
        jumpTtl = 25;
        animation.Play("raise");
      }
    }
  }
}

