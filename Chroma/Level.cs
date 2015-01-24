using System;
using Chroma.Actors;

namespace Chroma
{
  public class Level
  {
    private readonly Core core;

    private PlayerActor player;

    public Level(Core core)
    {
      this.core = core;
    }

    public float GetGroundLevel()
    {
      return 85f;  // its not magic number, i swear
    }
  }
}

