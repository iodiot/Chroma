using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public abstract class OneWayPlatform : PlatformActor
  {
    public OneWayPlatform(Core core, Vector2 position) : base(core, position)
    {

    }

    public override bool IsPassableFor(Actor actor)
    {
      var actorBox = actor.GetBoundingBoxW();
      var thisBox = GetBoundingBoxW();

      var f = actorBox.Bottom <= thisBox.Top + 2.0f; // Lick step = 2.0 

      return !f;
    }
  }
}

