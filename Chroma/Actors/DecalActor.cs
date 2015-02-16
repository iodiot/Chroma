using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class DecalActor : Actor
  {
    private readonly string spriteName;
    private readonly string layer;
    private readonly int depth;
    private readonly SpriteFlip spriteFlip;

    public DecalActor(Core core, Vector2 position, string spriteName, string layer = "", int depth = 0, bool flip = false) : base(core, position)
    {
      this.spriteName = spriteName;
      this.layer = layer;
      this.depth = depth;
      this.spriteFlip = flip ? SpriteFlip.Horizontal : SpriteFlip.None;

      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[layer, depth].DrawSpriteW(spriteName, Position, flip: spriteFlip);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

