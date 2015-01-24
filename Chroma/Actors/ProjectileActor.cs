﻿using System;
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
    private readonly Animation animation;

    public ProjectileActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;

      boundingBox = new Rectangle(0, 0, 14, 8);

      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("projectile_", new List<int>() { 1, 2, 3, 4 }));

      IsStatic = false;
      CanFall = false;
      CanLick = false;

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });

      Ttl = 250;
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);

      Velocity.X += 3.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      Color color = MagicManager.MagicColors[this.color];
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, color * 0.5f);
      core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(20, 23), color * 0.4f);
      //core.Renderer["fg_add"].DrawSpriteW(animation.GetCurrentFrame(), Position, color);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (!(other is ProjectileActor))
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SwarmActor(core, Position, animation.GetCurrentFrame())), this);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return !actor.IsStatic;
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      if (other is PlatformActor)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
      }

      base.OnBoundingBoxTrigger(other);
    }
  }
}

