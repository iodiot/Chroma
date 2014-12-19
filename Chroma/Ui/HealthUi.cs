using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Ui
{
  class HealthUi : Ui
  {
    private int hearts;
    private Sprite sprite;
    private Animation animation;

    public HealthUi(Core core) : base(core)
    {
      hearts = 3;

      sprite = core.SpriteManager.GetSprite("heart_1");
      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("heart_", new List<int> { 1, 2, 3, 4  }));
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      for (var i = 0; i < hearts; ++i)
      {
        core.Renderer.DrawSpriteS(animation.GetCurrentFrame(), new Vector2(5 + 1.1f * i * sprite.Width, 5), Color.White);
      }

      base.Draw();
    }
  }
}

