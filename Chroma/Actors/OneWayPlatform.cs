using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class OneWayPlatform : PlatformActor
  {
    private readonly float width;

    public OneWayPlatform(Core core, Vector2 position, float width) : base(core, position)
    {
      this.width = width;
    }
  }
}

