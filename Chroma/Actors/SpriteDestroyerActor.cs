using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.Actors
{
  public class SpriteDestroyerActor : Actor
  {
    private readonly ParticleManager pm;
    private float groundLevel;

    public SpriteDestroyerActor(Core core, Vector2 position, Sprite sprite) : base(core, position)
    {
      boundingBox = Rectangle.Empty;

      pm = new ParticleManager(core, 0.0f);
      pm.OnPreUpdate = OnParticlePreUpdate;

      SpawnParticlesFromSprite(position, sprite);

      CanMove = true;
      CanFall = true;
      CanLick = false;

      Ttl = 100;

      groundLevel = position.Y + sprite.Height;
    }

    public override void Update(int ticks)
    {
      pm.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      pm.Draw();

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }

    private void SpawnParticlesFromSprite(Vector2 position, Sprite sprite)
    {
      const float PixelRate = 0.5f;
      const float Scale = 2.0f;

      var texture = core.SpriteManager.GetTexture(sprite.TextureName);
      var textureData = core.SpriteManager.GetTextureData(sprite.TextureName);
      var random = new Random();

      for (var y = sprite.Y; y < sprite.Y + sprite.Height; ++y)
      {
        for (var x = sprite.X; x < sprite.X + sprite.Width; ++x)
        {
          var color = textureData[x + y * texture.Width];

          if (color.A == 0 || (float)random.NextDouble() > PixelRate)
          {
            continue;
          }

          var p = new Particle();

          p.Position = position + new Vector2(x - sprite.X, y - sprite.Y);
          p.Color = color;
          p.Ttl = 100 + random.Next() % 25;
          p.Sprite = core.SpriteManager.OnePixelSprite;
          p.Scale = new Vector2(Scale, Scale);
          p.RotationSpeed = ((float)random.NextDouble() * 2.0f - 1.0f) * 0.25f;

          p.Velocity.X = (float)random.NextDouble() * 2.0f - 1.0f;
          p.Velocity.Y = (float)random.NextDouble() * -2.0f;

          pm.Spawn(p);
        }
      }
    }

    private void OnParticlePreUpdate(Particle particle)
    {
      particle.Color *= 0.99f;

      if (particle.Position.Y >= groundLevel)
      {
        particle.Velocity = Vector2.Zero;
      }
      else
      {
        // apply gravity
        particle.Velocity.Y += 0.1f;
      }
    }
  }
}

