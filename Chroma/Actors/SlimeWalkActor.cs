﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class SlimeWalkActor : CollidableActor
  {
    private readonly Animation walkAnimation, legAnimation; 
    public readonly MagicColor color;

    public SlimeWalkActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;
      boundingBox = new Rectangle(15, 0, 18, 20);

      walkAnimation = new Animation(0.15f);
      walkAnimation.AddAndPlay("live", core.SpriteManager.GetFrames("slime_walk_", new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }));

      legAnimation = new Animation(0.15f);
      legAnimation.AddAndPlay("live", core.SpriteManager.GetFrames("slime_walk_leg_", new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }));
    }

    public override void Update(int ticks)
    {
      walkAnimation.Update(ticks);
      legAnimation.Update(ticks);

      X -= 1.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(walkAnimation.GetCurrentFrame(), Position, MagicManager.MagicColors[color], 1.0f, 0);
      core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(10, 15), MagicManager.MagicColors[color] * 0.6f);
      core.Renderer["fg"].DrawSpriteW(legAnimation.GetCurrentFrame(), Position, Color.White, 1.0f, 0);

      base.Draw();
    }

    public override void OnCollide(CollidableActor other)
    {
      if (other is ProjectileActor && ((ProjectileActor)other).color == this.color)
      {
        core.MessageManager.Send(new RemoveActorMessage(this), this);
        core.MessageManager.Send(new AddActorMessage(new SwarmActor(core, Position, walkAnimation.GetCurrentFrame())), this);
      }

      base.OnCollide(other);
    }
  }
}
