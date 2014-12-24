using System;
using Microsoft.Xna.Framework;

namespace Chroma.Particles
{
  public class DispersalParticleBehaviour : ParticleBehaviour
  {
    private readonly Vector2 gravity;
    private readonly float velocityDrag;
    private readonly float groundLevel;

    public DispersalParticleBehaviour(Vector2 gravity, float velocityDrag, float groundLevel)
    {
      this.gravity = gravity;
      this.velocityDrag = velocityDrag;
      this.groundLevel = groundLevel;
    }

    public override void Update(Particle particle)
    {
      if (particle.Position.Y < groundLevel)
      {
        particle.Position += gravity;

        particle.Position += particle.Velocity;
        particle.Velocity *= velocityDrag;
      }
    }
  }
}

