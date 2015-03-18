using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class GapActor : Actor
  {
    private Area area;
    private Sprite brim;
    private Color color;
    private int dy;

    public GapActor(Core core, Vector2 position, int width, Area area) : base(core, position)
    {
      this.area = area;

      boundingBox = new Rectangle(0, 0, width, 300);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      CanMove = false;
      CanFall = false;

      IsSolid = false;

      switch (area)
      {
        default:
        case Area.Jungle:
          brim = core.SpriteManager.GetSprite(SpriteName.earth_hole_brim);
          color = new Color(5, 7, 14);
          dy = 0;
          break;
        case Area.Ruins:
          brim = core.SpriteManager.GetSprite(SpriteName.stone_hole_brim);
          color = new Color(4, 9, 12);
          dy = 0;
          break;
      }
    }

    public override void Draw()
    {
      var box = GetBoundingBoxW();
      box.X -= 6;
      box.Width += 12;
      box.Y -= dy;

      box.Y += brim.Height - 1;
      core.Renderer["bg", 1000].DrawRectangleW(box, color);
      box.Y -= brim.Height - 1;
      core.Renderer["bg", 1000].DrawSpriteTiledW(brim, box, tileVertically: false);

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (!other.CanFall || other.Fell)
        return;

      if (other.GetBoundingBoxW().Top > GetBoundingBoxW().Top + 30)
      {
        // Play falling sound
        core.SoundManager.Play("fall");
        other.OnFall();
      }
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

