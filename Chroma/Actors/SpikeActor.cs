using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class SpikeActor : CollidableActor
  {
    private readonly int spikesNumber;

    public SpikeActor(Core core, Vector2 position, int spikesNumber) : base(core, position)
    {
      this.spikesNumber = spikesNumber;

      boundingBox = new Rectangle(0, 0, 5 * spikesNumber, 5);
    }

    public override void Draw()
    {
      for (var i = 0; i < spikesNumber; ++i)
      {
        core.Renderer.DrawSpriteW("spike", Position + new Vector2(i * 5, 0), Color.White);
      }

      base.Draw();
    }
  }
}

