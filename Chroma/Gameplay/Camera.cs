using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Helpers;

namespace Chroma.Gameplay
{
  public class Camera
  {
    public enum FollowMode
    {
      Simple,
      Platformer
    }

    private Core core;

    private Vector2 position;
    public Vector2 Position { 
      get { return position + shakeOffset; } 
    }

    private Vector2 target;
    private Actor targetActor;
    public FollowMode Mode { get; set; }

    private int shakeDuration = 0;
    private float shakeStrength = 0f;
    private Vector2 shakeOffset;

    public Camera(Core core)
    {
      this.core = core;
      targetActor = null;
      shakeOffset = new Vector2(0f);
      Mode = FollowMode.Simple;
    }

    public void JumpTo(Vector2 newPosition)
    {
      target = newPosition;
      position = target;
    }

    public void JumpTo(float x, float y)
    {
      target = new Vector2(x, y);
      position = target;
    }

    public void Follow(Actor target)
    {
      targetActor = target;
    }

    public void Update(int ticks) 
    {
      if (shakeDuration > 0)
      {
        shakeOffset = SciHelper.GetRandomVectorInCircle(shakeStrength);
        shakeDuration--;
      }
      else
      {
        shakeOffset = new Vector2(0f);
      }

      if (targetActor != null)
      {
        switch (Mode)
        {
          case FollowMode.Simple:
            target = targetActor.Position;
            position += (target - position) * 0.7f;
            break;
          case FollowMode.Platformer:
            var player = targetActor as PlayerActor;
            if (player != null)
            {
              const int HUD_HEIGHT = 120;
              var targetPlatformScreenPos = (core.Renderer.ScreenHeight - HUD_HEIGHT) * 0.9f;

              target.X = player.Position.X - 40;
              target.Y = (player.PlatformY + player.Position.Y) / 2 - targetPlatformScreenPos;

              var newX = position.X + (target.X - position.X) * 0.5f;
              var newY = position.Y + (target.Y - position.Y) * 0.05f;

              newY = Math.Min(newY, player.Position.Y - 10);
              newY = Math.Max(newY, player.Position.Y - targetPlatformScreenPos);

              position = new Vector2(newX, newY);
            }
            break;
        }
      }
    }

    public void Shake(int duration = 10, float strength = 3.0f)
    {
      shakeDuration = duration;
      shakeStrength = strength;
    }

  }
}

