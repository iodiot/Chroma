using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class WaterBodyActor : GapActor
  {
    private Sprite edge;
    private Sprite surface;
    private Color surfaceTint, bodyTint, edgeTint;

    public WaterBodyActor(Core core, Vector2 position, int width, Area area) : base(core, position, width, area)
    {
      edge = core.SpriteManager.GetSprite("water_edge");
      surface = core.SpriteManager.GetSprite("water_surface_edge");
      switch (area)
      {
        default:
        case Area.Jungle:
          surfaceTint = new Color(75, 134, 176);
          edgeTint = new Color(166, 201, 226);
          bodyTint = new Color(0, 69, 119);
          break;
        case Area.Ruins:
          surfaceTint = new Color(56, 130, 90);
          edgeTint = new Color(123, 168, 144);
          bodyTint = new Color(0, 46, 21);
          break;
      }
    }

    public override void Draw()
    {
      base.Draw();

      var box = GetBoundingBoxW();
      box.Y += 20;

      box.X -= 6;
      box.Width += 12;
      box.Height = 4;
      box.Y -= 5;
      core.Renderer["add"].DrawSpriteW(surface, 
        new Vector2(box.Left, box.Top), tint: surfaceTint);
      box.X += surface.Width;
      box.Width -= surface.Width * 2;
      core.Renderer["add"].DrawSpriteW(surface, 
        new Vector2(box.Right - 1, box.Top), flip: SpriteFlip.Horizontal, tint: surfaceTint);
      core.Renderer["bg_add"].DrawRectangleW(box, surfaceTint);
      box.Y = box.Bottom;
      box.Height = 4;
      core.Renderer["add"].DrawRectangleW(box, surfaceTint);

      box.Y = box.Bottom;
      box.Height = 1;
      core.Renderer["add"].DrawRectangleW(box, edgeTint);
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Left - edge.Width, box.Top), scale: new Vector2(1f, box.Height), tint: edgeTint);
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Right, box.Top), scale: new Vector2(1f, box.Height), flip: SpriteFlip.Horizontal, tint: edgeTint);

      box.Y = box.Bottom;
      box.Height = 300;
      core.Renderer["add"].DrawRectangleW(box, bodyTint);
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Left - edge.Width, box.Top), scale: new Vector2(1f, box.Height), tint: bodyTint);
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Right, box.Top), scale: new Vector2(1f, box.Height), flip: SpriteFlip.Horizontal, tint: bodyTint);
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (!other.CanFall || other.Drowned)
        return;

      if (other.GetBoundingBoxW().Top > GetBoundingBoxW().Top + 20)
      {
        // Play falling sound
        core.SoundManager.Play("splash");
        other.OnDrown();
      }
    }
  }
}

