using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class FlatPlatformActor : PlatformActor
  {
    private readonly int width;

    public FlatPlatformActor(Core core, Vector2 position, int width) : base(core, position)
    {
      this.width = width;

      boundingBox = new Rectangle(0, 0, width, 300);
      LeftY = RightY = (int)position.Y;

      CanMove = false;
      CanFall = true;
    }

    public override void Draw()
    {
      DrawEdges();
      DrawGround();
      base.Draw();
    }

    private void DrawGround()
    {
      var floor = core.SpriteManager.GetSprite("floor");
      var grass = core.SpriteManager.GetSprite("floor_grass");

      var n = width / floor.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? floor.ClampWidth(width % floor.Width) : floor, 
          new Vector2(Position.X + floor.Width * i, Position.Y - 11)
        );
        core.Renderer.DrawSpriteW(
          i == n ? grass.ClampWidth(width % grass.Width) : grass, 
          new Vector2(Position.X + grass.Width * i, Position.Y - 4)
        );
      }

    }
  }
}

