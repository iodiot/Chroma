﻿using System;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class RuinsSlopedPlatformActor: SlopedPlatformActor
  {

    public RuinsSlopedPlatformActor(Core core, Vector2 position, SlopeDirection direction, Area area, int sections = 1) : 
    base(core, position, direction, area, sections)
    {
    }

    protected override void DrawGround() 
    {
      for (int i = 0; i < Sections; i++) 
      {
        var yPos = dir * i * 16;
        if (Direction == SlopeDirection.Down)
          yPos += 16;

        core.Renderer.DrawSpriteW(
          "ruins_slope",
          new Vector2(Position.X + i * 33, Position.Y + yPos - 26),
          flip: Direction == SlopeDirection.Up ? SpriteFlip.None : SpriteFlip.Horizontal
        );
      }
    }
  }
}

