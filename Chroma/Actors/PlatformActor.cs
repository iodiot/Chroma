using System;
using Microsoft.Xna.Framework;

namespace Chroma.Actors
{
  public class PlatformActor : Actor
  {
    private readonly int width;

    public PlatformActor(Core core, Vector2 position, int width) : base(core, position)
    {
      this.width = width;

      boundingBox = new Rectangle(0, 3, width, core.SpriteManager.GetSprite("earth").Height + 5);

      CanMove = false;
      CanFall = true;
    }

    public override void Draw()
    {
      DrawGround();

      base.Draw();
    }

    private void DrawGround()
    {
      var ground = core.SpriteManager.GetSprite("ground");
      var earth = core.SpriteManager.GetSprite("earth");
      var floor = core.SpriteManager.GetSprite("floor");

      var n = width / earth.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? earth.ClampWidth(width % earth.Width) : earth, 
          new Vector2(Position.X + earth.Width * i, Position.Y + 8), 
          Color.White
        );        
      }

      n = width / floor.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? floor.ClampWidth(width % floor.Width) : floor, 
          new Vector2(Position.X + floor.Width * i, Position.Y - 9), 
          Color.White
        ); 
      }

      n = width / ground.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? ground.ClampWidth(width % ground.Width) : ground, 
          new Vector2(Position.X + ground.Width * i, Position.Y + 3), 
          Color.White
        );     
      }
    }

    /*private void DrawFgGrass()
    {
      var grass = core.SpriteManager.GetSprite("floor_grass");

      for (var i = 0; i <= (core.Renderer.ScreenWidth / grass.Width) + 1; ++i)
      {
        core.Renderer.DrawSpriteS(grass, new Vector2(grass.Width * i - groundScroll % grass.Width + 7, level.GetGroundLevel() - 3), Color.White);        
      }
    }*/
  }
}

