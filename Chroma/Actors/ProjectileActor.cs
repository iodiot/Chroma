using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class ProjectileActor : CollidableActor
  {
    public readonly MagicColor color;
    private readonly Animation animation;

    public ProjectileActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 14, 8);

      Ttl = 200;

      this.color = color;

      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("projectile_", new List<int>() { 1, 2, 3, 4 }));
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);

      X += 8.0f;

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

    public override void OnCollide(CollidableActor other)
    {        
      core.MessageManager.Send(new RemoveActorMessage(this), this);
      core.MessageManager.Send(new AddActorMessage(new SwarmActor(core, Position, animation.GetCurrentFrame())), this);

      core.Renderer.ShakeScreen(5.0f, 10);
    }
  }
}

