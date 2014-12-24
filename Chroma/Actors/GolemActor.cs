using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma
{
  public class GolemActor : CollidableActor
  {
    private readonly Animation animation;
    private int ttl = 50;

    public GolemActor(Core core, Vector2 position) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 20, 28);

      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("golem_", new List<int>{ 1, 2, 3, 4, 5, 6 }));
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
        core.MessageManager.Send(new AddActorMessage(new SwarmActor(core, Position, animation.GetCurrentFrame())), this);
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

