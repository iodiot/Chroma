using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Helpers;

namespace Chroma
{
  public sealed class Particle
  {
    public Sprite Sprite;
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public Vector2 Scale;
    public float Rotation;
    public int Ttl;
    public float RotationSpeed;
  }

  public sealed class ParticleManager
  {
    public float SpawnRate;

    private readonly Core core;

    public Action<Particle> OnSpawn;
    public Action<Particle> OnUpdate;

    private List<Particle> particles;

    private int deadCounter;

    public ParticleManager(Core core, float spawnRate)
    {
      this.core = core;
      SpawnRate = spawnRate;

      particles = new List<Particle>();
    }

    public void Spawn()
    {
      var particle = new Particle();

      if (OnSpawn != null)
      {
        // Set defaults
        particle.Sprite = core.SpriteManager.OnePixelSprite;
        particle.Scale = new Vector2(1f, 1f);

        OnSpawn(particle);
      }

      // Try to find free slot
      var n = -1;
      for (var i = 0; i < particles.Count; ++i)
      {
        if (particles[i].Ttl == 0)
        {
          n = i;
          break;
        }
      }

      if (n != -1)
      {
        particles[n] = particle;
      }
      else
      {
        particles.Add(particle);
      }
    }

    public void Spawn(Particle particle)
    {
      particles.Add(particle);
    }

    public void Update()
    {
      deadCounter = 0;

      if (SpawnRate > SciHelper.Eps)
      {
        var count = (int)Math.Round(SciHelper.GetNormalRandom(SpawnRate, SpawnRate * .5f));
        for (var i = 0; i < count; ++i)
        {
          Spawn();
        }
      }

      if (OnUpdate != null)
      {
        foreach (var p in particles)
        {
          if (p.Ttl > 0)
          {
            OnUpdate(p);
          }
        }
      }

      foreach (var p in particles)
      {
        if (p.Ttl > 0)
        {
          p.Position += p.Velocity;
          p.Rotation += p.RotationSpeed;
          p.Ttl -= 1;
        }
        else
        {
          ++deadCounter;
        }
      }
    }

    public void Draw()
    {
      foreach (var p in particles)
      {
        if (p.Ttl > 0)
        {
          core.Renderer[0].DrawSpriteW(
            p.Sprite,
            p.Position,
            p.Color,
            p.Scale,
            p.Rotation
          );
        }
      }
    }
  }
}

