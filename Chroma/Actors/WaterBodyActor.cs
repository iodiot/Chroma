using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class WaterBodyActor : Actor
  {
    private Sprite edge;
    private Sprite surface;

    public WaterBodyActor(Core core, Vector2 position, int width) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, width, 300);

      CanMove = false;
      CanFall = false;

      edge = core.SpriteManager.GetSprite("water_edge");
      surface = core.SpriteManager.GetSprite("water_surface_edge");
    }

    public override void Draw()
    {
      var box = GetBoundingBoxW();
      box.X -= 6;
      box.Width += 12;
      box.Height = 8;
      box.Y -= 5;
      core.Renderer["add"].DrawSpriteW(surface, 
        new Vector2(box.Left, box.Top));
      box.X += surface.Width;
      box.Width -= surface.Width * 2;
      core.Renderer["add"].DrawSpriteW(surface, 
        new Vector2(box.Right, box.Top), flip: SpriteFlip.Horizontal);
      core.Renderer["bg_add"].DrawRectangleW(box, new Color(75, 134, 176));
      box.Y = box.Bottom;
      box.Height = 1;
      core.Renderer["bg_add"].DrawRectangleW(box, new Color(166, 201, 226));

      box.Y = box.Bottom;
      box.Height = 300;
      core.Renderer["add"].DrawRectangleW(box, new Color(0, 69, 119));
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Left - edge.Width, box.Top), scale: new Vector2(1f, box.Height));
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Right, box.Top), scale: new Vector2(1f, box.Height), flip: SpriteFlip.Horizontal);

      base.Draw();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

