using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public abstract class PlatformActor : Actor
  {
    public PlatformActor PreviousPlatform { get; set; }
    public PlatformActor NextPlatform { get; set; }
    public int LeftY { get; protected set; }
    public int RightY { get; protected set; }

    public PlatformActor(Core core, Vector2 position) : base(core, position) 
    {
      PreviousPlatform = null;
      NextPlatform = null;
    }

    protected void DrawEdges()
    {
      var grass = core.SpriteManager.GetSprite("edge_grass");
      var earth = core.SpriteManager.GetSprite("edge_earth");
      var earthBottom = core.SpriteManager.GetSprite("edge_earth_bottom");

      // Right edge
      if (NextPlatform == null || NextPlatform.LeftY > RightY)
      {
        var bottom = 0;
        if (NextPlatform != null)
          bottom = NextPlatform.LeftY;
        else
          bottom = GetBoundingBoxW().Bottom;

        var i = bottom + 20;
        var edge = earthBottom;
        do
        {
          i -= edge.Height;
          core.Renderer.DrawSpriteW(edge, 
            new Vector2(GetBoundingBoxW().Right - 12, i));
          edge = earth;
        } while (i - edge.Height > RightY);

        core.Renderer.DrawSpriteW(grass, 
          new Vector2(GetBoundingBoxW().Right - 1, RightY - 9));
      }

      // Left edge
      if (PreviousPlatform == null || PreviousPlatform.RightY > LeftY)
      {
        var bottom = 0;
        if (PreviousPlatform != null)
          bottom = PreviousPlatform.RightY;
        else
          bottom = GetBoundingBoxW().Bottom;

        var i = bottom + 20;
        var edge = earthBottom;
        do
        {
          i -= edge.Height;
          core.Renderer.DrawSpriteW(edge, 
            new Vector2(GetBoundingBoxW().Left - edge.Width + 12, i),
            flip: SpriteFlip.Horizontal);
          edge = earth;
        } while (i - edge.Height > LeftY);

        core.Renderer.DrawSpriteW(grass, 
          new Vector2(GetBoundingBoxW().Left - grass.Width + 1, Y - 9), 
          flip: SpriteFlip.Horizontal);
      }
    }
  }
}

