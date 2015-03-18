using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using System.Collections.Generic;

namespace Chroma.Actors
{
  public class SpikesActor : Actor
  {
    private readonly Animation spike;
    private int width;

    public SpikesActor(Core core, Vector2 position, int width) : base (core, position)
    {
      this.width = (int)Math.Round(width / 5.0f) * 5;

      Y -= 10;
      boundingBox = new Rectangle(0, 0, width, 10);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      spike = new Animation();
      spike.Add("appear", new List<Sprite>
        {
          core.SpriteManager.GetSprite(SpriteName.spike_1),
          core.SpriteManager.GetSprite(SpriteName.spike_2),
          core.SpriteManager.GetSprite(SpriteName.spike_3),
          core.SpriteManager.GetSprite(SpriteName.spike_4)
        });
      spike.Play("appear");
      spike.Pong = true;

      CanMove = false;

      IsSolid = false;
    }

    public override void Update(int ticks)
    {
      spike.Update(ticks);
      base.Update(ticks);
    }

    public override void Draw()
    {
      for (var i = 0; i < width / 5; i++)
      {
        core.Renderer[5].DrawSpriteW(spike.GetCurrentFrame(), Position + new Vector2(i * 5, (i % 2 == 1) ? -3 : 0));
        if (i % 2 == 1)
          core.Renderer[10].DrawSpriteW(spike.GetCurrentFrame(), Position + new Vector2(i * 5, 2));
      }

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is PlayerActor)
      {
        (other as PlayerActor).Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }
  }
}

