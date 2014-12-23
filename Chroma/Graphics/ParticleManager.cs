using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Chroma.Graphics
{
  public sealed class ParticleManager
  {
    private readonly Core core;
    private readonly List<Particle> particles;

    private float groundLevel;
    private int ttl;

    public ParticleManager(Core core)
    {
      this.core = core;

      particles = new List<Particle>();

      groundLevel = 52;
      ttl = 100;
    }

    public void AddParticlesFromSprite(Sprite sprite, Vector2 position, int reductionFactor = 1)
    {
      var texture = core.SpriteManager.GetTexture(sprite.TextureName);
      var textureData = core.SpriteManager.GetTextureData(sprite.TextureName);
      var random = new Random();

      for (var y = sprite.Y; y < sprite.Y + sprite.Height; ++y)
      {
        for (var x = sprite.X; x < sprite.X + sprite.Width; ++x)
        {
          // randomly skip sprite pixels depending on reductionFactor
          if (reductionFactor > 1 && ((random.Next() % reductionFactor) != 0))
          {
            continue;
          }

          var color = textureData[x + y * texture.Width];

          if (color.A != 0)
          {
            // calc velocity
            var v = new Vector2(x - sprite.X, y - sprite.Y) - new Vector2(sprite.Width / 2, 0);
            v.Normalize();
            v.X *= 2.0f * (float)random.NextDouble();
            v.Y *= -5.0f * (float)random.NextDouble();

            AddParticle(new Particle() {
              Position = new Vector2(x - sprite.X, y - sprite.Y - 1) + position, 
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

        // gravity
        if (particle.Position.Y < groundLevel)
        {
          particle.Position.Y += 1.0f;
        }
        else
        {
          particle.Ttl = 0;
          continue;
        }

        particle.Position += particle.Velocity;
        particle.Velocity *= 0.9f;
        particle.Ttl -= 1;
      }

      --ttl;
    }

    public void Draw()
    {
      foreach (var particle in particles)
      {
        core.Renderer.DrawRectangleW(
          particle.Position,
          2,
          2,
          particle.Color * ((float)ttl / 100.0f)
        );
      }
    }
  }
}

