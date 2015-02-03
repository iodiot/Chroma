using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class InvisiblePlatformActor: Actor
  {
    public InvisiblePlatformActor(Core core, Vector2 position, int width, int height) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, width, height);

      CanMove = false;
      CanFall = true;
    }
  }
}

