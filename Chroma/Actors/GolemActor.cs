using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class GolemActor : Actor
  {
    private readonly Animation animation;
    private MagicColor color;

    public GolemActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;
      boundingBox = new Rectangle(0, 0, 20, 28);
      this.Position.Y -= 28;

      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("golem_", new List<int>{ 1, 2, 3, 4, 5, 6 }, color));

      CanMove = true;
      CanFall = true;
      CanLick = true;

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });
    }

    public override void Update(int ticks)
    {
      Velocity.X = -0.5f;

      animation.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      Color color = MagicManager.MagicColors[this.color];
      //core.Renderer.DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position + new Vector2(-1, -4), color, 0.35f);
      core.Renderer[1].DrawSpriteW(animation.GetCurrentFrame(), Position);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is ProjectileActor && ((ProjectileActor)other).color == this.color)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, animation.GetCurrentFrame())), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

