using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.Actors
{
  public enum ProjectileColor
  {
    Red,
    Green,
    Blue
  };

  public class ProjectileActor : CollidableActor
  {
    private readonly ProjectileColor color;
    private readonly Animation animation;

    public ProjectileActor(Core core, Vector2 position) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 14, 8);

      color = ProjectileColor.Red;

      animation = new Animation();
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("projectile_red_", new List<int>() { 1, 2, 3, 4 }));
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);

      X += 3.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      //core.Renderer.DrawSpriteW("glow", Position, Color.White);
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, Color.White);

      base.Draw();
    }

    public override void OnCollide(CollidableActor other)
    {        
      core.MessageManager.Send(new RemoveActorMessage(this), this);
      core.MessageManager.Send(new AddActorMessage(new SwarmActor(core, Position, animation.GetCurrentFrame())), this);
    }
  }
}

