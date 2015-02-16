using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Chroma.Actors;
using Chroma.States;
using Chroma.Messages;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class CoinActor : Actor
  {
    private readonly Animation animation;
    private int delay;

    public CoinActor(Core core, Vector2 position, bool canMove = false) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 8, 8);
      this.delay = (int)(position.X / 15) % 20;

      animation = new Animation();
      animation.AddAndPlay("spin", core.SpriteManager.GetFrames("coin_", new List<int>{ 0, 0, 0, 0, 1, 2, 3, 4 }));

      CanMove = canMove;
      CanFall = canMove;

      AddCollider(new Collider() { Name = "coin", BoundingBox = boundingBox });
    }
      
    public override void Update(int ticks)
    {
      if (delay > 0)
      {
        delay--;
      }

      if (delay == 0)
      {
        animation.Update(ticks);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {

      core.Renderer.DrawSpriteW("glow", new Vector2(Position.X - 8, Position.Y - 8), 
        Color.Gold * 0.25f, new Vector2(0.4f, 0.4f));

      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position);

      var frame = animation.GetCurrentFrameNumber();
      var scale = (6f / frame) * (6f / frame);
      if (frame > 5)
      {
        core.Renderer["fg_add"].DrawSpriteW("glow", new Vector2(Position.X - 0.5f, Position.Y - 0.5f), 
          Color.White * 1f * scale, new Vector2(0.13f * scale, 0.13f * scale));
      }

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is PlayerActor)
      {
        core.SoundManager.Play("click");

        core.MessageManager.Send(new RemoveActorMessage(this), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

