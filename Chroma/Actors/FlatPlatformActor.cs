using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;

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

      // Vines
      if (core.ChanceRoll(0.5f))
      {
        var x = Position.X + core.GetRandom(4, width - 12);
        var vine = core.GetRandom(1, 4);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y),
          "vine_" + vine.ToString(),
          flip: core.ChanceRoll(0.5f),
          depth: 10
            )), this);
        if (core.ChanceRoll(0.3f))
        {
          x += 8;
          var vine2 = core.GetRandom(1, 4);
          while (vine == vine2)
          {
            vine2 = core.GetRandom(1, 4);
          }
          core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
            new Vector2(x, Position.Y),
            "vine_" + vine2.ToString(),
            flip: core.ChanceRoll(0.5f),
            depth: 10
          )), this);
        }
      }

      // Boulders
      if (core.ChanceRoll(0.8f))
      {
        var boulder = core.GetRandom(1, 4);
        var x = Position.X + core.GetRandom(4, width - 4 - 15);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y + core.GetRandom(15, 60)),
          "earth_boulder_" + boulder.ToString()
        )), this);
      }
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
          new Vector2(Position.X + floorSprite.Width * i, Position.Y - 13)
        );
        core.Renderer[50].DrawSpriteW(
          i == n ? grassSprite.ClampWidth(width % grassSprite.Width) : grassSprite, 
          new Vector2(Position.X + grassSprite.Width * i, Position.Y - 6)
        );
      }

    }
  }
}

