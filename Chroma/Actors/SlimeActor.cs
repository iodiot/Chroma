using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;
using Chroma.StateMachines;

namespace Chroma.Actors
{
  public class SlimeActor : Actor
  {
    private Animation animation;
    public readonly MagicColor Color;

    enum SlimeState {
      Crawl,
      Roll
    }
    private SlimeState state; 

    public SlimeActor(Core core, Vector2 position, MagicColor color, bool roll) : base(core, position)
    {
      Color = color;
      boundingBox = new Rectangle(0, 0, 26, 17);
      this.Position.Y -= 17;

      state = roll ? SlimeState.Roll : SlimeState.Crawl;

      animation = new Animation(0.08f);
      animation.Add("crawl", core.SpriteManager.GetFrames("slime_crawl_", new List<int>() { 1, 2, 3 }, color));
      animation.Add("roll", core.SpriteManager.GetFrames("slime_roll_", new List<int>() { 1, 2, 3, 4, 5, 6 }, color));

      AddCollider(new Collider() { Name = "heart", BoundingBox = new Rectangle(0, 0, 25, 17) });
      //AddCollider(new Collider() { Name = "roll-trigger", BoundingBox = new Rectangle(-50, 0, 25, 17) });

      CanMove = true;
      CanFall = true;
      CanLick = true;
    }

    public override void Update(int ticks)
    {
      switch (state)
      {
        case SlimeState.Crawl:
          animation.Play("crawl");
          animation.Speed = 0.08f;
          Velocity.X = -0.15f;
          break;
        case SlimeState.Roll:
          animation.Play("roll");
          animation.Speed = 0.2f;
          Velocity.X = -1.3f;
          break;
      }

      animation.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer[9].DrawSpriteW(animation.GetCurrentFrame(), Position);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      var thisColliderName = GetCollider(thisCollider).Name;

      if (thisColliderName == "heart")
      {

        if (other is ProjectileActor && ((ProjectileActor)other).color == this.Color)
        {
          core.MessageManager.Send(new RemoveActorMessage(this), this);
          core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, animation.GetCurrentFrame())), this);

          DropCoin();
        }

        if (other is PlayerActor)
        {
          (other as PlayerActor).Hurt();
        }
      }

      if (thisColliderName == "roll-trigger")
      {
        if (other is PlayerActor)
        {
          state = SlimeState.Roll;
        }
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

