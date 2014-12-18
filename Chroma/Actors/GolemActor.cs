using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;

namespace Chroma
{
  class GolemActor : BodyActor
  {
    private readonly Animation animation;

    public GolemActor(Core core, Vector2 position) : base(core, position)
    {
      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("golem_", new List<int>{ 1, 2, 3, 4, 5, 6 }));

      Width = animation.GetCurrentFrame().Width;
      Height = animation.GetCurrentFrame().Height;
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, Color.White);
    }
  }
}

