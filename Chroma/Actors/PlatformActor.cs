using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public abstract class PlatformActor : Actor
  {
    protected Area area;

    public PlatformActor PreviousPlatform { get; set; }
    public PlatformActor NextPlatform { get; set; }
    public int LeftY { get; protected set; }
    public int RightY { get; protected set; }

    public PlatformActor(Core core, Vector2 position, Area area) : base(core, position) 
    {
      this.area = area;
      PreviousPlatform = null;
      NextPlatform = null;
    }
      
    protected void DrawEdges()
    {
      Sprite top;
      Sprite middle;
      Sprite bottomEdge;
      int edx, edy, tdx, tdy;

      switch (area) {
        default:
        case Area.Jungle:
          top = core.SpriteManager.GetSprite("edge_grass");
          middle = core.SpriteManager.GetSprite("edge_earth");
          bottomEdge = core.SpriteManager.GetSprite("edge_earth_bottom");
          edx = 12;
          edy = 0;
          tdx = 1;
          tdy = 9;
          break;
        case Area.Ruins:
          top = core.SpriteManager.GetSprite("stone_edge");
          middle = core.SpriteManager.GetSprite("bricks_edge");
          bottomEdge = core.SpriteManager.GetSprite("edge_earth_bottom");
          edx = 5;
          edy = 6;
          tdx = 0;
          tdy = 6;
          break;
      }

      // Right edge
      if (NextPlatform == null || NextPlatform.LeftY > RightY)
      {
        var bottom = 0;
        if (NextPlatform != null)
          bottom = NextPlatform.LeftY;
        else
          bottom = GetBoundingBoxW().Bottom;

        // Side
        var i = RightY;
        do {
          core.Renderer.DrawSpriteW(middle, 
            new Vector2(GetBoundingBoxW().Right - edx, i));
          i += middle.Height - edy;
        } while (i < bottom);

        // Bottom
        i = bottom - bottomEdge.Height + 20;
        core.Renderer.DrawSpriteW(bottomEdge, 
          new Vector2(GetBoundingBoxW().Right - edx, i));

        // Top
        core.Renderer.DrawSpriteW(top, 
          new Vector2(GetBoundingBoxW().Right - tdx, RightY - tdy));
      }

      // Left edge
      if (PreviousPlatform == null || PreviousPlatform.RightY > LeftY)
      {
        var bottom = 0;
        if (PreviousPlatform != null)
          bottom = PreviousPlatform.RightY;
        else
          bottom = GetBoundingBoxW().Bottom;

        // Side
        var i = LeftY;
        do {
          core.Renderer.DrawSpriteW(middle, 
            new Vector2(GetBoundingBoxW().Left - middle.Width + edx, i),
            flip: SpriteFlip.Horizontal);
          i += middle.Height - edy;
        } while (i < bottom);

        // Bottom
        i = bottom - bottomEdge.Height + 20;
        core.Renderer.DrawSpriteW(bottomEdge, 
          new Vector2(GetBoundingBoxW().Left - bottomEdge.Width + edx, i),
          flip: SpriteFlip.Horizontal);

        // Top
        core.Renderer.DrawSpriteW(top, 
          new Vector2(GetBoundingBoxW().Left - top.Width + tdx, LeftY - tdy),
          flip: SpriteFlip.Horizontal);

      }
    }
  }
}

