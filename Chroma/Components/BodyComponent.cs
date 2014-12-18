using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;

namespace Chroma.Components
{
  class BodyComponent : Component
  {
    public Vector2 Position { get; private set; }
    public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
    public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
      
    public BodyComponent(Actor actor, Vector2 position) : base(ComponentType.Body, actor)
    {
      Position = position; 
    }
  }
}

