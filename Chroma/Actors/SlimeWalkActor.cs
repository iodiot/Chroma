using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class SlimeWalkActor : Actor
  {
    private readonly Animation walkAnimation, legAnimation; 
    public readonly MagicColor Color;

    public SlimeWalkActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      Color = color;
      boundingBox = new Rectangle(0, 0, 48, 53);
      this.Position.Y -= 53;

      walkAnimation = new Animation(0.15f);
      walkAnimation.AddAndPlay("live", core.SpriteManager.GetFrames("slime_walk_", new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }, color));

      legAnimation = new Animation(0.15f);
      legAnimation.AddAndPlay("live", core.SpriteManager.GetFrames("slime_walk_leg_", new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }));

      AddCollider(new Collider() { Name = "heart", BoundingBox = new Rectangle(15, 0, 18, 20) });

      CanMove = true;
      CanFall = true;
      CanLick = true;
    }

    public override void Update(int ticks)
    {
      walkAnimation.Update(ticks);
      legAnimation.Update(ticks);

      Velocity.X = -1.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer[-1].DrawSpriteW(walkAnimation.GetCurrentFrame(), Position);
      core.Renderer[10].DrawSpriteW(legAnimation.GetCurrentFrame(), Position);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is ProjectileActor && ((ProjectileActor)other).color == this.Color)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, walkAnimation.GetCurrentFrame())), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

