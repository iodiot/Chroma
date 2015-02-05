using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  abstract public class PlatformActor : Actor
  {
    public PlatformActor previousPlatform { get; set; }
    public PlatformActor nextPlatform { get; set; }
    public int LeftY { get; protected set; }
    public int RightY { get; protected set; }


    public PlatformActor(Core core, Vector2 position) : base(core, position) {
      previousPlatform = null;
      nextPlatform = null;
    }

    protected void DrawEdges()
    {

      Sprite grass = core.SpriteManager.GetSprite("edge_grass");
      Sprite earth = core.SpriteManager.GetSprite("edge_earth");
      Sprite earth_bottom = core.SpriteManager.GetSprite("edge_earth_bottom");

      // Right edge
      if (nextPlatform == null || nextPlatform.LeftY > RightY)
      {
        int bottom = 0;
        if (nextPlatform != null)
          bottom = nextPlatform.LeftY;
        else
          bottom = GetWorldBoundingBox().Bottom;

        int i = bottom + 20;
        Sprite edge = earth_bottom;
        do
        {
          i -= edge.Height;
          core.Renderer.DrawSpriteW(edge, 
            new Vector2(GetWorldBoundingBox().Right - 12, i));
          edge = earth;
        } while (i - edge.Height > RightY);

        core.Renderer.DrawSpriteW(grass, 
          new Vector2(GetWorldBoundingBox().Right - 1, RightY - 9));
      }

      // Left edge
      if (previousPlatform == null || previousPlatform.RightY > LeftY)
      {
        int bottom = 0;
        if (previousPlatform != null)
          bottom = previousPlatform.RightY;
        else
          bottom = GetWorldBoundingBox().Bottom;

        int i = bottom + 20;
        Sprite edge = earth_bottom;
        do
        {
          i -= edge.Height;
          core.Renderer.DrawSpriteW(edge, 
            new Vector2(GetWorldBoundingBox().Left - edge.Width + 12, i),
            flip: SpriteFlip.Horizontal);
          edge = earth;
        } while (i - edge.Height > LeftY);

        core.Renderer.DrawSpriteW(grass, 
          new Vector2(GetWorldBoundingBox().Left - grass.Width + 1, Y - 9), 
          flip: SpriteFlip.Horizontal);
      }
     
    }
  }
}

