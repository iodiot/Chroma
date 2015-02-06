using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public class FlatPlatformActor : PlatformActor
  {
    private readonly int width;
    private readonly Sprite floorSprite, grassSprite;

    public FlatPlatformActor(Core core, Vector2 position, int width) : base(core, position)
    {
      this.width = width;

      boundingBox = new Rectangle(0, 0, width, 300);
      LeftY = RightY = (int)position.Y;

      CanMove = false;
      CanFall = true;

      floorSprite = core.SpriteManager.GetSprite("floor");
      grassSprite = core.SpriteManager.GetSprite("floor_grass");
    }

    public override void Draw()
    {
      DrawEdges();
      DrawGround();

      base.Draw();
    }

    private void DrawGround()
    {
      var n = width / floorSprite.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? floorSprite.ClampWidth(width % floorSprite.Width) : floorSprite, 
          new Vector2(Position.X + floorSprite.Width * i, Position.Y - 11)
        );
        core.Renderer.DrawSpriteW(
          i == n ? grassSprite.ClampWidth(width % grassSprite.Width) : grassSprite, 
          new Vector2(Position.X + grassSprite.Width * i, Position.Y - 4)
        );
      }

    }
  }
}

