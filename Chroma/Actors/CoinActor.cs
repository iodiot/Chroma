using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Chroma.Actors;
using Chroma.States;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class CoinActor : Actor
  {
    private readonly Animation animation;

    public CoinActor(Core core, Vector2 position, bool canMove = false) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 8, 8);

      animation = new Animation();
      animation.AddAndPlay("spin", core.SpriteManager.GetFrames("coin_", new List<int>{ 0, 0, 0, 0, 1, 2, 3, 4 }));

      CanMove = canMove;
      CanFall = canMove;
      CanBounce = canMove;

      // Flying out
      if (CanMove) {
        Velocity.X = ScienceHelper.GetRandom(1, 2);
        Velocity.Y = ScienceHelper.GetRandom(-3, -1);
      }

      AddCollider(new Collider() { Name = "coin", BoundingBox = boundingBox });
    }
      
    public override void Update(int ticks)
    {
      animation.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {

      core.Renderer[4].DrawSpriteW("glow", Position - new Vector2(8, 8), 
        Color.Gold * 0.20f, new Vector2(0.4f, 0.4f));

      core.Renderer[5].DrawSpriteW(animation.GetCurrentFrame(), Position);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is PlayerActor)
      {
        core.SoundManager.Play("coin");

        core.MessageManager.Send(new RemoveActorMessage(this), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override void OnBounce()
    {
      core.SoundManager.Play("coin_bounce");

      base.OnBounce();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

