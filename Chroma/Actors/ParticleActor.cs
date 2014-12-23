using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma
{
  public class ParticleActor : BodyActor
  {
    private readonly ParticleManager particleManager;

    public ParticleActor(Core core, Vector2 position, Sprite sprite) : base(core, position)
    {
      particleManager = new ParticleManager(core);
      particleManager.AddParticlesFromSprite(sprite, position, 2);

      Width = 1;
      Height = 1;
    }

    public override void Update(int ticks)
    {
      particleManager.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      particleManager.Draw();

      base.Draw();
    }
  }
}

