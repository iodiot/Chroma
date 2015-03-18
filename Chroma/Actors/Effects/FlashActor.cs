using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class FlashActor : Actor
  {
    private readonly MagicColor color;
    private readonly Sprite sprite;
    private readonly int lifetime;

    public FlashActor(Core core, Vector2 position, MagicColor color) : base(core, position)
    {
      this.color = color;
      sprite = core.SpriteManager.GetSprite("glow");
      lifetime = 10;
      Ttl = lifetime;

      CanMove = false;
      CanFall = false;
    }

    public override void Draw()
    {
      float scale = (float)Ttl * 2 / lifetime;
      float width = (float)sprite.Width * scale;
      core.Renderer["fg_add"].DrawSpriteW(sprite, Position - new Vector2(width / 2, width / 2), MagicManager.MagicColors[color], 
        new Vector2(scale));

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

