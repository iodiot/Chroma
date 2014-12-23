using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public abstract class BodyActor : Actor
  {
    public Vector2 Position;
    public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
    public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
    public int Width;
    public int Height;

    private bool debugDraw; 
    private Color debugColor;

    public BodyActor(Core core, Vector2 position) : base(core)
    {
      Position = position;

      Width = 0;
      Height = 0;

      debugDraw = false;
      debugColor = Color.LightPink;
    }

    public override void Load()
    {
      Debug.Assert(Width != 0, "BodyActor.Load() : Field Width was not set");
      Debug.Assert(Height != 0, "BodyActor.Load() : Field Height was not set");
    }

    public override void Draw()
    {
      if (debugDraw)
      {
        var bounding = GetBoundingRect();
        core.Renderer.DrawRectangleW(
          new Vector2(bounding.X, bounding.Y), 
          bounding.Width,
          bounding.Height,
          debugColor * 0.25f
        );
      }

      base.Draw();
    }

    public virtual Rectangle GetBoundingRect()
    {
      return new Rectangle((int)X, (int)Y, Width, Height);
    }

    public virtual void OnCollide(Actor actor)
    {
    }
  }
}

