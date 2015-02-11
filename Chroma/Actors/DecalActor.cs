using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class DecalActor : Actor
  {
    private Sprite sprite;
    private string layer;
    private int zIndex;
    private bool flip;

    public DecalActor(Core core, Vector2 position, string spriteName, string layer = "", int zIndex = 0, bool flip = false) : base(core, position)
    {
      sprite = core.SpriteManager.GetSprite(spriteName);
      this.layer = layer;
      this.zIndex = zIndex;
      this.flip = flip;
      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[layer, zIndex].DrawSpriteW(sprite, Position, flip: flip ? SpriteFlip.Horizontal : SpriteFlip.None);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

