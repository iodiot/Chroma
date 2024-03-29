﻿using System;
using Chroma.Graphics;
using Chroma.Gameplay;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class ItemActor : Actor
  {
    private Animation bubbleAnim, itemAnim;

    private bool bubble;
    private MagicColor color;

    private int animOffset;

    public ItemActor(Core core, Vector2 position) : base(core, position)
    {
      bubbleAnim = new Animation(0.1f);
      bubbleAnim.AddAndPlay("float", core.SpriteManager.GetFrames("powerup_bubble_", new List<int> { 1, 2, 1, 3 }));

      itemAnim = new Animation(0.1f);
      itemAnim.AddAndPlay("rotate", core.SpriteManager.GetFrames("heart_", new List<int> { 1, 1, 2, 3, 4 }));

      bubble = true;
      color = MagicManager.GetRandomColor(0.5f);

      CanFall = false;
      CanLick = false;
      CanMove = true;
      CanBounce = true;

      IsSolid = false;

      animOffset = SciHelper.GetRandom(0, 10000);

      boundingBox = new Rectangle(0, 0, bubbleAnim.GetCurrentFrame().Width, bubbleAnim.GetCurrentFrame().Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });
    }

    public override void Update(int ticks)
    {
      bubbleAnim.Update(ticks);
      itemAnim.Update(ticks);

      if (bubble)
      {
        Velocity.X = 0.6f * (float)Math.Cos((ticks + animOffset) / 20);
        Velocity.Y = 0.3f * (float)Math.Cos((ticks + animOffset) / 17);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      if (bubble)
      {
        core.Renderer[-1].DrawGlowW(Position + new Vector2(8), MagicManager.MagicColors[color] * 0.2f, 18);
        core.Renderer[2].DrawSpriteW(bubbleAnim.GetCurrentFrame(), Position, MagicManager.MagicColors[color]);
      }
      else
      {
        core.Renderer[-1].DrawGlowW(Position + new Vector2(8, 9), Color.Red * 0.2f, 10);
      }
      core.Renderer[1].DrawSpriteW(itemAnim.GetCurrentFrame(), Position + new Vector2(4, 5));

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (bubble && other is ProjectileActor && ((ProjectileActor)other).color == this.color)
      {
        (other as ProjectileActor).Explode(false);

        bubble = false;
        CanFall = true;
        Velocity.X += 0.5f * SciHelper.GetRandom(3, 5);
        core.SoundManager.Play("bubble_pop");
      }

      if (!bubble && other is PlayerActor)
      {
        (other as PlayerActor).PickItem();
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

