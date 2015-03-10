using System;
using Microsoft.Xna.Framework;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class InvisiblePlatformActor: PlatformActor
  {
    public InvisiblePlatformActor(Core core, Vector2 position, int width, int height, Area area) : base(core, position, area)
    {
      boundingBox = new Rectangle(0, 0, width, height);

      CanMove = false;
      CanFall = true;
    }
  }
}

