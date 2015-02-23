using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.Actors
{
  public class BoulderActor : Actor
  {
    private Sprite sprite;
    public BoulderActor(Core core, Vector2 position) : base (core, position)
    {
      sprite = core.SpriteManager.GetSprite("boulder_1");
      this.Position.Y -= sprite.Height - 1;
      boundingBox = new Rectangle(0, 3, sprite.Width, sprite.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[10].DrawSpriteW(sprite, Position);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor is SlimeWalkActor || actor is ProjectileActor;
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is ProjectileActor)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, sprite)), this);

        core.MessageManager.Send(new AddActorMessage(new CoinActor(core, this.Position, true)), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }
  }
}

