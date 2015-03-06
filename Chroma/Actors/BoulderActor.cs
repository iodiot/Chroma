using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.Actors
{
  public class BoulderActor : Actor
  {
    private readonly Sprite sprite;

    public BoulderActor(Core core, Vector2 position) : base (core, position)
    {
      sprite = core.SpriteManager.GetSprite("boulder_1");
      Y -= sprite.Height - 1;
      boundingBox = new Rectangle(0, 3, sprite.Width, sprite.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[5].DrawSpriteW(sprite, Position);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      var destroyed = false;

      if (other is PlayerActor)
      {
        var player = (other as PlayerActor);
        player.Hurt();
        destroyed = !player.HasLost;
      }
        
      if (other is ProjectileActor || destroyed)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, sprite)), this);

        DropCoin();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }
  }
}

