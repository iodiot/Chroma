using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Helpers;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class JungleFlatPlatformActor : FlatPlatformActor
  {
    private Sprite grassSprite;

    protected override void GetSprites()
    {
      floorSprite = core.SpriteManager.GetSprite("floor");
      grassSprite = core.SpriteManager.GetSprite("floor_grass");

      base.GetSprites();
    }

    public JungleFlatPlatformActor(Core core, Vector2 position, int width, Area area) : base(core, position, width, area)
    {
      // Vines
      if (ScienceHelper.ChanceRoll(0.5f))
      {
        var n = ScienceHelper.GetRandom(1, 4);
        var vine = core.SpriteManager.GetSprite("vine_" + n.ToString());
        var x = Position.X + ScienceHelper.GetRandom(4, width - vine.Width);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y),
          vine,
          flip: ScienceHelper.ChanceRoll(0.5f),
          depth: 10
        )), this);
        if (ScienceHelper.ChanceRoll(0.3f))
        {
          x += 8;
          var m = ScienceHelper.GetRandom(1, 4);
          while (m == n) m = ScienceHelper.GetRandom(1, 4);
          var vine2 = core.SpriteManager.GetSprite("vine_" + m.ToString());
          core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
            new Vector2(x, Position.Y),
            vine2,
            flip: ScienceHelper.ChanceRoll(0.5f),
            depth: 10
          )), this);
        }
      }

      // Boulders
      if (ScienceHelper.ChanceRoll(0.8f))
      {
        var boulder = core.SpriteManager.GetSprite("earth_boulder_" + ScienceHelper.GetRandom(1, 4).ToString());
        var x = Position.X + ScienceHelper.GetRandom(4, width - 4 - 30);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y + ScienceHelper.GetRandom(15, 60)),
          boulder
        )), this);
      }
    }

    protected override void DrawGround()
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

