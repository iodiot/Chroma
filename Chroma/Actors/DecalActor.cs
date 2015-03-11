using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class DecalActor : Actor
  {
    private readonly string layer;
    private readonly int depth;
    private readonly SpriteFlip spriteFlip;
    private readonly float scale;
    private Sprite sprite;

    public DecalActor(Core core, Vector2 position, string spriteName, string layer = "", int depth = 0, bool flip = false, float scale = 1) : base(core, position)
    {
      this.layer = layer;
      this.depth = depth;
      this.scale = scale;
      this.spriteFlip = flip ? SpriteFlip.Horizontal : SpriteFlip.None;

      sprite = core.SpriteManager.GetSprite(spriteName);

      boundingBox = new Rectangle(0, 0, (int)(sprite.Width * scale), (int)(sprite.Height * scale));
      //boundingBox = Rectangle.Empty;

      CanMove = false;
    }

    public override void Draw()
    {
      core.Renderer[layer, depth].DrawSpriteW(sprite, Position, flip: spriteFlip, scale: new Vector2(scale));

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

