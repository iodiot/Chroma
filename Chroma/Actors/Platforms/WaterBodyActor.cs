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

    public WaterBodyActor(Core core, Vector2 position, int width, Area area) : base(core, position, width, area)
    {
      edge = core.SpriteManager.GetSprite("water_edge");
      surface = core.SpriteManager.GetSprite("water_surface_edge");
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
        new Vector2(box.Left, box.Top));
      box.X += surface.Width;
      box.Width -= surface.Width * 2;
      core.Renderer["add"].DrawSpriteW(surface, 
        new Vector2(box.Right, box.Top), flip: SpriteFlip.Horizontal);
      core.Renderer["bg_add"].DrawRectangleW(box, new Color(75, 134, 176));
      box.Y = box.Bottom;
      box.Height = 4;
      core.Renderer["add"].DrawRectangleW(box, new Color(75, 134, 176));

      box.Y = box.Bottom;
      box.Height = 1;
      core.Renderer["add"].DrawRectangleW(box, new Color(166, 201, 226));

      box.Y = box.Bottom;
      box.Height = 300;
      core.Renderer["add"].DrawRectangleW(box, new Color(0, 69, 119));
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Left - edge.Width, box.Top), scale: new Vector2(1f, box.Height));
      core.Renderer["add"].DrawSpriteW(edge, 
        new Vector2(box.Right, box.Top), scale: new Vector2(1f, box.Height), flip: SpriteFlip.Horizontal);
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

