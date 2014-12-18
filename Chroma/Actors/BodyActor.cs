using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  abstract class BodyActor : Actor
  {
    public Vector2 Position { get; set; }
    public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
    public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
    public int Width;
    public int Height;

    private bool debugDraw; 
    private Color debugColor;

    public BodyActor(Core core, Vector2 position) : base(core)
    {
      Position = position;

      debugDraw = false;
      debugColor = Color.Red;
    }

    public override void Draw()
    {
      if (debugDraw)
      {
        core.Renderer.DrawRectangleW(Position, Width, Height, debugColor * 0.25f);
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

