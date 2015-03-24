using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.States;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class SpriteDestroyerActor : Actor
  {
    private readonly ParticleManager pm;
    private float groundLevel;

    public SpriteDestroyerActor(Core core, Vector2 position, Sprite sprite, float pixelRate = .25f) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, sprite.Width, sprite.Height);

      var playState = (core.GetCurrentState() as PlayState);
      Actor platform = null;
      if (playState != null)
      {
        platform = playState.ActorManager.FindPlatformUnder(position);
      }
      groundLevel = (platform != null) ? platform.GetBoundingBoxW().Y - 4 : 100500f;

      pm = new ParticleManager(core, 0f);
      pm.OnUpdate = OnParticleUpdate;

      SpawnParticlesFromSprite(position, sprite, pixelRate);

      CanMove = true;
      CanFall = true;
      CanLick = false;

      Ttl = 100;
    }

    public override void Update(int ticks)
    {
      pm.Update();

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

    private void SpawnParticlesFromSprite(Vector2 position, Sprite sprite, float pixelRate)
    {
      const float Scale = 2f;

      var texture = core.SpriteManager.GetTexture(sprite.TextureName);
      var textureData = core.SpriteManager.GetTextureData(sprite.TextureName);

      for (var y = sprite.Y; y < sprite.Y + sprite.SrcHeight; ++y)
      {
        for (var x = sprite.X; x < sprite.X + sprite.SrcWidth; ++x)
        {
          var color = textureData[x + y * texture.Width];

          if (color.A != 0 && SciHelper.ChanceRoll(pixelRate))
          {
            var p = new Particle();

            p.Position = position + new Vector2(x - sprite.X, y - sprite.Y) + sprite.GetOffset();
            p.Color = color;
            p.Ttl = SciHelper.GetRandom(100, 125);
            p.Sprite = core.SpriteManager.OnePixelSprite;
            p.Scale = new Vector2(Scale, Scale);

            p.Velocity.X = SciHelper.GetRandom(-1f, 1f);
            p.Velocity.Y = SciHelper.GetRandom(-5f, 0f);

            pm.Spawn(p);
          }
        }
      }
    }

    private void OnParticleUpdate(Particle particle)
    {
      particle.Color *= .99f;

      if (particle.Position.Y > groundLevel)
      {
        particle.Velocity = Vector2.Zero;
      }
      else
      {
        // Apply gravity
        particle.Velocity.Y += .2f;
      }
    }
  }
}

