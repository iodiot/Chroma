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
    public Actor Owner { get; protected set; }
    public readonly MagicColor color;
    private readonly Sprite sprite;

    public ProjectileActor(Core core, Vector2 position, MagicColor color, Actor owner) : base(core, position)
    {
      this.color = color;
      Owner = owner;

      boundingBox = new Rectangle(0, 0, 14, 8);

      sprite = core.SpriteManager.GetSprite("projectile", color);

      CanMove = true;
      CanFall = false;
      CanLick = false;

      IsSolid = false;

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
      core.Renderer["add"].DrawGlowW(Position + new Vector2(10, 3), color * 0.4f, 20);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other.IsSolid && other != Owner) 
      {
        Explode(!(other is PlatformActor));
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      if (other is PlatformActor || other is BoulderActor)
      {
        Explode(false);
      }

      base.OnBoundingBoxTrigger(other);
    }

    public void Explode(bool hit)
    {
      core.MessageManager.Send(new RemoveActorMessage(this), this);

      if (hit)
      {
        // TODO: Hit effect preset
        var effect = new FragmentActor(core, Position + new Vector2(10, -5), 
          core.SpriteManager.GetSprite(SpriteName.hit_1))
          {
            Scale = 1.5f,
            Ttl = 50,
            Layer = "add",
            CanMove = false,
            Tint = MagicManager.MagicColors[color],
            Opacity = 0.9f,
            OpacityStep = -0.05f
          };
        core.MessageManager.Send(new AddActorMessage(effect), this);
      }
      else
      {
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, sprite)), this);
      }
    }
  }
}


