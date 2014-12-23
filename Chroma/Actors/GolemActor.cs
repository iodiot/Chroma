using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma
{
  public class GolemActor : BodyActor
  {
    private readonly Animation animation;
    private int ttl = 50;

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

      if (ttl > 0)
      {
        --ttl;
      }
      else
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new ParticleActor(core, Position, animation.GetCurrentFrame())), this);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, Color.White);

      base.Draw();
    }
  }
}

