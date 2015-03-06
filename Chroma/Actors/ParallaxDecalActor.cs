using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class ParallaxDecalActor : DecalActor
  {
    private Vector2 parallax;
    private Vector2 oldWorld;
    private Vector2 oldPosition;

    public ParallaxDecalActor(
      Core core, Vector2 position, Vector2 parallax, string spriteName, 
      string layer = "", int depth = 0, bool flip = false, float scale = 1)
      : base(core, position, spriteName, layer, depth, flip, scale)
    {
      this.parallax = parallax;
      oldWorld = core.Renderer.World;
      oldPosition = position;
    }

    public override void Update(int ticks)
    {
      var newWorld = core.Renderer.World;
      var dWorld = newWorld - oldWorld;

      Position.X = oldPosition.X - dWorld.X + parallax.X * dWorld.X - (core.deviceTilt + 1.5f) * 20 * parallax.X;
      Position.Y = oldPosition.Y - dWorld.Y + parallax.Y * dWorld.Y;

      base.Update(ticks);
    }
  }
}

