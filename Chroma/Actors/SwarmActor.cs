using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Particles;

namespace Chroma.Actors
{
  public class SwarmActor : Actor
  {
    private readonly ParticleManager particleManager;

    public SwarmActor(Core core, Vector2 position, Sprite sprite) : base(core, position)
    {
      particleManager = new ParticleManager(
        core,
        new DispersalParticleBehaviour(new Vector2(0, 1.0f), 0.9f, 52.0f)
      );
      particleManager.AddParticlesFromSprite(sprite, position, new Vector2(5.0f, -5.0f), 1.0f, 3);
    }

    public override void Update(int ticks)
    {
      particleManager.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      particleManager.Draw(2.0f);

      base.Draw();
    }
  }
}

