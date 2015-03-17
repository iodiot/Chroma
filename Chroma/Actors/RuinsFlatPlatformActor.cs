using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Helpers;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class RuinsFlatPlatformActor : FlatPlatformActor
  {
    protected override void GetSprites()
    {
      floorSprite = core.SpriteManager.GetSprite("ruins_floor_" + ScienceHelper.GetRandom(1, 2).ToString());

      base.GetSprites();
    }

    public RuinsFlatPlatformActor(Core core, Vector2 position, int width, Area area) : base(core, position, width, area)
    {
      // Vines
      if (ScienceHelper.ChanceRoll(0.5f))
      {
        var n = ScienceHelper.GetRandom(1, 4);
        var vine = core.SpriteManager.GetSprite("vine_" + n.ToString());
        var x = Position.X + ScienceHelper.GetRandom(4, width - vine.Width - 4);
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
      else
      {
        var leaves = core.SpriteManager.GetSprite("leaves_" + ScienceHelper.GetRandom(1, 3).ToString());
        var x = Position.X + ScienceHelper.GetRandom(2, width - leaves.Width - 2);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y),
          leaves,
          flip: ScienceHelper.ChanceRoll(0.5f),
          depth: 10
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
      }

    }
  }
}

