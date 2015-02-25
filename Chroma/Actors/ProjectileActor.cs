using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class ProjectileActor : Actor
  {
    public readonly MagicColor color;
    private readonly Sprite sprite;

    public ProjectileActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;

      boundingBox = new Rectangle(0, 0, 14, 8);

      sprite = core.SpriteManager.GetSprite("projectile", color);

      CanMove = true;
      CanFall = false;
      CanLick = false;

      AddCollider(new Collider() { Name = "projectile", BoundingBox = boundingBox });

      Ttl = 250;
    }

    public override void Update(int ticks)
    {
      Velocity.X = 7.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      var color = MagicManager.MagicColors[this.color];
      core.Renderer.DrawSpriteW(sprite, Position);
      core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(20, 23), color * 0.4f);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (!(other is ProjectileActor || other is CoinActor || other is PlayerActor || other is ItemActor))
      {
        Explode();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      if (other is PlatformActor || other is BoulderActor) // TODO: features!
      {
        Explode();
      }

      base.OnBoundingBoxTrigger(other);
    }

    public void Explode()
    {
      core.MessageManager.Send(new RemoveActorMessage(this), this);
      core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, sprite)), this);
    }
  }
}


