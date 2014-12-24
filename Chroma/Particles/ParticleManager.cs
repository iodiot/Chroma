using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Particles
{
  public sealed class ParticleManager
  {
    private readonly Core core;
    private readonly List<Particle> particles;
    private readonly ParticleBehaviour behaviour;

    private int ttl;

    public ParticleManager(Core core, ParticleBehaviour behaviour)
    {
      this.core = core;
      this.behaviour = behaviour;

      particles = new List<Particle>();

      ttl = 100;
    }

    public void AddParticlesFromSprite(Sprite sprite, Vector2 position, Vector2 velocity,
      float scale = 1.0f, int reductionFactor = 1)
    {
      var texture = core.SpriteManager.GetTexture(sprite.TextureName);
      var textureData = core.SpriteManager.GetTextureData(sprite.TextureName);
      var random = new Random();

      for (var y = sprite.Y; y < sprite.Y + sprite.Height; ++y)
      {
        for (var x = sprite.X; x < sprite.X + sprite.Width; ++x)
        {
          // randomly skip sprite pixels depending on reductionFactor
          if ((reductionFactor > 1) && ((random.Next() % reductionFactor) != 0))
          {
            continue;
          }

          var color = textureData[x + y * texture.Width];

          if (color.A != 0)
          {
            // calc velocity
            var v = new Vector2(x - sprite.X, y - sprite.Y) - new Vector2(sprite.Width / 2, 0);
            v.Normalize();
            v.X *= velocity.X * (float)random.NextDouble();
            v.Y *= velocity.Y * (float)random.NextDouble();

            AddParticle(new Particle() {
              Position = new Vector2(x - sprite.X, y - sprite.Y - 1) * scale + position, 
              Velocity = v, 
              Color = color, 
              Ttl = 100,
              Rotation = 0,
              Sprite = null
            });
          }
        }
      }
    }

    public void AddParticle(Particle particle)
    {
      particles.Add(particle);
    }

    public void Clear()
    {
      particles.Clear();
    }

    public void Update(int ticks)
    {
      if (ttl == 0)
      {
        return;
      }
       
      foreach (var particle in particles)
      {
        if (particle.IsDead)
        {
          continue;
        }

        behaviour.Update(particle);

        particle.Ttl -= 1;
      }

      --ttl;
    }

    public void Draw(float scale)
    {
      if (ttl == 0)
      {
        return;
      }

      foreach (var particle in particles)
      {
        core.Renderer.DrawRectangleW(
          particle.Position,
          scale,
          scale,
          particle.Color * ((float)ttl / 100.0f)
        );
      }
    }
  }
}

