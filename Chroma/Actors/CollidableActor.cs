using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public abstract class CollidableActor : Actor
  {
    protected Rectangle boundingBox;

    private bool debugDraw; 
    private Color debugColor;

    public CollidableActor(Core core, Vector2 position) : base(core, position)
    {
      debugDraw = Settings.DrawBoundingBoxes;
      debugColor = Color.LightPink;

      boundingBox = Rectangle.Empty;
    }

    public override void Load()
    {
    }

    public override void Draw()
    {
      if (debugDraw)
      {
        var box = GetBoundingBox();

        core.Renderer.DrawRectangleW(
          new Vector2(box.X, box.Y), 
          box.Width,
          box.Height,
          debugColor * 0.25f
        );
      }

      base.Draw();
    }

    public virtual Rectangle GetBoundingBox()
    {
      return new Rectangle(
        (int)Position.X + boundingBox.X,
        (int)Position.Y + boundingBox.Y,
        boundingBox.Width,
        boundingBox.Height
      );
    }

    public virtual void OnCollide(CollidableActor other)
    {
    }
  }
}

