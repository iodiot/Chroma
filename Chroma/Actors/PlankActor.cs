﻿using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.States;

namespace Chroma.Actors
{
  public class PlankActor : OneWayPlatform
  {
    Sprite sprite;
    public int Width { get; private set; }
    private bool isSteppedOn = false;
    private int dy = 0;

    public enum PlankOrigin
    {
      Left,
      Middle,
      Right
    }

    public PlankActor(Core core, Vector2 position, PlankOrigin origin = PlankOrigin.Middle) : base(core, position)
    {
      sprite = core.SpriteManager.GetSprite(
        (origin == PlankOrigin.Left) ? "plank_left" :
        (origin == PlankOrigin.Right) ? "plank_right" :
        "plank_" + core.GetRandom(1, 3).ToString());
      boundingBox = new Rectangle(0, 0, sprite.Width, 7);

      Width = sprite.Width;

      CanFall = false;
      CanLick = false;
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(sprite, new Vector2(X, Y - 7 + dy));

      base.Draw();
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      isSteppedOn = true;
      base.OnBoundingBoxTrigger(other);
    }

    public override void Update(int ticks)
    {
      if (!isSteppedOn && dy > 0)
        dy--;
      if (isSteppedOn && dy < 3)
        dy++;

      isSteppedOn = false;

      base.Update(ticks);
    }
  }
}

