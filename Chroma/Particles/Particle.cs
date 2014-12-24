using Microsoft.Xna.Framework;
using System;
using Chroma.Graphics;

namespace Chroma.Particles
{
  public sealed class Particle
  {
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public int Ttl;
    public float Rotation;
    public Sprite Sprite;

    public bool IsDead { get { return Ttl == 0; } }
  }
}

