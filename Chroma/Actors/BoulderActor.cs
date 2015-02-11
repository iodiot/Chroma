using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class BoulderActor : Actor
  {
    private Sprite sprite;
    public BoulderActor(Core core, Vector2 position) : base (core, position)
    {
      sprite = core.SpriteManager.GetSprite("boulder_1");
      this.Position.Y -= sprite.Height - 1;
      boundingBox = new Rectangle(0, 3, sprite.Width, sprite.Height);

      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[10].DrawSpriteW(sprite, Position);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor is SlimeWalkActor;
    }
  }
}

