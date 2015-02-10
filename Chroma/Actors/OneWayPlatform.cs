using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class OneWayPlatform : PlatformActor
  {
    public OneWayPlatform(Core core, Vector2 position, float width, float height) : base(core, position)
    {
      SetBoundingBox(0, 0, width, height);
    }

    public override bool IsPassableFor(Actor actor)
    {
      var actorBox = actor.GetBoundingBoxW();
      var thisBox = GetBoundingBoxW();

      var f = actorBox.Y + actorBox.Height <= thisBox.Y;

      return !f;
    }
  }
}

